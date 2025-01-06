using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will animate a blendshape by it's index. (adapted from MMF_Scale, remapping may be off since blendshapes are 0-100).
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Renderer/Blendshape")]
	[FeedbackHelp("This feedback will animate the target's scale on the 3 specified animation curves, for the specified duration (in seconds). You can apply a multiplier, that will multiply each animation curve value.")]
	public class BlendshapeFeedback : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes this feedback can operate on
		public enum Modes { Absolute, Additive, ToDestination }
		/// the possible timescales for the animation of the scale
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (Skin == null); }
		public override string RequiredTargetText { get { return Skin != null ? Skin.name : ""; } }
		public override string RequiresSetupText { get { return "This feedback requires that an SkinnedMeshRenderer be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => Skin = FindAutomatedTarget<SkinnedMeshRenderer>();

		[MMFInspectorGroup("Weight Mode", true, 12, true)]
		/// the mode this feedback should operate on
		/// Absolute : follows the curve
		/// Additive : adds to the current weight of the target
		/// ToDestination : sets the weight to the destination target, whatever the current weight is
		[Tooltip("the mode this feedback should operate on" +
				 "Absolute : follows the curve" +
				 "Additive : adds to the current weight of the target" +
				 "ToDestination : sets the weight to the destination target, whatever the current weight is")]
		public Modes Mode = Modes.Absolute;
		/// the object to animate
		[Tooltip("the object to animate")]
		public SkinnedMeshRenderer Skin;
		[Tooltip("The blenshape to animate.")]
		public int TargetBlendshapeIndex = 0;

		[MMFInspectorGroup("Weight Animation", true, 13)]
		/// the duration of the animation
		[Tooltip("the duration of the animation")]
		public float AnimateWeightDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 1f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[FormerlySerializedAs("Multiplier")]
		public float RemapCurveOne = 2f;
		/// if this is true, should animate the weight value
		[Tooltip("if this is true, should animate the weight value")]
		public bool Animate = true;
		/// the weight animation definition
		[Tooltip("the weight animation definition")]
		[MMFCondition("Animate", true)]
		public MMTweenType AnimateWeightTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 100), new Keyframe(1, 0)));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")]
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination weights will be recomputed on every play
		[Tooltip("if this is true, initial and destination weights will be recomputed on every play")]
		public bool DetermineWeightOnPlay = false;
		/// the weight to reach when in ToDestination mode
		[Tooltip("the weight to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationWeight = 100;

		/// the duration of this feedback is the duration of the weight animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateWeightDuration); } set { AnimateWeightDuration = value; } }
		public override bool HasRandomness => true;

		/// [DEPRECATED] the z weight animation definition
		[HideInInspector] public AnimationCurve AnimateWeight = null;

		protected float _initialWeight;
		protected float _newWeight;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial weight
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (Skin != null))
			{
				GetInitialWeight();
			}
		}

		/// <summary>
		/// Stores initial weight for future use
		/// </summary>
		protected virtual void GetInitialWeight()
		{
			_initialWeight = Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
		}

		/// <summary>
		/// On Play, triggers the weight animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (Skin == null))
			{
				return;
			}

			if (DetermineWeightOnPlay && NormalPlayDirection)
			{
				GetInitialWeight();
			}

			//there is certainly a better way to do this. (blendshapes should be 0-100) remapping changes if using MMTweens.
			if (AnimateWeightTween.MMTweenDefinitionType == MMTweenDefinitionTypes.MMTween)
            {
				RemapCurveOne = Mathf.Clamp(RemapCurveOne * 50, 0, 100); 
            }

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(AnimateBlendshape(Skin, 0, FeedbackDuration, AnimateWeightTween, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(WeightToDestination());
				}
			}
		}

		/// <summary>
		/// An internal coroutine used to weight the target to its destination weight
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator WeightToDestination()
		{
			if (Skin == null)
			{
				yield break;
			}

			if (AnimateWeightTween == null)
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialWeight = Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
			_newWeight = _initialWeight;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				if (Animate)
				{
					_newWeight = Mathf.LerpUnclamped(_initialWeight, DestinationWeight, AnimateWeightTween.Evaluate(percent));
					_newWeight = MMFeedbacksHelpers.Remap(_newWeight, 0f, 1f, RemapCurveZero, RemapCurveOne);
				}

				Skin.SetBlendShapeWeight(TargetBlendshapeIndex, Mathf.Clamp(_newWeight, 0, 100));

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;

				yield return null;
			}

			Skin.SetBlendShapeWeight(TargetBlendshapeIndex, NormalPlayDirection ? DestinationWeight : _initialWeight);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		protected virtual IEnumerator AnimateBlendshape(SkinnedMeshRenderer skin, float weight, float duration, MMTweenType curve, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (skin == null)
			{
				yield break;
			}

			if (curve == null)
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : duration;

			_initialWeight = Skin.GetBlendShapeWeight(TargetBlendshapeIndex);

			IsPlaying = true;

			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				weight = 0;
				float percent = Mathf.Clamp01(journey / duration);

				if (Animate)
				{
					weight = Animate ? curve.Evaluate(percent) : Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
					weight = MMFeedbacksHelpers.Remap(weight, 0f, 1f, remapCurveZero, remapCurveOne);
					if (Mode == Modes.Additive)
					{
						weight += _initialWeight;
					}
				}
				else
				{
					weight = Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
				}

				skin.SetBlendShapeWeight(TargetBlendshapeIndex, weight);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;

				yield return null;
			}

			weight = 0;

			if (Animate)
			{
				weight = Animate ? curve.Evaluate(FinalNormalizedTime) : Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
				weight = MMFeedbacksHelpers.Remap(weight, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					weight += _initialWeight;
				}
			}
			else
			{
				weight = Skin.GetBlendShapeWeight(TargetBlendshapeIndex);
			}

			skin.SetBlendShapeWeight(TargetBlendshapeIndex, Mathf.Clamp(weight,0,100));
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;

		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}

		/// <summary>
		/// On Validate, we migrate our deprecated animation curves to our tween types if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(AnimateWeight, AnimateWeightTween, Owner);
		}

		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			Skin.SetBlendShapeWeight(TargetBlendshapeIndex, _initialWeight);
		}
	}
}

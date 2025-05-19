using UnityEngine;
using FMODUnity;
using MoreMountains.Tools;


namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("")]
	[FeedbackHelp("Feedback to set a parameter on an fmod event.")]
	[FeedbackPath("Audio/fmod Set Parameter")]
	public class FMODParameterFeedback : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// use this override to specify the duration of your feedback (don't hesitate to look at other feedbacks for reference)
		public override float FeedbackDuration { get { return 0f; } }
		/// pick a color here for your feedback's inspector
    		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
    		#endif

		[MMFInspectorGroup("fmod Parameter", true, 87)]
		[Tooltip("the target emitter to set a parameter on")]
		public StudioEventEmitter targetEmitter;
		[Tooltip("the name of the parameter")]
		public string paramName;
		[Tooltip("the value to set the parameter to")]
		public float paramValue;

		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			// your init code goes here
		}

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}            
			FMOD.Studio.PLAYBACK_STATE pS;
			targetEmitter.EventInstance.getPlaybackState(out pS);
			if (pS == FMOD.Studio.PLAYBACK_STATE.SUSTAINING) {
				targetEmitter.EventInstance.keyOff();
			}
			targetEmitter.SetParameter(paramName, paramValue);
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!FeedbackTypeAuthorized)
			{
				return;
			}            
		}
	}
}


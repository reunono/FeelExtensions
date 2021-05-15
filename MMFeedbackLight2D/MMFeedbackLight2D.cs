using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will let you control the color and intensity of a 2D Light when played
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you control the color and intensity of a 2D Light in your scene for a certain duration (or instantly).")]
    [FeedbackPath("Light2D")]
    public class MMFeedbackLight : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LightColor; } }
        #endif

        /// the possible modes for this feedback
        public enum Modes { OverTime, Instant, ShakerEvent }

        [Header("Light")]
        /// the light to affect when playing the feedback
        public Light2D BoundLight;
        /// whether the feedback should affect the light instantly or over a period of time
        public Modes Mode = Modes.OverTime;
        /// how long the light should change over time
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float Duration = 0.2f;
        /// whether or not that light should be turned off on start
        public bool StartsOff = true;
        /// whether or not the values should be relative or not
        public bool RelativeValues = true;
        /// the channel to broadcast on
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public int Channel = 0;
        /// whether or not to reset shaker values after shake
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetShakerValuesAfterShake = true;
        /// whether or not to reset the target's values after shake
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool ResetTargetValuesAfterShake = true;
        /// whether or not to broadcast a range to only affect certain shakers
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public bool UseRange = false;
        /// the range of the event, in units
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public float EventRange = 100f;
        /// the transform to use to broadcast the event as origin point
        [MMFEnumCondition("Mode", (int)Modes.ShakerEvent)]
        public Transform EventOriginTransform;


        [Header("Color")]
        public bool ModifyColor = true;
        /// the colors to apply to the light over time
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public Gradient ColorOverTime;
        /// the color to move to in instant mode
        [MMFEnumCondition("Mode", (int)Modes.Instant, (int)Modes.ShakerEvent)]
        public Color InstantColor;

        [Header("Intensity")]
        /// the curve to tween the intensity on
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// the value to remap the intensity curve's 0 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapIntensityZero = 0f;
        /// the value to remap the intensity curve's 1 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapIntensityOne = 1f;
        /// the value to move the intensity to in instant mode
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantIntensity;

        [Header("Range")]
        /// the range to apply to the light over time
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve RangeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// the value to remap the range curve's 0 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapRangeZero = 0f;
        /// the value to remap the range curve's 0 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapRangeOne = 10f;
        /// the value to move the intensity to in instant mode
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantRange;

        [Header("Shadow Strength")]
        /// the range to apply to the light over time
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// the value to remap the shadow strength's curve's 0 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapShadowStrengthZero = 0f;
        /// the value to remap the shadow strength's curve's 1 to
        [MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ShakerEvent)]
        public float RemapShadowStrengthOne = 1f;
        /// the value to move the shadow strength to in instant mode
        [MMFEnumCondition("Mode", (int)Modes.Instant)]
        public float InstantShadowStrength;

        protected float _initialRange;
        protected float _initialShadowStrength;
        protected float _initialIntensity;

        /// the duration of this feedback is the duration of the light, or 0 if instant
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : Duration; } }

        /// <summary>
        /// On init we turn the light off if needed
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (BoundLight.lightType == Light2D.LightType.Point)
            {
                _initialRange = BoundLight.pointLightOuterRadius;
            }
            _initialShadowStrength = BoundLight.shadowIntensity;
            _initialIntensity = BoundLight.intensity;

            if (EventOriginTransform == null)
            {
                EventOriginTransform = this.transform;
            }

            if (Active)
            {
                if (StartsOff)
                {
                    Turn(false);
                }
            }
        }

        /// <summary>
        /// On Play we turn our light on and start an over time coroutine if needed
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                Turn(true);
                switch (Mode)
                {
                    case Modes.Instant:
                        BoundLight.intensity = InstantIntensity;
                        BoundLight.shadowIntensity = InstantShadowStrength;
                        if (BoundLight.lightType == Light2D.LightType.Point)
                        {
                            BoundLight.pointLightOuterRadius = InstantRange;
                        }
                        if (ModifyColor)
                        {
                            BoundLight.color = InstantColor;
                        }                        
                        break;
                    case Modes.OverTime:
                        StartCoroutine(LightSequence());
                        break;
                    case Modes.ShakerEvent:
                        MMLightShakeEvent.Trigger(Duration, RelativeValues, ModifyColor, ColorOverTime, IntensityCurve,
                            RemapIntensityZero, RemapIntensityOne, RangeCurve, RemapRangeZero, RemapRangeOne,
                            ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, attenuation,
                            Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
                            UseRange, EventRange, EventOriginTransform.position);
                        break;
                }
            }
        }

        /// <summary>
        /// This coroutine will modify the intensity and color of the light over time
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LightSequence()
        {
            float journey = 0f;
            while (journey < Duration)
            {
                float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, Duration, 0f, 1f);

                SetLightValues(remappedTime);

                journey += FeedbackDeltaTime;
                yield return null;
            }
            SetLightValues(1f);
            if (StartsOff)
            {
                Turn(false);
            }            
            yield return null;
        }

        /// <summary>
        /// Sets the various values on the light on a specified time (between 0 and 1)
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetLightValues(float time)
        {
            float intensity = MMFeedbacksHelpers.Remap(IntensityCurve.Evaluate(time), 0f, 1f, RemapIntensityZero, RemapIntensityOne);
            float range = MMFeedbacksHelpers.Remap(RangeCurve.Evaluate(time), 0f, 1f, RemapRangeZero, RemapRangeOne);
            float shadowStrength = MMFeedbacksHelpers.Remap(ShadowStrengthCurve.Evaluate(time), 0f, 1f, RemapShadowStrengthZero, RemapShadowStrengthOne);        

            if (RelativeValues)
            {
                intensity += _initialIntensity;
                shadowStrength += _initialShadowStrength;
                range += _initialRange;
            }

            BoundLight.intensity = intensity;
            if(BoundLight.lightType == Light2D.LightType.Point)
            {
                BoundLight.pointLightOuterRadius = range;
            }
            BoundLight.shadowIntensity = Mathf.Clamp01(shadowStrength);
            if (ModifyColor)
            {
                BoundLight.color = ColorOverTime.Evaluate(time);
            }
        }

        /// <summary>
        /// Turns the light off on stop
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomStopFeedback(Vector3 position, float attenuation = 1)
        {
            base.CustomStopFeedback(position, attenuation);
            if (Active)
            {
                Turn(false);
            }
        }

        /// <summary>
        /// Turns the light on or off
        /// </summary>
        /// <param name="status"></param>
        protected virtual void Turn(bool status)
        {
            BoundLight.gameObject.SetActive(status);
            BoundLight.enabled = status;
        }
    }
}

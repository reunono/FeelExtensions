using Cinemachine.PostFX;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// A Vignette Shaker that works with a Cinemachine Post Processing component with a Vignette effect.
/// This Shaker should be on the same game object as the Cinemachine Virtual Camera.
///
/// The MMVignetteShaker requires a post process volume which is not needed for Cinemachine
/// </summary>
[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachineVignetteShaker")]
#if MM_CINEMACHINE
[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
#endif
public class MMCinemachineVignetteShaker : MMShaker
{
    [MMInspectorGroup("Vignette Intensity", true, 53)]
    /// whether or not to add to the initial value
    [Tooltip("whether or not to add to the initial value")]
    public bool RelativeIntensity = true;

    /// the curve used to animate the intensity value on
    [Tooltip("the curve used to animate the intensity value on")]
    public AnimationCurve ShakeIntensity =
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    /// the value to remap the curve's 0 to
    [Tooltip("the value to remap the curve's 0 to")]
    [Range(0f, 1f)]
    public float RemapIntensityZero = 0f;

    /// the value to remap the curve's 1 to
    [Tooltip("the value to remap the curve's 1 to")]
    [Range(0f, 1f)]
    public float RemapIntensityOne = 0.1f;

    [MMInspectorGroup("Vignette Color", true, 51)]
    /// whether or not to also animate  the vignette's color
    [Tooltip("whether or not to also animate the vignette's color")]
    public bool InterpolateColor = false;

    /// the curve to animate the color on
    [Tooltip("the curve to animate the color on")]
    public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.05f, 1f),
        new Keyframe(0.95f, 1), new Keyframe(1, 0));

    /// the value to remap the curve's 0 to
    [Tooltip("the value to remap the curve's 0 to")]
    [Range(0, 1)]
    public float RemapColorZero = 0f;

    /// the value to remap the curve's 1 to
    [Tooltip("the value to remap the curve's 1 to")]
    [Range(0f, 1f)]
    public float RemapColorOne = 1f;

    /// the color to lerp towards
    [Tooltip("the color to lerp towards")]
    public Color TargetColor = Color.red;

    protected Vignette _vignette;
    protected float _initialIntensity;
    protected float _originalShakeDuration;
    protected AnimationCurve _originalShakeIntensity;
    protected float _originalRemapIntensityZero;
    protected float _originalRemapIntensityOne;
    protected bool _originalRelativeIntensity;

    protected bool _originalInterpolateColor;
    protected AnimationCurve _originalColorCurve;
    protected float _originalRemapColorZero;
    protected float _originalRemapColorOne;
    protected Color _originalTargetColor;
    protected Color _initialColor;

    protected override void Initialization()
    {
        var _virtualCamera = gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        var _postProcessing = _virtualCamera.GetComponent<CinemachinePostProcessing>();
        _postProcessing.m_Profile.TryGetSettings(out _vignette);
        _initialColor = _vignette.color;
    }

    public virtual void SetVignette(float newValue)
    {
        _vignette.intensity.Override(newValue);
    }

    /// <summary>
    /// Shakes values over time
    /// </summary>
    protected override void Shake()
    {
        float newValue = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeIntensity,
            _initialIntensity);
        _vignette.intensity.Override(newValue);

        if (InterpolateColor)
        {
            float newColorValue = ShakeFloat(ColorCurve, RemapColorZero, RemapColorOne, RelativeIntensity, 0);
            _vignette.color.Override(Color.Lerp(_initialColor, TargetColor, newColorValue));
        }
    }

    /// <summary>
    /// Collects initial values on the target
    /// </summary>
    protected override void GrabInitialValues()
    {
        _initialIntensity = _vignette.intensity;
    }

    /// <summary>
    /// When we get the appropriate event, we trigger a shake
    /// </summary>
    /// <param name="intensity"></param>
    /// <param name="duration"></param>
    /// <param name="amplitude"></param>
    /// <param name="relativeIntensity"></param>
    /// <param name="feedbacksIntensity"></param>
    /// <param name="channel"></param>
    public virtual void OnVignetteShakeEvent(AnimationCurve intensity, float duration, float remapMin,
        float remapMax, bool relativeIntensity = false,
        float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true,
        bool resetTargetValuesAfterShake = true,
        bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false,
        bool restore = false,
        bool interpolateColor = false, AnimationCurve colorCurve = null, float remapColorZero = 0f,
        float remapColorOne = 1f, Color targetColor = default(Color))
    {
        if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
        {
            return;
        }

        if (stop)
        {
            Stop();
            return;
        }

        if (restore)
        {
            ResetTargetValues();
            return;
        }

        _resetShakerValuesAfterShake = resetShakerValuesAfterShake;
        _resetTargetValuesAfterShake = resetTargetValuesAfterShake;

        if (resetShakerValuesAfterShake)
        {
            _originalShakeDuration = ShakeDuration;
            _originalShakeIntensity = ShakeIntensity;
            _originalRemapIntensityZero = RemapIntensityZero;
            _originalRemapIntensityOne = RemapIntensityOne;
            _originalRelativeIntensity = RelativeIntensity;
            _originalInterpolateColor = InterpolateColor;
            _originalColorCurve = ColorCurve;
            _originalRemapColorZero = RemapColorZero;
            _originalRemapColorOne = RemapColorOne;
            _originalTargetColor = TargetColor;
        }

        if (!OnlyUseShakerValues)
        {
            TimescaleMode = timescaleMode;
            ShakeDuration = duration;
            ShakeIntensity = intensity;
            RemapIntensityZero = remapMin * feedbacksIntensity;
            RemapIntensityOne = remapMax * feedbacksIntensity;
            RelativeIntensity = relativeIntensity;
            ForwardDirection = forwardDirection;
            InterpolateColor = interpolateColor;
            ColorCurve = colorCurve;
            RemapColorZero = remapColorZero;
            RemapColorOne = remapColorOne;
            TargetColor = targetColor;
        }

        Play();
    }

    /// <summary>
    /// Resets the target's values
    /// </summary>
    protected override void ResetTargetValues()
    {
        base.ResetTargetValues();
        _vignette.intensity.Override(_initialIntensity);
        _vignette.color.Override(_initialColor);
    }

    /// <summary>
    /// Resets the shaker's values
    /// </summary>
    protected override void ResetShakerValues()
    {
        base.ResetShakerValues();
        ShakeDuration = _originalShakeDuration;
        ShakeIntensity = _originalShakeIntensity;
        RemapIntensityZero = _originalRemapIntensityZero;
        RemapIntensityOne = _originalRemapIntensityOne;
        RelativeIntensity = _originalRelativeIntensity;
        InterpolateColor = _originalInterpolateColor;
        ColorCurve = _originalColorCurve;
        RemapColorZero = _originalRemapColorZero;
        RemapColorOne = _originalRemapColorOne;
        TargetColor = _originalTargetColor;
    }

    /// <summary>
    /// Starts listening for events
    /// </summary>
    public override void StartListening()
    {
        base.StartListening();
        MMVignetteShakeEvent.Register(OnVignetteShakeEvent);
    }

    /// <summary>
    /// Stops listening for events
    /// </summary>
    public override void StopListening()
    {
        base.StopListening();
        MMVignetteShakeEvent.Unregister(OnVignetteShakeEvent);
    }
}
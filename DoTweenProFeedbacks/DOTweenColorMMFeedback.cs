using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;


/// <summary>
/// This feedback will let you change the color of a target Renderer over time.
/// I would be happy to more DOTween feedback or add to some of the functionality, address me via github:@pauldyatlov  
/// </summary>
[AddComponentMenu("")]
[FeedbackHelp("This feedback will let you change the color of a target Renderer over time.")]
[FeedbackPath("DOTween/Color")]
[Serializable]
public sealed class DOTweenColorMMFeedback : MMF_Feedback
{
    [MMFInspectorGroup("Renderer", true, 54, true)]
    [Tooltip("The Renderer to affect when playing the feedback")]
    [SerializeField] private Renderer _renderer;

    [Tooltip("For how long the Renderer should change its color over time")]
    [SerializeField] private float _duration = 0.2f;

    [Tooltip("Ff this is true, the target will be disabled when this feedbacks is stopped")]
    [SerializeField] private bool _disableOnStop = true;

    [Tooltip("The color to move to")]
    [SerializeField] private Color _toColor;

    private Tweener _colorTweener;

#if UNITY_EDITOR
    public override Color FeedbackColor => MMFeedbacksInspectorColors.UIColor;

    public override bool EvaluateRequiresSetup() => _renderer == null;

    public override string RequiredTargetText => _renderer != null ? _renderer.name : string.Empty;
    public override string RequiresSetupText => "This feedback requires that a TargetGraphic be set to be able to work properly. You can set one below.";
#endif

    public override float FeedbackDuration
    {
        get => ApplyTimeMultiplier(_duration);
        set => _duration = value;
    }

    public override bool HasChannel => true;

    /// <summary>
    /// On Play we turn our Graphic on and start an over time coroutine if needed
    /// </summary>
    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (Active == false)
            return;

        Turn(true);

        var fromColor = _renderer.material.color;
        _colorTweener = _renderer.material.DOColor(_toColor, _duration)
            .OnComplete(() => _renderer.material.DOColor(fromColor, _duration));
    }

    /// <summary>
    /// Turns the Graphic off on stop
    /// </summary>
    protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
    {
        if (Active == false)
            return;

        IsPlaying = false;

        _colorTweener.Kill();

        if (_disableOnStop)
            Turn(false);
    }

    /// <summary>
    /// Turns the Graphic on or off
    /// </summary>
    private void Turn(bool status)
    {
        _renderer.gameObject.SetActive(status);
        _renderer.enabled = status;
    }
}
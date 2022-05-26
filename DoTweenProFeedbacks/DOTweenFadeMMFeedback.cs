using System;
using DG.Tweening;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will make the material of a chosen Renderer to change alpha over time
    /// I would be happy to add more DOTween feedbacks or add new functionality by your request. Feel free to address me via github:@pauldyatlov
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will make the material of a chosen Renderer to change alpha over time. Please note that your Renderer material should support changing its alpha")]
    [FeedbackPath("DOTween/Renderer Fade")]
    [Serializable]
    public sealed class DOTweenFadeMMFeedback : MMF_Feedback
    {
        [MMFInspectorGroup("Renderer", true, 54, true)]
        [Tooltip("The Renderer to affect when playing the feedback")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("For how long the Renderer should change its color over time (per one iteration)")]
        [SerializeField] private float _duration = 0.2f;

        [Tooltip("Ff this is true, the target will be disabled when this feedbacks is stopped")]
        [SerializeField] private bool _disableOnStop;

        [Tooltip("The value to move the alpha to")]
        [SerializeField] private float _toAlpha = 0.1f;

        [Tooltip("Should the alpha value go back to the original")]
        [SerializeField] private bool _goBack = true;

        private Tweener _alphaTweener;

#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.UIColor;

        public override bool EvaluateRequiresSetup() => _renderer == null;

        public override string RequiredTargetText => _renderer != null ? _renderer.name : string.Empty;
        public override string RequiresSetupText => "This feedback requires a Renderer to be set to be able to work properly. You can set one below.";
#endif

        public override float FeedbackDuration
        {
            get => ApplyTimeMultiplier(_duration);
            set => _duration = value;
        }

        public override bool HasChannel => true;

        /// <summary>
        /// On Play we turn our Renderer on and start an over time coroutine if needed
        /// </summary>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active == false)
                return;

            Turn(true);
            EndTweener();

            var fromAlpha = _renderer.material.color.a;
            _alphaTweener = _renderer.material.DOFade(_toAlpha, _duration);

            if (_goBack)
                _alphaTweener = _alphaTweener.OnComplete(() => _renderer.material.DOFade(fromAlpha, _duration));
        }

        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (Active == false)
                return;

            IsPlaying = false;

            EndTweener();

            if (_disableOnStop)
                Turn(false);
        }

        private void EndTweener()
        {
            _alphaTweener?.Kill(true);
        }

        /// <summary>
        /// Turns the Renderer on or off
        /// </summary>
        private void Turn(bool status)
        {
            _renderer.gameObject.SetActive(status);
            _renderer.enabled = status;
        }
    }
}
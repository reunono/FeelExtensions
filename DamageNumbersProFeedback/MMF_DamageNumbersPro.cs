using System.Globalization;
using DamageNumbersPro;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

[AddComponentMenu("")]
[FeedbackPath("Popups/Damage Numbers Pro")]
public class MMF_DamageNumbersPro : MMF_Feedback
{
    /// a static bool used to disable all feedbacks of this type at once
    public static bool FeedbackTypeAuthorized = true;
    /// sets the inspector color for this feedback
    #if UNITY_EDITOR
    public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
    #endif

    /// the duration of this feedback is a fixed value or the lifetime
    public override float FeedbackDuration { get { return ApplyTimeMultiplier(Lifetime); } set { Lifetime = value; } }
    public override bool HasChannel => true;
    public override bool HasRandomness => true;

    /// the possible places where the floating text should spawn at
    public enum PositionModes { TargetTransform, FeedbackPosition, PlayPosition }

    [MMFInspectorGroup("Damage Numbers Pro", true, 64)]
    public DamageNumber DamageNumber;
    [Tooltip("the value to display when spawning this text")]
    public string Value = "100";
    [Tooltip("if this is true, the intensity passed to this feedback will be the value displayed")]
    public bool UseIntensityAsValue = true;
    
    public enum RoundingMethods { NoRounding, Round, Ceil, Floor }
    
    public RoundingMethods RoundingMethod = RoundingMethods.NoRounding;

    public bool ForceColor = false;
    [MMFCondition("ForceColor", true)]
    public VertexGradient AnimateColorGradient = new();

    public bool ForceLifetime;
    [MMFCondition("ForceLifetime", true)]
    public float Lifetime = 0.5f;

    [MMFInspectorGroup("Position", true, 67)]
    public PositionModes PositionMode = PositionModes.PlayPosition;
    [MMFEnumCondition("PositionMode", (int)PositionModes.TargetTransform)]
    public Transform TargetTransform;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (!Active || !FeedbackTypeAuthorized) return;

        position = PositionMode switch{
            PositionModes.FeedbackPosition => Owner.transform.position,
            PositionModes.TargetTransform => TargetTransform.position,
            _ => position
        };

        if (ForceLifetime) DamageNumber.lifetime = Lifetime;
        if (ForceColor) DamageNumber.SetGradientColor(AnimateColorGradient);
        DamageNumber.Spawn(position, UseIntensityAsValue ? ApplyRounding(feedbacksIntensity).ToString(CultureInfo.InvariantCulture) : Value);
        
        float ApplyRounding(float value)
            => RoundingMethod switch{
                RoundingMethods.Round => Mathf.Round(value),
                RoundingMethods.Ceil => Mathf.Ceil(value),
                RoundingMethods.Floor => Mathf.Floor(value),
                _ => value
            };
    }
}

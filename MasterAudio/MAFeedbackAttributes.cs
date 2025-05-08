using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A custom property attribute used within the MoreMountains Feedbacks system to provide contextual help annotations
    /// for feedback-related properties in Unity inspectors.
    /// </summary>
    /// <remarks>
    /// This attribute decoratively applies additional information for visual or functional guidance on specific fields
    /// in inspectors. It is commonly used to enhance user experience by providing meaningful descriptions or warnings
    /// about the associated field.
    /// </remarks>
    public class MAFeedbackHelpAttribute : PropertyAttribute
    {
        public MAFeedbackHelpAttribute()
        {
        }
    }
}

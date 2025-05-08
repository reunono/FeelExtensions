using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public class MAFeedbackHelpAttribute : PropertyAttribute
    {
        public string propertyLabel;

        public MAFeedbackHelpAttribute(string _label)
        {
            propertyLabel = _label;
        }
    }
}

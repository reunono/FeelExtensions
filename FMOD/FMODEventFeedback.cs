using UnityEngine;
using MoreMountains.Feedbacks;

namespace YOURNAMESPACE
{
    [AddComponentMenu("")]
    [FeedbackHelp("FMOD Event feedback")]
    [FeedbackPath("YOURPATH/FMODEvent")]
    public class FmodEventFeedback : MMFeedback
    {
        [FMODUnity.EventRef]
        public string EventName;

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (EventName != "")
            {
                FMODUnity.RuntimeManager.PlayOneShot(EventName, position);
            }
        }
    }
}
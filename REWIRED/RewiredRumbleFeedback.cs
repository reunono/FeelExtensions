using MoreMountains.Feedbacks;
using UnityEngine;
using Rewired;
using System.Collections;

namespace FeelExtensions.REWIRED
{
    [AddComponentMenu("")]
    [FeedbackHelp("Rumble Feedback")]
    [FeedbackPath("REWIRED/Rumble Feedback")]
    public class RewiredRumbleFeedback : MMF_Feedback
    {
        public const string RewiredSettingsGroupName = "Rewired Settings";

        [MMFInspectorGroup(RewiredSettingsGroupName, true, 0)]
        [Tooltip("The Player ID as configured in Rewired.")]
        public int PlayerID = 0;

        [Tooltip("The motor index to use. Set -1 to affect all motors.")]
        public int MotorIndex = -1;

        [Tooltip("The vibration intensity (0.0 to 1.0).")]
        [Range(0f, 1f)]
        public float VibrationIntensity = 1.0f;

        [Tooltip("The duration of the vibration in seconds.")]
        public float VibrationDuration = 0.5f;

        protected override void CustomPlayFeedback(Vector3 position, float intensity = 1.0f)
        {
            if (!Active || Owner == null)
                return;

            // Get the Rewired player
            Player player = ReInput.players.GetPlayer(PlayerID);
            if (player == null)
            {
                Debug.LogWarning($"Rewired Player with ID {PlayerID} not found.");
                return;
            }

            float finalDuration = VibrationDuration * intensity;

            foreach (Joystick joystick in player.controllers.Joysticks)
            {
                if (MotorIndex == -1)
                {
                    joystick.SetVibration(VibrationIntensity, finalDuration);
                }
                else
                {
                    joystick.SetVibration(MotorIndex, VibrationIntensity, finalDuration);
                }
            }

            if (finalDuration > 0)
            {
                Owner.StartCoroutine(StopVibrationAfterDelay(player, finalDuration));
            }
        }

        protected override void CustomStopFeedback(Vector3 position, float intensity = 1.0f)
        {
            if (Owner == null)
                return;

            Player player = ReInput.players.GetPlayer(PlayerID);
            if (player == null)
                return;

            foreach (Joystick joystick in player.controllers.Joysticks)
            {
                joystick.StopVibration();
            }
        }

        private IEnumerator StopVibrationAfterDelay(Player player, float duration)
        {
            yield return new WaitForSeconds(duration);

            foreach (Joystick joystick in player.controllers.Joysticks)
            {
                joystick.StopVibration();
            }
        }
    }
}

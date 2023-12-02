using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you apply forces and torques (relative or not) to a Rigidbody (Extended).
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you apply forces and torques (relative or not) to a Rigidbody.")]
	[FeedbackPath("GameObject/RigidbodyExtended")]
	public class CustomRigidbodyFeedback : MMF_Rigidbody
	{	
		[Tooltip("Clears velocity before applying force (usefull for pooled objects)")]
		public bool zeroVelocity = true;
		public bool useForwardForce = true;
		public float forwardForce = 1;

		/// <summary>
		/// On Custom Play, we apply our force or torque to the target rigidbody
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRigidbody == null))
			{
				return;
			}

			_force.x = Random.Range(MinForce.x, MaxForce.x);
			_force.y = Random.Range(MinForce.y, MaxForce.y);
			_force.z = Random.Range(MinForce.z, MaxForce.z);

			if (!Timing.ConstantIntensity)
			{
				_force *= feedbacksIntensity;
			}

			ApplyForce(TargetRigidbody);
			foreach (Rigidbody rb in ExtraTargetRigidbodies)
			{
				ApplyForce(rb);
			}
		}

		/// <summary>
		/// Applies the computed force to the target rigidbody
		/// </summary>
		/// <param name="rb"></param>
		protected override void ApplyForce(Rigidbody rb)
		{
			if(zeroVelocity == true)
            {
				rb.velocity = Vector3.zero;
            }
			if(useForwardForce == true)
            {
				_force = rb.transform.forward * forwardForce;
            }
			switch (Mode)
			{
				case Modes.AddForce:
					rb.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					rb.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					rb.AddTorque(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeTorque:
					rb.AddRelativeTorque(_force, AppliedForceMode);
					break;
			}
		}
	}
}

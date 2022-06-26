using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
	public class NoclipBehavior : MonoBehaviour
	{
		private CharacterBody body;
		private InputBankTest inputBank;
		private CharacterMotor motor;
		private Rigidbody rigidbody;
		
		private bool useGravity;
		private bool isFlying;
		private int collisionMask;
		public bool shouldUseInteractForDown = true;

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
			inputBank = body.inputBank; 

			motor = body.characterMotor;
			if (!motor)
			{
				rigidbody = GetComponent<Rigidbody>();
				return;
			}

			var colliderObject = motor.capsuleCollider.gameObject;
			collisionMask = colliderObject.layer;
			colliderObject.layer = 9;
			motor.Motor.RebuildCollidableLayers();
			isFlying = motor.isFlying;
			useGravity = motor.useGravity;
			motor.useGravity = false;
			motor.isFlying = true;
		}

		public void OnDestroy()
		{
			if (!motor) return;
			motor.useGravity = useGravity;
			motor.isFlying = isFlying;
			motor.capsuleCollider.gameObject.layer = collisionMask;
			motor.Motor.RebuildCollidableLayers();
		}

		public void FixedUpdate()
		{
			var vertical = inputBank.jump.down ? Vector3.up : shouldUseInteractForDown && inputBank.interact.down ? Vector3.down : Vector3.zero;
			if (motor)
			{
				motor.AddDisplacement(inputBank.moveVector + vertical);
			}
		}
	}
}
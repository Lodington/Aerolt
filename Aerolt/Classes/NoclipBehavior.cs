using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
    public class NoclipBehavior : MonoBehaviour
    {
        public bool shouldUseInteractForDown = true;
        private CharacterBody body;
        private int collisionMask;
        private InputBankTest inputBank;
        private bool isFlying;
        private CharacterMotor motor;
        private Rigidbody rigidbody;

        private bool useGravity;

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

        public void FixedUpdate()
        {
            var vertical = inputBank.jump.down ? Vector3.up :
                shouldUseInteractForDown && inputBank.interact.down ? Vector3.down : Vector3.zero;
            if (motor) motor.AddDisplacement(inputBank.moveVector + vertical);
        }

        public void OnDestroy()
        {
            if (!motor) return;
            motor.useGravity = useGravity;
            motor.isFlying = isFlying;
            motor.capsuleCollider.gameObject.layer = collisionMask;
            motor.Motor.RebuildCollidableLayers();
        }
    }
}
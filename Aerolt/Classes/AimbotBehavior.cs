using System.Linq;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{

    public class AimbotBehavior : MonoBehaviour
    {
        public float weight = 0.5f;

        private CharacterBody body = null!;
        private Vector3? targetDirection;
        private InputBankTest inputBank = null!;
        private TeamIndex team;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            if (!body.master.playerCharacterMasterController.networkUser.cameraRigController) Destroy(this);
            inputBank = body.inputBank;
            team = body.teamComponent.teamIndex;
        }

        private void Update()
        {
            if (inputBank.interact.down) return;
            if (targetDirection is null) return;

            var t = weight >= 0.999f ? 1f : Mathf.Clamp01(Mathf.Lerp(0.15f, 0.9f, weight));

            inputBank.aimDirection = Vector3.Slerp(inputBank.aimDirection, targetDirection.Value.normalized, t);
        }

        private void FixedUpdate()
        {
            targetDirection = null;

            if (!body.isPlayerControlled) return;
            var cam = body.master.playerCharacterMasterController.networkUser.cameraRigController;
            if (!cam) return;

            var ray = inputBank.GetAimRay();
            var aimDir = (cam.crosshairWorldPosition - ray.origin).normalized;
            var maxDot = Mathf.Cos(Mathf.Deg2Rad * Mathf.Lerp(10f, 180f, weight));

            var best = HurtBox.sniperTargetsList.Union(HurtBox.bullseyesList)
                .Where(x => x && x.healthComponent && x.healthComponent.alive && x.teamIndex != team)
                .Select(x =>
                {
                    var toTarget = x.transform.position - ray.origin;
                    return new
                    {
                        hurtBox = x,
                        dir = toTarget.normalized,
                        dot = Vector3.Dot(aimDir, toTarget.normalized),
                        distSqr = toTarget.sqrMagnitude
                    };
                })
                .Where(x => x.dot >= maxDot && HasLineOfSight(ray.origin, x.hurtBox))
                .OrderByDescending(x => x.dot + (x.hurtBox.isSniperTarget ? 2f : 0f))
                .FirstOrDefault();

            if (best == null || !best.hurtBox) return;

            targetDirection = best.dir;
        }

        private static bool HasLineOfSight(Vector3 origin, HurtBox target)
        {
            var dir = target.transform.position - origin;
            return !Physics.Raycast(origin, dir, out _, dir.magnitude, LayerIndex.world.mask,
                QueryTriggerInteraction.UseGlobal);
        }
    }
}

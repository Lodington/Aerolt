using System;
using System.Linq;
using Aerolt.Helpers;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
	public class AimbotBehavior : MonoBehaviour
	{
		private CharacterBody body;
		private InputBankTest inputBank;
		private TeamIndex team;
		private Vector3? direction;
		private BullseyeSearch search;
		public float weight = 0.5f;

		private void Awake()
		{
			body = GetComponent<CharacterBody>();
			inputBank = body.inputBank;
			team = body.teamComponent.teamIndex;
		}

		private void FixedUpdate()
		{
			if (!body.isPlayerControlled) return;
			var ray = inputBank.GetAimRay();
			ray.direction = (body.master.playerCharacterMasterController.networkUser.cameraRigController.crosshairWorldPosition - ray.origin).normalized;
			var targets = HurtBox.sniperTargetsList.Union(HurtBox.bullseyesList).Select(x =>
			{
				var position = x.transform.position;
				var vector = position - ray.origin;
				var info = new BullseyeSearch.CandidateInfo
				{
					hurtBox = x,
					position = position,
					dot = Vector3.Dot(ray.direction, vector.normalized),
					distanceSqr = vector.sqrMagnitude
				};
				return info;
			}).Where(x =>
			{
				var dir = x.hurtBox.transform.position - ray.origin;
				var passesLos = !Physics.Raycast(ray.origin, dir, out _, dir.magnitude, LayerIndex.world.mask,
					QueryTriggerInteraction.UseGlobal);
				return x.hurtBox.teamIndex != team && passesLos && x.dot > weight - 0.5f;
			}).ToArray();
			if (!targets.Any())
			{
				direction = null;
				return;
			};
			var dotMax = targets.Max(x => x.dot);
			var distMax = targets.Max(x => x.distanceSqr);
			targets = targets.OrderByDescending(x => x.dot / dotMax * weight - x.distanceSqr / distMax * (1 - weight) + (x.hurtBox.isSniperTarget ? 10 : 0)).ToArray();
			var target = targets.FirstOrDefault();
			if (target.Equals(default) || !target.hurtBox)
			{
				direction = null;
				return;
			}
			direction = target.hurtBox.transform.position - ray.origin;
		}

		private void Update()
		{
			if (inputBank.interact.down) return;
			if (direction is not null)
				inputBank.aimDirection = direction.Value;
		}
		
	}
}
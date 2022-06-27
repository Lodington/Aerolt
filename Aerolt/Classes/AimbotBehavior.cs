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
			/*
			search = new BullseyeSearch
			{
				teamMaskFilter = TeamMask.AllExcept(team),
				viewer = body,
				sortMode = BullseyeSearch.SortMode.DistanceAndAngle // required for dot to be populated
			};*/
		}

		private void FixedUpdate()
		{
			if (!body.isPlayerControlled) return;
			var ray = inputBank.GetAimRay();
			ray.direction = (body.master.playerCharacterMasterController.networkUser.cameraRigController.crosshairWorldPosition - ray.origin).normalized;
			//search.searchOrigin = ray.origin;
			//search.searchDirection = ray.direction;
			//search.RefreshCandidates();
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
				return x.hurtBox.teamIndex != team && passesLos;
			}).ToArray();
			if (!targets.Any())
			{
				direction = null;
				return;
			};
			var dotMax = targets.Max(x => x.dot);
			var distMax = targets.Max(x => x.distanceSqr);
			//weight = 0.5f; // 0 weighted 100% to distance; 1 weighted 100% to angle
			targets = targets.OrderByDescending(x => x.dot / dotMax * weight - x.distanceSqr / distMax * (1 - weight) + (x.hurtBox.isSniperTarget ? 1 : 0)).ToArray();
			//var targets = search.candidatesEnumerable;
			var target = targets.FirstOrDefault();
			if (target.Equals(default) || !target.hurtBox)
			{
				direction = null;
				return;
			}
			direction = target.hurtBox.transform.position - ray.origin;
		}
		/*
		private void FixedUpdate()
		{
			var pos = body.transform.position;
			var targets = HurtBox.bullseyesList.Where(x => x.teamIndex != team).OrderBy(x => Vector3.Distance(x.transform.position, pos) + (x.isSniperTarget ? -1000 : 0));
			var target = targets.FirstOrDefault();
			if (!target) return;
			var aimRay = inputBank.GetAimRay();
			direction = target.transform.position - aimRay.origin;
		}*/

		private void Update()
		{
			if (direction is not null)
				inputBank.aimDirection = direction.Value;
		}

		private void UpdateOld()
		{
			if (Tools.CursorIsVisible())
				return;
			var localUser = LocalUserManager.GetFirstLocalUser();
			var controller = localUser.cachedMasterController;
			if (!controller)
				return;
			var body = controller.master.GetBody();
			if (!body)
				return;
			var inputBank = body.GetComponent<InputBankTest>();
			var aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
			var bullseyeSearch = new BullseyeSearch();
			var team = body.GetComponent<TeamComponent>();
			bullseyeSearch.teamMaskFilter = TeamMask.all;
			bullseyeSearch.teamMaskFilter.RemoveTeam(team.teamIndex);
			bullseyeSearch.filterByLoS = true;
			bullseyeSearch.searchOrigin = aimRay.origin;
			bullseyeSearch.searchDirection = aimRay.direction;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.maxDistanceFilter = float.MaxValue;
			bullseyeSearch.maxAngleFilter = 20f;// ;// float.MaxValue;
			bullseyeSearch.RefreshCandidates();
			var hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
			if (hurtBox)
			{
				Vector3 direction = hurtBox.transform.position - aimRay.origin;
				inputBank.aimDirection = direction;
			}
		}
	}
}
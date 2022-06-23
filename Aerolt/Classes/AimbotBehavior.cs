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

		private void Awake()
		{
			body = GetComponent<CharacterBody>();
			inputBank = body.inputBank;
			team = body.teamComponent.teamIndex;
		}

		private void FixedUpdate()
		{
			var pos = body.transform.position;
			var targets = HurtBox.bullseyesList.Where(x => x.teamIndex != team).OrderBy(x => Vector3.Distance(x.transform.position, pos));
			var target = targets.FirstOrDefault();
			if (!target) return;
			var aimRay = inputBank.GetAimRay();
			var direction = target.transform.position - aimRay.origin;
			inputBank.aimDirection = direction;
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
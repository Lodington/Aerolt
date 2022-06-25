using System;
using RoR2;
using UnityEngine;

namespace Aerolt.Classes
{
	public class AlwaysSprintBehavior : MonoBehaviour
	{
		private LocalUser localUser;
		private PlayerCharacterMasterController controller;

		/* fuck this
		private CharacterBody body;
		private bool notPlayer;

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
			notPlayer = !body.master || !body.master.playerCharacterMasterController || !body.master.playerCharacterMasterController.networkUser || body.master.playerCharacterMasterController.networkUser.localUser?.inputPlayer == null;
		}

		private void Update()
		{
			var ignoreSprint = notPlayer || !body.master.playerCharacterMasterController.networkUser.localUser!.inputPlayer!.GetButton("Sprint");
			
			if (body && !body.isSprinting && ignoreSprint) //.!localUser.inputPlayer.GetButton("Sprint"))
				body.inputBank.sprint.PushState(true); //controller.sprintInputPressReceived = true;
		}
		*/
		public void Awake()
		{
			localUser = LocalUserManager.GetFirstLocalUser();
			if (localUser == null || localUser.cachedMasterController == null || localUser.cachedMasterController.master == null) return;
			controller = localUser.cachedMasterController;
		}

		public void Update()
		{
			if (!controller)
			{
				Destroy(this);
				Load.CallPopup("Always Sprint Error", "Tried to apply always sprint to something that wasn't a player.");
				return;
			}
			
			var body = controller.master.GetBody();
			if (body && !body.isSprinting && !localUser.inputPlayer.GetButton("Sprint"))
				controller.sprintInputPressReceived = true;
		}
	}
}
using RoR2;
using UnityEngine;

namespace Aerolt.Managers
{
	public class LobbyPlayerPageManager : MonoBehaviour
	{
		private NetworkUser currentUser;

		public void SetUser(NetworkUser user) => currentUser = user;
	}
}
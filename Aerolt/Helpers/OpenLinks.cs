using System.Globalization;
using System.Text.RegularExpressions;
using Aerolt.Social;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class JoinLobbyOnClick : MonoBehaviour, IPointerClickHandler {

        [SerializeField]
        private TextMeshProUGUI textMessage;

        // Get link and open page
        public void OnPointerClick (PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log("YEs");
                int linkIndex = TMP_TextUtilities.FindIntersectingLink (textMessage, Input.mousePosition, null);
                if (linkIndex == -1) 
                    return;
                var linkInfo = textMessage.textInfo.linkInfo[linkIndex];
                string selectedLink = linkInfo.GetLinkID();
                if (selectedLink != "") {
                    Debug.LogFormat ("Joining Lobby {0}", selectedLink);
                    Console.instance.SubmitCmd(null, string.Format(CultureInfo.InvariantCulture, "steam_lobby_join {0}", selectedLink), true);
                }
            }
           
        }

    }
}
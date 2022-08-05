using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Console = RoR2.Console;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(EventTrigger))]
    public class JoinLobbyOnClick : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI textMessage;

        private EventTrigger _eventTrigger;
        void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnPointerClick, EventTriggerType.PointerDown);
        }

        // Get link and open page
        public void OnPointerClick (BaseEventData eventData)
        {
            if (eventData.currentInputModule.input.GetMouseButtonDown((int) MouseButton.LeftMouse)) 
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
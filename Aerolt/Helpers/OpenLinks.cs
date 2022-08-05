using System.Globalization;
using Aerolt.Enums;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Aerolt.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(EventTrigger))]
    public class JoinLobbyOnClick : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMessage;

        private EventTrigger _eventTrigger;

        private void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnPointerClick, EventTriggerType.PointerDown);
        }

        // Get link and open page
        public void OnPointerClick(BaseEventData eventData)
        {
            if (eventData.currentInputModule.input.GetMouseButtonDown((int) MouseButton.LeftMouse))
            {
                var linkIndex = TMP_TextUtilities.FindIntersectingLink(textMessage, Input.mousePosition, null);
                if (linkIndex == -1)
                    return;
                var linkInfo = textMessage.textInfo.linkInfo[linkIndex];
                var selectedLink = linkInfo.GetLinkID();
                if (selectedLink != "")
                {
                    Tools.Log(LogLevel.Information, $"Joining Lobby {selectedLink}");
                    Console.instance.SubmitCmd(null,
                        string.Format(CultureInfo.InvariantCulture, "steam_lobby_join {0}", selectedLink), true);
                }
            }
        }
    }
}
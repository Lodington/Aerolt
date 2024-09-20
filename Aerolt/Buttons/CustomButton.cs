using System.Collections;
using JetBrains.Annotations;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Aerolt.Buttons
{
    public class CustomButton : MonoBehaviour, IPointerClickHandler
    {
        public TMP_Text buttonText;
        public Image image;
        public RawImage rawImage;
        public Button.ButtonClickedEvent onRightClick;
        [CanBeNull] private Button _button;

        private MPEventSystemLocator eventSystemLocator;
        public Button button => _button ??= GetComponent<Button>();
        public MPEventSystem EventSystem => eventSystemLocator.eventSystem;

        public void Awake()
        {
            eventSystemLocator = GetComponent<MPEventSystemLocator>();
        }

        public void Update()
        {
            if (!eventSystemLocator) return;
            if (!EventSystem) return;
            if (!EventSystem.currentInputModule) return;
            if ((EventSystem.currentInputModule.input.GetMouseButtonDown(1) ||
                 (EventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad &&
                  EventSystem.player.GetButtonDown(5))) &&
                EventSystem.currentSelectedGameObject == gameObject) InvokeRightClick();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventSystemLocator) return; // dont send the event if we're already handling it via mpeventsystem
            if (eventData.button == PointerEventData.InputButton.Right) InvokeRightClick();
        }

        public void InvokeRightClick()
        {
            onRightClick?.Invoke();
            // Display the click
            if (!button) return; // going to escape this just in case lodington is bad
            button.DoStateTransition(Selectable.SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = button.colors.fadeDuration;
            var elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            button.DoStateTransition(button.currentSelectionState, false);
        }
    }
}
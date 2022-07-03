using System;
using System.Collections;
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
        [NonSerialized]
        public Button button;
        public Button.ButtonClickedEvent onRightClick;
        
        private MPEventSystemLocator eventSystemLocator;
        public MPEventSystem EventSystem => eventSystemLocator.eventSystem;
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

        public void Awake()
        {
            eventSystemLocator = GetComponent<MPEventSystemLocator>();
            button = GetComponent<Button>();
        }
        public void Update()
        {
            if (!eventSystemLocator) return;
            if (!EventSystem) return;
            if ((EventSystem.currentInputModule.input.GetMouseButtonDown((int) MouseButton.RightMouse) || EventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad && EventSystem.player.GetButtonDown(5)) && EventSystem.currentSelectedGameObject == gameObject)
            {
                InvokeRightClick();
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventSystemLocator) return; // dont send the event if we're already handling it via mpeventsystem
            if (eventData.button == PointerEventData.InputButton.Right) InvokeRightClick();
        }
    }
}

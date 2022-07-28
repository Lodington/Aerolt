using Aerolt.Buttons;
using Aerolt.Messages;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aerolt.Managers
{
    public class BodyManager : MonoBehaviour
    {

        public GameObject buttonPrefab;
        public GameObject buttonParent;
        
        private GameObject _newBody;
        private NetworkUser target;

        private void Awake()
        {
            foreach(var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                var customButton = newButton.GetComponent<CustomButton>(); 
                customButton.buttonText.text = Language.GetString(body.baseNameToken);
                customButton.image.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));
                customButton.button.onClick.AddListener(() => SetBodyDef(body));
            }
        }

        public void SpawnAsBody()
        {
                new SetBodyMessage(target, _newBody.GetComponent<CharacterBody>()).SendToServer();
        }

        public void SetBodyDef(CharacterBody body)
        {
            _newBody = BodyCatalog.FindBodyPrefab(body);
            SpawnAsBody();
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }

        public void Initialize(NetworkUser currentUser)
        {
            target = currentUser;
        }
    }
    
}
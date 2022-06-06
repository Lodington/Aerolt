using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Aerolt.Buttons
{
    public class MonsterButtonGenerator : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public TMP_Text monsterText;
        private CharacterBody _body;
        private CharacterMaster _master;

        private void Awake()
        {
            foreach (var master in MasterCatalog.allAiMasters)
            {
                var body = master.bodyPrefab.GetComponent<CharacterBody>();
                
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(master.name);
                newButton.GetComponent<Image>().sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));
                newButton.GetComponent<Button>().onClick.AddListener(() => SetMonsterName(body,master));
            }
        }

        public void SetMonsterName(CharacterBody body, CharacterMaster master)
        {
            monsterText.text = Language.GetString(body.baseNameToken);
            _master = master;
            _body = body;
            
        }
        public void SpawnMonster(int amount = 1)
        {
            var localUser = LocalUserManager.GetFirstLocalUser();
            if(localUser == null || !localUser.cachedBody)
                return;
            
            var location = localUser.cachedBody.transform.position;
            var body = _master.GetComponent<CharacterMaster>().bodyPrefab;

            var bodyGameObject = UnityEngine.Object.Instantiate<GameObject>(_master.gameObject, location, Quaternion.identity);
            var master = bodyGameObject.GetComponent<CharacterMaster>();
            for (var i = 0; i < amount; i++)
            {
                NetworkServer.Spawn(bodyGameObject);
                master.bodyPrefab = body;
                master.SpawnBody(localUser.cachedBody.transform.position, Quaternion.identity);
            }
        }
        
    }
}
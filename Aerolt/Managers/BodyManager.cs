using System;
using System.Collections.Generic;
using System.Text;
using Aerolt.Buttons;
using Rewired.Utils.Libraries.TinyJson;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Aerolt.Managers
{
    public class BodyManager : MonoBehaviour
    {

        public GameObject buttonPrefab;
        public GameObject buttonParent;

        public TMP_Text bodyText;
        private GameObject _newBody;

        private void Awake()
        {
            foreach(var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
                newButton.GetComponent<CustomButton>().ButtonText.text = Language.GetString(body.baseNameToken);
                newButton.GetComponent<Image>().sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));
                newButton.GetComponent<Button>().onClick.AddListener(() => SetBodyDef(body));
            }
            
        }

        public void SpawnAsBody()
        {
            if (_newBody)
            {
                var localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser.cachedMasterController && localUser.cachedMasterController.master)
                {
                    var master = localUser.cachedMaster;
                    master.bodyPrefab = _newBody;
                    master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);
                }
            }
        }

        public void SetBodyDef(CharacterBody body)
        {
            bodyText.text = Language.GetString(body.baseNameToken);
            _newBody = BodyCatalog.FindBodyPrefab(body);;
        }
    }
    
}
using System;
using System.Collections.Generic;
using System.Text;
using Aerolt.Buttons;
using Aerolt.Classes;
using Aerolt.Helpers;
using Rewired.Utils.Libraries.TinyJson;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
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

        public void SpawnAsBody() // TODO network this
        {
            if (!_newBody)
            {
                return;
            }

            if (target && target.master)
            {
                var master = target.master;
                master.bodyPrefab = _newBody;
                var transfor = master.GetBody().transform;
                master.Respawn(transfor.position, transfor.rotation);
            }
        }

        public void SetBodyDef(CharacterBody body)
        {
            _newBody = BodyCatalog.FindBodyPrefab(body);;
            SpawnAsBody();
            GetComponentInParent<LobbyPlayerPageManager>().SwapViewState();
        }

        public void Initialize(NetworkUser currentUser)
        {
            target = currentUser;
        }
    }
    
}
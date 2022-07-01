using System; 
using System.Linq;
using Aerolt.Buttons;
using Aerolt.Enums;
using Aerolt.Managers;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Console = RoR2.Console;
using LobbyManager = Aerolt.Managers.LobbyManager;

namespace Aerolt.Classes
{
    public class LobbyManagerStrip : MonoBehaviour
    {
        private NetworkUser _user;
        
        public Image icon;
        public TMP_Text userName;
        public TMP_InputField moneyInputField;
        public TMP_InputField lunarCoinsInputField;
        public TMP_InputField voidMarkersInputField;
        public ItemInventoryDisplay itemInventoryDisplay;
        private bool setup;

        public void Init(NetworkUser user, LobbyManager lobbyManager)
        {
            _user = user;

            itemInventoryDisplay.SetSubscribedInventory(user.master.inventory);
            
            userName.text = user.userName;
            moneyInputField.m_OnEndEdit.AddListener(UpdateMoney);
            lunarCoinsInputField.m_OnEndEdit.AddListener(UpdateLunarCoin);
            voidMarkersInputField.m_OnEndEdit.AddListener(UpdateVoidMarkers);
        }

        public void FixedUpdate()
        {
            ((TextMeshProUGUI) moneyInputField.placeholder).text = _user.master.money.ToString();
            ((TextMeshProUGUI) lunarCoinsInputField.placeholder).text = _user.NetworknetLunarCoins.ToString();
            ((TextMeshProUGUI) voidMarkersInputField.placeholder).text = _user.master.voidCoins.ToString();
            if (setup) return;
            var body = _user.master.GetBody();
            if (!body) return;
            icon.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0, 0, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f));;
            setup = true;
        }

        private void UpdateVoidMarkers(string arg0)
        {
            if (int.TryParse(voidMarkersInputField.text, out int value)) _user.master.voidCoins = (uint) value;
            voidMarkersInputField.text = string.Empty;
        }

        private void UpdateLunarCoin(string arg0)
        {
            if (int.TryParse(lunarCoinsInputField.text, out int value))
            {
                var val = value - _user.NetworknetLunarCoins;
                if (val < 0)
                {
                    _user.DeductLunarCoins((uint) Math.Abs(val));
                }
                else
                {
                    _user.AwardLunarCoins((uint) val);
                }
            }
            lunarCoinsInputField.text = string.Empty;
        }


        public void UpdateMoney(string arg0)
        {
            if (int.TryParse(moneyInputField.text, out int value)) _user.master.money = (uint) value;
            moneyInputField.text = String.Empty;
        }

        public void OpenInventory()
        {
            var panelManager = GetComponentInParent<PanelManager>();
            panelManager.ShowPanel("EditPlayerItemsView", PanelShowBehaviour.HIDE_PREVIOUS);
            var instance = panelManager._panelInstanceModels.FirstOrDefault(x => x.PanelId == "EditPlayerItemsView");
            instance?.PanelInstance.GetComponent<EditPlayerItemButton>().Initialize(_user, panelManager.configFile);
        }
        
        public void KickPlayer()
        {
            Console.instance.RunClientCmd(_user, "kick_steam", new string[]
            {
                _user.Network_id.steamId.ToString()
            });
        }

        public void BanPlayer()
        {
            Console.instance.RunClientCmd(_user, "ban_steam", new string[]
            {
                _user.Network_id.steamId.ToString()
            });
        }

        public void RevivePlayer()
        {
            var gameover = FindObjectOfType<GameOverController>();
            if (gameover)
            {
                foreach (var gameEndReportPanelController in gameover.reportPanels)
                {
                    Destroy(gameEndReportPanelController.Value.gameObject);
                }
                Destroy(gameover.gameObject);
            }
            if (NetworkClient.active && !_user.master.isServer)
                _user.master.CmdRespawn(_user.master.bodyPrefab.name);
            else
            {
                Run.instance.isGameOverServer = false;
                Stage.instance.RespawnCharacter(_user.master);
            }
        }
    }
}
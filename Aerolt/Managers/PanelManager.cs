using System;
using System.Collections;
using System.Collections.Generic;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Models;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Aerolt.Managers
{

    public class PanelManager : MonoBehaviour
    {
        public List<PanelInstanceModel> _panelInstanceModels = new List<PanelInstanceModel>();
        [NonSerialized] public ObjectPool objectPool;

        [NonSerialized] public NetworkUser owner;
        [NonSerialized] public ZioConfigFile.ZioConfigFile configFile;
        public HUD hud;
        private Canvas parentCanvas;
        
        private void StartLate()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            PauseManager.onPauseStartGlobal += FuckingUnitySorting;
            objectPool = transform.parent.GetComponentInChildren<ObjectPool>();
            foreach (var obj in objectPool.prefabsForPool)
            {
                ShowPanel(obj.gameObject.name, PanelShowBehaviour.HIDE_PREVIOUS);
                HideLastPanel();
            }
        }

        private void OnDestroy()
        {
            PauseManager.onPauseStartGlobal -= FuckingUnitySorting;
        }

        private void FuckingUnitySorting()
        {
            StartCoroutine(Example());
            
            IEnumerator Example()
            {
                yield return new WaitForSecondsRealtime(0.01f);
                parentCanvas.sortingOrder = 1000; // something keeps fucking setting this back to 0
            }
        }

        public void ShowPanel(string panelId, PanelShowBehaviour behaviour = PanelShowBehaviour.KEEP_PREVIOUS)
        {
            GameObject panelInstance = objectPool.GetObjectFromPool(panelId);
            if (panelInstance != null)
            {
                if (behaviour == PanelShowBehaviour.HIDE_PREVIOUS && GetAmountPanelsInQueue() > 0)
                {
                    panelInstance = objectPool.GetObjectFromPool(panelId);
                    var lastPanel = GetLastPanel();
                    lastPanel?.PanelInstance.SetActive(false);
                }
                _panelInstanceModels.Add(new PanelInstanceModel
                {
                    PanelId = panelId,
                    PanelInstance = panelInstance
                });
            }
            else
            {
                Load.CallPopup("Error", $"Trying to use panelId = {panelId}, but this is not found in the ObjectPool", objectPool.transform);
                ShowPanel("Menu");
            }
        }

        public void HideLastPanel()
        {
            if (AnyPanelShowing())
            {
                var lastPanel = GetLastPanel();

                _panelInstanceModels.Remove(lastPanel);
                objectPool.PoolObject(lastPanel.PanelInstance);

                if (GetAmountPanelsInQueue() > 0)
                {
                    lastPanel = GetLastPanel();
                    if (lastPanel != null && !lastPanel.PanelInstance.activeInHierarchy)
                    {
                        lastPanel.PanelInstance.SetActive(true);
                    }
                }
            }
        }

        PanelInstanceModel GetLastPanel()
        {
            return _panelInstanceModels[_panelInstanceModels.Count - 1];
        }

        public bool AnyPanelShowing()
        {
            return GetAmountPanelsInQueue() > 0;
        }
        public int GetAmountPanelsInQueue()
        {
            return _panelInstanceModels.Count;
        }
        private void Awake()
        {
            Initialize(Load.tempViewer);
            hud = Load.tempHud;
            StartLate();
        }

        public void Initialize(NetworkUser ownerIn)
        {
            owner = ownerIn;
            transform.parent.GetComponentInChildren<ToggleWindow>().Init(owner);
            if (ownerIn.localUser != null)
                configFile = new ZioConfigFile.ZioConfigFile(RoR2Application.cloudStorage, $"/Aerolt/Profiles/{ownerIn.localUser.userProfile.fileName}.cfg", true);
        }
    }

}

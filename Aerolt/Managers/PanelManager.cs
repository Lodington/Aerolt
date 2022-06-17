using System;
using System.Collections.Generic;
using Aerolt.Classes;
using Aerolt.Enums;
using Aerolt.Helpers;
using Aerolt.Models;
using RoR2;
using UnityEngine;

namespace Aerolt.Managers
{

    public class PanelManager : MonoBehaviour
    {
        private List<PanelInstanceModel> _panelInstanceModels = new List<PanelInstanceModel>();
        private ObjectPool _objectPool;

        private void Start()
        {
            _objectPool = transform.parent.GetComponentInChildren<ObjectPool>();
            ShowPanel("Menu");
        }

        public void ShowPanel(string panelId, PanelShowBehaviour behaviour = PanelShowBehaviour.KEEP_PREVIOUS)
        {
            GameObject panelInstance = _objectPool.GetObjectFromPool(panelId);;
            if (panelInstance != null)
            {
                if (behaviour == PanelShowBehaviour.HIDE_PREVIOUS && GetAmountPanelsInQueue() > 0)
                {
                    panelInstance = _objectPool.GetObjectFromPool(panelId);
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
                Load.CallPopup("Error", $"Trying to use panelId = {panelId}, but this is not found in the ObjectPool", _objectPool.transform);
        }

        public void HideLastPanel()
        {
            if (AnyPanelShowing())
            {
                var lastPanel = GetLastPanel();

                _panelInstanceModels.Remove(lastPanel);
                _objectPool.PoolObject(lastPanel.PanelInstance);

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

        public NetworkUser owner;
        public ZioConfigFile.ZioConfigFile configFile;

        private void Awake()
        {
            Initialize(Load.tempViewer);
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

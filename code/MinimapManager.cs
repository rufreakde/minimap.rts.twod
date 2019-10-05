using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using manager.ioc;

namespace minimap.rts.twod
{
    public class MinimapManager : MonoBehaviour, IamSingleton
    {
        [Mandatory]
        public GameObject DefaultEventSystemPrefab;
        [AutoAssign]
        public EventSystem EventSystem;

        [Mandatory]
        public GameObject DefaultMinimapCameraPrefab;
        [AutoAssign]
        public Camera MinimapCamera;

        [Mandatory]
        public GameObject DefaultMinimapPrefab;
        [AutoAssign]
        public Minimap Minimap;

        public void iInitialize()
        {
            EventSystem = MANAGER.CheckScriptAvailability<EventSystem>("EventSystem", DefaultEventSystemPrefab);

            MinimapCamera = MANAGER.CheckScriptAvailability<Camera>("MinimapCamera", DefaultMinimapCameraPrefab);

            Minimap = MANAGER.CheckScriptAvailability<Minimap>("MinimapCanvas", DefaultMinimapPrefab);
        }
    }
}
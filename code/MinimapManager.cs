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
            EventSystem = CheckScriptAvailability<EventSystem>("EventSystem", DefaultEventSystemPrefab);

            MinimapCamera = CheckScriptAvailability<Camera>("MinimapCamera", DefaultMinimapCameraPrefab);

            Minimap = CheckScriptAvailability<Minimap>("MinimapCanvas", DefaultMinimapPrefab);
        }

        protected T CheckScriptAvailability<T>(string _TagOfHolder, GameObject _DefaultPrefab)
        {

            GameObject ScriptHolder = null;

            try
            {
                ScriptHolder = GameObject.FindWithTag(_TagOfHolder);
            }
            catch (UnityException _Exception)
            {
                Debug.LogError(_Exception);
            }

            if (ScriptHolder == null)
            {
                ScriptHolder = GameObject.Instantiate(_DefaultPrefab, _DefaultPrefab.transform.position, _DefaultPrefab.transform.rotation) as GameObject;
                ScriptHolder.name = ScriptHolder.name.Split('(')[0]; // "(Clone)" suffix... is annoying here since this happens in the init
                ScriptHolder.transform.SetParent(this.transform);
            }

            T Script = ScriptHolder.GetComponentInChildren<T>();
            return Script;
        }
    }
}
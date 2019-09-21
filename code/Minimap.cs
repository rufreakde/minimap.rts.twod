using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace minimap.rts.twod
{
    public class Minimap : MonoBehaviour
    {
        public float GizmoSize = 4f;
        public Icons UnitIcons;

        protected CoordinateSystem MinimapSystem = new CoordinateSystem();

        [Mandatory]
        public GameObject MinimapDebugMarker = null;

        protected RectTransform ULMarker = null;
        protected RectTransform URMarker = null;
        protected RectTransform LRMarker = null;
        protected RectTransform LLMarker = null;


        public CoordinateSystem WorldmapSystem;



        [AutoAssign]
        public RectTransform MinimapPanelTransform;

        public List<MinimapObject> ObjectList = new List<MinimapObject>();

        void Awake()
        {
            if (MinimapPanelTransform == null)
            {
                GameObject ScriptHolder = null;
                try
                {
                    ScriptHolder = GameObject.FindWithTag("MinimapCanvas");
                    foreach (Transform child in ScriptHolder.transform)
                    {
                        if (child.name == "minimap")
                        {
                            ScriptHolder = child.gameObject;
                            break;
                        }
                    }
                }
                catch (UnityException _Exception)
                {
                    Debug.LogError(_Exception);
                }

                MinimapPanelTransform = ScriptHolder.GetComponent<RectTransform>();

                //TODO mark the corners for calculation

                //TODO mark the WORLDMAP points on the minimap

                //TODO calculate all the objects in the list to show them on the Minimap

                //init Minimap
                MinimapSystem.UL = new Vector2(MinimapPanelTransform.rect.x, MinimapPanelTransform.rect.y + MinimapPanelTransform.rect.height);
                ULMarker = Instantiate(MinimapDebugMarker).GetComponent<RectTransform>();
                ULMarker.SetParent(MinimapPanelTransform);
                ULMarker.GetComponent<Text>().text = "UL";

                MinimapSystem.UR = new Vector2(MinimapPanelTransform.rect.x + MinimapPanelTransform.rect.width, MinimapPanelTransform.rect.y + MinimapPanelTransform.rect.height);
                URMarker = Instantiate(MinimapDebugMarker).GetComponent<RectTransform>();
                URMarker.SetParent(MinimapPanelTransform);
                URMarker.GetComponent<Text>().text = "UR";

                MinimapSystem.LR = new Vector2(MinimapPanelTransform.rect.x + MinimapPanelTransform.rect.width, MinimapPanelTransform.rect.y);
                LRMarker = Instantiate(MinimapDebugMarker).GetComponent<RectTransform>();
                LRMarker.SetParent(MinimapPanelTransform);
                LRMarker.GetComponent<Text>().text = "LR";

                MinimapSystem.LL = new Vector2(MinimapPanelTransform.rect.x, MinimapPanelTransform.rect.y);
                LLMarker = Instantiate(MinimapDebugMarker).GetComponent<RectTransform>();
                LLMarker.SetParent(MinimapPanelTransform);
                LLMarker.GetComponent<Text>().text = "LL";
            }
            if (UnitIcons.Aircraft == null)
            {
                UnitIcons.Aircraft = UnitIcons.GroundUnit;
            }
            if (UnitIcons.Ship == null)
            {
                UnitIcons.Ship = UnitIcons.GroundUnit;
            }
        }

        void LateUpdate()
        {
            ULMarker.anchoredPosition = MinimapSystem.UL;
            //Debug.Log(MinimapSystem.UL);
            URMarker.anchoredPosition = MinimapSystem.UR;
            //Debug.Log(MinimapSystem.UR);
            LRMarker.anchoredPosition = MinimapSystem.LR;
            //Debug.Log(MinimapSystem.LR);
            LLMarker.anchoredPosition = MinimapSystem.LL;
            //Debug.Log(MinimapSystem.LL);
        }

        void OnDrawGizmos()
        {
            Color saved = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(WorldmapSystem.UL, GizmoSize);
            Gizmos.DrawSphere(WorldmapSystem.UR, GizmoSize);
            Gizmos.DrawSphere(WorldmapSystem.LR, GizmoSize);
            Gizmos.DrawSphere(WorldmapSystem.LL, GizmoSize);
            Gizmos.color = saved;
        }

        public Vector2 calculatePosition(Vector3 realWorldPosition)
        {
            return Vector2.zero;
        }

        public GameObject GetIcon(IconType _Type)
        {
            switch (_Type)
            {
                case IconType.GroundUnit: { return UnitIcons.GroundUnit; break; }
                case IconType.Building: { return UnitIcons.Building; break; }
                case IconType.Aircraft: { return UnitIcons.Aircraft; break; }
                case IconType.Ship: { return UnitIcons.Ship; break; }
                default: { return UnitIcons.GroundUnit; break; }
            }
        }

        public enum IconType
        {
            GroundUnit,
            Building,
            Aircraft,
            Ship
        }

        [System.Serializable]
        public struct MinimapObject
        {
            public GameObject ChosenIcon;
            public int Player;
            public int Team;
            public Color TeamColor;
        }

        [System.Serializable]
        public class CoordinateSystem
        {
            public Vector2 UL = new Vector2(-50, 0);
            public Vector2 UR = new Vector2(50, 50);
            public Vector2 LR = new Vector2(50, -50);
            public Vector2 LL = new Vector2(-50, -50);
        }

        [System.Serializable]
        public struct Icons
        {
            [Mandatory]
            public GameObject GroundUnit;
            [Mandatory]
            public GameObject Building;
            [AutoAssign]
            public GameObject Aircraft;
            [AutoAssign]
            public GameObject Ship;
        }
    }

}
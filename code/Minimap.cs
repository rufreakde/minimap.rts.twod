using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

        public Dictionary<string, MinimapCornerMarker> MapCorners = new Dictionary<string, MinimapCornerMarker>();


        [AutoAssign]
        public RectTransform MinimapPanelTransform;

        [AutoAssign]
        public Camera MinimapCamera;
        [Slider(0.4f, 1.0f)]
        public float ZoomDelta = 1.0f;
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
            if (MinimapCamera == null)
            {
                MinimapCamera = GameObject.FindWithTag("MinimapCamera").GetComponentInChildren<Camera>();
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

        void Start()
        {
            //calculate the distances between all the corner points and get the size for the camera
            MinimapCamera.orthographicSize = getOptimalOrthographicCameraSize();
            MinimapCamera.transform.position = calculatePseudoCentroid(MapCorners.Values.ToList().ToArray());
        }

        private float getOptimalOrthographicCameraSize() {
            float xMin = -1f;
            float xMax = 1f;
            float yMin = -1f;
            float yMax = 1f;

            if(MapCorners.Count <= 0)
            {
                Debug.LogError(this.ToString() + "You forgot to place some GameObjects holding 'MinimapCornerMarker.cs' for the perfect quadratic minimap size!");
                return 17f;
            }

            foreach (KeyValuePair<string, MinimapCornerMarker> item in MapCorners)
            {
                Transform tempTrans = item.Value.transform;
                xMin = checkDimension(xMin, tempTrans.GetPositionX(), false);
                xMax = checkDimension(xMax, tempTrans.GetPositionX(), true);
                yMin = checkDimension(yMin, tempTrans.GetPositionY(), false);
                yMax = checkDimension(yMax, tempTrans.GetPositionY(), true);
            }

            float distanceX = Vector2.Distance(new Vector2(xMin, 0), new Vector2(xMax, 0));
            float distanceY = Vector2.Distance(new Vector2(0, yMin), new Vector2(0, yMax));

            float biggestDistance = checkDimension(distanceX, distanceY, true);

            return biggestDistance * 0.50f;
        }

        public Vector3 calculatePseudoCentroid(MinimapCornerMarker[] points)
        {
            float xMin = -1f;
            float xMax = 1f;
            float yMin = -1f;
            float yMax = 1f;

            foreach (KeyValuePair<string, MinimapCornerMarker> item in MapCorners)
            {
                Transform tempTrans = item.Value.transform;
                xMin = checkDimension(xMin, tempTrans.GetPositionX(), false);
                xMax = checkDimension(xMax, tempTrans.GetPositionX(), true);
                yMin = checkDimension(yMin, tempTrans.GetPositionY(), false);
                yMax = checkDimension(yMax, tempTrans.GetPositionY(), true);
            }

            float distanceX = Vector2.Distance(new Vector2(xMin, 0), new Vector2(xMax, 0));
            float distanceY = Vector2.Distance(new Vector2(0, yMin), new Vector2(0, yMax));

            return new Vector3(xMin + (distanceX * 0.5f), yMin + (distanceY * 0.5f), MinimapCamera.transform.position.z);
        }

        private float checkDimension(float _Value, float _NewValue, bool _ReturnTheHigherOne) {
            //should only return distance of on dimension o
            if (_ReturnTheHigherOne)
            {
                if(_NewValue > _Value)
                {
                    return _NewValue;
                }
                else
                {
                    return _Value;
                }
            }
            else
            {
                if (_NewValue < _Value)
                {
                    return _NewValue;
                }
                else
                {
                    return _Value;
                }
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

        public void addCorner(string _UniqueID, MinimapCornerMarker _Marker)
        {
            MapCorners.Add(_UniqueID, _Marker);
        }

        public void removeCorner(string _UniqueID)
        {
            MapCorners.Remove(_UniqueID);
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
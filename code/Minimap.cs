using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;

namespace minimap.rts.twod
{
    public class Minimap : MonoBehaviour
    {
        public Icons UnitIcons;
        [AutoAssign]
        public RectTransform MinimapPanelTransform;

        [AutoAssign]
        public Camera MinimapCamera;
        [AutoAssign]
        public Camera MainCamera;
        public bool ClampMainCameraToWorld = true;
        private Transform MainCameraTransform;
        [Slider(0.4f, 1.0f)]
        [SimpleButton("UpdateMinimapCamera", typeof(Minimap))]
        public float ZoomDelta = 1.0f;

        public Dictionary<string, MinimapCornerMarker> MapCorners = new Dictionary<string, MinimapCornerMarker>();
        protected CoordinateSystem MinimapSystem = new CoordinateSystem();
        public CoordinateSystem WorldSystem = new CoordinateSystem();

        private LineRenderer MiniMapViewRenderer;
        private Vector3[] ViewRenderCorners = new Vector3[4];
        private RectTransform minimapRectTrans;
        private Bounds MainCameraBounds = new Bounds();

        private float leftMargin;
        private float xWorldPercentageMin;
        private float bottomMargin;
        private float yWorldPercentageMin;
        private float rightMargin;
        private float xWorldPercentageMax;
        private float topMargin;
        private float yWorldPercentageMax;


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
            }
            if (MainCamera == null)
            {
                MainCamera = GameObject.FindWithTag("MainCamera").GetComponentInChildren<Camera>();
                Canvas MiniMapCanvas = this.GetComponentInParent<Canvas>();
                MiniMapCanvas.worldCamera = MainCamera;
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
            if (MainCameraTransform == null)
            {
                MainCameraTransform = MainCamera.transform;
            }
            if(MiniMapViewRenderer == null)
            {
                MiniMapViewRenderer = this.gameObject.AddComponent<LineRenderer>();
            }
            if (minimapRectTrans == null)
            {
                minimapRectTrans = this.transform.GetComponent<RectTransform>();
            }
        }

        void Start()
        {
            //calculate the distances between all the corner points and get the size for the camera
            UpdateMinimapCamera();
            SetupViewLineRenderer();
        }

        private float getOptimalOrthographicCameraSize() {
            float xMin = -1f;
            float xMax = 1f;
            float yMin = -1f;
            float yMax = 1f;

            if(MapCorners.Count <= 0)
            {
                Debug.LogError(this.ToString() + "You forgot to place some GameObjects holding 'MinimapCornerMarker.cs'. Need 2 at least!");
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

        void Update()
        {
            UpdateMainCameraView();
            OptionalMainCameraClamping();
        }

        private void LateUpdate()
        {
            OptionalMainCameraClamping();
        }

        void SetupViewLineRenderer()
        {
            MiniMapViewRenderer.sortingLayerName = "OnTop";
            MiniMapViewRenderer.sortingOrder = 20;
            MiniMapViewRenderer.positionCount = 4;

            for( int i =0; i< ViewRenderCorners.Length; i++)
            {
                MiniMapViewRenderer.SetPosition(i, ViewRenderCorners[i]);
            }

            MiniMapViewRenderer.numCapVertices = 1;
            MiniMapViewRenderer.numCornerVertices = 1;
            MiniMapViewRenderer.startWidth = 0.040000000000000000000000000f;
            MiniMapViewRenderer.endWidth = 0.040000000000000000000000000f;
            MiniMapViewRenderer.useWorldSpace = false;
            MiniMapViewRenderer.loop = true;
            MiniMapViewRenderer.material = new Material(Shader.Find("UI/Default"));
            MiniMapViewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MiniMapViewRenderer.alignment = LineAlignment.View;
        }

        public void OptionalMainCameraClamping()
        {
            if (ClampMainCameraToWorld == true) {
                MainCamera.transform.position = new Vector3(
                Mathf.Clamp(MainCamera.transform.position.x, WorldSystem.LL.x + (MainCameraBounds.size.x * 0.5f), WorldSystem.UR.x - +(MainCameraBounds.size.x * 0.5f)),
                Mathf.Clamp(MainCamera.transform.position.y, WorldSystem.LL.y + (MainCameraBounds.size.y * 0.5f), WorldSystem.UR.y - +(MainCameraBounds.size.y * 0.5f)),
                0);
            }
        }

        void UpdateMainCameraView()
        {
            // TODO okey it is to late atm dnk what I do here ... well lets calculate positions from real later...
            MainCameraBounds = MainCamera.OrthographicBounds();
            WorldSystem.recalculateSize();

            leftMargin = Vector2.Distance(new Vector2(MainCameraBounds.min.x, 0), new Vector2(WorldSystem.LL.x, 0));
            xWorldPercentageMin = leftMargin / WorldSystem.width;
            bottomMargin = Vector2.Distance(new Vector2(MainCameraBounds.min.y, 0), new Vector2(WorldSystem.LL.y, 0));
            yWorldPercentageMin = bottomMargin / WorldSystem.height;

            rightMargin = Vector2.Distance(new Vector2(MainCameraBounds.max.x, 0), new Vector2(WorldSystem.UR.x, 0));
            xWorldPercentageMax = rightMargin / WorldSystem.width;
            topMargin = Vector2.Distance(new Vector2(MainCameraBounds.max.y, 0), new Vector2(WorldSystem.UR.y, 0));
            yWorldPercentageMax = topMargin / WorldSystem.height;

            float leftMinimapOfRect = minimapRectTrans.rect.xMin + (minimapRectTrans.rect.width * xWorldPercentageMin);
            float upMinimapOfRect = minimapRectTrans.rect.yMax - (minimapRectTrans.rect.height * yWorldPercentageMax);
            float rightMinimapOfRect = minimapRectTrans.rect.xMax - (minimapRectTrans.rect.width * xWorldPercentageMax);
            float downMinimapOfRect = minimapRectTrans.rect.yMin + (minimapRectTrans.rect.height * yWorldPercentageMin);

            ViewRenderCorners[0] = new Vector3(leftMinimapOfRect, upMinimapOfRect, -10);
            ViewRenderCorners[1] = new Vector3(rightMinimapOfRect, upMinimapOfRect, -10);
            ViewRenderCorners[2] = new Vector3(rightMinimapOfRect, downMinimapOfRect, -10);
            ViewRenderCorners[3] = new Vector3(leftMinimapOfRect, downMinimapOfRect, -10);

            for (int i = 0; i < ViewRenderCorners.Length; i++)
            {
                MiniMapViewRenderer.SetPosition(i, ViewRenderCorners[i]);
            }
        }

        void OnDrawGizmos()
        {
            Handles.Label(MainCameraBounds.center + (Vector3.left * 10f), "(" + xWorldPercentageMin + "/" + leftMargin + ")");
            Handles.Label(MainCameraBounds.center + (Vector3.down * 5f), "(" + yWorldPercentageMin + "/" + bottomMargin + ")");
            Handles.Label(MainCameraBounds.center + (Vector3.right * 10f), "(" + xWorldPercentageMax + "/" + rightMargin + ")");
            Handles.Label(MainCameraBounds.center + (Vector3.up * 5f ), "(" + yWorldPercentageMax + "/" + topMargin + ")");

            Handles.Label(WorldSystem.UL, "(" + WorldSystem.UL.x + "," + WorldSystem.UL.y + ")");
            Gizmos.DrawWireSphere(WorldSystem.UL, 1f);
            Handles.Label(WorldSystem.UR, "(" + WorldSystem.UR.x + "," + WorldSystem.UR.y + ")");
            Gizmos.DrawWireSphere(WorldSystem.UR, 1f);
            Handles.Label(WorldSystem.LR, "(" + WorldSystem.LR.x + "," + WorldSystem.LR.y + ")");
            Gizmos.DrawWireSphere(WorldSystem.LR, 1f);
            Handles.Label(WorldSystem.LL, "(" + WorldSystem.LL.x + "," + WorldSystem.LL.y + ")");
            Gizmos.DrawWireSphere(WorldSystem.LL, 1f);

            Handles.Label(WorldSystem.LL + (Vector2.left * 10f), "(" + WorldSystem.height + ")");
            Handles.Label(WorldSystem.LL + (Vector2.down * 10f), "(" + WorldSystem.width + ")");
            Handles.Label(WorldSystem.UR + (Vector2.right * 10f), "(" + WorldSystem.height + ")");
            Handles.Label(WorldSystem.UR + (Vector2.up * 10f), "(" + WorldSystem.width + ")");

            Handles.Label(MainCameraBounds.min, "(" + MainCameraBounds.min.x + "," + MainCameraBounds.min.y + ")");
            Gizmos.DrawWireSphere(MainCameraBounds.min, 1f);
            Handles.Label(MainCameraBounds.center, "(" + MainCameraBounds.center.x + "," + MainCameraBounds.center.y + ")");
            Gizmos.DrawWireSphere(MainCameraBounds.center, 1f);
            Handles.Label(MainCameraBounds.max, "(" + MainCameraBounds.max.x + "," + MainCameraBounds.max.y + ")");
            Gizmos.DrawWireSphere(MainCameraBounds.max, 1f);
        }

        public void UpdateMinimapCamera() {
            MinimapCamera.orthographicSize = getOptimalOrthographicCameraSize();
            MinimapCamera.transform.position = calculatePseudoCentroid(MapCorners.Values.ToList().ToArray());
            UpdateWorldCoordinates(MinimapCamera.orthographicSize, MinimapCamera.transform.position);
        }

        public void UpdateWorldCoordinates(float _CameraSize, Vector3 _CameraPosition)
        {
            WorldSystem.UL = new Vector2(_CameraPosition.x - _CameraSize, _CameraPosition.y + _CameraSize);
            WorldSystem.UR = new Vector2(_CameraPosition.x + _CameraSize, _CameraPosition.y + _CameraSize);
            WorldSystem.LR = new Vector2(_CameraPosition.x + _CameraSize, _CameraPosition.y - _CameraSize);
            WorldSystem.LL = new Vector2(_CameraPosition.x - _CameraSize, _CameraPosition.y - _CameraSize);
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

            public float height = 0f;
            public float width = 0f;

            public CoordinateSystem()
            {
                recalculateSize();
            }
            public CoordinateSystem(Vector2 _UL, Vector2 _UR, Vector2 _LR, Vector2 _LL)
            {
                this.UL = _UL;
                this.UR = _UR;
                this.LR = _LR;
                this.LL = _LL;

                recalculateSize();
            }

            public void recalculateSize() {
                this.width = Vector2.Distance(new Vector2(this.UL.x, 0), new Vector2(this.UR.x, 0));
                this.height = Vector2.Distance(new Vector2(this.UL.y, 0), new Vector2(this.LL.y, 0));
            }
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
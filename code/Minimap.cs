using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;
using UnityEngine.EventSystems;

namespace minimap.rts.twod
{
    public class Minimap : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        public Icons UnitIcons;
        public bool ClampMainCameraToWorld = true;
        [Slider(0.4f, 1.0f)]
        [SimpleButton("UpdateMinimapCamera", typeof(Minimap))]
        [SimpleButton("test", typeof(Minimap))]
        public float ZoomDelta = 1.0f;
        [AutoAssign]
        public RectTransform MinimapPanelTransform;
        [AutoAssign]
        public Camera MinimapCamera;
        [AutoAssign]
        public Camera MainCamera;
        public Dictionary<string, MinimapCornerMarker> MapCorners = new Dictionary<string, MinimapCornerMarker>();
        public Rect MinimapSystem = new Rect();
        public Rect WorldSystem = new Rect();

        public delegate void dragMinimapEvent(PointerEventData _PointerEvent);
        public delegate void clickMinimapEvent(PointerEventData _PointerEvent);
        public dragMinimapEvent mouseDragDelegate;
        public clickMinimapEvent mouseClickDelegate;

        private Transform MainCameraTransform;
        private LineRenderer MiniMapViewRenderer;
        private Vector3[] ViewRenderCorners = new Vector3[4];
        private RectTransform minimapRectTrans;
        private Rect MainCameraBounds = new Rect();

        private float leftMargin;
        private float xWorldPercentageMin;
        private float bottomMargin;
        private float yWorldPercentageMin;
        private float rightMargin;
        private float xWorldPercentageMax;
        private float topMargin;
        private float yWorldPercentageMax;

        private PointerEventData dragData;
        private PointerEventData clickData;

        #region unity
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
            if (MiniMapViewRenderer == null)
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
            UpdateMinimapCoordinates();
            SetupViewLineRenderer();
        }
        void Update()
        {
            drag(dragData);
            click(clickData);
            UpdateMainCameraView();
            clearDragClickInput();
        }
        void LateUpdate()
        {
            OptionalMainCameraClamping();
        }
        public void OnDrag(PointerEventData eventData)
        {
            dragData = eventData;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            clickData = eventData;
        }
        public void UpdateMinimapCamera()
        {
            if (MinimapCamera)
            {
                MinimapCamera.orthographicSize = getOptimalOrthographicCameraSize();
                MinimapCamera.transform.position = calculatePseudoCentroid(MapCorners.Values.ToList().ToArray());
                UpdateWorldCoordinates(MinimapCamera.orthographicSize, MinimapCamera.transform.position);
            }
        }
        /// <summary>
        /// Calculate Point in World from Minimap. Z index is at Camera Z!
        /// </summary>
        /// <param name="_ClickPosition">Pixel (0,0)LL and (xMax,yMax)UR</param>
        /// <param name="_MainCamera">The Camera used to render UI usually the main Camera!</param>
        /// <returns></returns>
        public Vector3 minimapPointToWorld(Vector2 _ClickPosition, Camera _MainCamera)
        {
            Vector2 localCursor;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRectTrans, _ClickPosition, _MainCamera, out localCursor);

            Vector2 UIPercentage = new Vector2(
                    1 - Mathf.Abs(localCursor.x - MinimapSystem.xMin) / MinimapSystem.width,
                    1 - Mathf.Abs(localCursor.y - MinimapSystem.yMin) / MinimapSystem.height
                    );
            Vector3 RealWorld = new Vector3(
                WorldSystem.width * UIPercentage.x,
                WorldSystem.height * UIPercentage.y,
                MainCameraTransform.position.z
                );
            Vector3 correctedRealWorld = new Vector3(RealWorld.x + WorldSystem.xMin, RealWorld.y + WorldSystem.yMin, MainCameraTransform.position.z);

            return new Vector3(
                Mathf.Clamp(correctedRealWorld.x, WorldSystem.xMin + (MainCameraBounds.width * 0.5f), WorldSystem.xMax - (MainCameraBounds.width * 0.5f)),
                Mathf.Clamp(correctedRealWorld.y, WorldSystem.yMin + (MainCameraBounds.height * 0.5f), WorldSystem.yMax - (MainCameraBounds.height * 0.5f)),
                correctedRealWorld.z);
        }

        public void test()
        {
            Debug.Log(worldPointToMinimap(new Vector2(-61,-61)));
            Debug.Log(worldPointToMinimap(Vector2.zero));
            Debug.Log(worldPointToMinimap(new Vector2(61, 61)));
        }

        public Vector2 worldPointToMinimap(Vector2 _ClickPosition)
        {
            Vector2 realWorldPercentage = new Vector2(
                    Mathf.Abs(_ClickPosition.x - WorldSystem.xMin) / WorldSystem.width,
                    Mathf.Abs(_ClickPosition.y - WorldSystem.yMin) / WorldSystem.height
                    );
            Vector2 minimapWorld = new Vector3(
                MinimapSystem.width * realWorldPercentage.x,
                MinimapSystem.height * realWorldPercentage.y
                );

            return minimapWorld;
        }
        #endregion

        #region public
        public Vector3 calculatePseudoCentroid(MinimapCornerMarker[] points)
        {
            Rect worldDim = calculateWorldBordersRect();
            return new Vector3(worldDim.xMin + (worldDim.width * 0.5f), worldDim.yMin + (worldDim.height * 0.5f), MinimapCamera.transform.position.z);
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
        public void addCorner(string _UniqueID, MinimapCornerMarker _Marker)
        {
            MapCorners.Add(_UniqueID, _Marker);
        }
        public void removeCorner(string _UniqueID)
        {
            MapCorners.Remove(_UniqueID);
            UpdateMinimapCamera();
        }
        #endregion

        protected float getOptimalOrthographicCameraSize() {

            Rect worldDim = calculateWorldBordersRect();
            float biggestDistance = checkDimension(worldDim.width, worldDim.height, true);
            return biggestDistance * 0.50f;
        }
        protected Rect calculateWorldBordersRect()
        {
            float xMin = Mathf.Infinity;
            float xMax = Mathf.NegativeInfinity;
            float yMin = Mathf.Infinity;
            float yMax = Mathf.NegativeInfinity;

            foreach (KeyValuePair<string, MinimapCornerMarker> item in MapCorners)
            {
                Transform tempTrans = item.Value.transform;
                xMin = checkDimension(xMin, tempTrans.GetPositionX(), false);
                xMax = checkDimension(xMax, tempTrans.GetPositionX(), true);
                yMin = checkDimension(yMin, tempTrans.GetPositionY(), false);
                yMax = checkDimension(yMax, tempTrans.GetPositionY(), true);
            }

            float distanceX = Mathf.Abs(xMin - xMax);
            float distanceY = Mathf.Abs(yMin - yMax);

            return new Rect(xMin, yMin, distanceX, distanceY);
        }
        protected float checkDimension(float _Value, float _NewValue, bool _ReturnTheHigherOne) {
            //should only return distance of on dimension o
            if (_ReturnTheHigherOne)
            {
                if (_NewValue > _Value)
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
        protected void drag(PointerEventData _PointerEventData)
        {
            if (_PointerEventData != null && _PointerEventData.pointerCurrentRaycast.gameObject != null && _PointerEventData.pointerCurrentRaycast.gameObject.name == this.gameObject.name)
            {
                if (mouseDragDelegate != null) {
                    mouseClickDelegate(_PointerEventData);
                }
                handleLeftDrag(_PointerEventData);
                handleRightDrag(_PointerEventData);
            }
        }
        protected void click(PointerEventData _PointerEventData)
        {
            if (_PointerEventData != null && _PointerEventData.pointerCurrentRaycast.gameObject != null && _PointerEventData.pointerCurrentRaycast.gameObject.name == this.gameObject.name)
            {
                if (mouseDragDelegate != null)
                {
                    mouseClickDelegate(_PointerEventData);
                }
                handleLeftClick(_PointerEventData);
                handleRightClick(_PointerEventData);
            }
        }
        protected void clearDragClickInput()
        {
            dragData = null;
            clickData = null;
        }
        protected void SetupViewLineRenderer()
        {
            MiniMapViewRenderer.sortingLayerName = "OnTop";
            MiniMapViewRenderer.sortingOrder = 20;
            MiniMapViewRenderer.positionCount = 4;

            for (int i = 0; i < ViewRenderCorners.Length; i++)
            {
                MiniMapViewRenderer.SetPosition(i, ViewRenderCorners[i]);
            }

            MiniMapViewRenderer.numCapVertices = 1;
            MiniMapViewRenderer.numCornerVertices = 1;
            MiniMapViewRenderer.startWidth = 0.10000000000000000000000000f;
            MiniMapViewRenderer.endWidth = 0.10000000000000000000000000f;
            MiniMapViewRenderer.useWorldSpace = false;
            MiniMapViewRenderer.loop = true;
            MiniMapViewRenderer.material = new Material(Shader.Find("UI/Default"));
            MiniMapViewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MiniMapViewRenderer.alignment = LineAlignment.View;
        }
        protected void handleLeftDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                setMainCameraWithMinimapEvent(eventData);
            }
        }
        protected void handleRightDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                return;
            }
        }
        protected void handleLeftClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                setMainCameraWithMinimapEvent(eventData);
            }
        }
        protected void handleRightClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
            }
        }
        protected void OptionalMainCameraClamping()
        {
            if (ClampMainCameraToWorld == true) {
                MainCameraTransform.position = new Vector3(
                Mathf.Clamp(MainCameraTransform.position.x, WorldSystem.xMin + (MainCameraBounds.width * 0.5f), WorldSystem.xMax - +(MainCameraBounds.width * 0.5f)),
                Mathf.Clamp(MainCameraTransform.position.y, WorldSystem.yMin + (MainCameraBounds.height * 0.5f), WorldSystem.yMax - +(MainCameraBounds.height * 0.5f)),
                MainCameraTransform.position.z);
            }
        }
        protected void setMainCameraWithMinimapEvent(PointerEventData eventData)
        {
            MainCameraTransform.position = minimapPointToWorld(eventData.position, eventData.pressEventCamera);
        }
        protected void UpdateMainCameraView()
        {
            MainCameraBounds = MainCamera.OrthographicRect();
            leftMargin =  Mathf.Abs(MainCameraBounds.min.x - WorldSystem.xMin);
            xWorldPercentageMin = leftMargin / WorldSystem.width;
            bottomMargin = Mathf.Abs(MainCameraBounds.min.y - WorldSystem.yMin);
            yWorldPercentageMin = bottomMargin / WorldSystem.height;

            rightMargin = Mathf.Abs(MainCameraBounds.max.x - WorldSystem.xMax);
            xWorldPercentageMax = rightMargin / WorldSystem.width;
            topMargin = Mathf.Abs(MainCameraBounds.max.y - WorldSystem.yMax);
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
        protected void UpdateWorldCoordinates(float _CameraSize, Vector3 _CameraPosition)
        {
            WorldSystem.max = new Vector2(_CameraPosition.x + _CameraSize, _CameraPosition.y + _CameraSize);
            WorldSystem.min = new Vector2(_CameraPosition.x - _CameraSize, _CameraPosition.y - _CameraSize);
        }
        protected void UpdateMinimapCoordinates()
        {
            MinimapSystem.max = new Vector2(this.minimapRectTrans.anchoredPosition.x, this.minimapRectTrans.anchoredPosition.y);
            MinimapSystem.min = new Vector2(0, 0);
        }
        void OnDrawGizmosSelected()
        {
            Handles.Label((Vector3)MainCameraBounds.center + (Vector3.left * 20f), "(" + xWorldPercentageMin + "/" + leftMargin + ")");
            Handles.Label((Vector3)MainCameraBounds.center + (Vector3.down * 20f), "(" + yWorldPercentageMin + "/" + bottomMargin + ")");
            Handles.Label((Vector3)MainCameraBounds.center + (Vector3.right * 20f), "(" + xWorldPercentageMax + "/" + rightMargin + ")");
            Handles.Label((Vector3)MainCameraBounds.center + (Vector3.up * 20f), "(" + yWorldPercentageMax + "/" + topMargin + ")");

            Gizmos.color = Color.cyan;
            //WORLDSYSTEM CYAN
            Handles.Label(WorldSystem.minMax(), "(" + WorldSystem.xMin + "," + WorldSystem.yMax + ")");
            Gizmos.DrawWireSphere(WorldSystem.minMax(), 1f);
            Handles.Label(WorldSystem.max, "(" + WorldSystem.xMax + "," + WorldSystem.yMax + ")");
            Gizmos.DrawWireSphere(WorldSystem.max, 1f);
            Handles.Label(WorldSystem.maxMin(), "(" + WorldSystem.xMax + "," + WorldSystem.yMin + ")");
            Gizmos.DrawWireSphere(WorldSystem.maxMin(), 1f);
            Handles.Label(WorldSystem.min, "(" + WorldSystem.xMin + "," + WorldSystem.yMin + ")");
            Gizmos.DrawWireSphere(WorldSystem.min, 1f);

            Gizmos.DrawLine(WorldSystem.min, WorldSystem.minMax());
            Gizmos.DrawLine(WorldSystem.minMax(), WorldSystem.max);
            Gizmos.DrawLine(WorldSystem.max, WorldSystem.maxMin());
            Gizmos.DrawLine(WorldSystem.maxMin(), WorldSystem.min);

            Gizmos.color = Color.magenta;
            Handles.Label(WorldSystem.center + (Vector2.left * 20f), "(" + WorldSystem.height + ")");
            Handles.Label(WorldSystem.center + (Vector2.down * 20f), "(" + WorldSystem.width + ")");
            Handles.Label(WorldSystem.center + (Vector2.right * 20f), "(" + WorldSystem.height + ")");
            Handles.Label(WorldSystem.center + (Vector2.up * 20f), "(" + WorldSystem.width + ")");

            //CAMERABOUNDS MAGENTA
            Handles.Label(MainCameraBounds.min, "(" + MainCameraBounds.min.x + "," + MainCameraBounds.min.y + ")");
            Handles.Label(MainCameraBounds.center, "(" + MainCameraBounds.center.x + "," + MainCameraBounds.center.y + ")");
            Handles.Label(MainCameraBounds.max, "(" + MainCameraBounds.max.x + "," + MainCameraBounds.max.y + ")");

            Gizmos.DrawLine(MainCameraBounds.min, MainCameraBounds.minMax());
            Gizmos.DrawLine(MainCameraBounds.minMax(), MainCameraBounds.max);
            Gizmos.DrawLine(MainCameraBounds.max, MainCameraBounds.maxMin());
            Gizmos.DrawLine(MainCameraBounds.maxMin(), MainCameraBounds.min);
        }

        #region struct definitions
        public enum IconType
        {
            GroundUnit,
            Building,
            Aircraft,
            Ship
        }

        [System.Serializable]
        public class MinimapObject
        {
            public GameObject IconGO = null;
            public IconType Icon = IconType.GroundUnit;
            public int Player = 1;
            public int Team = 1;
            public Color TeamColor = Color.red;
            public SpriteRenderer ColorRenderer = null;
        }

        [System.Serializable]
        public struct Icons
        {
            public GameObject GroundUnit;
            public GameObject Building;
            public GameObject Aircraft;
            public GameObject Ship;
        }
        #endregion
    }

}
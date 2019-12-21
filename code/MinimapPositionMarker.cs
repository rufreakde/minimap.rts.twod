using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace minimap.rts.twod
{
    public class MinimapPositionMarker : MonoBehaviour
    {
        public Color renderedColor = Color.red;
        protected Minimap MinimapScript;
        // TODO this is temporary and should be on the unit class seems like I have to add a Players and Units Manager before finishing the color stuff!
        public Minimap.MinimapObject UnitInfo; // okay also there is like local player shinanigance I have to think about here! So definetly first the new manager :)

        public void UpdateMinimapIconColor()
        {

        }

        private void setIcon(Minimap.IconType _IconType)
        {
            GameObject prefab = MinimapScript.GetIcon(_IconType);
            GameObject temp = Instantiate(prefab, new Vector2(0f, 0f), Quaternion.identity) as GameObject;
            temp.name = "Minimap-icon";
            Transform iconTrans = temp.GetComponent<Transform>();
            Transform markerTrans = this.GetComponent<Transform>();
            iconTrans.SetParent(markerTrans.transform, false);
            iconTrans.position = Vector3.zero;
            iconTrans.localPosition = Vector3.zero;
            UnitInfo.ColorRenderer = iconTrans.FindChildRecursive("inside").GetComponentInChildren<SpriteRenderer>();

            UnitInfo.ColorRenderer.color = UnitInfo.getColorMinimapShown(MinimapScript, UnitInfo.Player, UnitInfo.Team);
            UnitInfo.IconGO = temp;
        }

        void Start()
        {
            MinimapScript = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();
            setIcon(UnitInfo.Icon);
        }
    }
}
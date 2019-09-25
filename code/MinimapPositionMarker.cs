using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace minimap.rts.twod
{
    public class MinimapPositionMarker : MonoBehaviour
    {
        //send:
        //icon unit type
        //player controlled
        //transform
        public Minimap.MinimapObject UnitInfo;
        protected Minimap MinimapScript;

        public Color getMapTeamColor()
        {
            return UnitInfo.TeamColor;
        }
        public void setMapTeamColor(Color _Color)
        {
            UnitInfo.TeamColor = _Color;
        }
        public int getMapTeam()
        {
            return UnitInfo.Team;
        }
        public void setMapTeam(int _Team)
        {
            UnitInfo.Team = _Team;
        }
        private void setIcon(Minimap.IconType _IconType)
        {
            GameObject prefab = MinimapScript.GetIcon(_IconType);
            GameObject temp = Instantiate(prefab, new Vector2(0f, 0f), Quaternion.identity) as GameObject;
            temp.name = "Minimap-icon";
            Transform iconTrans = temp.GetComponent<Transform>();
            Transform markerTrans = this.GetComponent<Transform>();
            iconTrans.parent = markerTrans.transform;
            iconTrans.position = Vector3.zero;
            iconTrans.localPosition = Vector3.zero;
            this.UnitInfo.ColorRenderer = iconTrans.FindChildRecursive("inside").GetComponentInChildren<SpriteRenderer>();
            this.UnitInfo.ColorRenderer.color = getMapTeamColor();
            this.UnitInfo.IconGO = temp;
        }

        void Start()
        {
            MinimapScript = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();
            setIcon(UnitInfo.Icon);
        }
    }
}
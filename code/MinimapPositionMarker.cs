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
    protected MinimapIconHolder MinimapIconHolder;
    protected Minimap.MinimapObject UnitInfo;
    protected Minimap MinimapScript;

    public Color getMapTeamColor()
    {
        return UnitInfo.TeamColor;
    }
    public void setMapTeamColor(Color _Color)
    {
        UnitInfo.TeamColor = _Color;
    }
    public int getMapTeam() {
        return UnitInfo.Team;
    }
    public void setMapTeam(int _Team)
    {
        UnitInfo.Team = _Team;
    }
    public void setIcon(Minimap.IconType _IconType) {
        // this removes the old icon and creates a new one because well it is a pain in the ass in unity to work with sprites directly...
        GameObject temp = Instantiate(MinimapScript.GetIcon(_IconType), new Vector2(0f, 0f), Quaternion.identity);
        temp.transform.SetParent(MinimapScript.transform, false);
        temp.name = "Minimap-icon";
        temp.transform.localPosition = Vector2.zero; // TODO set to current units position with minimap helper function MinimapScript.calculatePosition( realWorldPosition )
        MinimapIconHolder.MinimapSymbol = temp.GetComponentInChildren<MinimapPositionMarker>();
        Destroy(this.gameObject);
    }

    public void initialize(Minimap.MinimapObject _Info)
    {
        // store reference for easy retreavage with minimap manager when traversing! 
        // The MinimapIconHolder.cs has a ref to this script and can use public functions in Enable and Disable...!
        MinimapScript = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();
        UnitInfo = _Info;
    }

    void OnEnable() {
        //subscribe self to minimap
    }

    void OnDisable() {
        //unsubscribe self from minimap
    }

}
}
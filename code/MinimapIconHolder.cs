using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace minimap.rts.twod
{
    public class MinimapIconHolder : MonoBehaviour
    {
        public Minimap.IconType MinimapIcon = Minimap.IconType.GroundUnit;

        [AutoAssign]
        public MinimapPositionMarker MinimapSymbol = null;


        private void OnEnable()
        {
            Minimap tempMinimap = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();

            GameObject temp = Instantiate(tempMinimap.GetIcon(MinimapIcon), new Vector2(0f, 0f), Quaternion.identity);
            temp.transform.SetParent(tempMinimap.transform, false);
            temp.name = "Minimap-icon";
            temp.transform.localPosition = Vector2.zero; // TODO set to current units position with minimap helper function MinimapScript.calculatePosition( realWorldPosition )

            MinimapSymbol = temp.GetComponent<MinimapPositionMarker>();
        }

        private void LateUpdate()
        {
            // TODO we update the position of the unit in the rendering
        }

        private void OnDestroy()
        {
            // TODO we remove our icon since it is now left
        }
    }
}
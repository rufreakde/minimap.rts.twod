using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace minimap.rts.twod
{
    public class MinimapCornerMarker : MonoBehaviour
    {
        protected Minimap MinimapScript;
        protected string UniqueID;

        void Awake()
        {
            MinimapScript = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();
            UniqueID = System.Guid.NewGuid().ToString();
            Debug.Log(UniqueID);
        }

        void OnEnable()
        {
            //subscribe self to minimap
            MinimapScript.addCorner(UniqueID, this);
        }

        void OnDisable()
        {
            //unsubscribe self from minimap
            MinimapScript.removeCorner(UniqueID);
        }

    }
}
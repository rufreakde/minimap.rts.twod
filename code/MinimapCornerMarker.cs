using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace minimap.rts.twod
{
    public class MinimapCornerMarker : MonoBehaviour
    {
        protected Minimap MinimapScript;
        protected string UniqueID;
        protected int StartPositionIndex = 0;
        
        [SimpleButton("InsertCurrentPositionAtFront", typeof(MinimapCornerMarker))]
        [SimpleButton("triggerPositionNext", typeof(MinimapCornerMarker))]
        [SimpleButton("triggerPositionPrevious", typeof(MinimapCornerMarker))]
        [InfoBox("The current 'transform.position' will be inserted automatically 'NextPositionsOverTime' is empty!")]
        public List<Vector2> NextPositionsOverTime;

        void Awake()
        {
            MinimapScript = GameObject.FindWithTag("MinimapCanvas").GetComponentInChildren<Minimap>();
            UniqueID = System.Guid.NewGuid().ToString();
            if(NextPositionsOverTime.Count <= 0)
            {
                InsertCurrentPositionAtFront();
            }
            this.transform.position = NextPositionsOverTime[0];
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

        public void triggerPosition(int _PositionArrayIndex)
        {
            if (NextPositionsOverTime.Count > _PositionArrayIndex && _PositionArrayIndex >= 0)
            {
                StartPositionIndex = _PositionArrayIndex;
                this.transform.position = NextPositionsOverTime[StartPositionIndex];
            } else {
                Debug.LogError(this.ToString() + " You tried to set a higher position index than there is available on GO: " + this.gameObject.name);
            }
        }
        public void InsertCurrentPositionAtFront()
        {
            NextPositionsOverTime.Insert(0, this.transform.position);
        }

        public void triggerPositionNext() {
                int temp = StartPositionIndex + 1;
                triggerPosition(temp);
        }

        public void triggerPositionPrevious()
        {
            int temp = StartPositionIndex - 1;
            triggerPosition(temp);
        }

        void OnDrawGizmos()
        {
            Color temp = Gizmos.color;
            Gizmos.color = new Color(0.1f, 1f, 0.1f);
            Gizmos.DrawWireSphere(this.transform.position, 1f);
            Gizmos.color = temp;
        }

        private void OnDrawGizmosSelected()
        {
            //draw centroid and borders!
            Color temp = Gizmos.color;
            Gizmos.color = new Color(0.8f, 0.1f, 0.4f);
            MinimapCornerMarker[] CornerPieces = GameObject.FindObjectsOfType<MinimapCornerMarker>();
            for (int i=0; i< CornerPieces.Length; i++)
            {
                if (i == CornerPieces.Length - 1) {
                    Gizmos.DrawLine(CornerPieces[i].transform.position, CornerPieces[0].transform.position);
                }
                else
                {
                    Gizmos.DrawLine(CornerPieces[i].transform.position, CornerPieces[i + 1].transform.position);
                }
            }

            if (MinimapScript)
            {
                Vector2 centroid = MinimapScript.calculatePseudoCentroid(CornerPieces);
                Gizmos.color = new Color(0.1f, 0.1f, 0.9f);
                Gizmos.DrawWireSphere(centroid, 1f);
            }

            Gizmos.color = temp;
        }

    }
}
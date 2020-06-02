using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts._AI.Senses
{
    public class Sight : Sense
    {
        protected int fov = 30;
        protected int viewRange = 30;

        GameManager gm { get { return GameManager.main; } }

        private bool found = false;

        private Transform playerTrans;
        private Vector3 rayDirection;

        //Base for sending messages upon spotting or losing the player
        public UnityEvent spotted;
        public UnityEvent lost;

        protected override void Initialize()
        {
            playerTrans = FindObjectOfType<Player>().transform;
            Debug.Log("Detect Rate = " + detectRate);
        }

        protected override void UpdateSenses()
        {
            elapsedTime += Time.deltaTime;
            //Debug.Log("elapsedTime = " + elapsedTime);
            if (elapsedTime >= detectRate)
            {
                if (gm.GetCurrentGamePhase().Equals(GamePhase.Spirit))
                    DetectPlayer();
            }
            
        }

        private void DetectPlayer()
        {
            RaycastHit hit;

            rayDirection = playerTrans.position - transform.position;

            if ((Vector3.Angle(rayDirection, transform.forward)) >= -fov && (Vector3.Angle(rayDirection, transform.forward)) <= fov)
            {
                if (Physics.Raycast(transform.position, rayDirection, out hit, viewRange))
                {
                    Player player = hit.collider.GetComponent<Player>();

                    if (player != null)
                    {
                        Debug.Log("Player detected");
                        found = true;
                        spotted.Invoke();
                    }
                }
            }
            else if(found == true)
            {
                found = false;
                lost.Invoke();
            }
            elapsedTime = 0.0f;
        }

        void OnDrawGizmos()
        {
            if (!Application.isEditor || playerTrans == null) return;
            Debug.DrawLine(transform.position, playerTrans.position, Color.red);
            Vector3 frontRayPoint = transform.position +
            (transform.forward * viewRange);
            //Approximate perspective visualization
            Vector3 leftRayPoint = Quaternion.Euler(0, fov * 0.5f, 0) *
            frontRayPoint;
            Vector3 rightRayPoint = Quaternion.Euler(0, -fov * 0.5f, 0) *
            frontRayPoint;
            Debug.DrawLine(transform.position, frontRayPoint, Color.green);
            Debug.DrawLine(transform.position, leftRayPoint, Color.green);
            Debug.DrawLine(transform.position, rightRayPoint, Color.green);
        }
    }
}

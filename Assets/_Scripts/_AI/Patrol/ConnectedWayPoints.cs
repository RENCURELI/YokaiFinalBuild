using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedWayPoints : PatrolPoints
{
    [SerializeField]
    public float _connectivityRadius = 50f;

    List<ConnectedWayPoints> _connections;

    public void Start()
    {
        //Grab all waypoints in scene
        /*GameObject[] allWayPoints = GameObject.FindGameObjectsWithTag("waypoint");

        //Create a list of waypoints referable to later on
        _connections = new List<ConnectedWayPoints>();

        //Check for connected waypoints
        for (int i = 0; i < allWayPoints.Length; i++)
        {
            ConnectedWayPoints nextWaypoint = allWayPoints[i].GetComponent<ConnectedWayPoints>();

            //waypoint found
            if (nextWaypoint != null)
            {
                if (Vector3.Distance(this.transform.position, nextWaypoint.transform.position) <= _connectivityRadius && nextWaypoint != this)
                {
                    _connections.Add(nextWaypoint);
                }
            }
        }*/
    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);

        /*Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _connectivityRadius);*/
    }

    /*public ConnectedWayPoints NextWayPoint(ConnectedWayPoints previousWayPoint)
    {
        
        if (_connections.Count == 0)
        {
            Debug.Log("Insufficient waypoint count");
            return null;
        }
        else if (_connections.Count == 1 && _connections.Contains(previousWayPoint))
        {
            return previousWayPoint;
        }
        else
        {
            ConnectedWayPoints nextWaypoint;
            int nextIndex = 0;

            do
            {
                nextIndex = UnityEngine.Random.Range(0, _connections.Count);
                nextWaypoint = _connections[nextIndex];

            } while (nextWaypoint == previousWayPoint);

            return nextWaypoint;
        }

    }*/
}

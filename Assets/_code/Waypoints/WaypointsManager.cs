using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    public static WaypointsManager Instance;

    public List<WaypointCluster> allClusters = new List<WaypointCluster>();
    public List<Waypoint> allWaypoints = new List<Waypoint>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create WaypointsManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Direction
{
    public string name;
    public Island nextIsland;
    public WaypointSection section;
}

public class WaypointCluster : MonoBehaviour
{
	public Island island;
    public WaypointSection islandSection;
    public List<Direction> directions = new();

	[Header("Waypoints Creating")]
	public GameObject waypointPrefab;
	public float createdWaypointsSpacing = 10;
	public float islandCorrectionRadius = 100;

	[Range(0, 5)]
	public int smoothIterations = 1;

    float distance;
	float distanceFromIsland;
	List<GameObject> waypoints = new List<GameObject>();
	List<GameObject> waypointsForCorrection = new List<GameObject>();

    IslandsManager islandsManager;

    void Start()
    {
        islandsManager = IslandsManager.Instance;
        WaypointsManager.Instance.allClusters.Add(this);
		CreateIslandToIslandSections();
    }

	void CreateIslandToIslandSections()
	{
		for (int i = 0; i < islandsManager.allIslands.Count; i++)
		{
			if (islandsManager.allIslands[i] != island)
			{
				GameObject sectionGO = new GameObject(island.gameObject.name + " - " + islandsManager.allIslands[i].gameObject.name);
				sectionGO.transform.parent = transform;
				sectionGO.transform.localPosition = Vector3.zero;
				sectionGO.transform.localRotation = Quaternion.identity;
				sectionGO.transform.localScale = Vector3.one;

				WaypointSection section = sectionGO.AddComponent<WaypointSection>();

				CreateIslandToIslandWaypoints(sectionGO.transform, islandsManager.allIslands[i]);

                Direction direction = new Direction();
                direction.section = section;
                direction.nextIsland = islandsManager.allIslands[i];
                direction.name = sectionGO.name;

                directions.Add(direction);
            }
		}
	}

	void CreateIslandToIslandWaypoints(Transform parent, Island nextIsland)
	{
        waypoints.Clear();

        distance = Vector3.Distance(island.islandWaypoint.transform.position, nextIsland.islandWaypoint.transform.position);

		float currentAbsolutePosition = 0;
		float currentNormalizedPosition = 0;
		int id = 0;

		for (currentAbsolutePosition = 0; currentAbsolutePosition <= distance; currentAbsolutePosition += createdWaypointsSpacing)
		{
			id++;
			currentNormalizedPosition = currentAbsolutePosition / distance;

            CreateWaypoint(parent, nextIsland, currentNormalizedPosition);
        }

        for (int i = 0; i < waypoints.Count; i++)
            waypoints[i].name = waypoints[i].name + " - Waypoint " + i;

        SmoothWay(nextIsland, parent);
    }

    void CreateWaypoint(Transform parent, Island nextIsland, float currentPosition)
    {
        Vector3 position = Vector3.Lerp(island.islandWaypoint.transform.position, nextIsland.islandWaypoint.transform.position, currentPosition);

        for (int i = 0; i < islandsManager.allIslands.Count; i++)
        {
            distanceFromIsland = Vector3.Distance(position, islandsManager.allIslands[i].transform.position);

            if (distanceFromIsland <= islandCorrectionRadius)
            {
                GameObject waypointGO = Instantiate(waypointPrefab);
                waypointGO.transform.position = position;
                waypointGO.transform.localRotation = Quaternion.identity;
                waypointGO.name = islandsManager.allIslands[i].name;
                waypointGO.transform.parent = parent;
                waypointGO.transform.localScale = Vector3.one;

                waypoints.Add(waypointGO);
            }
        }
    }

    void SmoothWay(Island nextIsland, Transform parent)
	{
        for (int i = 0; i < islandsManager.allIslands.Count; i++)
        {
            waypointsForCorrection.Clear();

            for (int w = 0; w < waypoints.Count; w++)
            {
                distanceFromIsland = Vector3.Distance(waypoints[w].transform.position, islandsManager.allIslands[i].transform.position);

                if (distanceFromIsland <= islandCorrectionRadius)
                    waypointsForCorrection.Add(waypoints[w]);
            }

            CorrectWaypointPosition(islandsManager.allIslands[i], parent);
            waypointsForCorrection.Clear();
        }

        if (waypoints.Count >= 2)
        {
            for (int s = 0; s < smoothIterations; s++)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    if (i == 0)
                        waypoints[i].transform.position = (island.islandWaypoint.transform.position + waypoints[i + 1].transform.position) / 2;
                    else if (i == waypoints.Count - 1)
                        waypoints[i].transform.position = (waypoints[i - 1].transform.position + nextIsland.islandWaypoint.transform.position) / 2;
                    else
                        waypoints[i].transform.position = (waypoints[i - 1].transform.position + waypoints[i + 1].transform.position) / 2;
                }
            }
        }
    }

    //коррекция позиции, чтобы быть вне островов
    void CorrectWaypointPosition(Island island, Transform parent)
    {
        //return;

        if (waypointsForCorrection.Count == 0)
            return;

        Vector3 directionStart = Vector3.Normalize(island.transform.position - waypointsForCorrection[0].transform.position);
        Vector3 directionFinish = Vector3.Normalize(waypointsForCorrection[waypointsForCorrection.Count - 1].transform.position - island.transform.position);

        float angleStart = Vector3.Angle(directionStart, new Vector3(0,0,1));
        float angleFinish = Vector3.Angle(directionFinish, new Vector3(0,0,1));
        float angleDelta = (angleFinish - angleStart + 180) / waypointsForCorrection.Count;

        for (int i = 0; i < waypointsForCorrection.Count; i++)
        {
            waypointsForCorrection[i].transform.parent = null;
            waypointsForCorrection[i].transform.Rotate(0, angleStart + 180 + angleDelta * i, 0);
            waypointsForCorrection[i].transform.parent = parent;
            waypointsForCorrection[i].transform.position = island.transform.position;
            waypointsForCorrection[i].transform.Translate(waypointsForCorrection[i].transform.forward * islandCorrectionRadius, Space.World);
        }
    }
}

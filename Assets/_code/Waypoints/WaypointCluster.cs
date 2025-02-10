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
    public float islandRadiusCorrectionMultiplier = 0.1f;

	[Range(0, 3)]
	public int smoothIterations = 1;

    float distance;
	float distanceFromIsland;
	List<GameObject> waypoints = new List<GameObject>();

    ResourcesManager resourcesManager;

    void Start()
    {
        resourcesManager = ResourcesManager.Instance;
        WaypointsManager.Instance.allClusters.Add(this);
		CreateIslandToIslandSection();
    }

	void CreateIslandToIslandSection()
	{
		for (int i = 0; i < resourcesManager.allIslands.Count; i++)
		{
			if (resourcesManager.allIslands[i] != island)
			{
				GameObject sectionGO = new GameObject(island.gameObject.name + " - " + resourcesManager.allIslands[i].gameObject.name);
				sectionGO.transform.parent = transform;
				sectionGO.transform.localPosition = Vector3.zero;
				sectionGO.transform.localRotation = Quaternion.identity;
				sectionGO.transform.localScale = Vector3.one;

				WaypointSection section = sectionGO.AddComponent<WaypointSection>();

				CreateIslandToIslandWaypoints(sectionGO.transform, resourcesManager.allIslands[i]);

                Direction direction = new Direction();
                direction.section = section;
                direction.nextIsland = resourcesManager.allIslands[i];
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

        SmoothWay(nextIsland);
    }

    void CreateWaypoint(Transform parent, Island nextIsland, float currentPosition)
    {
        Vector3 position = Vector3.Lerp(island.islandWaypoint.transform.position, nextIsland.islandWaypoint.transform.position, currentPosition);

        for (int i = 0; i < resourcesManager.allIslands.Count; i++)
        {
            distanceFromIsland = Vector3.Distance(position, resourcesManager.allIslands[i].transform.position);

            if (distanceFromIsland <= islandCorrectionRadius)
            {
                GameObject waypointGO = Instantiate(waypointPrefab, parent);
                waypointGO.transform.position = position;
                waypointGO.transform.localRotation = Quaternion.identity;
                waypointGO.transform.localScale = Vector3.one;

                CorrectWaypointPosition(resourcesManager.allIslands[i], waypointGO);

                waypoints.Add(waypointGO);
            }
        }

        for (int i = 0; i < waypoints.Count; i++)
            waypoints[i].name = "Waypoint " + i;
    }

    void SmoothWay(Island nextIsland)
	{
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

        for (int i = 0; i < resourcesManager.allIslands.Count; i++)
        {
            for (int w = 0; w < waypoints.Count; w++)
            {
                distanceFromIsland = Vector3.Distance(waypoints[w].transform.position, resourcesManager.allIslands[i].transform.position);

                if (distanceFromIsland <= islandCorrectionRadius)
                    CorrectWaypointPosition(resourcesManager.allIslands[i], waypoints[w]);
            }
        }
    }

    //коррекция позиции, чтобы быть вне островов
    void CorrectWaypointPosition(Island island, GameObject waypoint)
    {
        Vector3 direction = Vector3.Normalize(island.transform.position - waypoint.transform.position);
        waypoint.transform.position = island.transform.position;
        waypoint.transform.Translate(direction * -islandRadiusCorrectionMultiplier * islandCorrectionRadius, Space.World);
    }
}

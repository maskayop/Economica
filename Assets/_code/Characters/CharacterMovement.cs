using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	public WaypointCluster currentCluster = null;

	public float speed;

	[HideInInspector] public Character character;

	Vector3 startPoint;
	Vector3 endPoint;

	float currentPosition = 0;
	int pointIndex = 0;

	WaypointSection currentSection = null;
	Waypoint currentWaypoint;

	Waypoint startWaypoint;
	Waypoint endWaypoint;

	WaypointsManager waypointsManager;

    void Start()
	{
		waypointsManager = WaypointsManager.Instance;

		currentCluster = waypointsManager.allClusters[Random.Range(0, waypointsManager.allClusters.Count)];
		currentSection = currentCluster.islandSection;
        startPoint = transform.position;
        endWaypoint = currentSection.points[0];
        endPoint = endWaypoint.transform.position;
        character.finishIsland = endWaypoint.island;
    }

	void Update()
	{
		MoveFromPointToPoint();
	}

	void Move()
	{
		if(startWaypoint)
			startPoint = startWaypoint.transform.position;

        if (endWaypoint)
			endPoint = endWaypoint.transform.position;

        transform.position = Vector3.Lerp(startPoint, endPoint, currentPosition);
		currentPosition += speed * Time.deltaTime / Vector3.Distance(startPoint, endPoint);

        transform.LookAt(endPoint);
		Quaternion q = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z));
		transform.rotation = q;
	}

	void MoveFromPointToPoint()
	{
		Move();

		if (currentPosition >= 1)
		{
			currentPosition = 0;

			if (currentSection.points.Count == 1)
			{
                startWaypoint = endWaypoint;
                character.startIsland = startWaypoint.island;

                Island nextIsland = character.CalculateNextIsland();
                if (nextIsland)
				{
					currentCluster = character.startIsland.waypointCluster;
					currentSection = GetNextSection(character.startIsland, character.finishIsland);
                    endWaypoint = currentSection.points[0];
				}

                pointIndex = 0;
            }
			else
			{
                pointIndex++;
				startWaypoint = currentSection.points[pointIndex];

                if (pointIndex + 1 < currentSection.points.Count)
				{
                    endWaypoint = currentSection.points[pointIndex + 1];
                }
				else
				{
                    endWaypoint = character.finishIsland.islandWaypoint;
					currentSection = character.finishIsland.waypointCluster.islandSection;
                    pointIndex = 0;
                }
			}
		}

		WaypointSection GetNextSection(Island startIsland, Island finishIsland)
		{
			WaypointSection section = null;

			for (int i = 0; i < startIsland.waypointCluster.directions.Count; i++)
			{
				if (startIsland.waypointCluster.directions[i].nextIsland == finishIsland)
					return startIsland.waypointCluster.directions[i].section;
            }

			return section;
		}
	}
}

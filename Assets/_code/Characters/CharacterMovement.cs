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

	Waypoint currentWaypoint;

	Waypoint startWaypoint;
	Waypoint endWaypoint;

	WaypointsManager waypointsManager;

    void Start()
	{
		waypointsManager = WaypointsManager.Instance;

		currentCluster = waypointsManager.allClusters[Random.Range(0, waypointsManager.allClusters.Count)];
        startPoint = transform.position;

		endWaypoint = currentCluster.islandSection.points[0];
        endPoint = endWaypoint.transform.position;
        character.finishIsland = endWaypoint.island;
    }

	void Update()
	{
		MoveFromPointToPoint();
	}

	void Move()
	{
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

			if (currentCluster)
			{
				pointIndex++;

				if (pointIndex + 1 < currentCluster.islandSection.points.Count)
				{
					currentWaypoint = currentCluster.islandSection.points[pointIndex];

					startWaypoint = currentCluster.islandSection.points[pointIndex];
                    startPoint = startWaypoint.transform.position;
					character.startIsland = currentWaypoint.island;

					Island nextIsland = character.CalculateNextIsland();
					if(nextIsland)
						endWaypoint = nextIsland.islandWaypoint;

                    endPoint = endWaypoint.transform.position;
                    character.finishIsland = endWaypoint.island;
                }
				else
				{
					startWaypoint = endWaypoint;
                    startPoint = endPoint;
                    character.startIsland = startWaypoint.island;

                    Island nextIsland = character.CalculateNextIsland();
                    if (nextIsland)
                        endWaypoint = nextIsland.islandWaypoint;

                    endPoint = endWaypoint.transform.position;
                    character.finishIsland = endWaypoint.island;
                }
			}
		}
	}
}

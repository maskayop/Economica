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

		endWaypoint = currentCluster.Points[0];
        endPoint = endWaypoint.transform.position;
        character.finishIsland = endWaypoint.island;
    }

	void Update()
	{
		MoveFromPointToPoint();
	}

	void TransformPosition()
	{
		transform.position = Vector3.Lerp(startPoint, endPoint, currentPosition);
		var passedDistance = speed * Time.deltaTime;
		var fullDistance = Vector3.Distance(startPoint, endPoint);
		var relativePostition = passedDistance / fullDistance;
		currentPosition += relativePostition;

		transform.LookAt(endPoint);
		Quaternion q = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z));
		transform.rotation = q;
	}

	void MoveFromPointToPoint()
	{
		TransformPosition();

		if (currentPosition >= 1)
		{
			currentPosition = 0;

			if (currentCluster)
			{
				pointIndex++;

				if (pointIndex + 1 < currentCluster.Points.Count)
				{
					currentWaypoint = currentCluster.Points[pointIndex];

					startWaypoint = currentCluster.Points[pointIndex];
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

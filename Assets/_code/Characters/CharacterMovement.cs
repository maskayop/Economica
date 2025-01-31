using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	public WaypointCluster currentCluster = null;

	public float speed;

	public Character character;

	Vector3 startPoint;
	Vector3 endPoint;

	float currentPosition = 0;
	int pointIndex = 0;

	Waypoint currentWaypoint;
	WaypointsManager waypointsManager;

    void Start()
	{
		waypointsManager = WaypointsManager.Instance;
		currentCluster = waypointsManager.allClusters[Random.Range(0, waypointsManager.allClusters.Count)];
        startPoint = transform.position;
		endPoint = currentCluster.Points[0].transform.position;
    }

	private void Update()
	{
		MoveFromPointToPoint();
	}

	private void TransformPosition()
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

	private void MoveFromPointToPoint()
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
					startPoint = currentCluster.Points[pointIndex].transform.position;
					endPoint = waypointsManager.allClusters[Random.Range(0, waypointsManager.allClusters.Count)].Points[0].transform.position;
				}
				else
				{
                    startPoint = endPoint;
                    endPoint = waypointsManager.allClusters[Random.Range(0, waypointsManager.allClusters.Count)].Points[0].transform.position;
                }
			}
		}
	}
}

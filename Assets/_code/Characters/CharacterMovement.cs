using UnityEngine;
using UnityEngine.UIElements;

public class CharacterMovement : MonoBehaviour
{
	public WaypointCluster currentCluster = null;

	public float speed;
	public float rotationTime = 0.5f;

	[HideInInspector] public Character character;

	Vector3 startPoint;
	Vector3 endPoint;

	float currentPosition = 0;
	int pointIndex = 0;

	Vector3 prevEndPoint;
	Quaternion prevRotation = Quaternion.identity;
	bool rotate = false;
	float currentRotation = 0;

	WaypointSection currentSection = null;

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

		if (prevEndPoint != endPoint)
			rotate = true;

		Quaternion q = Quaternion.identity;

        if (rotate)
		{
			currentRotation += Time.deltaTime;

            transform.LookAt(endPoint);
			q = Quaternion.Lerp(prevRotation, transform.rotation, currentRotation);
            q = Quaternion.Euler(new Vector3(0, q.eulerAngles.y, 0));
            transform.rotation = q;
		}
		else
		{
            transform.LookAt(endPoint);
			q = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0));
			transform.rotation = q;
        }
		
        prevRotation = transform.rotation;
        prevEndPoint = endPoint;

		if (currentRotation > rotationTime)
		{
			currentRotation = 0;
			rotate = false;
        }
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

                pointIndex = -1;
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
                    pointIndex = -1;
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

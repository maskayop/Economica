using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public Island island;

	public enum Type
	{
		checkpoint,
		island,
	}

	public Type type;

	void Start()
	{
		WaypointsManager.Instance.allWaypoints.Add(this);
	}
}

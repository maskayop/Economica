using System.Collections.Generic;
using UnityEngine;

public class WaypointSection : MonoBehaviour
{
	List<Waypoint> points = new List<Waypoint>();

	public List<Waypoint> Points
	{
		private set => points = value;
		get => points;
	}

	public bool shouldReverse = false;

	void Start()
	{
		CollectPoints();
		if (shouldReverse)
			points.Reverse();
	}

	public void CollectPoints()
	{
		foreach (Transform child in transform)
		{
			if (child.TryGetComponent<Waypoint>(out Waypoint point))
				points.Add(point);
		}
	}
}

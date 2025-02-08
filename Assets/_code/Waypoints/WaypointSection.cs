using System.Collections.Generic;
using UnityEngine;

public class WaypointSection : MonoBehaviour
{
	public List<Waypoint> points = new List<Waypoint>();

	void Start()
	{
		Init();
	}

	public void Init()
	{
		CollectPoints();
	}

	void CollectPoints()
	{
		foreach (Transform child in transform)
		{
			if (child.TryGetComponent<Waypoint>(out Waypoint point))
				points.Add(point);
		}
	}
}

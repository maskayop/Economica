using System.Collections.Generic;
using UnityEngine;

public class WaypointCluster : MonoBehaviour
{
	public List<WaypointSection> sections = new();
	public List<Waypoint> points = new();

	public List<Waypoint> Points
	{
		private set => points = value;
		get => points;
	}

	[Range(1, 10)]
	[SerializeField]
	float collapseDistance = 1;

    void Start()
    {
        WaypointsManager.Instance.allClusters.Add(this);
		CollectClusterPoints();
    }

    void CollectSections()
	{
		foreach (Transform child in transform)
		{
			if (child.TryGetComponent<WaypointSection>(out WaypointSection section))
				sections.Add(section);
		}
	}

	void CollectClusterPoints()
	{
		CollectSections();

		foreach (var section in sections)
		{
			points.AddRange(section.Points);
		}

		for (int i = 0; i < (points.Count - 1); i++)
		{
			float dist = Vector3.Distance(points[i].transform.position, points[i + 1].transform.position);
			if (dist < collapseDistance)
				points.RemoveAt(i);
		}
	}
}

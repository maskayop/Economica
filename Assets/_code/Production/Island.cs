using UnityEngine;

public class Island : MonoBehaviour
{
	[HideInInspector]
	public ResourcesController resCont;

	public int population = 10;
	public int richPopulation = 0;
	public int elitePopulation = 0;

	[Space(20)]
    public int shoppedTotal = 0;
    public int starvingPopulation = 0;

	[Space(20)]
	public Vector2Int populationGrowthRate;
	public Vector2 popToProdMultiplier;

	[Space(20)]
	public Waypoint islandWaypoint;
	public WaypointCluster waypointCluster;

    int currentDay = 0;

    void Start()
	{
		Init();
    }

	void Update()
	{
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            AddPeople();

			//идут закупаться все
            GoShoping(population, true);
			/*
			//идут закупаться ещё раз богатые
			richPopulation = Mathf.FloorToInt(resCont.totalAvailableResources / population);

            if (richPopulation >= population)
                richPopulation = population;

            GoShoping(richPopulation, false);

            //идут закупаться ещё раз элита
            elitePopulation = Mathf.FloorToInt(resCont.totalAvailableResources / (population * population));

            if (elitePopulation >= population)
                elitePopulation = population;

            GoShoping(elitePopulation, false);
			*/
            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }

	public void Init()
	{
        resCont = GetComponent<ResourcesController>();
        currentDay = GlobalTimeController.Instance.currentDay;
		resCont.island = this;
		ResourcesManager.Instance.allIslands.Add(this);
		ResourcesManager.Instance.totalIndustries += resCont.industriesCount;
        ResourcesManager.Instance.UpdatePopulation();
        waypointCluster.island = this;
    }

	void AddPeople()
	{
		population += Random.Range(populationGrowthRate.x, populationGrowthRate.y + 1);
		population -= Random.Range(0, starvingPopulation);

		if (population <= 0)
			population = 1;

        ResourcesManager.Instance.UpdatePopulation();
	}

	void GoShoping(int customers, bool isStarving)
	{
        resCont.GoShoping(customers, isStarving);

		if(isStarving)
			starvingPopulation = population - shoppedTotal;
    }

	public float GetPopToProdMultiplier()
	{
		float value = Random.Range(popToProdMultiplier.x, popToProdMultiplier.y) * population;
		return value;
	}
}

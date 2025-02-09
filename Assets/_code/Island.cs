using UnityEngine;

public class Island : MonoBehaviour
{
	public ResourcesController resourcesController;
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

			//идут закупаться ещё раз богатые
			richPopulation = Mathf.FloorToInt(resourcesController.totalAvailableResources / population);

            if (richPopulation >= population)
                richPopulation = population;

            GoShoping(richPopulation, false);

            //идут закупаться ещё раз элита
            elitePopulation = Mathf.FloorToInt(resourcesController.totalAvailableResources / (population * population));

            if (elitePopulation >= population)
                elitePopulation = population;

            GoShoping(elitePopulation, false);

            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }

	public void Init()
	{
        currentDay = GlobalTimeController.Instance.currentDay;
		resourcesController.island = this;
		ResourcesManager.Instance.allIslands.Add(this);
		ResourcesManager.Instance.totalIndustries += resourcesController.industriesCount;
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
		if (customers == 0)
			return;
		
		shoppedTotal = 0;
		int shopped = 0;

        for (int i = 0; i < resourcesController.availableInStorage.Count; i++)
        {
			if (resourcesController.availableInStorage[i].amountInStorage != 0 && shoppedTotal <= customers)
			{
                shopped = resourcesController.availableInStorage[i].amountInStorage;

                if (shopped + shoppedTotal <= customers)
				{
                    shoppedTotal += shopped;
                    resourcesController.availableInStorage[i].amountInStorage -= shopped;
				}
				else
				{
                    resourcesController.availableInStorage[i].amountInStorage -= customers - shoppedTotal;
                    shoppedTotal = customers;
                }

			}			
			else if (shoppedTotal > customers)
				break;
        }

        resourcesController.UpdateAvailableInStorage(true);
        ResourcesManager.Instance.UpdateStorage();

		if(isStarving)
			starvingPopulation = population - shoppedTotal;
    }

	public float GetPopToProdMultiplier()
	{
		float value = Random.Range(popToProdMultiplier.x, popToProdMultiplier.y) * population;
		return value;
	}
}

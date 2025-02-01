using UnityEngine;

public class Island : MonoBehaviour
{
	public ResourcesController resourcesController;
	public int population = 10;
	public Vector2Int populationGrowthRate;
	public Vector2 popToProdMultiplier;

	[Space(20)]
	public Waypoint islandWaypoint;

    int currentDay = 0;
	float welfareRatio;

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
            GoShoping(population);

			//идут закупаться ещё раз зажиточные
            welfareRatio = resourcesController.totalAvailableResources / population;

			if (welfareRatio >= population)
				welfareRatio = population;

            GoShoping(Mathf.FloorToInt(welfareRatio));

            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }

	public void Init()
	{
        currentDay = GlobalTimeController.Instance.currentDay;
		resourcesController.island = this;
		ResourcesManager.Instance.allIslands.Add(this);
        ResourcesManager.Instance.UpdatePopulation();
    }

	void AddPeople()
	{
		population += Random.Range(populationGrowthRate.x, populationGrowthRate.y + 1);
		ResourcesManager.Instance.UpdatePopulation();
	}

	void GoShoping(int customers)
	{
		for (int i = 0; i < customers; i++)
		{
			int randomValue = Random.Range(0, resourcesController.availableResourcesInStorage.Count);

			if (resourcesController.availableResourcesInStorage.Count != 0 &&
				resourcesController.availableResourcesInStorage[randomValue].amountInStorage != 0)
			{
				resourcesController.availableResourcesInStorage[randomValue].amountInStorage--;
                resourcesController.UpdateAvailableResourcesInStorage(false);
            }
        }

        resourcesController.UpdateAvailableResourcesInStorage(true);
        ResourcesManager.Instance.UpdateStorage();
    }

	public float GetPopToProdMultiplier()
	{
		float value = Random.Range(popToProdMultiplier.x, popToProdMultiplier.y) * population;
		return value;
	}
}

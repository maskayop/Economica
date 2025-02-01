using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resource
{
    public string name;
    public Sprite sprite;
    public Vector2Int productionRange;
    public int productionAmount;
    public int amountInStorage;
    public int price;
}

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    public List<Resource> allResources = new List<Resource>();
    public List<Resource> storage = new List<Resource>();

    [HideInInspector]
    public List<Island> allIslands = new List<Island>();

    public int pricesMultiplier = 1000000;
    public int totalAvailableResources = 0;
    public int totalPopulation = 0;

    int currentDay = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create ResourcesManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        allResources.Sort( (x, y) => { return x.name.CompareTo(y.name); } );
        CreateStorageList();
    }

    void Start()
    {
        allIslands.Clear();
    }

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            currentDay = GlobalTimeController.Instance.currentDay;
            UpdateStorage();
        }
    }

    public Resource GetRandomResource()
    {
        Resource resource = allResources[Random.Range(0, allResources.Count)];
        return resource;
    }

    public void UpdateStorage()
    {
        for (int i = 0; i < storage.Count; i++)
            storage[i].amountInStorage = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                if (storage[x].name == allIslands[i].resourcesController.storage[x].name)
                {
                    storage[x].amountInStorage += allIslands[i].resourcesController.storage[x].amountInStorage;
                }
            }
        }

        UpdatePrices();
    }

    void CreateStorageList()
    {
        storage.Clear();

        for (int i = 0; i < allResources.Count; i++)
        {
            Resource newres = new Resource();
            newres.name = allResources[i].name;
            newres.sprite = allResources[i].sprite;
            newres.productionRange = allResources[i].productionRange;
            newres.productionAmount = allResources[i].productionAmount;
            newres.amountInStorage = allResources[i].amountInStorage;
            newres.price = allResources[i].price;

            storage.Add(newres);
        }
    }

    void UpdatePrices()
    {
        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].amountInStorage;

        for (int i = 0; i < storage.Count; i++)
        {
            if (totalAvailableResources != 0 && storage[i].amountInStorage != 0)
                storage[i].price = Mathf.FloorToInt(pricesMultiplier / (totalAvailableResources * storage[i].amountInStorage));
            else if (totalAvailableResources != 0 && storage[i].amountInStorage == 0)
                storage[i].price = pricesMultiplier / totalAvailableResources;
            else if (totalAvailableResources == 0)
                storage[i].price = pricesMultiplier;

            if (storage[i].price <= 1)
                storage[i].price = 1;
        }

        for (int i = 0; i < allIslands.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                allIslands[i].resourcesController.storage[x].price = storage[x].price;
            }
        }
    }

    public void UpdatePopulation()
    {
        totalPopulation = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            totalPopulation += allIslands[i].population;
        }
    }
}

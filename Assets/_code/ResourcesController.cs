using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class ResourcesController : MonoBehaviour
{
    public Vector2Int industriesAmountRange;
    public int industriesCount;

    public List<Resource> producingResources = new List<Resource>();
    public List<Resource> storage = new List<Resource>();
    public List<Resource> availableResourcesInStorage = new List<Resource>();
    public int totalAvailableResources = 0;

    [HideInInspector] public Island island;

    [Space(20)]
    public ResourceWidgetsController resourceWidgetController;

    ResourcesManager resourcesManager;
    int currentDay = 0;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            Produce();
            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }
    void Init()
    {
        resourcesManager = ResourcesManager.Instance;
        CalculateIndustries();
        UpdateAvailableResourcesInStorage(true);
        currentDay = GlobalTimeController.Instance.currentDay;
    }

    void CalculateIndustries()
    {
        industriesCount = Random.Range(industriesAmountRange.x, industriesAmountRange.y);

        for (int i = 0; i <= industriesCount; i++)
        {
            Resource res = resourcesManager.GetRandomResource();
            
            Resource newres = new Resource();
            newres.name = res.name;
            newres.sprite = res.sprite;
            newres.productionRange = res.productionRange;
            newres.productionAmount = Random.Range(newres.productionRange.x, newres.productionRange.y);
            res.productionAmount += newres.productionAmount;
            newres.price = res.price;

            producingResources.Add(newres);
        }

        for (int i = 0; i < resourcesManager.allResources.Count; i++)
        {
            Resource res = resourcesManager.allResources[i];

            Resource storeRes = new Resource();
            storeRes.name = res.name;
            storeRes.sprite = res.sprite;
            storeRes.price = res.price;

            storage.Add(storeRes);
        }
    }

    void Produce()
    {
        for (int i = 0; i < producingResources.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                if (producingResources[i].name == storage[x].name)
                {
                    float popToProdMult = island.GetPopToProdMultiplier();
                    int prodAmount = Mathf.FloorToInt(producingResources[i].productionAmount * popToProdMult);

                    if (prodAmount <= 1)
                        prodAmount = 1;

                    storage[x].productionRange = producingResources[i].productionRange;
                    storage[x].productionAmount = prodAmount;
                    storage[x].amountInStorage += prodAmount;
                    producingResources[i].amountInStorage += prodAmount;
                    resourcesManager.allResources[x].amountInStorage += prodAmount;
                }
            }
        }

        UpdateAvailableResourcesInStorage(true);
    }

    public void UpdateAvailableResourcesInStorage(bool updateWidgets)
    {
        availableResourcesInStorage.Clear();

        for (int i = 0; i < storage.Count; i++)
        {
            if (storage[i].amountInStorage > 0)
            {
                storage[i].price = Mathf.FloorToInt((float)(resourcesManager.storage[i].price * resourcesManager.storage[i].amountInStorage) / (float)storage[i].amountInStorage);
                availableResourcesInStorage.Add(storage[i]);
            }
            else if (storage[i].amountInStorage <= 0)
            {
                storage[i].price = resourcesManager.storage[i].price * resourcesManager.pricesMultiplier;
            }
        }

        availableResourcesInStorage.Sort((x, y) => { return x.price.CompareTo(y.price); });

        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].amountInStorage;

        if (!updateWidgets)
            return;

        foreach (Transform t in resourceWidgetController.widgetsPanel.transform)
            Destroy(t.gameObject);

        resourceWidgetController.widgets.Clear();

        for (int i = 0; i < availableResourcesInStorage.Count; i++)
        {
            resourceWidgetController.CreateWidget(availableResourcesInStorage[i]);
        }
    }
}

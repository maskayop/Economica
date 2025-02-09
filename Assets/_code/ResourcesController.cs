using System.Collections.Generic;
using UnityEngine;

public class ResourcesController : MonoBehaviour
{
    public Vector2Int industriesAmountRange;
    public int industriesCount;

    public List<Resource> producingResources = new List<Resource>();
    public List<Article> storage = new List<Article>();
    public List<Article> availableInStorage = new List<Article>();
    public int totalAvailableResources = 0;

    public Vector2 spoilingFactor = new Vector2(0.85f, 0.95f);

    [HideInInspector] public Island island;

    [Space(20)]
    public ResourceWidgetsController resourceWidgetsController;

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
        UpdateAvailableInStorage(true);
        currentDay = GlobalTimeController.Instance.currentDay;
        CreateWidgets();
    }

    void CalculateIndustries()
    {
        industriesCount = Random.Range(industriesAmountRange.x, industriesAmountRange.y + 1);

        for (int i = 0; i < industriesCount; i++)
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

            Article storeArticle = new Article();
            storeArticle.name = res.name;
            storeArticle.sprite = res.sprite;
            storeArticle.price = res.price;

            storage.Add(storeArticle);
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

                    storage[x].amountInStorage += prodAmount;
                    producingResources[i].amountInStorage += prodAmount;
                    resourcesManager.allResources[x].amountInStorage += prodAmount;
                }
            }
        }

        for (int i = 0; i < availableInStorage.Count; i++)
        {
            availableInStorage[i].amountInStorage *= Mathf.CeilToInt(Random.Range(spoilingFactor.x, spoilingFactor.y));
        }

        UpdateAvailableInStorage(true);
    }

    public void UpdateAvailableInStorage(bool updateWidgets)
    {
        availableInStorage.Clear();

        for (int i = 0; i < storage.Count; i++)
        {
            if (storage[i].amountInStorage > 0)
            {
                storage[i].price = Mathf.FloorToInt(
                    (float)(resourcesManager.storage[i].price * resourcesManager.storage[i].amountInStorage) / (float)storage[i].amountInStorage
                    );
                availableInStorage.Add(storage[i]);
            }
            else if (storage[i].amountInStorage <= 0)
            {
                storage[i].price = resourcesManager.storage[i].price * resourcesManager.pricesMultiplier;
            }
        }

        availableInStorage.Sort((x, y) => { return x.price.CompareTo(y.price); });

        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].amountInStorage;

        if (updateWidgets)
            UpdateWidgets();
    }

    void CreateWidgets()
    {
        resourceWidgetsController.Init();

        for (int x = 0; x < storage.Count; x++)
            resourceWidgetsController.CreateWidget(storage[x]);
    }

    void UpdateWidgets()
    {
        for (int i = 0; i < resourceWidgetsController.widgets.Count; i++)
        {
            resourceWidgetsController.widgets[i].amount = 0;
            resourceWidgetsController.widgets[i].price = 0;

            for (int r = 0; r < availableInStorage.Count; r++)
            {
                if (resourceWidgetsController.widgets[i].articleName == availableInStorage[r].name)
                {
                    resourceWidgetsController.widgets[i].amount = availableInStorage[r].amountInStorage;
                    resourceWidgetsController.widgets[i].price = availableInStorage[r].price;
                }
            }
        }

        resourceWidgetsController.UpdateWidgets();
    }    
}

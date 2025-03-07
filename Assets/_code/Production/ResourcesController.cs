using System.Collections.Generic;
using UnityEngine;

public class ResourcesController : MonoBehaviour
{
    public long money;
    public int maintenance;

    [Range(0, 1)] public float pricesSpread = 0;
    [Range(0, 0.1f)] public float pricesSpreadRandomFactor = 0;

    [Space(20)]
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

    ResourcesManager resManager;
    int currentDay = 0;
    
    public float prodMultiplier = 1;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            money -= maintenance;

            if (money < 0)
                prodMultiplier /= 2f;
            else
                prodMultiplier = 1;

            Produce();
            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }
    void Init()
    {
        resManager = ResourcesManager.Instance;
        CalculateIndustries();
        UpdateAvailableInStorage(true);
        currentDay = GlobalTimeController.Instance.currentDay;
        CreateWidgets();
        pricesSpread = Random.Range(pricesSpread - pricesSpreadRandomFactor, pricesSpread + pricesSpreadRandomFactor);
    }

    void CalculateIndustries()
    {
        industriesCount = Random.Range(industriesAmountRange.x, industriesAmountRange.y + 1);

        for (int i = 0; i < industriesCount; i++)
        {
            Resource res = resManager.GetRandomResource();
            
            Resource newres = new Resource();
            newres.name = res.name;
            newres.sprite = res.sprite;
            newres.prodRange = res.prodRange;
            newres.prodAmountBase = Random.Range(newres.prodRange.x, newres.prodRange.y);
            res.prodAmountBase += newres.prodAmountBase;

            producingResources.Add(newres);
        }

        for (int i = 0; i < resManager.allResources.Count; i++)
        {
            Resource res = resManager.allResources[i];

            Article storeArticle = new Article();
            storeArticle.name = res.name;
            storeArticle.sprite = res.sprite;

            storage.Add(storeArticle);
        }
    }

    void Produce()
    {
        float popToProdMult = 0;
        int prodAmount = 0;

        for (int i = 0; i < producingResources.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                if (producingResources[i].name == storage[x].name)
                {
                    popToProdMult = island.GetPopToProdMultiplier();
                    prodAmount = Mathf.FloorToInt(producingResources[i].prodAmountBase * popToProdMult * prodMultiplier);

                    if (prodAmount <= 1)
                        prodAmount = 1;

                    producingResources[i].prodAmountActual = prodAmount;
                    producingResources[i].totalProduced += prodAmount;
                    storage[x].inStorage += prodAmount;
                    resManager.allResources[x].totalProduced += prodAmount;
                    resManager.storage[x].inStorage += prodAmount;

                    CalculateArticlePrice(x);

                    money -= Mathf.FloorToInt(resManager.storage[x].price * prodAmount * prodMultiplier);
                }
            }
        }

        for (int i = 0; i < availableInStorage.Count; i++)
            availableInStorage[i].inStorage = Mathf.CeilToInt(
                Random.Range(availableInStorage[i].inStorage * spoilingFactor.x, availableInStorage[i].inStorage * spoilingFactor.y)
                );

        UpdateAvailableInStorage(true);
    }

    public void UpdateAvailableInStorage(bool updateWidgets)
    {
        availableInStorage.Clear();

        for (int i = 0; i < storage.Count; i++)
        {

            if (storage[i].inStorage != 0)
            {
                CalculateArticlePrice(i);
                availableInStorage.Add(storage[i]);
            }
        }

        availableInStorage.Sort((x, y) => { return x.price.CompareTo(y.price); });

        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].inStorage;

        if (updateWidgets)
            UpdateWidgets();
    }

    void CalculateArticlePrice(int id)
    {
        float lerp = (float)storage[id].inStorage / (float)resManager.storage[id].inStorage;
        storage[id].price = Mathf.CeilToInt(Mathf.Lerp((float)resManager.pricesMultiplier * (float)resManager.storage[id].price, (float)resManager.storage[id].price, lerp));
    }

    public void GoShoping(int customers, bool isStarving)
    {
        if (customers == 0)
            return;

        UpdateAvailableInStorage(false);

        island.shoppedTotal = 0;
        int shopped = 0;

        for (int i = 0; i < availableInStorage.Count; i++)
        {
            if (availableInStorage[i].inStorage != 0 && island.shoppedTotal < customers)
            {
                shopped = availableInStorage[i].inStorage;

                if (shopped + island.shoppedTotal <= customers)
                {
                    money += Mathf.FloorToInt(availableInStorage[i].price * shopped * (1 + pricesSpread));
                    availableInStorage[i].inStorage -= shopped;

                    island.shoppedTotal += shopped;
                }
                else if (shopped + island.shoppedTotal > customers)
                {
                    money += Mathf.FloorToInt(availableInStorage[i].price * (customers - island.shoppedTotal) * (1 + pricesSpread));
                    availableInStorage[i].inStorage -= customers - island.shoppedTotal;

                    island.shoppedTotal = customers;
                }
            }
            else
                break;
        }

        resManager.UpdateStorage();
        resManager.UpdateMoney();
        UpdateAvailableInStorage(true);
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
                    resourceWidgetsController.widgets[i].amount = availableInStorage[r].inStorage;
                    resourceWidgetsController.widgets[i].price = availableInStorage[r].price;
                }
            }
        }

        resourceWidgetsController.UpdateWidgets();
    }    
}

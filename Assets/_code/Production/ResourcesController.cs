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

    ResourcesManager resMan;
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
        resMan = ResourcesManager.Instance;
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
            Resource res = resMan.GetRandomResource();
            
            Resource newres = new Resource();
            newres.name = res.name;
            newres.sprite = res.sprite;
            newres.prodRange = res.prodRange;
            newres.prodAmountBase = Random.Range(newres.prodRange.x, newres.prodRange.y);
            res.prodAmountBase += newres.prodAmountBase;

            producingResources.Add(newres);
        }

        for (int i = 0; i < resMan.allResources.Count; i++)
        {
            Resource res = resMan.allResources[i];

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
                    resMan.allResources[x].totalProduced += prodAmount;
                    resMan.storage[x].inStorage += prodAmount;

                    storage[x].price = Mathf.CeilToInt((float)(resMan.storage[x].price * resMan.storage[x].inStorage) / (float)storage[x].inStorage);

                    money -= Mathf.FloorToInt(resMan.storage[x].price * prodAmount * prodMultiplier);
                }
            }
        }

        for (int i = 0; i < availableInStorage.Count; i++)
            availableInStorage[i].inStorage = Mathf.CeilToInt(
                Random.Range(availableInStorage[i].inStorage * spoilingFactor.x, availableInStorage[i].inStorage * spoilingFactor.y)
                );

        UpdateAvailableInStorage(true);
        //UpdatePrevAvailableInStorage();
    }

    public void UpdateAvailableInStorage(bool updateWidgets)
    {
        availableInStorage.Clear();

        for (int i = 0; i < storage.Count; i++)
        {
            if (storage[i].inStorage > 0)
            {
                storage[i].price = Mathf.CeilToInt((float)(resMan.storage[i].price * resMan.storage[i].inStorage) / (float)storage[i].inStorage);
                availableInStorage.Add(storage[i]);
            }
            else if (storage[i].inStorage <= 0)
            {
                storage[i].price = resMan.pricesMultiplier * resMan.pricesMultiplier;
            }
        }

        availableInStorage.Sort((x, y) => { return x.price.CompareTo(y.price); });

        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].inStorage;

        if (updateWidgets)
            UpdateWidgets();
    }
    /*
    void UpdatePrevAvailableInStorage()
    {
        prevAvailableInStorage.Clear();

        for (int i = 0; i < storage.Count; i++)
        {
            if (storage[i].totalProduced > 0)
            {
                Article storeArticle = new Article();
                storeArticle.name = storage[i].name;
                storeArticle.totalProduced = storage[i].totalProduced;
                storage[i].price = Mathf.CeilToInt((float)(resMan.storage[i].price * resMan.storage[i].totalProduced) / (float)storage[i].totalProduced);
                storeArticle.price = storage[i].price;

                prevAvailableInStorage.Add(storeArticle);
            }
        }
    }
    */

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

        resMan.UpdateStorage();
        resMan.UpdateMoney();
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

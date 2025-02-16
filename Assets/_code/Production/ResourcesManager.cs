using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resource
{
    public string name;
    public Sprite sprite;
    public Vector2Int prodRange;
    public int prodAmountBase;
    public int prodAmountActual;
    public int totalProduced;
}

[System.Serializable]
public class Article
{
    public string name;
    public Sprite sprite;
    public int inStorage;
    public int price;
}

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    public List<Resource> allResources = new List<Resource>();
    public List<Article> storage = new List<Article>();

    public List<Island> allIslands = new List<Island>();

    public int pricesMultiplier = 1000;
    public long totalAvailableResources = 0;
    public int totalIndustries = 0;
    public int totalPopulation = 0;
    public int totalRichPopulation = 0;
    public int totalElitePopulation = 0;
    public long totalMoney = 0;

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

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            currentDay = GlobalTimeController.Instance.currentDay;

            UpdateAllResources();
            UpdateStorage();
            UpdateMoney();

            allIslands.Sort((x, y) => { return x.resCont.totalAvailableResources.CompareTo(y.resCont.totalAvailableResources); });
        }
    }

    public Resource GetRandomResource()
    {
        Resource resource = allResources[Random.Range(0, allResources.Count)];
        return resource;
    }

    void UpdateAllResources()
    {
        for (int i = 0; i < allResources.Count; i++)
            allResources[i].prodAmountActual = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            for (int p = 0; p < allIslands[i].resCont.producingResources.Count; p++)
            {
                for (int x = 0; x < allResources.Count; x++)
                {                
                    if (allResources[x].name == allIslands[i].resCont.producingResources[p].name)
                    {
                        allResources[x].prodAmountActual += allIslands[i].resCont.producingResources[p].prodAmountActual;
                    }
                }
            }
        }
    }

    public void UpdateStorage()
    {
        for (int i = 0; i < storage.Count; i++)
            storage[i].inStorage = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                if (storage[x].name == allIslands[i].resCont.storage[x].name)
                {
                    storage[x].inStorage += allIslands[i].resCont.storage[x].inStorage;
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
            Article newArticle = new Article();
            newArticle.name = allResources[i].name;
            newArticle.sprite = allResources[i].sprite;
            newArticle.inStorage = allResources[i].totalProduced;

            storage.Add(newArticle);
        }
    }

    void UpdatePrices()
    {
        if (storage.Count == 0)
            return;

        totalAvailableResources = 0;

        for (int i = 0; i < storage.Count; i++)
            totalAvailableResources += storage[i].inStorage;

        for (int i = 0; i < storage.Count; i++)
        {
            if (totalAvailableResources != 0 && storage[i].inStorage != 0)
                storage[i].price = Mathf.CeilToInt(pricesMultiplier * (float)totalAvailableResources / (float)storage[i].inStorage);
            else
                storage[i].price = pricesMultiplier * pricesMultiplier;
        }
    }

    public void UpdatePopulation()
    {
        totalPopulation = 0;
        totalRichPopulation = 0;
        totalElitePopulation = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            totalPopulation += allIslands[i].population;
            totalRichPopulation += allIslands[i].richPopulation;
            totalElitePopulation += allIslands[i].elitePopulation;
        }
    }

    public void UpdateMoney()
    {
        totalMoney = 0;

        for (int i = 0; i < allIslands.Count; i++)
        {
            totalMoney += allIslands[i].GetComponent<ResourcesController>().money;
        }
    }
}

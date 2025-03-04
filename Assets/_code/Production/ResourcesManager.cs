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

    public int pricesMultiplier = 1000;
    public long totalAvailableResources = 0;
    public int totalIndustries = 0;
    
    public long totalMoney = 0;

    int currentDay = 0;

    IslandsManager islandsManager;

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

        islandsManager = IslandsManager.Instance;
    }

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            currentDay = GlobalTimeController.Instance.currentDay;

            UpdateAllResources();
            UpdateStorage();
            UpdateMoney();
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

        for (int i = 0; i < islandsManager.allIslands.Count; i++)
        {
            for (int p = 0; p < islandsManager.allIslands[i].resCont.producingResources.Count; p++)
            {
                for (int x = 0; x < allResources.Count; x++)
                {                
                    if (allResources[x].name == islandsManager.allIslands[i].resCont.producingResources[p].name)
                    {
                        allResources[x].prodAmountActual += islandsManager.allIslands[i].resCont.producingResources[p].prodAmountActual;
                    }
                }
            }
        }
    }

    public void UpdateStorage()
    {
        for (int i = 0; i < storage.Count; i++)
            storage[i].inStorage = 0;

        for (int i = 0; i < islandsManager.allIslands.Count; i++)
        {
            for (int x = 0; x < storage.Count; x++)
            {
                if (storage[x].name == islandsManager.allIslands[i].resCont.storage[x].name)
                {
                    storage[x].inStorage += islandsManager.allIslands[i].resCont.storage[x].inStorage;
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
            CalculateArticlePrice(i);
    }

    void CalculateArticlePrice(int id)
    {
        float lerp = (float)storage[id].inStorage / (float)totalAvailableResources;
        storage[id].price = Mathf.CeilToInt(Mathf.Lerp((float)pricesMultiplier * storage[id].price, (float)storage[id].price, lerp));
    }

    public void UpdateMoney()
    {
        totalMoney = 0;

        for (int i = 0; i < islandsManager.allIslands.Count; i++)
        {
            totalMoney += islandsManager.allIslands[i].GetComponent<ResourcesController>().money;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Character : MonoBehaviour
{
    public long money;
    public int maintenance;

    [Space(20)]
    [Range(0, 1)] public float scaleSpread = 0;
    [Range(0, 1)] public float speedSpread = 0;
    
	public Vector2Int cargoHoldCapacitySpread;
    public Vector2 spoilingFactor = new Vector2(0.9f, 0.95f);

	[Space(20)]
    public int cargoHoldCapacity = 10;
    public int cargoHold = 0;
    public List<Article> articlesInCargo = new List<Article>();

    [Space(20)]
    public Island startIsland;
    public Island finishIsland;

    public Article forBuying;

    [Space(20)]
    public ResourceWidgetsController resourceWidgetController;

    Article lastBoughtArticle;
    bool canBuy = true;
    int buyIndex = 0;

	CharacterMovement characterMovement;
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
            money -= maintenance;

            for (int i = 0; i < articlesInCargo.Count; i++)
            {
                if (articlesInCargo[i].inStorage != 0)
                {
                    articlesInCargo[i].inStorage = Mathf.CeilToInt(
                        Random.Range(articlesInCargo[i].inStorage * spoilingFactor.x, articlesInCargo[i].inStorage *  spoilingFactor.y)
                        );
                    cargoHold = articlesInCargo[i].inStorage;
                }
            }

            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }

    public void Init()
	{
        resourcesManager = ResourcesManager.Instance;
        characterMovement = GetComponent<CharacterMovement>();
        characterMovement.character = this;
        CharactersManager.Instance.allCharacters.Add(this);

        transform.localScale += transform.localScale * Random.Range(-scaleSpread, scaleSpread);

        characterMovement.speed += characterMovement.speed * Random.Range(-speedSpread, speedSpread);
        characterMovement.speed /= transform.localScale.x;

        cargoHoldCapacity = Random.Range(cargoHoldCapacitySpread.x, cargoHoldCapacitySpread.y);
        cargoHoldCapacity = Mathf.FloorToInt(cargoHoldCapacity * transform.localScale.x);

        resourceWidgetController.Init();
    }

    public void CalculateNextIsland()
    {
        if (resourcesManager.allIslands.Count <= 1)
        {
            Kill();
            return;
        }

        Island island = null;
        startIsland = finishIsland;
        
        int nextIslandId = 0;
        
        if (cargoHold == 0)
        {
            for (int i = resourcesManager.allIslands.Count - 1; i >= 0; i--)
            {
                int randomValue = Random.Range(-2, 2);
                bool randomBool;

                if (randomValue <= 0)
                    randomBool = false;
                else
                    randomBool = true;

                if (randomBool)
                {
                    nextIslandId = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < resourcesManager.allIslands.Count; i++)
            {
                int randomValue = Random.Range(-2, 2);
                bool randomBool;

                if (randomValue <= 0)
                    randomBool = false;
                else
                    randomBool = true;

                if (randomBool)
                {
                    nextIslandId = i;
                    break;
                }
            }
        }

        island = resourcesManager.allIslands[nextIslandId];

        if (island == startIsland)
        {
            if (nextIslandId + 1 < resourcesManager.allIslands.Count)
                island = resourcesManager.allIslands[nextIslandId + 1];
            else
                island = resourcesManager.allIslands[0];
        }

        finishIsland = island;

        TrySell();

        if (money < 0)
        {
            Kill();
            return;
        }

        TryBuy();
        
        UpdateWidgets();
    }

	public void Kill()
	{
        CharactersManager.Instance.allCharacters.Remove(this);
		Destroy(gameObject);
	}

    void TrySell()
    {
        if (cargoHold == 0)
        {
            canBuy = true;            
            return;
        }

        startIsland.resCont.UpdateAvailableInStorage(false);

        for (int i = 0; i < articlesInCargo.Count; i++)
        {
            for (int x = 0; x < startIsland.resCont.storage.Count; x++)
            {
                if (articlesInCargo[i].name == startIsland.resCont.storage[x].name)
                {
                    if (articlesInCargo[i].price >= startIsland.resCont.storage[x].price)
                    {
                        canBuy = false;
                        return;
                    }

                    Sell(startIsland.resCont.storage[x], articlesInCargo[i]);

                    buyIndex = 0;
                    canBuy = true;
                }
            }
        }

        startIsland.resCont.UpdateAvailableInStorage(true);
    }

    void Sell(Article inStore, Article inCargo)
    {
        int cost = Mathf.FloorToInt(inCargo.inStorage * inStore.price * (1 - startIsland.resCont.pricesSpread));

        if (cost > startIsland.resCont.money)
        {
            canBuy = false;
            return;
        }

        startIsland.resCont.money -= cost;
        money += cost;

        inStore.inStorage += inCargo.inStorage;
        cargoHold = 0;
        inCargo.inStorage = 0;

        startIsland.resCont.UpdateAvailableInStorage(false);
    }

    void TryBuy()
    {
        if (!canBuy)
            return;

        if (startIsland.resCont.availableInStorage.Count <= 1)
            return;

        if (buyIndex >= startIsland.resCont.availableInStorage.Count)
        {
            buyIndex = 0;
            return;
        }

        startIsland.resCont.UpdateAvailableInStorage(false);

        forBuying = startIsland.resCont.availableInStorage[buyIndex];
        
        if (forBuying != null && lastBoughtArticle != null)
        {
            if (forBuying.name == lastBoughtArticle.name)
            {
                if (buyIndex + 1 < startIsland.resCont.availableInStorage.Count)
                    forBuying = startIsland.resCont.availableInStorage[buyIndex + 1];
                else
                    forBuying = startIsland.resCont.availableInStorage[0];
            }

            if (forBuying.name == lastBoughtArticle.name && lastBoughtArticle != null)
                return;
        }

        for (int i = 0; i < finishIsland.resCont.storage.Count; i++)
        {
            if (forBuying.name == finishIsland.resCont.storage[i].name)
            {
                if (forBuying.price >= finishIsland.resCont.storage[i].price)
                {
                    buyIndex++;
                    TryBuy();
                    return;
                }
            }
        }

        bool resourceIsExists = false;

        for (int i = 0; i < articlesInCargo.Count; i++)
        {
            if (articlesInCargo[i].name == forBuying.name)
            {
                resourceIsExists = true;
                break;
            }            
        }

        if (!resourceIsExists)
        {
            Article newArticle = new Article();
            newArticle.name = forBuying.name;
            newArticle.sprite = forBuying.sprite;
            newArticle.inStorage = 0;
            newArticle.price = forBuying.price;

            articlesInCargo.Add(newArticle);
        }

        for (int i = 0; i < articlesInCargo.Count; i++)
        {
            if (articlesInCargo[i].name == forBuying.name && articlesInCargo[i].inStorage == 0)
            {
                Buy(articlesInCargo[i]);
            }
        }

        startIsland.resCont.UpdateAvailableInStorage(true);
        canBuy = true;
    }
    
    void Buy(Article inCargo)
    {
        if (forBuying.inStorage > cargoHoldCapacity)
        {
            int cost = Mathf.FloorToInt(cargoHoldCapacity * inCargo.price * (1 + startIsland.resCont.pricesSpread));

            if (cost > money)
                return;

            money -= cost;
            startIsland.resCont.money += cost;

            inCargo.inStorage += cargoHoldCapacity;
            cargoHold += cargoHoldCapacity;
            forBuying.inStorage -= cargoHoldCapacity;
        }
        else
        {
            int cost = Mathf.FloorToInt(forBuying.inStorage * forBuying.price * (1 + startIsland.resCont.pricesSpread));

            if (cost > money)
                return;

            money -= cost;
            startIsland.resCont.money += cost;

            inCargo.inStorage += forBuying.inStorage;
            cargoHold += forBuying.inStorage;
            forBuying.inStorage = 0;
        }

        lastBoughtArticle = forBuying;
    }    

    void UpdateWidgets()
    {
        if (lastBoughtArticle != null && cargoHold != 0)
        {
            bool widgetIsExists = false;

            for (int i = 0; i < resourceWidgetController.widgets.Count; i++)
            {
                if (resourceWidgetController.widgets[i].articleName == lastBoughtArticle.name)
                {
                    widgetIsExists = true;
                    break;
                }
            }

            if (!widgetIsExists)
                resourceWidgetController.CreateWidget(lastBoughtArticle);
        }

        for (int i = 0; i < resourceWidgetController.widgets.Count; i++)
        {
            for (int r = 0; r < articlesInCargo.Count; r++)
            {
                if (resourceWidgetController.widgets[i].articleName == articlesInCargo[r].name)
                {
                    resourceWidgetController.widgets[i].amount = articlesInCargo[r].inStorage;
                    resourceWidgetController.widgets[i].price = articlesInCargo[r].price;
                }
            }
        }

        resourceWidgetController.UpdateWidgets();
    }
}

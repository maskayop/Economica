using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public long money;
    public int maintenance;

    [Header("Свойства корабля")]
    public Vector2 scaleSpread = new Vector2(0.8f, 1.2f);
    public Vector2 speedSpread = new Vector2(90, 110);
    public Vector2Int cargoHoldCapacitySpread = new Vector2Int(100, 200);
    public Vector2 spoilingFactor = new Vector2(0.9f, 0.95f);

	[Header("Грузовой отсек")]
    public int cargoHoldCapacity = 10;
    public int cargoHold = 0;
    public List<Article> articlesInCargo = new List<Article>();

    [Space(20)]
    public Island startIsland;
    public Island finishIsland;

    [Space(20)]
    public Article forBuying;
    public Article lastSold;

    [Space(20)]
    public ResourceWidgetsController resourceWidgetController;

    [Space(20)]
    public GameObject sellingFX;
    public GameObject buyingFX;

    Article lastBought;
    public bool canBuy = true;
    int buyIndex = 0;

	CharacterMovement characterMovement;
    IslandsManager islandsManager;

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
                        Random.Range(articlesInCargo[i].inStorage * spoilingFactor.x, articlesInCargo[i].inStorage * spoilingFactor.y)
                        );
                    cargoHold = articlesInCargo[i].inStorage;
                }
            }

            currentDay = GlobalTimeController.Instance.currentDay;
        }
    }

    public void Init()
	{
        islandsManager = IslandsManager.Instance;

        characterMovement = GetComponent<CharacterMovement>();
        characterMovement.character = this;
        CharactersManager.Instance.allCharacters.Add(this);

        transform.localScale *= Random.Range(scaleSpread.x, scaleSpread.y);

        characterMovement.speed = Random.Range(speedSpread.x, speedSpread.y);
        characterMovement.speed /= transform.localScale.x;

        cargoHoldCapacity = Random.Range(cargoHoldCapacitySpread.x, cargoHoldCapacitySpread.y);
        cargoHoldCapacity = Mathf.FloorToInt(cargoHoldCapacity * transform.localScale.x);

        resourceWidgetController.Init();
    }

    public void GoTrading()
    {
        if (islandsManager.allIslands.Count <= 1)
        {
            Kill();
            return;
        }
        
        startIsland = finishIsland;

        TrySell();

        if (money < 0)
        {
            Kill();
            return;
        }

        ChooseNextIsland();
        TryBuy();        
        UpdateWidgets();
    }

    void ChooseNextIsland()
    {
        int nextIslandId = 0;
        Island island = null;

        if (cargoHold != 0)
        {
            for (int i = 0; i < islandsManager.allIslands.Count; i++)
            {
                if (IsRandomIslandId())
                {
                    nextIslandId = i;
                    break;
                }
            }
        }
        else
        {
            for (int i = islandsManager.allIslands.Count - 1; i >= 0; i--)
            {
                if (IsRandomIslandId())
                {
                    nextIslandId = i;
                    break;
                }
            }
        }

        island = islandsManager.allIslands[nextIslandId];

        if (island == startIsland)
        {
            if (nextIslandId + 1 < islandsManager.allIslands.Count)
                island = islandsManager.allIslands[nextIslandId + 1];
            else
                island = islandsManager.allIslands[0];
        }

        finishIsland = island;
    }

    bool IsRandomIslandId()
    {
        int randomValue = Random.Range(-2, 2);

        if (randomValue <= 0)
            return false;
        else
            return true;
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

                    if (articlesInCargo[i].inStorage != 0)
                    {
                        Sell(startIsland.resCont.storage[x], articlesInCargo[i]);
                    }
                }
            }
        }

        startIsland.resCont.UpdateAvailableInStorage(true);
    }

    void Sell(Article inStore, Article inCargoHold)
    {
        int cost = Mathf.FloorToInt(inCargoHold.inStorage * inStore.price * (1 - startIsland.resCont.pricesSpread));

        if (cost > startIsland.resCont.money)
        {
            canBuy = false;
            return;
        }

        money += cost;
        startIsland.resCont.money -= cost;

        lastSold.name = inCargoHold.name;
        lastSold.inStorage = inCargoHold.inStorage;
        lastSold.price = inStore.price;

        inStore.inStorage += inCargoHold.inStorage;
        cargoHold = 0;
        inCargoHold.inStorage = 0;

        startIsland.resCont.UpdateAvailableInStorage(false);

        Instantiate(sellingFX, transform);

        buyIndex = 0;
        canBuy = true;
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
        
        if (forBuying != null && lastBought != null)
        {
            if (forBuying.name == lastBought.name)
            {
                if (buyIndex + 1 < startIsland.resCont.availableInStorage.Count)
                    forBuying = startIsland.resCont.availableInStorage[buyIndex + 1];
                else
                    forBuying = startIsland.resCont.availableInStorage[0];
            }

            if (forBuying.name == lastBought.name && lastBought != null)
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

        CreateArticle();

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
    
    void Buy(Article inCargoHold)
    {
        if (forBuying.inStorage > cargoHoldCapacity)
        {
            int cost = Mathf.FloorToInt(cargoHoldCapacity * inCargoHold.price * (1 + startIsland.resCont.pricesSpread));

            if (cost > money)
                return;

            startIsland.resCont.money += cost;
            money -= cost;

            inCargoHold.inStorage += cargoHoldCapacity;
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

            inCargoHold.inStorage += forBuying.inStorage;
            cargoHold += forBuying.inStorage;
            forBuying.inStorage = 0;
        }

        lastBought = forBuying;

        Instantiate(buyingFX, transform);
    }

    void CreateArticle()
    {
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
    }

    void UpdateWidgets()
    {
        if (lastBought != null && cargoHold != 0)
        {
            bool widgetIsExists = false;

            for (int i = 0; i < resourceWidgetController.widgets.Count; i++)
            {
                if (resourceWidgetController.widgets[i].articleName == lastBought.name)
                {
                    widgetIsExists = true;
                    break;
                }
            }

            if (!widgetIsExists)
                resourceWidgetController.CreateWidget(lastBought);
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

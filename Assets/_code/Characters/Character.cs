using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public long money;

    [Space(20)]
    [Range(0, 1)] public float scaleSpread = 0;
    [Range(0, 1)] public float speedSpread = 0;
    
	public Vector2Int cargoHoldCapacitySpread;

	[Space(20)]
    public int cargoHoldCapacity = 10;
    public int cargoHold = 0;
    public List<Article> resourcesInCargoHold = new List<Article>();

    [Space(20)]
    public Island startIsland;
    public Island finishIsland;

    public Article resourceForBuying;

    [Space(20)]
    public ResourceWidgetsController resourceWidgetController;

    Article lastBoughtResource;
    bool canBuy = true;
    int buyIndex = 0;

	CharacterMovement characterMovement;
    ResourcesManager resourcesManager;

    void Start()
	{
		Init();
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

        Sell();

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

        Buy();
        
        UpdateWidgets();
    }

	public void Kill()
	{
        CharactersManager.Instance.allCharacters.Remove(this);
		Destroy(gameObject);
	}

    void Sell()
    {
        if (cargoHold == 0)
        {
            canBuy = true;            
            return;
        }

        startIsland.resCont.UpdateAvailableInStorage(false);

        for (int i = 0; i < resourcesInCargoHold.Count; i++)
        {
            for (int x = 0; x < startIsland.resCont.storage.Count; x++)
            {
                if (resourcesInCargoHold[i].name == startIsland.resCont.storage[x].name)
                {
                    if (resourcesInCargoHold[i].price >= startIsland.resCont.storage[x].price)
                    {
                        canBuy = false;
                        return;
                    }

                    startIsland.resCont.storage[x].amountInStorage += resourcesInCargoHold[i].amountInStorage;
                    startIsland.resCont.money -= resourcesInCargoHold[i].amountInStorage * startIsland.resCont.storage[x].price;
                    money += resourcesInCargoHold[i].amountInStorage * startIsland.resCont.storage[x].price;
                    cargoHold -= resourcesInCargoHold[i].amountInStorage;
                    resourcesInCargoHold[i].amountInStorage -= resourcesInCargoHold[i].amountInStorage;

                    startIsland.resCont.UpdateAvailableInStorage(false);

                    buyIndex = 0;
                    canBuy = true;
                }
            }
        }

        startIsland.resCont.UpdateAvailableInStorage(true);
    }

    void Buy()
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

        resourceForBuying = startIsland.resCont.availableInStorage[buyIndex];
        
        if (resourceForBuying != null && lastBoughtResource != null)
        {
            if (resourceForBuying.name == lastBoughtResource.name)
            {
                if (buyIndex + 1 < startIsland.resCont.availableInStorage.Count)
                    resourceForBuying = startIsland.resCont.availableInStorage[buyIndex + 1];
                else
                    resourceForBuying = startIsland.resCont.availableInStorage[0];
            }

            if (resourceForBuying.name == lastBoughtResource.name && lastBoughtResource != null)
                return;
        }

        for (int i = 0; i < finishIsland.resCont.storage.Count; i++)
        {
            if (resourceForBuying.name == finishIsland.resCont.storage[i].name)
            {
                if (resourceForBuying.price >= finishIsland.resCont.storage[i].price)
                {
                    buyIndex++;
                    Buy();
                    return;
                }
            }
        }

        bool resourceIsExists = false;

        for (int i = 0; i < resourcesInCargoHold.Count; i++)
        {
            if (resourcesInCargoHold[i].name == resourceForBuying.name)
            {
                resourceIsExists = true;
                break;
            }            
        }

        if (!resourceIsExists)
        {
            Article newArticle = new Article();
            newArticle.name = resourceForBuying.name;
            newArticle.sprite = resourceForBuying.sprite;
            newArticle.amountInStorage = 0;
            newArticle.price = resourceForBuying.price;

            resourcesInCargoHold.Add(newArticle);
        }

        for (int i = 0; i < resourcesInCargoHold.Count; i++)
        {
            if (resourcesInCargoHold[i].name == resourceForBuying.name && resourcesInCargoHold[i].amountInStorage == 0)
            {
                if (resourceForBuying.amountInStorage > cargoHoldCapacity)
                {
                    resourcesInCargoHold[i].amountInStorage += cargoHoldCapacity;
                    cargoHold += cargoHoldCapacity;
                    money -= cargoHoldCapacity * resourcesInCargoHold[i].price;
                    startIsland.resCont.money += cargoHoldCapacity * resourcesInCargoHold[i].price;
                    resourceForBuying.amountInStorage -= cargoHoldCapacity;
                }
                else
                {
                    resourcesInCargoHold[i].amountInStorage += resourceForBuying.amountInStorage;
                    cargoHold += resourceForBuying.amountInStorage;
                    money -= resourceForBuying.amountInStorage * resourceForBuying.price;
                    startIsland.resCont.money += resourceForBuying.amountInStorage * resourceForBuying.price;
                    resourceForBuying.amountInStorage -= resourceForBuying.amountInStorage;
                }

                lastBoughtResource = resourceForBuying;
            }
        }

        startIsland.resCont.UpdateAvailableInStorage(true);
        canBuy = true;
    }

    void UpdateWidgets()
    {
        if (lastBoughtResource != null && cargoHold != 0)
        {
            bool widgetIsExists = false;

            for (int i = 0; i < resourceWidgetController.widgets.Count; i++)
            {
                if (resourceWidgetController.widgets[i].articleName == lastBoughtResource.name)
                {
                    widgetIsExists = true;
                    break;
                }
            }

            if (!widgetIsExists)
                resourceWidgetController.CreateWidget(lastBoughtResource);
        }

        for (int i = 0; i < resourceWidgetController.widgets.Count; i++)
        {
            for (int r = 0; r < resourcesInCargoHold.Count; r++)
            {
                if (resourceWidgetController.widgets[i].articleName == resourcesInCargoHold[r].name)
                {
                    resourceWidgetController.widgets[i].amount = resourcesInCargoHold[r].amountInStorage;
                    resourceWidgetController.widgets[i].price = resourcesInCargoHold[r].price;
                }
            }
        }

        resourceWidgetController.UpdateWidgets();
    }
}

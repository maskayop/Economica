using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	CharacterMovement characterMovement;

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

    void Start()
	{
		Init();
	}

	public void Init()
	{
        characterMovement = GetComponent<CharacterMovement>();
        characterMovement.character = this;
        CharactersManager.Instance.allCharacters.Add(this);

        transform.localScale += transform.localScale * Random.Range(-scaleSpread, scaleSpread);
        characterMovement.speed += characterMovement.speed * Random.Range(-speedSpread, speedSpread);

        cargoHoldCapacity = Random.Range(cargoHoldCapacitySpread.x, cargoHoldCapacitySpread.y);
    }

    public Island CalculateNextIsland()
    {
        if (ResourcesManager.Instance.allIslands.Count <= 1)
        {
            Kill();
            return null;
        }

        Island island = null;

        startIsland = finishIsland;

        int randomValue = Random.Range(0, ResourcesManager.Instance.allIslands.Count);
        island = ResourcesManager.Instance.allIslands[randomValue];

        if (island == startIsland)
        {
            if (randomValue + 1 < ResourcesManager.Instance.allIslands.Count)
                island = ResourcesManager.Instance.allIslands[randomValue + 1];
            else
                island = ResourcesManager.Instance.allIslands[0];
        }

        finishIsland = island;

        GoTrading();

        return island;
    }

	public void Kill()
	{
        CharactersManager.Instance.allCharacters.Remove(this);
		Destroy(gameObject);
	}

    void GoTrading()
    {
        Sell();
        Buy();

        startIsland.resourcesController.UpdateAvailableInStorage(true);

        foreach (Transform t in resourceWidgetController.widgetsPanel.transform)
            Destroy(t.gameObject);

        resourceWidgetController.widgets.Clear();

        if(lastBoughtResource != null && cargoHold != 0)
            resourceWidgetController.CreateWidget(lastBoughtResource);
    }

    void Sell()
    {
        if (cargoHold == 0)
        {
            canBuy = true;            
            return;
        }

        for (int i = 0; i < resourcesInCargoHold.Count; i++)
        {
            for (int x = 0; x < startIsland.resourcesController.storage.Count; x++)
            {
                if (resourcesInCargoHold[i].name == startIsland.resourcesController.storage[x].name)
                {
                    if (resourcesInCargoHold[i].price >= startIsland.resourcesController.storage[x].price)
                    {
                        canBuy = false;
                        return;
                    }

                    startIsland.resourcesController.storage[x].amountInStorage += resourcesInCargoHold[i].amountInStorage;
                    cargoHold -= resourcesInCargoHold[i].amountInStorage;
                    resourcesInCargoHold[i].amountInStorage -= resourcesInCargoHold[i].amountInStorage;

                    startIsland.resourcesController.UpdateAvailableInStorage(false);

                    canBuy = true;
                }
            }
        }
    }

    void Buy()
    {
        if (!canBuy)
            return;

        if (startIsland.resourcesController.availableInStorage.Count == 0)
            return;

        int randomValue = Random.Range(0, startIsland.resourcesController.availableInStorage.Count);
        randomValue = 0;
        resourceForBuying = startIsland.resourcesController.availableInStorage[randomValue];
        
        if (resourceForBuying != null && lastBoughtResource != null)
        {
            if (resourceForBuying.name == lastBoughtResource.name)
            {
                if (randomValue + 1 < startIsland.resourcesController.availableInStorage.Count)
                    resourceForBuying = startIsland.resourcesController.availableInStorage[randomValue + 1];
                else
                    resourceForBuying = startIsland.resourcesController.availableInStorage[0];
            }

            if (resourceForBuying.name == lastBoughtResource.name && lastBoughtResource != null)
                return;
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

                    resourceForBuying.amountInStorage -= cargoHoldCapacity;
                }
                else
                {
                    resourcesInCargoHold[i].amountInStorage += resourceForBuying.amountInStorage;
                    cargoHold += resourceForBuying.amountInStorage;

                    resourceForBuying.amountInStorage -= resourceForBuying.amountInStorage;
                }

                lastBoughtResource = resourceForBuying;
            }
        }

        startIsland.resourcesController.UpdateAvailableInStorage(true);
        canBuy = true;
    }
}

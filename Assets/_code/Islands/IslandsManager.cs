using System.Collections.Generic;
using UnityEngine;

public class IslandsManager: MonoBehaviour
{
    public static IslandsManager Instance;

    public List<Island> allIslands = new List<Island>();

    public int totalPopulation = 0;
    public int totalRichPopulation = 0;
    public int totalElitePopulation = 0;

    int currentDay = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create IslandsManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            allIslands.Sort((x, y) => { return x.resCont.totalAvailableResources.CompareTo(y.resCont.totalAvailableResources); });
            currentDay = GlobalTimeController.Instance.currentDay;
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
}

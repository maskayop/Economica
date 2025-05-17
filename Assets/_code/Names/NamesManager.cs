using System.Collections.Generic;
using UnityEngine;

public class NamesManager : MonoBehaviour
{
    public static NamesManager Instance;

    public List<string> islandsNames = new List<string>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create NamesManager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        islandsNames.Shuffle();
    }
}

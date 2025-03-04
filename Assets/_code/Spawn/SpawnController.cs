using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Instance;

    public GameObject characterPrefab;
    public List<Spawner> spawners = new List<Spawner>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create SpawnController");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void CreateCharacter()
    {
        spawners[Random.Range(0, spawners.Count)].CreateCharacter(characterPrefab);
    }
}

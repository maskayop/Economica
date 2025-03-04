using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Island island;

    Character character;
    CharacterMovement characterMovement;

    void Start()
    {
        SpawnController.Instance.spawners.Add(this);
    }

    public void CreateCharacter(GameObject objectToCreate)
    {
        GameObject gameObject = Instantiate(objectToCreate, island.islandWaypoint.transform.position, Quaternion.identity);
        character = gameObject.GetComponent<Character>();
        characterMovement = gameObject.GetComponent<CharacterMovement>();
        characterMovement.character = character;
        characterMovement.currentCluster = island.waypointCluster;
    }
}

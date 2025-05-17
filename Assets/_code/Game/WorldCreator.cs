using System.Collections.Generic;
using UnityEngine;

public class WorldCreator : MonoBehaviour
{
    public static WorldCreator Instance;

    [SerializeField] int islandsAmount = 10;
    [SerializeField] List<GameObject> islandsPrefabs = new List<GameObject>();

    [Header("Cells Properties")]
    [SerializeField] Vector2Int worldSize = new Vector2Int(10, 10);
    [SerializeField] int cellSize = 100;
    [SerializeField] int spacing = 10;

    GameObject islandsContainer;
    GameObject worldCellsContainer;
    List<Transform> worldCellsTransforms = new List<Transform>();

    List<int> cellsIds = new List<int>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create WorldCreator");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        CreateIslands();
    }

    public void CreateIslands()
    {
        islandsContainer = new GameObject();
        islandsContainer.name = "Islands";

        CreateWorldCells();

        for (int i = 0; i < worldSize.x * worldSize.y; i++)
            cellsIds.Add(i);

        cellsIds.Shuffle();

        if (islandsAmount > cellsIds.Count)
            islandsAmount = cellsIds.Count;

        for (int i = 0; i < islandsAmount; i++)
        {
            int randomIsland = Random.Range(0, islandsPrefabs.Count);

            GameObject newIsland = Instantiate(islandsPrefabs[randomIsland], islandsContainer.transform);
            newIsland.name = NamesManager.Instance.islandsNames[i];            

            newIsland.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            newIsland.transform.position = worldCellsTransforms[cellsIds[i]].position;
            newIsland.transform.position += new Vector3(Random.Range(-cellSize / 2, cellSize / 2), 0, Random.Range(-cellSize / 2, cellSize / 2));

            if (newIsland.GetComponent<Island>())
                newIsland.GetComponent<Island>().Init();
        }
    }

    void CreateWorldCells()
    {
        worldCellsContainer = new GameObject();
        worldCellsContainer.name = "World Cells Container";

        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Cell - " + x + ":" + y;
                cube.transform.position = new Vector3((cellSize + spacing) * x + cellSize / 2, 0, (cellSize + spacing ) * y + cellSize / 2);

                int worldSizeX = worldSize.x * (cellSize + spacing) - spacing;
                int worldSizeY = worldSize.y * (cellSize + spacing) - spacing;

                cube.transform.position -= new Vector3(worldSizeX / 2, 0, worldSizeY / 2);
                cube.transform.localScale = new Vector3(cellSize, 10, cellSize);

                cube.transform.parent = worldCellsContainer.transform;
                cube.GetComponent<MeshRenderer>().enabled = false;
                Destroy(cube.GetComponent<Collider>());

                worldCellsTransforms.Add(cube.transform);
            }
        }
    }
}

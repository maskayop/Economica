using UnityEngine;

public class GlobalTimeController : MonoBehaviour
{
    public static GlobalTimeController Instance;

    public int currentDay = 1;
    public float dayLenght = 10;

    public float currentTime = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create GlobalTimeController");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= dayLenght) 
        {
            currentDay++;
            currentTime = 0;
        }
    }
}

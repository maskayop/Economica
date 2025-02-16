using UnityEngine;

public class GlobalTimeController : MonoBehaviour
{
    public static GlobalTimeController Instance;

    public int currentDay = 1;
    public float dayLenght = 10;
    public float currentTime = 0;

    [Space(20)]
    public bool useTimeDeceleration = true;
    [Range(0.001f, 1f)]
    public float miltiplier = 0.01f;

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

        if(useTimeDeceleration)
            dayLenght = miltiplier * currentDay;

        if (currentTime >= dayLenght) 
        {
            currentDay++;
            currentTime = 0;
        }
    }
}

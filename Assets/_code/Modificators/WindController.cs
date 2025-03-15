using UnityEngine;

public class WindController : MonoBehaviour
{
    public static WindController Instance;

    [SerializeField] WindZone windZone;
    [SerializeField] Vector2Int angleChangingSpread;
    [SerializeField] Vector2 strengthSpread;
    [SerializeField] float changingTime;

    public Quaternion currentRotation;
    public float currentStrength;

    Transform targetTransform;
    float targetStrength;

    Quaternion prevRotation;
    float prevStrength;

    int currentDay = 0;
    float currentValue = 0;
    bool changeValues = false;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Cannot create WindController");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        GameObject targetGameObject = new GameObject();
        targetGameObject.name = "Wind Target Transform";
        targetGameObject.transform.parent = transform;
        targetTransform = targetGameObject.transform;
    }

    void Update()
    {
        ChangeValues();
        
        if (GlobalTimeController.Instance.currentDay != currentDay)
        {
            currentDay = GlobalTimeController.Instance.currentDay;

            ChangeWind();
        }        
    }

    void ChangeWind()
    {
        targetStrength = Random.Range(strengthSpread.x, strengthSpread.y);

        int currentAngleChanging = Random.Range(angleChangingSpread.x, angleChangingSpread.y);
        targetTransform.Rotate(0, currentAngleChanging, 0);

        prevRotation = windZone.transform.rotation;
        prevStrength = currentStrength;

        changeValues = true;
    }

    void ChangeValues()
    {
        if(changeValues == false)
            return;

        if (changingTime > GlobalTimeController.Instance.dayLenght)
            changingTime = GlobalTimeController.Instance.dayLenght;

        if (changeValues)
        {
            currentValue += Time.deltaTime;

            currentRotation = Quaternion.Lerp(prevRotation, targetTransform.rotation, currentValue / changingTime);
            currentRotation = Quaternion.Euler(new Vector3(0, currentRotation.eulerAngles.y, 0));
            windZone.transform.rotation = currentRotation;

            currentStrength = Mathf.Lerp(prevStrength, targetStrength, currentValue / changingTime);
        }
        else
        {
            windZone.transform.rotation = targetTransform.rotation;
            currentStrength = targetStrength;
        }

        windZone.windMain = currentStrength;

        if (currentValue / changingTime >= 1)
        {
            currentValue = 0;
            changeValues = false;
        }
    }

    public float GetNormalizedCurrentStrength()
    {
        return currentStrength / strengthSpread.y;
    }
}

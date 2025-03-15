using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMainCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentDayText;
    [SerializeField] Image clockFill;
    
    [SerializeField] RectTransform windArrow;
    [SerializeField] Image windStrengthFillLeft;
    [SerializeField] Image windStrengthFillRight;

    int currentDay = 0;

    GlobalTimeController globalTime;

    void Start()
    {
        globalTime = GlobalTimeController.Instance;
        currentDayText.text = currentDay.ToString();
    }

    void Update()
    {
        if (globalTime.currentDay != currentDay)
        {
            currentDay = globalTime.currentDay;
            currentDayText.text = currentDay.ToString();
        }

        clockFill.fillAmount = globalTime.currentTime / globalTime.dayLenght;
        windArrow.rotation = Quaternion.Euler(0, 0, -WindController.Instance.currentRotation.eulerAngles.y);
        windStrengthFillLeft.fillAmount = windStrengthFillRight.fillAmount = WindController.Instance.GetNormalizedCurrentStrength() / 2;
    }

    public void GoToCamera(bool isNext)
    { 
        CameraController.Instance.GoToCamera(isNext);
    }

    public void CreateCharacter()
    {
        SpawnController.Instance.CreateCharacter();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMainCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentDayText;
    [SerializeField] Image clockFill;

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
    }
}

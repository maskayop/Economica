using UnityEngine;

public class DisableOnAwake : MonoBehaviour
{
    [SerializeField] bool disableOnAwake = true;

    void Awake()
    {
        if(disableOnAwake)
            gameObject.SetActive(false);
    }
}

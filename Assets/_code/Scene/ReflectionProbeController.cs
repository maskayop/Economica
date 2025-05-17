using UnityEngine;
using UnityEngine.Rendering;

public class ReflectionProbeController : MonoBehaviour
{
    [SerializeField] ReflectionProbe reflectionProbe;
    [SerializeField] float delay;

    float currentTime = 0;

    void Start()
    {
        reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
            currentTime = delay;
        }
        else
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
    }
}

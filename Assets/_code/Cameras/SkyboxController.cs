using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1.0f;

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}

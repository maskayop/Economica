using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public static CameraController Instance;

    public Camera mainCamera;

    [SerializeField] List<CinemachineCamera> virtualCameras = new List<CinemachineCamera>();

    [SerializeField] float movementSpeed = 1.0f;
    [SerializeField] Vector2 positionLimits = new Vector2(100f, 100f);

    [SerializeField] float scrollSpeed = 1;
    [SerializeField] float maxTranslationY = 50;
    [SerializeField] float minTranslationY = 10;
    [SerializeField] float currentZoom = 0.5f;

    int currentCamera;

	Vector2 startMousePosition;
	Vector2 currentMousePosition;
	Vector2 mousePositionOffset;

	Vector3 startCameraPosition;

	Vector3 currentCameraRotation;
	float cos;
	float sin;

	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogWarning("Cannot create CameraController");
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	void Update()
	{
		MoveCamera();
        ZoomView();
    }

	void MoveCamera()
	{
		if (Input.GetMouseButtonDown(0))
		{
			startMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			startCameraPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		}

		if (Input.GetMouseButton(0))
		{
			currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

			currentCameraRotation = mainCamera.transform.rotation.eulerAngles;
			cos = Mathf.Cos(currentCameraRotation.y * Mathf.PI / 180);
			sin = Mathf.Sin(currentCameraRotation.y * Mathf.PI / 180);

			mousePositionOffset = (startMousePosition - currentMousePosition) * movementSpeed;

			float positionX = mousePositionOffset.x * cos + mousePositionOffset.y * sin;
			float positionZ = mousePositionOffset.y * cos - mousePositionOffset.x * sin;

			positionX = Mathf.Clamp(startCameraPosition.x + positionX, -positionLimits.x, positionLimits.x);
			positionZ = Mathf.Clamp(startCameraPosition.z + positionZ, -positionLimits.y, positionLimits.y);

			transform.position = new Vector3(positionX, startCameraPosition.y, positionZ);
		}

		if (Input.GetMouseButtonUp(0))
		{
			startCameraPosition = transform.position;
		}
	}

	void SetCamera()
	{
		for (int i = 0; i < virtualCameras.Count; i++)
		{
			if (i == currentCamera)
				virtualCameras[i].Priority = 1;
			else
				virtualCameras[i].Priority = 0;
		}
	}

	public void GoToCamera(bool isNext)
	{
		if (isNext)
			currentCamera++;
		else
			currentCamera--;

		if (currentCamera >= virtualCameras.Count)
			currentCamera = 0;
		else if (currentCamera < 0)
			currentCamera = virtualCameras.Count - 1;

		SetCamera();
	}

    void ZoomView()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            currentZoom -= scrollSpeed;
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            currentZoom += scrollSpeed;

        currentZoom = Mathf.Clamp01(currentZoom);
        transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, minTranslationY, transform.localPosition.z),
                                               new Vector3(transform.localPosition.x, maxTranslationY, transform.localPosition.z), currentZoom);
    }
}

using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	[SerializeField]
	private float time = 1;

	[SerializeField]
	private bool destroyAtStart = false;

	public void DestroyGameObject()
	{
		Destroy(gameObject, time);
	}

	void Start()
	{
		if (destroyAtStart)
			DestroyGameObject();
	}
}

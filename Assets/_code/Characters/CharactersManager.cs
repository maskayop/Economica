using System.Collections.Generic;
using UnityEngine;

public class CharactersManager : MonoBehaviour
{
	public static CharactersManager Instance;
	public List<Character> allCharacters = new();

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogWarning("Cannot create CharactersManager");
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}
}

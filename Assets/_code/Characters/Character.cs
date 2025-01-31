using UnityEngine;

public class Character : MonoBehaviour
{
	CharacterMovement characterMovement;

    [Range(0, 1)]
    public float scaleSpread = 0;

    [Range(0, 1)]
    public float speedSpread = 0;

    void Start()
	{
		characterMovement = GetComponent<CharacterMovement>();
		characterMovement.character = this;
        CharactersManager.Instance.allCharacters.Add(this);

		transform.localScale += transform.localScale * Random.Range(-scaleSpread, scaleSpread);
        characterMovement.speed += characterMovement.speed * Random.Range(-speedSpread, speedSpread);
	}

	void Update() { }

	public void Kill()
	{
        CharactersManager.Instance.allCharacters.Remove(this);
		Destroy(gameObject);
	}
}

using UnityEngine;

public class RemoteCharacter : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer _spriteRenderer;

    public int Id
    {
        get; set;
    }

    void Start()
    {
        _spriteRenderer.color = Color.HSVToRGB(Random.Range(0.1f, 0.9f), 1f, 1f);

	}
}

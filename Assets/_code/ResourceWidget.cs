using UnityEngine;

public class ResourceWidget : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public string articleName;
    public int amount;
    public int price;

    public void ChangeSprite(Sprite s)
    {
        if(s != null)
            meshRenderer.material.SetTexture("_BaseMap", s.texture);
    }
}

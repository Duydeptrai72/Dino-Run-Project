using UnityEngine;

public class Parallax : MonoBehaviour
{
    private Material material;
    [SerializeField]
    private float parallaxFactor = 0.1f;
    private float offset;
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        ParallaxScroll();
    }
    private void ParallaxScroll()
    {
        float speed = GameManager.instance.GetGameSpeed() * parallaxFactor;
        offset += Time.deltaTime * speed;
        material.SetTextureOffset("_MainTex", Vector2.right*offset);
    }
}

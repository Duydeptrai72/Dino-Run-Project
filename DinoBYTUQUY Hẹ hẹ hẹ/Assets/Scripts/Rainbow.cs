using TMPro;
using UnityEngine;

public class RainbowText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Color[] rainbowColors = new Color[] 
    {
        Color.red, new Color(1f, 0.5f, 0f), Color.yellow, 
        Color.green, Color.blue, new Color(0.3f, 0f, 0.5f)
    };

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float time = Time.time;
        int colorIndex = (int)(time * 2) % rainbowColors.Length;
        int nextColorIndex = (colorIndex + 1) % rainbowColors.Length;
        float lerpValue = (time * 2) % 1f;
        
        textMesh.color = Color.Lerp(rainbowColors[colorIndex], rainbowColors[nextColorIndex], lerpValue);
    }
}
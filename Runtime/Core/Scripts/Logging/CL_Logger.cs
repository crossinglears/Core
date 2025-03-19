using UnityEngine;
using TMPro;

namespace CrossingLears
{
public static class FloatingTextUtility
{
    public static void CreateFloatingText2D(string text, Vector3 position, float duration = 1, float speed = 1f)
    {
        GameObject textObj = new GameObject("FloatingText");
        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 3;
        textMesh.color = Color.white;
        position.z = 0;
        textObj.transform.position = position;
        
        textObj.AddComponent<FloatingText>().Initialize(duration, speed);
    }
}

public class FloatingText : MonoBehaviour
{
    private float speed;

    public void Initialize(float duration, float speed)
    {
        this.speed = speed;
        Destroy(gameObject, duration);
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}
}
using UnityEngine;
using TMPro;

namespace CrossingLears
{
    public static class FloatingTextUtility
    {
        public static void CreateFloatingText2D(string text, Vector3 position, float duration = 1f, float speed = 1f)
        {
            GameObject textObj = new GameObject("FloatingText2D");
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 3;
            textMesh.color = Color.white;
            position.z = 0;
            textObj.transform.position = position;
            
            textObj.AddComponent<FloatingText>().Initialize(duration, speed);
        }
        
        public static void CreateFloatingTextUI(string text, Vector3 position, Transform parent, float duration = 1f, float speed = 50f)
        {
            GameObject textObj = new GameObject("FloatingTextUI");
            textObj.transform.SetParent(parent, false);
            
            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = text;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 24;
            textMesh.color = Color.white;
            
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            
            textObj.AddComponent<FloatingText>().Initialize(duration, speed);
        }
    }
    
    public class FloatingText : MonoBehaviour
    {
        private float speed;
        private float duration;
        private float timeElapsed;

        public void Initialize(float duration, float speed)
        {
            this.duration = duration;
            this.speed = speed;
        }

        void Update()
        {
            timeElapsed += Time.deltaTime;
            transform.position += Vector3.up * speed * Time.deltaTime;
            if (timeElapsed >= duration)
                Destroy(gameObject);
        }
    }
}

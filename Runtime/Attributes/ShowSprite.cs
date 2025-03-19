using UnityEngine;

namespace CrossingLears
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ShowSpriteAttribute))]
    public class ShowSpriteAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowSpriteAttribute attributeData = (ShowSpriteAttribute)attribute;
            float spacing = 6f;

            Rect objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                (property.objectReferenceValue != null && property.objectReferenceValue is not Sprite))
            {
                EditorGUI.LabelField(objectFieldRect, label.text, "Use with Sprite fields only");
                return;
            }

            if (property.objectReferenceValue is Sprite sprite)
            {
                float thumbnailWidth = attributeData.displayType == ShowSpriteAttribute.DisplayType.Adaptive
                    ? position.width * (attributeData.size / 100f)
                    : attributeData.size;

                float aspectRatio = sprite.rect.width / sprite.rect.height;
                float thumbnailHeight = thumbnailWidth / aspectRatio;

                Rect previewRect = new Rect(
                    position.x + (position.width - thumbnailWidth) / 2, // Centering the sprite preview
                    position.y,
                    thumbnailWidth,
                    thumbnailHeight
                );

                objectFieldRect.y += thumbnailHeight + spacing;

                Texture2D spriteTexture = sprite.texture;
                Rect spriteRect = sprite.rect;
                
                Rect uvRect = new Rect(
                    spriteRect.x / spriteTexture.width,
                    spriteRect.y / spriteTexture.height,
                    spriteRect.width / spriteTexture.width,
                    spriteRect.height / spriteTexture.height
                );

                GUI.DrawTextureWithTexCoords(previewRect, spriteTexture, uvRect);
            }

            EditorGUI.PropertyField(objectFieldRect, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return EditorGUIUtility.singleLineHeight; // Return default height for invalid types
        
            ShowSpriteAttribute attributeData = (ShowSpriteAttribute)attribute;
            float spacing = 6f;
            float thumbnailWidth = attributeData.displayType == ShowSpriteAttribute.DisplayType.Adaptive
                ? EditorGUIUtility.currentViewWidth * (attributeData.size / 100f)
                : attributeData.size;

            float thumbnailHeight = property.objectReferenceValue is Sprite sprite
                ? thumbnailWidth / (sprite.rect.width / sprite.rect.height)
                : 0;

            return property.objectReferenceValue is Sprite
                ? thumbnailHeight + EditorGUIUtility.singleLineHeight + spacing
                : EditorGUIUtility.singleLineHeight;
        }
    }
#endif

    public class ShowSpriteAttribute : PropertyAttribute
    {
        public float size;
        public DisplayType displayType;

        public enum DisplayType
        {
            Fixed,    // Uses the size as constant pixel value
            Adaptive  // Uses the size as percentage of available width
        }

        public ShowSpriteAttribute(float size = 50f, DisplayType displayType = DisplayType.Fixed)
        {
            this.size = size;
            this.displayType = displayType;
        }
    }
}
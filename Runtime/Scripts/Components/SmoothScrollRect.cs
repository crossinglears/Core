
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothScrollRect : ScrollRect
{
    public override void OnScroll(PointerEventData data)
    {
        if (!IsActive())
            return;

        UpdateBounds();

        Vector2 delta = data.scrollDelta;
        // Down is positive for scroll events, while in UI system up is positive.
        delta.y *= -1;
        if (vertical && !horizontal)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                delta.y = delta.x;
            delta.x = 0;
        }
        if (horizontal && !vertical)
        {
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                delta.x = delta.y;
            delta.y = 0;
        }

        Vector2 position = content.anchoredPosition;
        position += delta * scrollSensitivity;

        NewSet(position);
        UpdateBounds();
    }

    protected void NewSet(Vector2 position)
    {
        if (!horizontal)
            position.x = content.anchoredPosition.x;
        if (!vertical)
            position.y = content.anchoredPosition.y;

        if (position != content.anchoredPosition)
        {
            // content.anchoredPosition = position;
            if (chasing > 0)
            {
                position = ToChase + position - content.anchoredPosition;
            }
            chasing = ChaseTime;
            ToChase = position;
            UpdateBounds();
        }
    }

    public float Speed = 3, ChaseTime = 0.3f;
    protected override void LateUpdate()
    {
        if (chasing > 0)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, ToChase, Time.unscaledDeltaTime * Speed);
            chasing -= Time.unscaledDeltaTime;

            UpdateBounds();
        }
        base.LateUpdate();
    }

    float chasing = 0;
    Vector2 ToChase;


    public static void ReplaceWithSmoothScrollRect(GameObject target)
    {
        ScrollRect original = target.GetComponent<ScrollRect>();
        if (original == null)
            return;

        RectTransform content = original.content;
        Scrollbar horizontalScrollbar = original.horizontalScrollbar;
        Scrollbar verticalScrollbar = original.verticalScrollbar;
        MovementType movementType = original.movementType;
        ScrollbarVisibility horizontalVisibility = original.horizontalScrollbarVisibility;
        ScrollbarVisibility verticalVisibility = original.verticalScrollbarVisibility;
        float scrollSensitivity = original.scrollSensitivity;
        bool horizontal = original.horizontal;
        bool vertical = original.vertical;
        Vector2 normalizedPosition = original.normalizedPosition;
        RectTransform viewPort = original.viewport;
        float verticalScrollbarSpacing = original.verticalScrollbarSpacing;
        float horizontalScrollbarSpacing = original.horizontalScrollbarSpacing;
        float elasticity = original.elasticity;
        var inertia = original.inertia;
        var decelerationRate = original.decelerationRate;

        DestroyImmediate(original);

        SmoothScrollRect smooth = target.AddComponent<SmoothScrollRect>();
        smooth.content = content;
        smooth.horizontalScrollbar = horizontalScrollbar;
        smooth.verticalScrollbar = verticalScrollbar;
        smooth.movementType = movementType;
        smooth.horizontalScrollbarVisibility = horizontalVisibility;
        smooth.verticalScrollbarVisibility = verticalVisibility;
        smooth.scrollSensitivity = scrollSensitivity;
        smooth.horizontal = horizontal;
        smooth.vertical = vertical;
        smooth.normalizedPosition = normalizedPosition;
        smooth.viewport = viewPort;
        smooth.verticalScrollbarSpacing = verticalScrollbarSpacing;
        smooth.horizontalScrollbarSpacing = horizontalScrollbarSpacing;
        smooth.elasticity = elasticity;
        smooth.inertia = inertia;
        smooth.decelerationRate = decelerationRate;
    }
}

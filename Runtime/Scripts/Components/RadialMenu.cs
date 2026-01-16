using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [Header("Radial Settings")]
    [Tooltip("Base radius of the radial menu.")]
    [SerializeField] private float Radius = 100f;

    [Tooltip("Additional start angle applied per child.")]
    [SerializeField] private float StartAngleBonusPerChild = 45f;

    [Tooltip("Base starting angle of the first child.")]
    [SerializeField] private float StartAngle = 0f;

    [Tooltip("Smoothing speed for child movement.")]
    [SerializeField] private float SmoothSpeed = 10f;

    [Header("Child Settings")]
    [Tooltip("Approximate radius of each child element. Used to prevent overlap.")]
    [SerializeField] private float ChildRadius = 20f;

    // Total starting angle including per-child bonus
    float FinalStartAngle => StartAngle + StartAngleBonusPerChild * transform.childCount;

    private void Update()
    {
        SortChildren();
    }

    /// <summary>
    /// Arranges all child RectTransforms in a circular layout.
    /// Automatically adjusts radius if children would overlap.
    /// </summary>
    private void SortChildren()
    {
        int childCount = transform.childCount;
        if (childCount == 0)
        {
            return;
        }

        float step = 360f / childCount;

        // Adjust radius if children overlap
        float adjustedRadius = Mathf.Max(Radius, (childCount * ChildRadius) / Mathf.PI);

        for (int i = 0; i < childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (rect == null)
            {
                continue;
            }

            float angle = (FinalStartAngle + step * i) * Mathf.Deg2Rad;

            Vector2 targetPosition = new Vector2(
                Mathf.Cos(angle) * adjustedRadius,
                Mathf.Sin(angle) * adjustedRadius
            );

            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                targetPosition,
                SmoothSpeed * Time.deltaTime
            );
        }
    }
}

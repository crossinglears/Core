
using UnityEngine;

public class ClosedOnDisable : MonoBehaviour
{
    void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
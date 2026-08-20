using UnityEngine;

namespace CrossingLears
{
    public class ClosedOnDisable : MonoBehaviour
    {
        void OnDisable()
        {
            gameObject.SetActive(false);
        }
    }
}

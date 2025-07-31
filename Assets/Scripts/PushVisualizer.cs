using UnityEngine;

public class PushVisualizer : MonoBehaviour
{
    [SerializeField] private float moveDistance = default;

    public void Move()
    {
        transform.position += transform.right * moveDistance;
        Debug.Log($"Moved {moveDistance} forward for visualization");
    }
}

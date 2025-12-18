using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    // LateUpdate guarantees the player has finished moving for the frame
    // before the camera snaps to the new position.
    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
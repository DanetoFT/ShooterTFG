using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;

    void Update()
    {
        transform.position = cameraPos.position;
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float smoothFactor = 1f;
    
    List<Transform> targets = null;

    Camera cam = null;

    void LateUpdate()
    {
        int count = targets.Count;
        Vector3 center = Vector3.zero;
        Bounds bounds = new Bounds();

        foreach (var t in targets)
        {
            center += t.position;
            bounds.Encapsulate(t.position);
        }
        center /= count;
        center = new Vector3
        (
            x: center.x,
            y: center.y,
            z: -10f
        );
        var cameraSize = Mathf.Max(bounds.size.x, bounds.size.y) / 2;

        transform.position = Vector3.Lerp(transform.position, center, smoothFactor);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, cameraSize, smoothFactor);
    }

    void Start()
    {
        targets = new List<Transform>(GameObject.FindObjectsOfType<SteeringActor>().Select(sa => sa.transform));
        targets.AddRange(GameObject.FindObjectsOfType<PlayerController>().Select(pc => pc.transform));
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
    }
}

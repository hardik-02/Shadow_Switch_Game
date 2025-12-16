using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    bool toB = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            // create local A/B if missing
            pointA = new GameObject(gameObject.name + "_PointA").transform;
            pointB = new GameObject(gameObject.name + "_PointB").transform;
            pointA.position = transform.position + transform.right * -3f;
            pointB.position = transform.position + transform.right * 3f;
        }
    }

    void Update()
    {
        Transform target = toB ? pointB : pointA;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.05f) toB = !toB;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.collider.transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.collider.transform.SetParent(null);
        }
    }
}
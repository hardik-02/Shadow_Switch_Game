using UnityEngine;

public class KillPlane : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            GameManager3D.Instance?.PlayerHit();
        }
    }

    // Or if using trigger:
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager3D.Instance?.PlayerHit();
    }
}

using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;

        if (GameManager.Instance != null)
            GameManager.Instance.Die();
    }
}

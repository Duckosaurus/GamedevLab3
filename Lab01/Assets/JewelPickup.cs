using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JewelPickup : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;

        if (GameManager.Instance != null)
            GameManager.Instance.ShowVictory();

        gameObject.SetActive(false);
    }
}

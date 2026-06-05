using UnityEngine;
using TMPro;

public class SignDisplay : MonoBehaviour
{
    [SerializeField] private string signKey = "sign_hint_1";
    [SerializeField] private GameObject signUI;
    [SerializeField] private TextMeshProUGUI signText;

    [Header("Fallback texts (used if no Localization package)")]
    [SerializeField] [TextArea] private string fallbackText = "Watch your step!";

    private bool playerInRange;

    void Start()
    {
        if (signUI != null)
            signUI.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (signUI != null)
                signUI.SetActive(!signUI.activeSelf);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;

        playerInRange = true;

        if (signUI != null)
        {
            signUI.SetActive(true);
            if (signText != null)
                signText.text = fallbackText;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() == null) return;

        playerInRange = false;

        if (signUI != null)
            signUI.SetActive(false);
    }
}

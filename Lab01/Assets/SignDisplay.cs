using UnityEngine;
using TMPro;

/// <summary>
/// Shows a floating world-space text above the sign. The label billboards toward
/// the camera and (by default) appears only while the player is near the sign.
/// No manual UI setup needed - the label is created at runtime.
/// </summary>
public class SignDisplay : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string message = "Watch your step!";

    [Header("Label")]
    [SerializeField] private float height = 1.6f;
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private Color textColor = Color.white;
    [Tooltip("If on, the text is always visible. If off, it shows only when the player is near.")]
    [SerializeField] private bool alwaysVisible = false;

    private TextMeshPro label;
    private Transform labelTransform;

    void Start()
    {
        CreateLabel();
        if (label != null)
            label.gameObject.SetActive(alwaysVisible);
    }

    private void CreateLabel()
    {
        GameObject go = new GameObject("SignLabel");
        labelTransform = go.transform;
        labelTransform.SetParent(transform, false);
        labelTransform.localPosition = Vector3.up * height;

        label = go.AddComponent<TextMeshPro>();
        label.text = message;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(5f, 2f);
    }

    void LateUpdate()
    {
        if (label == null || !label.gameObject.activeSelf) return;
        if (Camera.main == null) return;

        // Billboard: always face the camera.
        labelTransform.rotation = Quaternion.LookRotation(
            labelTransform.position - Camera.main.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alwaysVisible) return;
        if (other.GetComponentInParent<CharacterController>() == null) return;
        if (label != null) label.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (alwaysVisible) return;
        if (other.GetComponentInParent<CharacterController>() == null) return;
        if (label != null) label.gameObject.SetActive(false);
    }
}

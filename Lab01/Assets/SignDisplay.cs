using UnityEngine;
using TMPro;

/// <summary>
/// Shows a floating world-space text above the sign. The label is an independent
/// object (so the sign's scale does not shrink the text), billboards toward the
/// camera, and by default is always visible. No manual UI setup needed.
/// </summary>
public class SignDisplay : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string message = "Watch your step!";

    [Header("Label")]
    [SerializeField] private float height = 1.6f;
    [SerializeField] private float fontSize = 6f;
    [SerializeField] private Color textColor = Color.white;
    [Tooltip("If on, the text is always visible. If off, it shows only when the player is near.")]
    [SerializeField] private bool alwaysVisible = true;

    private TextMeshPro label;
    private Transform labelTransform;

    void Start()
    {
        CreateLabel();
    }

    private void CreateLabel()
    {
        GameObject go = new GameObject("SignLabel_" + name);

        // AddComponent<TextMeshPro> swaps the Transform for a RectTransform,
        // so the transform reference must be fetched AFTER this call.
        label = go.AddComponent<TextMeshPro>();
        labelTransform = go.transform;
        labelTransform.position = transform.position + Vector3.up * height;

        label.text = message;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.rectTransform.sizeDelta = new Vector2(6f, 2f);

        // Dark outline so the text is readable against any background.
        label.outlineWidth = 0.2f;
        label.outlineColor = Color.black;

        label.gameObject.SetActive(alwaysVisible);

        Debug.Log($"[SignDisplay] '{name}': label created, message=\"{message}\", " +
                  $"pos={labelTransform.position}, alwaysVisible={alwaysVisible}", this);
    }

    void LateUpdate()
    {
        if (label == null || !label.gameObject.activeSelf) return;

        labelTransform.position = transform.position + Vector3.up * height;

        if (Camera.main != null)
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

    private void OnDestroy()
    {
        if (label != null) Destroy(label.gameObject);
    }
}

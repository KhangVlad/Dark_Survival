using UnityEngine;
using TMPro;
using System.Collections;

public class UIText : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float moveUpDistance = 2f;

    private Coroutine displayCoroutine;
    private Vector3 startPosition;
    private Color originalColor;

    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }

    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
            originalColor = color;
        }
    }

    public void SetWorldPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        startPosition = worldPosition;
    }

    public void LookAtCamera()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            // Flip 180 degrees so text faces the camera correctly
            transform.Rotate(0, 180, 0);
        }
    }

    public void StartCollectionAnimation()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(AnimateAndReturn());
    }

    private IEnumerator AnimateAndReturn()
    {
        float elapsedTime = 0f;
        Vector3 targetPosition = startPosition + Vector3.up * moveUpDistance;

        while (elapsedTime < animationDuration)
        {
            float progress = elapsedTime / animationDuration;

            // Move up
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            // Fade out
            Color currentColor = originalColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReturnToPool();
    }

    public void ResetDamageText()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        if (textMesh != null)
        {
            textMesh.text = "";
            textMesh.color = Color.white;
        }

        originalColor = Color.white;
    }

    private void ReturnToPool()
    {
        TextManager.Instance.ReturnToPool(this);
    }
}
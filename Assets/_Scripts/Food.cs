using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public FoodType Type { get; private set; }

    [Header("Hint Visual")]
    [SerializeField] private float hintPulseSpeed = 5f;
    [SerializeField] private float hintScaleAmount = 0.12f;

    private Vector3 originalScale;
    private Coroutine hintRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        StopHintImmediate();
    }

    public void Init(int x, int y, FoodType type)
    {
        X = x;
        Y = y;
        Type = type;
        gameObject.name = $"Food({Type}, {X},{Y})";
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        gameObject.name = $"Food({Type}, {X},{Y})";
    }

    public void SetHintHighlight(bool active)
    {
        if (active)
        {
            if (hintRoutine == null)
            {
                hintRoutine = StartCoroutine(HintPulseRoutine());
            }
        }
        else
        {
            StopHintImmediate();
        }
    }

    private IEnumerator HintPulseRoutine()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * hintPulseSpeed;
            float pulse = (Mathf.Sin(time) + 1f) * 0.5f;

            transform.localScale = originalScale * (1f + hintScaleAmount * pulse);

            yield return null;
        }
    }

    private void StopHintImmediate()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        transform.localScale = originalScale;
    }

    public IEnumerator MoveTo(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }
}
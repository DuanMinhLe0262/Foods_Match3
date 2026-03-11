using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public FoodType Type { get; private set; }

    public void Init(int x, int y, FoodType type)
    {
        X = x;
        Y = y;
        Type = type;
        gameObject.name = $"Food_{type}_{x}_{y}";
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        gameObject.name = $"Food_{Type}_{x}_{y}";
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
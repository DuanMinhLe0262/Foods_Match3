using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private BoardManager board;

    private Food selectedFood;
    private Vector2 mouseStartPos;
    private bool isDragging;

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnPointerDown();
        }

        if (Mouse.current.leftButton.isPressed && isDragging && selectedFood != null)
        {
            OnDrag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ResetInput();
        }
    }

    private void OnPointerDown()
{
    if (board.IsBusy) return;

    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

    if (hit.collider == null) return;

    Food food = hit.collider.GetComponent<Food>();
    if (food == null) return;

    selectedFood = food;
    mouseStartPos = mouseWorldPos;
    isDragging = true;
}

    private void OnDrag()
    {
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dragDelta = currentMousePos - mouseStartPos;

        float dragThreshold = 0.3f;
        if (dragDelta.magnitude < dragThreshold) return;

        Vector2Int direction = GetDragDirection(dragDelta);
        Food targetFood = board.GetNeighbor(selectedFood.X, selectedFood.Y, direction);

        if (targetFood != null)
        {
            board.TrySwap(selectedFood, targetFood);
        }

        ResetInput();
    }

    private Vector2Int GetDragDirection(Vector2 dragDelta)
    {
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
        {
            return dragDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }

        return dragDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    private void ResetInput()
    {
        selectedFood = null;
        isDragging = false;
    }
}
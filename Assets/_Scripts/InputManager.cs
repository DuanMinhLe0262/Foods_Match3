using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Food selectedFood;
    private Vector2 mouseStartPos;
    private bool isDragging;

    [SerializeField] private BoardManager board;
    [SerializeField] private float dragThreshold = 0.3f;

    private void Awake()
    {
        if (board == null)
        {
            Debug.LogError("BoardManager chưa được gán trong InputManager");
        }
    }

    private void Update()
    {
        if (board == null) return;
        if (Mouse.current == null) return;
        if (board.IsBusy) return;

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

    private void ResetInput()
    {
        selectedFood = null;
        isDragging = false;
    }

    private void OnPointerDown()
    {
        board.NotifyPlayerAction();

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        if (hit == null) return;

        Food food = hit.GetComponent<Food>();
        if (food == null) return;

        selectedFood = food;
        mouseStartPos = mouseWorldPos;
        isDragging = true;
    }

    private void OnDrag()
    {
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dragDelta = currentMousePos - mouseStartPos;

        if (dragDelta.magnitude < dragThreshold) return;

        Vector2Int direction = GetDragDirection(dragDelta);
        Food targetFood = board.GetNeighbor(selectedFood.X, selectedFood.Y, direction);

        if (targetFood != null)
        {
            board.TrySwap(selectedFood, targetFood);
            ResetInput();
        }
    }

    private Vector2Int GetDragDirection(Vector2 dragDelta)
    {
        if (Math.Abs(dragDelta.x) > Math.Abs(dragDelta.y))
        {
            return dragDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }

        return dragDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
    }
}
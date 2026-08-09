using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class DragSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera worldCamera;

    [SerializeField]
    private Canvas selectionCanvas;

    [SerializeField]
    private RectTransform selectionBox;

    [SerializeField]
    private GameObject appleManagerInstance;
    private AppleManager appleManager;
    private AppleRemover appleRemover;

    [Header("Selection Settings")]
    [Tooltip("켜면 Sprite 전체가 사각형 안에 들어와야 선택됩니다.")]
    [SerializeField]
    private bool requireFullyContained = true;

    [Tooltip("새로 드래그할 때 기존 선택을 해제합니다.")]
    [SerializeField]
    private bool clearPreviousSelection = true;

    [Tooltip("이 거리보다 짧은 드래그는 무시합니다.")]
    [SerializeField]
    [Min(0f)]
    private float minimumDragPixels = 5f;

    [Tooltip("UI 위에서 누른 경우 드래그 선택을 시작하지 않습니다.")]
    [SerializeField]
    private bool ignorePointerOverUI = true;

    private readonly List<SelectableSprite> candidates = new List<SelectableSprite>();
    private readonly List<SelectableSprite> selectedItems = new List<SelectableSprite>();

    private RectTransform canvasRect;

    private Vector2 dragStartScreen;
    private bool isDragging;

    private Camera UICamera
    {
        get
        {
            return selectionCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : selectionCanvas.worldCamera;
        }
    }

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (selectionCanvas == null)
        {
            Debug.LogError("Selection Canvas가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        if (selectionBox == null)
        {
            Debug.LogError("Selection Box가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        if (appleManagerInstance == null)
        {
            Debug.LogError("Apple Manager가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        appleRemover = appleManagerInstance.GetComponent<AppleRemover>();
        appleManager = appleManagerInstance.GetComponent<AppleManager>();
        if (appleRemover == null)
        {
            Debug.LogError("Apple Manager에 AppleRemover 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        canvasRect = selectionCanvas.transform as RectTransform;

        /*
         * Screen 좌표를 Canvas 로컬 좌표로 변환한 값을
         * anchoredPosition에 바로 넣기 위한 설정입니다.
         */
        selectionBox.anchorMin = canvasRect.pivot;
        selectionBox.anchorMax = canvasRect.pivot;
        selectionBox.pivot = Vector2.zero;

        selectionBox.gameObject.SetActive(false);

    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return;
        }

        Vector2 currentScreenPosition = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (ignorePointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            BeginDrag(currentScreenPosition);
        }

        if (!isDragging)
        {
            return;
        }

        if (mouse.leftButton.isPressed)
        {
            UpdateSelectionBox(currentScreenPosition);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag(currentScreenPosition);
        }
    }

    private void BeginDrag(Vector2 screenPosition)
    {
        isDragging = true;
        dragStartScreen = screenPosition;

        selectionBox.gameObject.SetActive(true);
        UpdateSelectionBox(screenPosition);
    }

    private void UpdateSelectionBox(Vector2 currentScreenPosition)
    {
        if (!TryConvertToCanvasLocal(dragStartScreen, out Vector2 startLocal) ||
            !TryConvertToCanvasLocal(currentScreenPosition, out Vector2 currentLocal))
        {
            return;
        }

        Vector2 min = Vector2.Min(startLocal, currentLocal);
        Vector2 max = Vector2.Max(startLocal, currentLocal);

        selectionBox.anchoredPosition = min;
        selectionBox.sizeDelta = max - min;
    }

    private void EndDrag(Vector2 currentScreenPosition)
    {
        isDragging = false;
        selectionBox.gameObject.SetActive(false);

        float dragDistance = Vector2.Distance(dragStartScreen, currentScreenPosition);

        if (dragDistance < minimumDragPixels)
        {
            return;
        }

        Rect screenSelectionRect = CreateRect(dragStartScreen, currentScreenPosition);

        SelectObjects(screenSelectionRect);

        int removed = appleRemover.DetermineDeadApples(selectedItems);
        if (removed > 0) appleManager.CheckIsGameOver();
    }

    private void SelectObjects(Rect selectionRect)
    {
        /*
         * 실행 중 활성화된 SelectableSprite 목록을 복사합니다.
         */
        SelectableSprite.CopyActiveItemsTo(candidates);

        if (clearPreviousSelection)
        {
            {
            foreach (var s in selectedItems)
                if (s != null)
                {
                    s.SetSelected(false);
                }
            }
            selectedItems.Clear();
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            SelectableSprite selectable = candidates[i];

            if (selectable == null || !selectable.isActiveAndEnabled)
            {
                continue;
            }

            if (!TryGetScreenRect(selectable.WorldBounds, out Rect spriteRect))
            {
                continue;
            }

            bool selected;

            if (requireFullyContained)
            {
                selected = ContainsRect(selectionRect, spriteRect);
            }
            else
            {
                selected = selectionRect.Overlaps(spriteRect);
            }

            if (!selected)
            {
                continue;
            }

            selectable.SetSelected(true);
            selectedItems.Add(selectable);
        }
    }

    private bool TryGetScreenRect(Bounds worldBounds, out Rect screenRect)
    {
        float z = worldBounds.center.z;

        Vector3 bottomLeft =
            worldCamera.WorldToScreenPoint(
                new Vector3(
                    worldBounds.min.x,
                    worldBounds.min.y,
                    z
                )
            );

        Vector3 topLeft =
            worldCamera.WorldToScreenPoint(
                new Vector3(
                    worldBounds.min.x,
                    worldBounds.max.y,
                    z
                )
            );

        Vector3 bottomRight =
            worldCamera.WorldToScreenPoint(
                new Vector3(
                    worldBounds.max.x,
                    worldBounds.min.y,
                    z
                )
            );

        Vector3 topRight =
            worldCamera.WorldToScreenPoint(
                new Vector3(
                    worldBounds.max.x,
                    worldBounds.max.y,
                    z
                )
            );

        /*
         * 카메라 뒤에 있는 Sprite는 선택하지 않습니다.
         */
        if (bottomLeft.z <= 0f ||
            topLeft.z <= 0f ||
            bottomRight.z <= 0f ||
            topRight.z <= 0f)
        {
            screenRect = default;
            return false;
        }

        float minX = Mathf.Min(
            bottomLeft.x,
            topLeft.x,
            bottomRight.x,
            topRight.x
        );

        float maxX = Mathf.Max(
            bottomLeft.x,
            topLeft.x,
            bottomRight.x,
            topRight.x
        );

        float minY = Mathf.Min(
            bottomLeft.y,
            topLeft.y,
            bottomRight.y,
            topRight.y
        );

        float maxY = Mathf.Max(
            bottomLeft.y,
            topLeft.y,
            bottomRight.y,
            topRight.y
        );

        screenRect = Rect.MinMaxRect(
            minX,
            minY,
            maxX,
            maxY
        );

        return true;
    }

    private bool TryConvertToCanvasLocal(Vector2 screenPosition, out Vector2 localPosition)
    {
        return RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                UICamera,
                out localPosition
            );
    }

    private static Rect CreateRect(Vector2 pointA, Vector2 pointB)
    {
        Vector2 min = Vector2.Min(pointA, pointB);
        Vector2 max = Vector2.Max(pointA, pointB);

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static bool ContainsRect(Rect outer, Rect inner)
    {
        return
            inner.xMin >= outer.xMin &&
            inner.xMax <= outer.xMax &&
            inner.yMin >= outer.yMin &&
            inner.yMax <= outer.yMax;
    }
}
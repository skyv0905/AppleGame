using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GuideBoxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera worldCamera;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform guideBox;

    private RectTransform canvasRect;

    public Vector2 StartPos { get; set; }
    public Vector2 EndPos { get; set; }

    private Camera UICamera
    {
        get
        {
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
    }

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        canvasRect = canvas.transform as RectTransform;

        guideBox.anchorMin = canvasRect.pivot;
        guideBox.anchorMax = canvasRect.pivot;
        guideBox.pivot = Vector2.zero;

        guideBox.gameObject.SetActive(false);
    }

    private void Update()
    {

    }

    public void DisableGuideBox()
    {
        guideBox.gameObject.SetActive(false);
    }

    public void UpdateGuideBox()
    {
        guideBox.gameObject.SetActive(true);

        if (!TryConvertWorldToCanvasLocal(StartPos, out Vector2 startLocal) ||
            !TryConvertWorldToCanvasLocal(EndPos, out Vector2 endLocal))
        {
            return;
        }

        Vector2 min = Vector2.Min(startLocal, endLocal);
        Vector2 max = Vector2.Max(startLocal, endLocal);

        guideBox.anchoredPosition = min;
        guideBox.sizeDelta = max - min;
    }

    private bool TryConvertWorldToCanvasLocal(Vector3 worldPosition, out Vector2 localPosition)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            UICamera,
            out localPosition
        );
    }
}

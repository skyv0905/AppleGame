using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SelectableSprite : MonoBehaviour
{
    private static readonly HashSet<SelectableSprite> activeItems = new HashSet<SelectableSprite>();

    [Header("Renderer")]
    [SerializeField]
    private SpriteRenderer targetRenderer;

    [Header("Selection")]
    [SerializeField]
    private Color selectedColor = Color.yellow;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onSelected;

    [SerializeField]
    private UnityEvent onDeselected;

    [SerializeField]
    private Vector3 radius = new Vector3(0.01f, 0.01f, 0);

    [SerializeField]
    private TMP_Text TextUI;

    private int Value { get; set; }

    public Vector2 Location { get; set; }

    public bool IsSelected { get; private set; }

    public Bounds WorldBounds
    {
        get
        {
            return new Bounds(transform.position - radius, radius * 2);
        }
    }

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
        if (TextUI == null)
        {
            TextUI = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        activeItems.Add(this);
    }

    private void OnDisable()
    {
        activeItems.Remove(this);
    }

    public static void CopyActiveItemsTo(List<SelectableSprite> destination)
    {
        destination.Clear();
        destination.AddRange(activeItems);
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected)
        {
            return;
        }

        IsSelected = selected;
        if (selected)
        {
            onSelected?.Invoke();
        }
        else
        {
            onDeselected?.Invoke();
        }
    }

    public int GetValue()
    {
        return Value;
    }

    public void SetValue(int value)
    {
        Value = value;
        if (TextUI != null)
        {
            TextUI.text = value.ToString();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AppleManager))]
public class AppleRemover : MonoBehaviour
{
    private AppleManager appleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        appleManager = GetComponent<AppleManager>();
    }

    private void SetApplesDead(List<SelectableSprite> killList)
    {
        foreach (var item in killList)
        {
            appleManager.SetDeadAppleOnBoard(item.Location);
            Destroy(item.gameObject);
        }
    }

    public int DetermineDeadApples(List<SelectableSprite> selectedItems)
    {
        int sum = 0;
        List<SelectableSprite> killList = new();
        foreach (var item in selectedItems)
        {
            if (item.isActiveAndEnabled && item.IsSelected)
            {
                killList.Add(item);
                sum += item.GetValue();
            }
        }

        if (sum == appleManager.TargetSum)
        {
            SetApplesDead(killList);
            return killList.Count;
        }

        return 0;
    }
}

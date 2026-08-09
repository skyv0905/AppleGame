using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AppleManager))]
public class AppleCreator : MonoBehaviour
{
    private AppleManager appleManager;

    private void Start()
    {
        appleManager = GetComponent<AppleManager>();

        switch (ModeSelector.Mode)
        {
            case 0:
                CreateAllRandomApples();
                break;

            case 1:
                CreateAllClearableApples();
                break;

            default:
                CreateAllRandomApples();
                break;
        }
    }

    public void CreateAllRandomApples()
    {
        List<List<int>> apples = new(appleManager.NumRows);
        for (int y = 0; y < appleManager.NumRows; y++)
        {
            apples.Add(new List<int>(appleManager.NumCols));
            for (int x = 0; x < appleManager.NumCols; x++)
            {
                Vector2 position = new Vector2(appleManager.StartPos.x + x * appleManager.IntervalX, appleManager.StartPos.y - y * appleManager.IntervalX);
                GameObject appleInstance = Instantiate(appleManager.ApplePrefab, position, Quaternion.identity);
                appleInstance.transform.SetParent(appleManager.AppleParent.transform);
                SelectableSprite selectableSprite = appleInstance.GetComponent<SelectableSprite>();

                int value = Random.Range(1, 10);
                selectableSprite.SetValue(value);
                selectableSprite.Location = new Vector2(x, y);
                apples[y].Add(value);
            }
        }

        appleManager.SetBoard(apples);
    }

    public void CreateAllClearableApples()
    {
        appleManager.TargetSum = 15;
        List<List<GameObject>> apples = new(appleManager.NumRows);
        for (int y = 0; y < appleManager.NumRows; y++)
        {
            apples.Add(new List<GameObject>(appleManager.NumCols));
            for (int x = 0; x < appleManager.NumCols; x++)
            {
                apples[y].Add(null);
            }
        }

        for (int y = 0; y < appleManager.NumRows; y++)
        {
            for (int x = 0; x < appleManager.NumCols; x++)
            {
                Vector2 position = new Vector2(appleManager.StartPos.x + x * appleManager.IntervalX, appleManager.StartPos.y - y * appleManager.IntervalX);
                GameObject appleInstance = Instantiate(appleManager.ApplePrefab, position, Quaternion.identity);
                appleInstance.transform.SetParent(appleManager.AppleParent.transform);
                SelectableSprite selectableSprite = appleInstance.GetComponent<SelectableSprite>();

                int score = Random.Range(1, 10);
                selectableSprite.SetValue(score);
                selectableSprite.Location = new Vector2(x, y);
                apples[y].Add(appleInstance);
            }
        }

        appleManager.SetBoard(apples.Select(inner => inner.Select(apple => apple.GetComponent<SelectableSprite>().GetValue()).ToList()).ToList());
    }
}

using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;

public class AppleManager : MonoBehaviour
{
    public int TargetSum;

    public bool EnableGuides;

    public GameObject ApplePrefab;

    public GameObject AppleParent;

    [SerializeField]
    private GuideBoxController GuideBox;

    public List<List<int>> Apples { get; private set; }

    public readonly Vector2 StartPos = new Vector2(-6.78f, 3.38f);
    public readonly int NumRows = 10;
    public readonly int NumCols = 16;
    public readonly float IntervalX = 0.75f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (AppleParent == null)
        {
            AppleParent = new GameObject("AppleParent");
        }
    }

    public void SetBoard(List<List<int>> apples)
    {
        this.Apples = apples;
        CheckIsGameOver();
    }

    public void SetDeadAppleOnBoard(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        if (y >= 0 && y < Apples.Count && x >= 0 && x < Apples[y].Count)
        {
            Apples[y][x] = 0;
        }
    }

    public void CheckIsGameOver()
    {
        int rows = Apples.Count;
        int cols = Apples[0].Count;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (Apples[y][x] == 0) continue;

                int sum = 0;
                for (int dy = 0; dy < rows - y; dy++)
                {
                    sum += Apples[y + dy][x];
                    if (sum == TargetSum)
                    {
                        if (EnableGuides)
                        {
                            GuideBox.StartPos = new Vector2(StartPos.x + (x - 0.65f) * IntervalX, StartPos.y - (y - 0.6f) * IntervalX);
                            GuideBox.EndPos = new Vector2(StartPos.x + (x + 0.5f) * IntervalX, StartPos.y - (y + dy + 0.7f) * IntervalX);
                            GuideBox.UpdateGuideBox();
                        }
                        Debug.Log($"Location: from {Apples[y][x]}({x}, {y}) to {Apples[y + dy][x]}({x}, {y + dy})");
                        return;
                    }
                    else if (sum > TargetSum)
                    {
                        break;
                    }

                    int sum2 = sum;
                    for (int dx = 1; dx < cols - x; dx++)
                    {
                        for (int y2 = y; y2 <= y + dy; y2++)
                        {
                            sum2 += Apples[y2][x + dx];
                        }
                        if (sum2 == TargetSum)
                        {
                            if (EnableGuides)
                            {
                                GuideBox.StartPos = new Vector2(StartPos.x + (x - 0.65f) * IntervalX, StartPos.y - (y - 0.6f) * IntervalX);
                                GuideBox.EndPos = new Vector2(StartPos.x + (x + dx + 0.5f) * IntervalX, StartPos.y - (y + dy + 0.7f) * IntervalX);
                                GuideBox.UpdateGuideBox();
                            }
                            Debug.Log($"Location: from {Apples[y][x]}({x}, {y}) to {Apples[y + dy][x + dx]}({x + dx}, {y + dy})");
                            return;
                        }
                        else if (sum2 > TargetSum)
                        {
                            break;
                        }
                    }
                }
            }
        }
        GuideBox.DisableGuideBox();
        Debug.Log("No More Apples can be Removed");
    }
}

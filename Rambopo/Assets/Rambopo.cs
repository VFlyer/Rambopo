using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Rambopo : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public SpriteRenderer LeftSpriteR;
    public SpriteRenderer RightSpriteR;

    public KMSelectable SwitchKMS;
    public GameObject SwitchObj;

    public Sprite[] Sprites;

    int[][][] Tables = {
      new int[][] {
      new int[] {8, 0, 3, 6, 2},
      new int[] {9, 7, 3, 5, 8},
      new int[] {8, 0, 5, 2, 7},
      new int[] {1,9, 2, 5, 0},
      new int[] {1, 4, 0,8, 3}
      },
      new int[][] {
      new int[] {11, 17, 16, 14, 13},
      new int[] {11, 15, 10, 17, 14},
      new int[] {18, 12, 19, 11, 15},
      new int[] {16, 12, 10, 19, 15},
      new int[] {13, 11, 10, 18, 16}
      },
      new int[][] {
      new int[] {21, 25, 26, 28, 20},
      new int[] {25, 24, 29, 28, 21},
      new int[] {28, 20, 21, 27, 23},
      new int[] {25, 28, 27, 26, 29},
      new int[] {23, 27, 22, 24, 26}
      },
      new int[][] {
      new int[] {37, 35, 39, 38, 30},
      new int[] {37, 38, 35, 32, 34},
      new int[] {35, 37, 39, 30, 31},
      new int[] {30, 36, 34, 37, 32},
      new int[] {32, 34, 31, 36, 33}
      },
      new int[][] {
      new int[] {41, 43, 42, 49, 45},
      new int[] {44, 42, 41, 40, 45},
      new int[] {49, 44, 45, 42, 47},
      new int[] {48, 43, 40, 44, 46},
      new int[] {46, 41, 47, 49, 44}
      },
      new int[][] {
      new int[] {54, 57, 51, 50, 58},
      new int[] {57, 54, 55, 53, 50},
      new int[] {54, 59, 56, 52, 53},
      new int[] {50, 52, 54, 58, 59},
      new int[] {51, 53, 55, 54, 50}
      },
      new int[][] {
      new int[] {60, 66, 67, 61, 63},
      new int[] {62, 63, 64, 69, 67},
      new int[] {65, 60, 61, 68, 64},
      new int[] {69, 67, 68, 62, 66},
      new int[] {67, 60, 65, 68, 64}
      }
   };
    int[] RatioList = { 11, 12, 13, 21, 23, 31, 32 };

    int LeftCycle;
    int RightCycle;
    int Row;
    int Col;

    int[] SelectedPair = { 0, 0 };

    int LeftFakePairs;
    int RightFakePairs;

    float CommonTime;

    List<int> LeftScreen = new List<int>() { };
    List<int> RightScreen = new List<int>() { };

    int LAnswer;
    int RAnswer;

    Coroutine CyclingL;
    Coroutine CyclingR;

    //int IndexofCycle;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    private bool _switchDir;
    private int _count;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        SwitchKMS.OnInteract += delegate () { SwitchFlip(); return false; };

    }

    void SwitchFlip()
    {
        _switchDir = !_switchDir;
        Audio.PlaySoundAtTransform("Switch" + (_switchDir ? "1" : "2"), transform);
        StartCoroutine(SwitchFlipAnimation(_switchDir ? 0 : 30, _switchDir ? 30 : 0));
        if (LeftSpriteR.sprite == Sprites[LAnswer] && RightSpriteR.sprite == Sprites[RAnswer])
        {
            Solve();
        }
        else
        {
            Strike();
        }
    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
        if (CyclingL != null)
            StopCoroutine(CyclingL);
        if (CyclingR != null)
            StopCoroutine(CyclingR);
        ModuleSolved = true;
    }

    void Strike()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }

    void Start()
    {
        LeftFakePairs = Rnd.Range(4, 7);
        RightFakePairs = Rnd.Range(4, 7);
        Generate();
    }

    void Generate()
    {
        LeftCycle = Rnd.Range(1, 4);
        RightCycle = Rnd.Range(1, 4);
        CommonTime = Rnd.Range(2f, 3.5f);
        Row = Rnd.Range(0, 5);
        Col = Rnd.Range(0, 4);

        if (LeftCycle == RightCycle)
        {
            LeftCycle = 1;
            RightCycle = 1;

            CommonTime = Rnd.Range(.75f, 1f);
        }
        Debug.LogFormat("[Rambopo #{0}] The ratio is {1}/{2}", ModuleId, LeftCycle, RightCycle);
        /*try {
           LeftScreen.Add(Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col]);
           RightScreen.Add(Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col + 1]);
        }
        catch (Exception) {
           Debug.Log(LeftCycle * 10 + RightCycle);
           Debug.Log(Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle));
           throw;
        }*/

        LAnswer = Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col];
        RAnswer = Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col + 1];

        Debug.LogFormat("[Rambopo #{0}] The correct pair should be {1}, {2}", ModuleId, Sprites[LAnswer].name, Sprites[RAnswer].name);

        LeftScreen.Add(Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col]);
        RightScreen.Add(Tables[Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle)][Row][Col + 1]);

        Fakes();
    }

    void Fakes()
    {
        int[] temp = Enumerable.Range(0, 70).ToArray().Shuffle();
        int index = 0;


        while (LeftScreen.Count() != 1 + LeftFakePairs)
        {
            for (int i = 0; i < LeftScreen.Count(); i++)
            {
                if (temp[index] / 10 == LeftScreen[0] / 10 && DuplicateChecker(Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle), temp[index], LeftScreen[i]))
                {
                    continue;
                }
            }
            LeftScreen.Add(temp[index]);
            index++;
        }

        while (RightScreen.Count() != 1 + RightFakePairs)
        {
            for (int i = 0; i < RightScreen.Count(); i++)
            {
                if (temp[index] / 10 == RightScreen[0] / 10 && DuplicateChecker(Array.IndexOf(RatioList, LeftCycle * 10 + RightCycle), temp[index], RightScreen[i]))
                {
                    continue;
                }
            }
            RightScreen.Add(temp[index]);
            index++;
        }

        _count = Rnd.Range(0, 5);
        CyclingL = StartCoroutine(CycleLeft());
        CyclingR = StartCoroutine(CycleRight());
    }

    bool DuplicateChecker(int T, int x, int y)
    {

        foreach (int[] R in Tables[T])
        {
            for (int i = 0; i < 4; i++)
            {
                if (x == R[i] && y == R[i + 1])
                {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator CycleLeft()
    {
        int Count = _count;
        while (true)
        {
            LeftSpriteR.sprite = Sprites[LeftScreen[Count % LeftScreen.Count()]];
            Count++;
            yield return new WaitForSeconds(CommonTime / (float)LeftCycle);
        }
    }

    IEnumerator CycleRight()
    {
        int Count = _count;
        while (true)
        {
            RightSpriteR.sprite = Sprites[RightScreen[Count % RightScreen.Count()]];
            Count++;
            yield return new WaitForSeconds(CommonTime / (float)RightCycle);
        }
    }

    private bool _togglingSwitch;
    private IEnumerator SwitchFlipAnimation(float from, float to)
    {
        while (_togglingSwitch)
            yield return null;
        _togglingSwitch = true;

        var cur = from;
        var stop = false;
        while (!stop)
        {
            cur += 250 * Time.deltaTime * (to > from ? 1 : -1);
            if ((to > from && cur >= to) || (to < from && cur <= to))
            {
                cur = to;
                stop = true;
            }
            SwitchObj.transform.localEulerAngles = new Vector3(90, 0, cur);
            yield return null;
        }
        _togglingSwitch = false;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!ModuleSolved)
        {
            if (LeftSpriteR.sprite == Sprites[LAnswer] && RightSpriteR.sprite == Sprites[RAnswer])
            {
                SwitchKMS.OnInteract();
            }
            else
            {
                yield return true;
            }
        }
    }
}

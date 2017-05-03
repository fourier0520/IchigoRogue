using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueGeneric
{
    static public int Distance(Vector2 v1, Vector2 v2)
    {
        return (int)System.Math.Max(Mathf.Abs((v1 - v2).x), Mathf.Abs((v1 - v2).y));
    }

    static public Vector2 GetUnitVector(Vector2 v)
    {
        if (v.x > 0) v.x = 1;
        if (v.x < 0) v.x = -1;
        if (v.y > 0) v.y = 1;
        if (v.y < 0) v.y = -1;
        return v;
    }

    static public Vector2 GetVectorFromNum(int n)
    {
        switch (n)
        {
            case 1:
                return new Vector2(-1, -1);
            case 2:
                return new Vector2(0, -1);
            case 3:
                return new Vector2(1, -1);
            case 4:
                return new Vector2(-1, 0);
            case 5:
                return new Vector2(0, 0);
            case 6:
                return new Vector2(1, 0);
            case 7:
                return new Vector2(-1, 1);
            case 8:
                return new Vector2(0, 1);
            case 9:
                return new Vector2(1, 1);
            default:
                return new Vector2(0, 0);
        }
    }

    static public int GetNumFromVector(Vector2 v)
    {
        if (v.x > 0 && v.y > 0) return 9;
        if (v.x > 0 && v.y == 0) return 6;
        if (v.x < 0 && v.y < 0) return 1;
        if (v.x == 0 && v.y < 0) return 2;
        if (v.x > 0 && v.y < 0) return 3;
        if (v.x < 0 && v.y == 0) return 4;
        if (v.x == 0 && v.y == 0) return 5;
        if (v.x < 0 && v.y > 0) return 7;
        if (v.x == 0 && v.y > 0) return 8;
        return 5;
    }

    /// <summary>
    /// Rotate Direction
    /// </summary>
    /// <param name="v">Original Direction</param>
    /// <param name="angle">1 means rotate 45deg clockwise (-8,8) </param>
    /// <returns></returns>
    static public Vector2 RotateDirection(Vector2 v, int angle)
    {
        int length = (int)System.Math.Max(Mathf.Abs(v.x), Mathf.Abs(v.y));
        int n = GetNumFromVector(v);

        if (angle < 0) angle += 8;

        for (int i = 0; i < angle; i++)
        {
            if (n == 1) n = 4;
            else if (n == 4) n = 7;
            else if (n == 7) n = 8;
            else if (n == 8) n = 9;
            else if (n == 9) n = 6;
            else if (n == 6) n = 3;
            else if (n == 3) n = 2;
            else if (n == 2) n = 1;
        }
        return GetVectorFromNum (n) * length;
    }

    static public int CalculateKeyNo(int key, int cri, out bool crit)
    {
        int result = 0;
        int dice2d6;
        crit = false;

        do
        {
            dice2d6 = Random.Range(1, 7) + Random.Range(1, 7);
            result += (dice2d6 * (key + 5) / 20);
            if (dice2d6 >= cri) crit = true;
        } while (dice2d6 >= cri);

        return result;
    }

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public static class Extensions
{
    public static string ToShortString(this int value, bool addMultSign = false)
    {
        string result = addMultSign ? "x" : string.Empty;
        if (value >= 1000000000)
        {
            result += DecimalCorrection(((float)value / 1000000000).ToString()) + "B";
        }
        else if (value >= 1000000)
        {
            result += DecimalCorrection(((float)value / 1000000).ToString()) + "M";

        }
        else if (value >= 1000)
        {
            result += DecimalCorrection(((float)value / 1000).ToString()) + "K";

        }
        else
            result += DecimalCorrection(value.ToString());

        return result;
    }
    /// <summary>
    /// float.ToString() data formatter
    /// max 2 decimal after , 
    /// </summary>
    private static string DecimalCorrection(string value)
    {
        string[] parts = value.Split(',');
        if (parts.Length == 1)
            return value;
        return value[..(parts[0].Length + Mathf.Min(parts[1].Length + 1, 3))];// value.Substring(0, parts[0].Length + Mathf.Min(parts[1].Length+1, 3))
    }
    public static string ToShortString(this uint value)
    {
        return ((int)value).ToShortString(true);
    }
}

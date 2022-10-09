// Filename: Extensions.cs
// Author: 0xFirekeeper
// Description: Favorite and most useful and generic extensions.

using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


public static class Extensions
{
    private static System.Random rng = new System.Random();

    // Tested for Screen Space - Overlay Canvas
    public static bool IsPointerOverUI_OverlayCanvas()
    {
        //check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
        //check touch
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                return true;
        }
        return false;
    }

    // Tested for Screen Space - Camera Canvas
    public static bool IsPointerOverUI_AnyCanvas()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    // Simple shuffler, not very random
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Edit this as you wish to give the impression of bigger numbers, for instance:
    // if (num >= 1000000000)
    //     newNum = (num / 1000000000D).ToString("0.##") + "B";
    // else if (num >= 1000000)
    //     newNum = (num / 1000000D).ToString("0.##") + "M";
    // else if (num >= 1000)
    //     newNum = (num / 1000D).ToString("0.##") + "K";
    public static string FormatNumber(this int num, bool usdSign = true)
    {
        // Ensure number has max 3 significant digits (no rounding up can happen)
        bool negative = false;
        if (num < 0)
            negative = true;

        num = System.Math.Abs(num);

        int i = (int)System.Math.Pow(10, (int)System.Math.Max(0, System.Math.Log10(num) - 2));
        num = num / i * i;

        string newNum = num.ToString("#,0");

        if (num >= 1000000000)
            newNum = (num / 1000000000D).ToString("0.##") + "B";
        else if (num >= 1000000)
            newNum = (num / 1000000D).ToString("0.##") + "M";
        else if (num >= 1000)
            newNum = (num / 1000D).ToString("0.##") + "K";

        if (usdSign)
            newNum = "$" + newNum;

        if (negative)
            newNum = "-" + newNum;

        return newNum;
    }

    public static string AddSpaces(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";
        System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if ((char.IsUpper(text[i]) || char.IsNumber(text[i])) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }

    public static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        if (Time.deltaTime < Mathf.Epsilon) return rot;
        // account for double-cover
        var Dot = Quaternion.Dot(rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

        // ensure deriv is tangent
        var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
        deriv.x -= derivError.x;
        deriv.y -= derivError.y;
        deriv.z -= derivError.z;
        deriv.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }

    public static bool HasInternet()
    {
        return !(Application.internetReachability == NetworkReachability.NotReachable);
    }

}
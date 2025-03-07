using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Battle;

public class Utils : MonoBehaviour
{
    public static void Shuffle<T>(T[] arr)  
    {  
        int n = arr.Length;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1); 
            T value = arr[k];  
            arr[k] = arr[n];  
            arr[n] = value;  
        }  
    }

    public static void Shuffle<T>(List<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Random.Range(0, n + 1); 
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static void Shuffle<T>(List<T> list, System.Random rng)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(0, n + 1);
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static void setScene(string scene){
        SceneManager.LoadScene(scene);
    }

    // % operator is strange with negative numbers
    public static int mod(int x, int m) 
    {
        return (x%m + m)%m;
    }

    // get formatted time from seconds
    public static string FormatTime(float s, bool showDecimal = false)
    {
        int minutes = (int)(s/60);
        int seconds = (int)(s % 60);
        int dec = (int)((s * 100) % 100);
        if (showDecimal) {
            return minutes + ":" + (seconds+"").PadLeft(2, '0')+"<size=40%>."+(dec+"").PadLeft(2, '0');
        } else {
            return minutes + ":" + (seconds+"").PadLeft(2, '0');
        }
    }

    public static string KeySymbol(KeyCode keyCode) {
        // number keys are named "Alpha", "Alpha", ect.
        if (keyCode.ToString().Contains("Alpha"))
        {
            return keyCode.ToString()[keyCode.ToString().Length - 1].ToString();
        }

        switch(keyCode) {
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.Period: return ".";
            case KeyCode.Comma: return ",";
            case KeyCode.Slash: return "/";
            case KeyCode.None: return "-";
            case KeyCode.Delete: return "DEL";
            // temp fix for char select text wrapping
            case KeyCode.Backspace: return "Back Space";
            default: return keyCode.ToString();
        }
    }

    public static string KeySymbols(KeyCode up, KeyCode left, KeyCode down, KeyCode right) {
        if (up == KeyCode.UpArrow && left == KeyCode.LeftArrow && down == KeyCode.DownArrow && right == KeyCode.RightArrow)
        {
            return "Arrows";
        } else {
            return KeySymbol(up)+KeySymbol(left)+KeySymbol(down)+KeySymbol(right);
        }
    }

    public static Vector2 SnapScrollToChildPos(UnityEngine.UI.ScrollRect scrollRect, RectTransform child)
        {
            Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(0 - (viewportLocalPosition.x + childLocalPosition.x), 0 - (viewportLocalPosition.y + childLocalPosition.y));
            return result;
        }
}
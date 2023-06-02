using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Battle;

public class Utils : MonoBehaviour
{

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

    public static void setScene(string scene){
        SceneManager.LoadScene(scene);
    }

    // % operator is strange with negative numbers
    public static int mod(int x, int m) 
    {
        return (x%m + m)%m;
    }

    // get formatted time from seconds
    public static string FormatTime(float s)
    {
        int seconds = (int)(s % 60);
        int minutes = (int)(s/60);
        return minutes + ":" + (seconds+"").PadLeft(2, '0');
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
}
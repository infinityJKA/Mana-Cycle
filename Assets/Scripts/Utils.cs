using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Battle;
using System.Collections;
using UnityEngine.UI;

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

    public static Vector2 CalculateFocusedScrollPosition(ScrollRect scrollView, Vector2 focusPoint)
    {
        Vector2 contentSize = scrollView.content.rect.size;
        Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
        Vector2 contentScale = scrollView.content.localScale;

        contentSize.Scale(contentScale);
        focusPoint.Scale(contentScale);

        Vector2 scrollPosition = scrollView.normalizedPosition;
        if (scrollView.horizontal && contentSize.x > viewportSize.x)
            scrollPosition.x = Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
        if (scrollView.vertical && contentSize.y > viewportSize.y)
            scrollPosition.y = Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));

        return scrollPosition;
    }

    public static Vector2 CalculateFocusedScrollPosition(ScrollRect scrollView, RectTransform item)
    {
        Vector2 itemCenterPoint = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

        Vector2 contentSizeOffset = scrollView.content.rect.size;
        contentSizeOffset.Scale(scrollView.content.pivot);

        return CalculateFocusedScrollPosition(scrollView, itemCenterPoint + contentSizeOffset);
    }

    public static void FocusAtPoint(ScrollRect scrollView, Vector2 focusPoint)
    {
        scrollView.normalizedPosition = CalculateFocusedScrollPosition(scrollView, focusPoint);
    }

    public static void FocusOnItem(ScrollRect scrollView, RectTransform item)
    {
        scrollView.normalizedPosition = CalculateFocusedScrollPosition(scrollView, item);
    }

    public static IEnumerator LerpToScrollPositionCoroutine(ScrollRect scrollView, Vector2 targetNormalizedPos, float speed)
    {
        Vector2 initialNormalizedPos = scrollView.normalizedPosition;

        float t = 0f;
        while (t < 1f)
        {
            scrollView.normalizedPosition = Vector2.LerpUnclamped(initialNormalizedPos, targetNormalizedPos, 1f - (1f - t) * (1f - t));

            yield return null;
            t += speed * Time.unscaledDeltaTime;
        }

        scrollView.normalizedPosition = targetNormalizedPos;
    }

    public static IEnumerator FocusAtPointCoroutine(ScrollRect scrollView, Vector2 focusPoint, float speed)
    {
        yield return LerpToScrollPositionCoroutine(scrollView, CalculateFocusedScrollPosition(scrollView, focusPoint), speed);
    }

    public static IEnumerator FocusOnItemCoroutine(ScrollRect scrollView, RectTransform item, float speed)
    {
        yield return LerpToScrollPositionCoroutine(scrollView, CalculateFocusedScrollPosition(scrollView, item), speed);
    }
}
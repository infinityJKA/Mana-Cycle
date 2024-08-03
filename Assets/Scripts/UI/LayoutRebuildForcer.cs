using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LayoutRebuildForcer : MonoBehaviour {
    [SerializeField] private bool forceUpdateCanvases = true;

    private void OnEnable() {
        StartCoroutine(GoofyAhhLayoutRebuildCoroutine());
    }

    IEnumerator GoofyAhhLayoutRebuildCoroutine() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.05f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }
}
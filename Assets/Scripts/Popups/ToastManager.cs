using System.Collections.Generic;
using TMPro;
using UnityEngine;

// on same obj as PopupManager
// manages little toast notifications that come down from the top of the screen in a queue
public class ToastManager : MonoBehaviour {
    public static ToastManager instance {get; private set;}

    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private float remainTime = 3f;
    
    [SerializeField] private RectTransform toastTransform;
    [SerializeField] private TMP_Text toastMessage;
    [SerializeField] private Vector2 hidePosition, showPosition;
    [SerializeField] private Color normalColor, successColor, errorColor;

    private static Queue<Toast> queue = new Queue<Toast>();
    private Toast currentToast;
    private float t;

    private Phase phase;
    private enum Phase {
        None,
        In,
        Remain,
        Out
    }

    private void Awake() {
        instance = this;
    }

    public static void ShowToast(string message, Status status = Status.Message) {
        queue.Enqueue(new Toast(){
            message = message,
            status = status,
        });
    }

    private void Update() {
        if (phase != Phase.None) t += Time.deltaTime;

        if (phase == Phase.In) {
            if (t >= transitionTime) {
                toastTransform.anchoredPosition = showPosition;
                t = 0;
                phase = Phase.Remain;
            } else {
                toastTransform.anchoredPosition = Vector2.Lerp(hidePosition, showPosition, t/transitionTime);
            }
        } else if (phase == Phase.Remain) {
            if (t >= remainTime) {
                t = 0;
                phase = Phase.Out;
            }
        } else if (phase == Phase.Out) {
            if (t >= transitionTime) {
                toastTransform.anchoredPosition = hidePosition;
                t = 0;
                phase = Phase.None;
            } else {
                toastTransform.anchoredPosition = Vector2.Lerp(showPosition, hidePosition, t/transitionTime);
            }
        }
        if (phase == Phase.None) {
            if (queue.TryDequeue(out Toast toast)) {
                currentToast = toast;
                toastMessage.text = toast.message;
                if (toast.status == Status.Success) {
                    toastMessage.color = successColor;
                } else if (toast.status == Status.Error) {
                    toastMessage.color = errorColor;
                } else {
                    toastMessage.color = normalColor;
                }
                t = 0;
                phase = Phase.In;
            }
        }
    }
}

public struct Toast {
    public string message;
    public Status status;
}

public enum Status {
    Message,
    Success,
    Error
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class OverworldInteractable : MonoBehaviour
{
    [SerializeField] Transform playerTransform;

    // how far away this interactable can be triggered
    [SerializeField] float interactionRadius;

    // the text that the ui label should display
    [SerializeField] string interactableName;

    // the ui text object itself
    [SerializeField] TMPro.TextMeshProUGUI labelText;
    // the animator on the ui object
    [SerializeField] Animator LabelAnimator;

    // name of the scene to transition to when selected
    [SerializeField] string selectSceneName;

    private bool inRadius;
    private bool inRadiusLastFrame;

    private PlayerInput playerInput;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        float distFromPlayer = Vector2.Distance(new Vector2(playerTransform.position.x, playerTransform.position.z), new Vector2(transform.position.x, transform.position.z));
        inRadius = (distFromPlayer <= interactionRadius);

        if (inRadius && !inRadiusLastFrame) OnRadiusEnter();
        else if (!inRadius && inRadiusLastFrame) OnRadiusExit();

        inRadiusLastFrame = inRadius;
    }

    void OnRadiusEnter()
    {
        Debug.Log("Radius enter");
        labelText.text = interactableName;
        LabelAnimator.ResetTrigger("Hide");
        LabelAnimator.SetTrigger("Show");
        playerInput.enabled = true;
    }

    void OnRadiusExit()
    {
        Debug.Log("Radius exit");
        LabelAnimator.ResetTrigger("Show");
        LabelAnimator.SetTrigger("Hide");
        playerInput.enabled = false;
    }

    void OnSubmit()
    {
        GameObject.Find("TransitionHandler").GetComponent<TransitionScript>().WipeToScene(selectSceneName);
    }
}

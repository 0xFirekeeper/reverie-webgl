// Filename: ButtonSwipeTrigger.cs
// Author: 0xFirekeeper
// Description: Use case of SwipeManager - a generic button swipe detector that triggers next/previous onclicks, extendable.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSwipeTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button leftSwipeEvent, rightSwipeEvent;

    private bool pointerDown;
    private bool swipedRight, swipedLeft;

    private Selectable selectable;

    private void Awake()
    {
        selectable = this.GetComponent<Selectable>();
    }

    private void Start()
    {
        pointerDown = false;
        swipedLeft = false;
        swipedRight = false;
    }

    //Detect current clicks on the GameObject (the one with the script attached)
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        //Output the name of the GameObject that is being clicked
        Debug.Log(name + "Game Object Click in Progress");

        pointerDown = true;
        swipedLeft = false;
        swipedRight = false;

        pointerEventData.eligibleForClick = true;

    }

    private void Update()
    {
        if (pointerDown)
        {
            if (SwipeManager.IsSwipingRight())
            {
                swipedRight = true;
            }
            else if (SwipeManager.IsSwipingLeft())
            {
                swipedLeft = true;
            }
            else
            {
                swipedLeft = false;
                swipedRight = false;
            }
        }
    }

    //Detect if clicks are no longer registering
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        pointerDown = false;

        if (swipedRight)
        {
            Debug.Log("Invoking Swipe Right Event");
            rightSwipeEvent.onClick?.Invoke();
            pointerEventData.eligibleForClick = false;
        }
        else if (swipedLeft)
        {
            Debug.Log("Invoking Swipe Left Event");

            leftSwipeEvent.onClick?.Invoke();
            pointerEventData.eligibleForClick = false;
        }
        else
        {
            Debug.Log("No Swipe Detected");

            pointerEventData.eligibleForClick = true;
        }


    }

}
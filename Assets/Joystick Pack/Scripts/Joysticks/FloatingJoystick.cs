using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
    }

    /// <summary>
    /// Resets the joystick handle and hides the background.
    /// </summary>
    public void ResetJoystick()
    {
        // Hide the joystick background
        background.gameObject.SetActive(false);
        // Reset handle position to center
        handle.anchoredPosition = Vector2.zero;
        // Clear input vector (protected member of base class)
        input = Vector2.zero;
    }
}
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ButtonAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    RectTransform rectTransform;
    Vector3 targetScale;
    public Dictionary<string, float> buttonEventScales = new Dictionary<string, float>();
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private UnityEngine.UI.Button button;

    private void playSound(AudioClip sound)
    {
        if (!button.interactable) return;
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = sound;
        audio.Play();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        targetScale = new Vector3(1f, 1f, 1f);
        button = GetComponent<UnityEngine.UI.Button>();

        buttonEventScales.Add("Enter", 1.07f);
        buttonEventScales.Add("Exit", 1f);
        buttonEventScales.Add("Down", 0.93f);
        buttonEventScales.Add("Up", 1f);
    }

    void Update()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, 0.1f);
        if (!button.interactable)
        {
            targetScale = new Vector3(1f, 1f, 1f);
        }
    }

    void OnButtonEvent(string eventName, PointerEventData eventData)
    {
        if (!button.interactable) return;

        var scale = buttonEventScales[eventName];
        targetScale = new Vector3(scale, scale, scale);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnButtonEvent("Enter", eventData);
        playSound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnButtonEvent("Exit", eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnButtonEvent("Down", eventData);
        playSound(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnButtonEvent("Up", eventData);
    }
}
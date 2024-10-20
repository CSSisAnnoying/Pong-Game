using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Pages
{
    Home = 0,
    Settings = 1,
    HowToPlay = 2,
}

public class MenuScripts : MonoBehaviour
{
    public GameObject logo;
    public GameObject menu;
    public GameObject[] pageObjects;
    public GameObject[] pageButtons;
    private Pages _currentPage = Pages.Home;
    public Pages CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPageChanged();
            }
        }
    }
    public GameObject playButton;

    private int timer = 0;

    private bool _menuIsOpen = true;
    public bool MenuIsOpen
    {
        get => _menuIsOpen;
        set
        {
            if (_menuIsOpen != value)
            {
                _menuIsOpen = value;
                OnMenuStateChanged();
            }
        }
    }

    private void RunAnimationOnMenu(string AnimationName, float crossFade = 0.2f) {
        Animator animator = menu.GetComponent<Animator>();
        AnimationsHandler animationsHandler = gameObject.AddComponent<AnimationsHandler>();
        animationsHandler.RunAnimation(animator, AnimationName, crossFade);
    }

    private IEnumerator LoopThroughParallel(GameObject[] array, Action<GameObject> action) {
        foreach (GameObject item in array) {
            action(item);
            yield return null;
        }
    }

    void OnMenuStateChanged()
    {
        GameObject[] menuButtons = GameObject.FindGameObjectsWithTag("MenuButton");
        foreach (GameObject menuButton in menuButtons)
        {
            var buttonComponent = menuButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.interactable = _menuIsOpen;
            }
        }

        if (_menuIsOpen) {
            RunAnimationOnMenu("MenuOpen");
        } else {
            RunAnimationOnMenu("MenuClose");
        }
    }

    void HandleLogoTilt()
    {
        timer += 1;
        logo.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(timer * 0.01f) * 2);
    }

    public void HandlePlayButton()
    {
        Debug.Log("Clicked");
        MenuIsOpen = false;
    }

    void OnPageChanged() {
        Action<GameObject> action = pageObject => {
            Debug.Log((int)CurrentPage - 1);
            pageObject.SetActive(pageObject == pageObjects[(int)CurrentPage - 1]);
        };
        
        StartCoroutine(LoopThroughParallel(pageObjects, action));
    }

    void Start()
    {
        OnMenuStateChanged();

        for (int i = 0; i < pageButtons.Length; i++)
        {
            Button button = pageButtons[i].GetComponent<Button>();
            button.onClick.AddListener(() => CurrentPage = (Pages)i);
        }
    }
}

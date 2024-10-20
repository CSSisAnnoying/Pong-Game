using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CounterHandler : MonoBehaviour
{
    AudioSource CountdownSound;
    AudioSource CountdownEndSound;

    public enum CountdownSounds {
        Countdown,
        CountdownEnd
    }

    void Start() {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        CountdownSound = audioSources[0];
        CountdownEndSound = audioSources[1];
    }

    public void PlaySound(CountdownSounds sound) {
        if (sound == CountdownSounds.Countdown) {
            CountdownSound.Play();
        } else if (sound == CountdownSounds.CountdownEnd) {
            CountdownEndSound.Play();
        }
    } 
}

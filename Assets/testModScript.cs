using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;


public class testModScript : MonoBehaviour {
    public KMAudio Audio;
    public KMBombInfo BombInfo;
    public KMBombModule Module;
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public KMSelectable[] keyboard;
    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach(KMSelectable letter in keyboard)
        {
            KMSelectable pressedLetter = letter;
            letter.OnInteract += delegate () { letterPress(pressedLetter.GetComponentInChildren<TextMesh>().text); return false; };
        }
        moduleSolved = false;
    }

    //On a key press
    void letterPress(String let)
    {
        if (!moduleSolved)
        {
            Module.HandlePass();
            moduleSolved = true;
        }
       
    }

    // Twitch Plays
    #pragma warning disable 414
    private string TwitchHelpMessage = "!{0} solve to test Forget Enigma!";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        yield return "sendtochaterror I don't recognize anything except '!{1} solve'!";
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        keyboard[0].OnInteract();
        yield return new WaitForSeconds(0.1f);
    }
}

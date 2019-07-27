using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;


public class forgetEnigma : MonoBehaviour {
    public KMAudio Audio;
    public KMBomb Bomb;
    public KMBombInfo BombInfo;
    public KMBombModule Module;
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public KMSelectable[] keyboard;
    private KMSelectable prevButton;
    public Material[] materials;
    public TextMesh[] rotorStart;
    public TextMesh configuration;
    public TextMesh stageText;
    public AudioClip[] sounds;
    private String configText;
    private String[] r1 =
    {
        "EKMFLGDQVZNTOWYHXUSPAIBRCJ",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "DQ"
    };
    private String[] r2 =
   {
        "AJDKSIRUXBLHWTMCQGZNPYFVOE",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "ER"
    };
    private String[] r3 =
   {
        "BDFHJLCPRTXVZNYEIWGAKMUSQO",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "IV"
    };
    private String[] r4 =
   {
        "ESOVPZJAYQUIRHXLNFTGKDCMWB",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "JW"
    };
    private String[] r5 =
   {
        "VZBRGITYUPSDNHLXAWMJQOFECK",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "MZ"
    };
    private String[] r6 =
   {
        "JPGVOUMFYQBENHZRDKASXLICTW",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "LY"
    };
    private String[] r7 =
   {
        "NZJHGRCXMYSWBOUFAIVLPEKQDT",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "HU"
    };
    private String[] r8 =
   {
        "FKQHTLXOCBJSPDZRAMEWNIUYGV",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "CP"
    };
    private String[] refA =
    {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "LUSNPQOMJIYAHDGEFXCVBTZRKW"
    };
    private String[] refB =
    {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "XQUMFEPOWLTJDZHGBVYKCRIASN"
    };
    private String[] refC =
    {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "ESKOAQMJYHCPGTDLFUBNRXZVIW"
    };
    private String[] enigKeyboard =
    {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    };
    private String order = "QWERTYUIOPASDFGHJKLZXCVBNM";
    private String[][] rotors;

    private int count = 0;
    private String answer = "";
    private String encryptedAnswer = "";
    private String rotorSetup;
    private int stage;
    private int ticker;
    private bool done;
    private bool visible;
    public static string[] ignoredModules = null;
    void Awake()
    {
        moduleId = moduleIdCounter;
        foreach(KMSelectable letter in keyboard)
        {
            KMSelectable pressedLetter = letter;
            letter.OnInteract += delegate () { letterPress(pressedLetter.GetComponentInChildren<TextMesh>().text); return false; };
        }
        moduleSolved = false;
        if (ignoredModules == null)
        {
            ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("Forget Enigma", new string[]{
                   "Forget Everything",
                    "Forget Infinity",
                    "Forget Me Not",
                    "Forget Them All",
                    "Forget This",
                    "Forget Enigma",
                    "Four-Card Monte",
                    "Purgatory",
                    "Simon's Stages",
                    "Souvenir",
                    "Tallordered Keys",
                    "The Time Keeper",
                    "Timing is Everything",
                    "Turn The Key"
            });
        }
    }
    // Use this for initialization
    void Start ()
    {
        done = false;
        count = BombInfo.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
        Debug.LogFormat("[Forget Enigma #{0}] Number of stages is {1}", moduleId, count);
        //count = 1;
        if (count == 0)
        { //Prevent deadlock
            Debug.LogFormat("[Forget Enigma #{0}] No valid stage modules, auto-solving.", moduleId);
            Audio.PlaySoundAtTransform(sounds[1].name, transform);
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            return;
        }
        if (count > 99)
        { //More than 99 stages will cause issues as the stage display only has 2 digits
            Debug.LogFormat("[Forget Enigma #{0}] More than 99 stages, capping at 99.", moduleId);
            count = 99;
        }

        //Generate Reflector
        rotors = new String[4][];
        rotors[0] = generateReflector();
        //Generating Rotors
        int[] nums = new int[2];
        for (int aa = 1; aa < 3; aa++)
        {
            int num = UnityEngine.Random.Range(0, 8);
            while(isThere(nums, num, aa - 1))
            {
                num = UnityEngine.Random.Range(0, 8);
            }
            rotors[aa] = generateRotor(num);
            nums[aa - 1] = num;
            num = UnityEngine.Random.Range(0, 26);
            for(int bb = 0; bb < num; bb++)
            {
                rotors[aa] = turnOver(rotors[aa]);
            }
          

        }
        Debug.LogFormat("[Forget Enigma #{0}] Enigma Configuration: {1}", moduleId, configText);
        Debug.LogFormat("[Forget Enigma #{0}] Rotor Setup: {1}", moduleId, rotors[1][1][0] + "" + rotors[2][1][0]);
        //Adding in the keyboard
        rotors[3] = enigKeyboard;

        rotorSetup = rotors[1][1][0] + "" + rotors[2][1][0];
        //Generating answer;
        for (int dd = 0; dd < count; dd++)
        {
            answer = answer + "" + generateLetter();
        }
        Debug.LogFormat("[Forget Enigma #{0}] Generated Answer: {1}", moduleId, answer);
        //Encrypting answer
        int tempCounter = 1;
        foreach (char let in answer)
        {
            encryptedAnswer = encryptedAnswer + "" + encrypt(rotors, let, tempCounter);
            rotors = turn(rotors);
            tempCounter++;
        }
        //Reseting rotors back to stage 1 config.
        rotors = resetRotors(rotors);
        rotorStart[0].text = rotors[1][1][0] + "";
        rotorStart[1].text = rotors[2][1][0] + "";
       
        //Show the encrypted letter at stage 1.
        getLitKey();
        //Show configuration of Ref-Rot-Rot-Rot
        configuration.text = configText;
        //Show stage number
        stage = 0;
        stageText.text = (stage + 1) + "";
        Debug.LogFormat("[Forget Enigma #{0}] Generated Answer: {1}", moduleId, encryptedAnswer);
    }

    // Update is called once per frame
    //private int ticker = 0;
    void Update()
    {
        ticker++;
        if (ticker == 15)
        {
            ticker = 0;
            int progress = BombInfo.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
            if (progress > stage && !done)
            {
                stage++;
                if (stage >= count)
                {
                    Debug.LogFormat("[Forget Enigma #{0}] All {1} stages have been displayed. Going to submit mode.", moduleId, count);
                    done = true;
                    wipeScreens();
                    rotors = resetRotors(rotors);
                    stage = 0;
                    stageText.text = (stage + 1) + "";
                    visible = false;
                    turnOffKey();
                }
                else
                {
                    Debug.LogFormat("[Forget Enigma #{0}] A module was solved, moving on to stage {1} out of {2}", moduleId, stage + 1, count);
                    rotors = turn(rotors);
                    stageText.text = (stage + 1) + "";
                    rotorStart[0].text = rotors[1][1][0] + "";
                    rotorStart[1].text = rotors[2][1][0] + "";
                    getLitKey();
                }
            }
        }
    }
    //Generates a random letter.
    char generateLetter()
    {
        return order[UnityEngine.Random.Range(0, 26)];
    }
    //Generates a random reflector.
    String[] generateReflector()
    {
        switch(UnityEngine.Random.Range(0, 3))
        {
            case 0:
                configText = "A";
                return refA;
            case 1:
                configText = "B";
                return refB;
            default:
                configText = "C";
                return refC;
        }
    }
    //Generates a random rotor.
    String[] generateRotor(int num)
    {
        switch (num)
        {
            case 0:
                configText = configText + "-I";
                return r1;
            case 1:
                configText = configText + "-II";
                return r2;
            case 2:
                configText = configText + "-III";
                return r3;
            case 3:
                configText = configText + "-IV";
                return r4;
            case 4:
                configText = configText + "-V";
                return r5;
            case 5:
                configText = configText + "-VI";
                return r6;
            case 6:
                configText = configText + "-VII";
                return r7;
            default:
                configText = configText + "-VIII";
                return r8;
        }
    }
    //Checks if the rotor is being used.
    bool isThere(int[] nums, int num, int stage)
    {
        for(int aa = 0; aa < stage; aa++)
        {
            if(nums[aa] == num)
            {
                return true;
            }
        }
        return false;
    }
    
   
    //Lights up the encrypted key at stage #.
    void getLitKey()
    {
        for (int i = 0; i < order.Length; i++)
        {
            if (encryptedAnswer[stage] == order[i])
            {
                toggleColor(keyboard[i]);
                break;
            }
        }
    }
    void turnOffKey()
    {
        if (prevButton != null)
        {
            prevButton.GetComponentInChildren<MeshRenderer>().material = materials[0];
            prevButton.GetComponentInChildren<TextMesh>().color = Color.white;
            prevButton = null;
        }
    }
    //Resets the rotors back to stage 1 config.
    String[][] resetRotors(String[][] r)
    {
        for (int cc = 0; cc < 2; cc++)
        {
            while (r[cc + 1][1][0] != rotorSetup[cc])
            {
                r[cc + 1] = turnOver(r[cc + 1]);
            }
        }
        return r;
    }
    //On a key press
    void letterPress(String let)
    {
        Debug.LogFormat("[Forget Enigma #{0}] You pressed {1}", moduleId, let);
        Debug.LogFormat("[Forget Enigma #{0}] Stage {1} out of {2}", moduleId, stage + 1, answer.Length);
        if (!moduleSolved)
        {
            Debug.LogFormat("[Forget Enigma #{0}] Expecting the letter {1} to be pressed", moduleId, answer[stage]);
            Audio.PlaySoundAtTransform(sounds[0].name, transform);
            if (done)
            {
                if(visible)
                {
                    wipeScreens();
                }
                if (let[0] == answer[stage])
                {
                    if (stage >= answer.Length - 1)
                    {
                        Debug.LogFormat("[Forget Enigma #{0}] That is correct, module solved", moduleId);
                        configuration.text = "";
                        stageText.text = "--";
                        Audio.PlaySoundAtTransform(sounds[1].name, transform);
                        Module.HandlePass();
                        moduleSolved = true;
                    }
                    else
                    {
                        rotors = turn(rotors);
                        stage++;
                        stageText.text = (stage + 1) + "";
                        Debug.LogFormat("[Forget Enigma #{0}] That is correct, moving on to stage {1}", moduleId, stage + 1);
                    } 
                }
                else
                {
                    Module.HandleStrike();
                    getLitKey();
                    rotorStart[0].text = rotors[1][1][0] + "";
                    rotorStart[1].text = rotors[2][1][0] + "";
                    Debug.LogFormat("");
                    visible = true;
                }
            }
            else
            {
                Debug.LogFormat("[Forget Enigma #{0}] Strike! I wasn't expecting any input yet", moduleId);
                Module.HandleStrike();
            }
        }
       
    }
    void wipeScreens()
    {
        rotorStart[0].text = "-";
        rotorStart[1].text = "-";
        turnOffKey();
    }
    String[][] turn(String[][] r)
    {
        if (isTurnOver(r[2]))
            r[1] = turnOver(r[1]);
        r[2] = turnOver(r[2]);
        return rotors;
    }
    private String[] turnOver(String[] r)
    {
        r[0] = r[0].Substring(1, 25) + r[0][0];
        r[1] = r[1].Substring(1, 25) + r[1][0];
        return r;
    }
    bool isTurnOver(String[] r)
    {
        for (int aa = 0; aa < r[2].Length; aa++)
        {
            if (r[1][0] == r[2][aa])
            {
                return true;
            }
        }
        return false;
    }
    private char encrypt(String[][] r, char let, int s)
    {
        String letterLog = "";
        letterLog = letterLog + "" + let + "->";
        for (int aa = r.Length - 1; aa > 0; aa--)
        {
            for (int bb = 0; bb < r[aa][1].Length; bb++)
            {
                if (let == r[aa][1][bb])
                {
                    let = r[aa - 1][0][bb];
                    letterLog = letterLog + "" + let + "->";
                    break;
                }
            }
            
        }
        for (int cc = 0; cc < r[0][1].Length; cc++)
        {
            if (let == r[0][1][cc])
            {
                let = r[0][0][cc];
                break;
            }
        }
        letterLog = letterLog + "" + let + "->";
        for (int aa = 0; aa < r.Length - 1; aa++)
        {
            for (int bb = 0; bb < r[aa][0].Length; bb++)
            {
                if (let == rotors[aa][0][bb])
                {
                    let = rotors[aa + 1][1][bb];
                    break;
                }
            }
            letterLog = letterLog + "" + let + "->";
        }
        Debug.LogFormat("[Forget Enigma #{0}] Stage {1}: {2}: {3}", moduleId, s, r[1][1][0] + "" + r[2][1][0], letterLog.Substring(0, letterLog.Length - 2));
        return let;
    }
    
    void toggleColor(KMSelectable button)
    {
        turnOffKey();
        button.GetComponentInChildren<MeshRenderer>().material = materials[1];
        button.GetComponentInChildren<TextMesh>().color = Color.black;
        prevButton = button;
    }
    private int getPositionFromChar(char c)
    {
        return "QWERTYUIOPASDFGHJKLZXCVBNM".IndexOf(c);
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Submit the decrypted word with !{0} submit qwertyuiopasdfghjklzxcvbnm";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2 || !split[0].Equals("SUBMIT")) yield break;
        int[] buttons = split[1].Select(getPositionFromChar).ToArray();
        if (buttons.Any(x => x < 0)) yield break;
        yield return null;

        yield return new WaitForSeconds(0.1f);
        foreach (char let in split[1])
        {
            letterPress(let + "");
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.1f);
    }
}

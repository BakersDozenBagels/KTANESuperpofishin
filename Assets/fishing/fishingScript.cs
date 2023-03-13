using System.Collections;
using UnityEngine;
using KModkit;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class fishingScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMBombModule bombModule;
    public KMAudio audio;
    public GameObject FishingRod;
    public GameObject Hook;
    public GameObject Line;
    public GameObject Sprites;
    public GameObject KeepThrow;
    public TextMesh FishCaught;
    public TextMesh TrashCollected;
    public KMSelectable Reel;
    public KMSelectable Keep;
    public KMSelectable Throw;
    public Sprite[] Fish;
    public SpriteRenderer Fishies;
    public Renderer Background;
    public KMSelectable ModuleSelectable;
    public Material redWater;

    private KeyCode space = KeyCode.C;

    int ParallelPort, PS2Port, Vowels, Consonants, Evens, Odds, FishGoal, FishTotal, TrashTotal, arrayFish;

    private bool isSolved, focused;
    private static int moduleIdCounter = 1;
    private int moduleId;
    private double tpscore = 0;

    private bool showFish, showHook, showReel, showRod, castThrow, overfishing;

    // Use this for initialization
    void Start()
    {
        isSolved = false;
        showFish = false;
        overfishing = false;
        castThrow = false;
        showHook = true;
        showReel = true;
        showRod = true;
        FishTotal = 0;
        arrayFish = 0;
        TrashTotal = 0;
        moduleId = moduleIdCounter++;
        Reel.OnInteract += delegate { if (showReel) ReelFish(); return false; };
        Keep.OnInteract += delegate { if (castThrow) KeepFish(); return false; };
        Throw.OnInteract += delegate { if (castThrow) ThrowFish(); return false; };

        if (Application.isEditor)
            focused = true;
        ModuleSelectable.OnFocus += delegate () { focused = true; };
        ModuleSelectable.OnDefocus += delegate () { focused = false; };

        Debug.LogFormat("[Fishing #{0}] Module Count: {1}", moduleId, Bomb.GetSolvableModuleNames().Count());
        GetSolveFish();
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(space))
        {
            HandleOverfishing();
        }
        FishCaught.text = "Fish: " + FishTotal;
        TrashCollected.text = "Trash: " + TrashTotal;

        if (!showFish)
        {
            Sprites.gameObject.SetActive(false);
        }
        else
        {
            Sprites.gameObject.SetActive(true);
        }
        if (!castThrow)
        {
            KeepThrow.gameObject.SetActive(false);
        }
        else
        {
            KeepThrow.gameObject.SetActive(true);
        }
        if (!showHook)
        {
            Hook.gameObject.SetActive(false);
            Line.gameObject.SetActive(true);
        }
        else
        {
            Hook.gameObject.SetActive(true);
            Line.gameObject.SetActive(false);
        }
        if (!showReel)
        {
            Reel.gameObject.SetActive(false);
        }
        else
        {
            Reel.gameObject.SetActive(true);
        }
        if (!showRod)
        {
            FishingRod.gameObject.SetActive(false);
        }
        else
        {
            FishingRod.gameObject.SetActive(true);
        }
    }

    void GetSolveFish()
    {
        ParallelPort = Bomb.GetPortCount(Port.Parallel);
        PS2Port = Bomb.GetPortCount(Port.PS2);
        for (int i = 0; i < 6; i++)
        {
            if ("AEIOU".Contains(Bomb.GetSerialNumber()[i]))
            {
                Vowels++;
            }
            if ("BCDFGHJKLMNPQRSTVWXYZ".Contains(Bomb.GetSerialNumber()[i]))
            {
                Consonants++;
            }
        }
        for (int i = 0; i < 6; i++)
        {
            if ("02468".Contains(Bomb.GetSerialNumber()[i]))
            {
                Evens++;
            }
            if ("13579".Contains(Bomb.GetSerialNumber()[i]))
            {
                Odds++;
            }
        }

        if (ParallelPort > 0)
        {
            FishGoal += 8;
        }
        if (PS2Port > 0)
        {
            FishGoal += 4;
        }
        if (Consonants == 0 && Vowels != 0)
        {
            FishGoal += 2;
        }
        if (Odds == 0 && Evens != 0)
        {
            FishGoal += 1;
        }
        FishGoal = FishGoal + 46;
        Debug.LogFormat("[Fishing #{0}] Calculated Fish: {1}", moduleId, Fish[FishGoal].name);
    }

    void ReelFish()
    {
        showReel = false;
        showHook = false;
        StartCoroutine(Fishing());
    }

    void KeepFish()
    {
        audio.PlaySoundAtTransform("Fishing_rod_reel_in2", transform);
        Debug.LogFormat("[Fishing #{0}] Catch Kept", moduleId);
        if ((arrayFish >= 18 && arrayFish <= 45) || (arrayFish >= 92 && arrayFish <= 99))
        {
            TrashTotal++;
            if (overfishing)
            {
                FishTotal = 0;
            }
        }
        else
        {
            FishTotal++;
        }
        if ((FishGoal == arrayFish || FishTotal >= Bomb.GetSolvableModuleNames().Count()) && !overfishing)
        {
            Debug.LogFormat("[Fishing #{0}] Module Solved", moduleId);
            bombModule.HandlePass();
            isSolved = true;
            showFish = false;
        }
        else if (FishGoal == arrayFish && overfishing)
        {
            Debug.LogFormat("[Fishing #{0}] Module Solved", moduleId);
            bombModule.HandlePass();
            isSolved = true;
            showFish = false;
        }
        else
        {
            showRod = true;
            showReel = true;
            showHook = true;
            showFish = false;
        }
        castThrow = false;
    }

    void ThrowFish()
    {
        audio.PlaySoundAtTransform("Fishing_rod_reel_in2", transform);
        Debug.LogFormat("[Fishing #{0}] Catch Thrown", moduleId);
        showRod = true;
        showReel = true;
        showHook = true;
        showFish = false;
        castThrow = false;
        if (overfishing && !((arrayFish >= 18 && arrayFish <= 45) || (arrayFish >= 92 && arrayFish <= 99)))
        {
            FishTotal = 0;
        }
    }

    void HandleOverfishing()
    {
        if (focused && !overfishing)
        {
            overfishing = true;
            audio.PlaySoundAtTransform("brain bell", transform);
            FishGoal = 999999;
            Background.material = redWater;
        }
    }

    IEnumerator Fishing()
    {
        audio.PlaySoundAtTransform("Fishing_rod_cast", transform);
        yield return new WaitForSecondsRealtime(2.0f);
        arrayFish = UnityEngine.Random.Range(0, Fish.Length);
        Fishies.sprite = Fish[arrayFish];
        showFish = true;
        showRod = false;
        yield return new WaitForSecondsRealtime(1.0f);
        Debug.LogFormat("[Fishing #{0}] Caught: {1}", moduleId, Fish[arrayFish].name);
        castThrow = true;
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} reel [Presses the reel button] | !{0} keep [Presses the keep button] | !{0} throw [Presses the throw button]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*reel\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!showReel)
            {
                yield return "sendtochaterror The reel button cannot be pressed right now!";
                yield break;
            }
            Reel.OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*keep\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!castThrow)
            {
                yield return "sendtochaterror The keep button cannot be pressed right now!";
                yield break;
            }
            else if (!(arrayFish >= 18 && arrayFish <= 45) || (arrayFish >= 92 && arrayFish <= 99))
            {
                tpscore += 0.2;
            }
            Keep.OnInteract();
            if (isSolved && tpscore >= 1f)
            {
                yield return "awardpoints " + (int)Math.Floor(tpscore);
            }
        }
        if (Regex.IsMatch(command, @"^\s*throw\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!castThrow)
            {
                yield return "sendtochaterror The throw button cannot be pressed right now!";
                yield break;
            }
            Throw.OnInteract();
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (true)
        {
            if (showReel)
                Reel.OnInteract();
            while (!castThrow) { yield return true; }
            Keep.OnInteract();
            yield return new WaitForSeconds(0.1f);
            if (isSolved)
                break;
        }
    }
}
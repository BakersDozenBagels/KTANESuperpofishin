using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;

using Random = UnityEngine.Random;

public class Superpofishin : MonoBehaviour
{
    public KMAudio Audio;
    public GameObject FishingRod;
    public GameObject Sprites;
    public KMSelectable[] MainButtons;
    public Sprite[] Fish;
    public SpriteRenderer[] FishDisplays;
    public GameObject InputScreen;
    public GameObject Text;
    public KMSelectable[] InputButtons;

    private bool _isSolved, _struck;
    private static int _idc;
    private int _id = ++_idc;

    private const int PlayerCount = 5;

    private bool RodVisible
    {
        get
        {
            return FishingRod.activeSelf;
        }
        set
        {
            FishingRod.SetActive(value);
        }
    }
    private bool KeepVisible
    {
        get
        {
            return MainButtons[0].gameObject.activeSelf;
        }
        set
        {
            MainButtons[0].gameObject.SetActive(value);
        }
    }
    private bool ReelVisible
    {
        get
        {
            return MainButtons[1].gameObject.activeSelf;
        }
        set
        {
            MainButtons[1].gameObject.SetActive(value);
        }
    }
    private bool ThrowVisible
    {
        get
        {
            return MainButtons[2].gameObject.activeSelf;
        }
        set
        {
            MainButtons[2].gameObject.SetActive(value);
        }
    }
    private bool SpritesVisible
    {
        get
        {
            return Sprites.activeSelf;
        }
        set
        {
            Sprites.SetActive(value);
        }
    }
    private bool InputVisible
    {
        get
        {
            return InputScreen.activeSelf;
        }
        set
        {
            InputScreen.SetActive(value);
        }
    }
    private bool TextVisible
    {
        get
        {
            return Text.activeSelf;
        }
        set
        {
            Text.SetActive(value);
        }
    }

    private enum SuperpofishinState
    {
        Start,
        Display,
        Input,
        Solved
    }
    private SuperpofishinState _state;
    private SuperpofishinState State
    {
        get
        {
            return _state;
        }
        set
        {
            switch(value)
            {
                case SuperpofishinState.Start:
                    InputVisible = false;
                    RodVisible = true;
                    TextVisible = true;
                    goto default;
                case SuperpofishinState.Display:
                    InputVisible = false;
                    KeepVisible = true;
                    ReelVisible = false;
                    ThrowVisible = true;
                    SpritesVisible = true;
                    RodVisible = false;
                    TextVisible = false;
                    break;
                case SuperpofishinState.Input:
                    InputVisible = true;
                    RodVisible = false;
                    TextVisible = false;
                    goto default;
                case SuperpofishinState.Solved:
                    InputVisible = false;
                    KeepVisible = false;
                    ReelVisible = false;
                    ThrowVisible = false;
                    SpritesVisible = false;
                    RodVisible = false;
                    TextVisible = false;
                    _isSolved = true;
                    break;

                default:
                    KeepVisible = false;
                    ReelVisible = true;
                    ThrowVisible = false;
                    SpritesVisible = false;
                    break;
            }
            _state = value;
        }
    }

    private struct Turn
    {
        public int Asked;
        public int Card;
        public bool Response;

        public Turn(int asked, int card, bool response)
        {
            if(asked < 0 || asked >= PlayerCount)
                throw new ArgumentException("Bad player number " + asked + " (expected [0-" + (PlayerCount - 1) + "])");

            if(card < 0 || card >= PlayerCount)
                throw new ArgumentException("Bad card number " + card + " (expected [0-" + (PlayerCount - 1) + "])");

            Asked = asked;
            Card = card;
            Response = response;
        }
    }
    private readonly List<Turn> _pages = new List<Turn>();
    private List<List<int>> _solution;
    private readonly List<int> _inputSequence = new List<int>();
    private List<int> _fishOrder;

    private int _page, _solutionCount;
    private int Page
    {
        get
        {
            return _page;
        }
        set
        {
            if(value > LastPage || value < 0)
                throw new ArgumentException("Bad page number " + value + " (expected [0-" + LastPage + "])");

            FishDisplays[0].sprite = Fish[_pages[value].Asked];
            FishDisplays[1].sprite = Fish[_pages[value].Card + PlayerCount];
            FishDisplays[2].sprite = Fish[_pages[value].Response ? PlayerCount + PlayerCount : PlayerCount + PlayerCount + 1];

            _page = value;
        }
    }
    private int LastPage
    {
        get
        {
            return _pages.Count - 1;
        }
    }

    private void Start()
    {
        MainButtons[0].OnInteract += delegate { Sound(); Keep(); return false; };
        MainButtons[1].OnInteract += delegate { Sound(); Reel(); return false; };
        MainButtons[2].OnInteract += delegate { Sound(); Throw(); return false; };

        State = SuperpofishinState.Start;

        GeneratePuzzle();
    }

    private void GeneratePuzzle()
    {
        Fish.Shuffle();
        FishDisplays.Shuffle();
        int majoriter = 0;

        TryAgain:
        majoriter++;
        if(majoriter > 1000)
            throw new Exception("Generation limit reached!");

        _pages.Clear();
        int[,] heldCards = new int[PlayerCount, PlayerCount];
        int[] cardCounts = Enumerable.Repeat(4, PlayerCount).ToArray();
        int current = 0;
        int iter = 0;

        do
        {
            if(cardCounts[current] == 0)
                goto Continue;

            int asked = Enumerable.Range(0, PlayerCount).Except(new int[] { current }).PickRandom();
            int card;
            try
            {
                card = Enumerable.Range(0, PlayerCount).Where(c => heldCards[current, c] != -1).PickRandom();
            }
            catch(Exception)
            {
                Debug.Log(heldCards.Columns().Select(c => c.Join(" ")).Join(" | "));
                Debug.Log(Enumerable.Range(0, PlayerCount).Select(p => cardCounts[p]).Join(" | "));
                throw;
            }
            if(heldCards[current, card] == 0)
                heldCards[current, card] = 1;

            bool response = heldCards[asked, card] > 0;
            if(heldCards[asked, card] == 0
                && !(heldCards.Row(card).SumPositive() >= 4))
                response = Random.Range(0, 2) == 1;

            if(response)
            {
                heldCards[current, card]++;
                cardCounts[current]++;

                if(heldCards[asked, card] > 0)
                    heldCards[asked, card]--;

                cardCounts[asked]--;
            }
            else
            {
                heldCards[asked, card] = -1;
            }

            for(int row = 0; row < PlayerCount; ++row)
                if(heldCards.Row(row).SumPositive() == 4)
                    for(int col = 0; col < PlayerCount; ++col)
                        if(heldCards[col, row] == 0)
                            heldCards[col, row] = -1;

            for(int col = 0; col < PlayerCount; ++col)
                if(heldCards.Column(col).SumPositive() == cardCounts[col])
                    for(int row = 0; row < PlayerCount; ++row)
                        if(heldCards[col, row] == 0)
                            heldCards[col, row] = -1;

            _pages.Add(new Turn(asked, card, response));

            iter++;
            if(iter > 30)
            {
                Log("Generation failed (code 1), retrying...", quiet: true);
                goto TryAgain;
            }

            Continue:

            current++;
            current %= PlayerCount;
        }
        while(heldCards.Rows().SelectMany(i => i).Any(i => i == 0));

        if(!CheckUnique())
        {
            Log("Generation failed (code 0), retrying...", quiet: true);
            goto TryAgain;
        }

        Log("Fish shown:");
        foreach(Turn t in _pages)
            Log(Fish[t.Asked].name + " " + Fish[t.Card + PlayerCount].name + " " + Fish[t.Response ? PlayerCount + PlayerCount : PlayerCount + PlayerCount + 1].name);

        _solution = Enumerable
            .Range(0, PlayerCount)
            .Select(p =>
                Enumerable
                .Repeat(p, 1)
                .Concat(
                    heldCards.Column(p)
                    .Select((e, i) => new { e, i })
                    .SelectMany(x => x.e <= 0 ? new int[0] : Enumerable.Repeat(x.i + PlayerCount, x.e)))
                .ToList())
            .ToList();
        Log("Solution: " + _solution.Select(pool => "{" + pool.Select(i => Fish[i].name).Join(" ") + "}").Join(""));

        foreach(List<int> l in _solution)
            l.Sort();

        _fishOrder = Enumerable.Range(0, 12).OrderBy(_ => Random.value).ToList();
        for(int button = 0; button < 12; ++button)
        {
            Transform btn = InputScreen.transform.GetChild(button);
            int j = _fishOrder[button];
            btn.GetComponentInChildren<SpriteRenderer>().sprite = Fish[j];
            btn.GetComponent<KMSelectable>().OnInteract += () => { InputFish(j); return false; };
        }

        _solutionCount = _solution.SelectMany(i => i).Count();
    }

    private void InputFish(int fish)
    {
        if(State != SuperpofishinState.Input)
            return;

        Sound();

        List<int> currentPool = new List<int>();
        List<int> consumed = new List<int>();
        for(int i = 0; i < _inputSequence.Count; i++)
        {
            currentPool.Add(_inputSequence[i]);
            currentPool.Sort();
            for(int player = 0; player < _solution.Count; player++)
            {
                if(!consumed.Contains(player) &&
                    currentPool.SequenceEqual(_solution[player]))
                {
                    consumed.Add(player);
                    currentPool.Clear();
                }
            }
        }

        List<int> validFish = new List<int>();
        IEnumerable<List<int>> available = Enumerable.Range(0, _solution.Count).Where(i => !consumed.Contains(i)).Select(i => _solution[i]);
        foreach(List<int> possible in available)
        {
            List<int> pool = possible.ToList();
            for(int i = 0; i < currentPool.Count; i++)
            {
                if(pool.Contains(currentPool[i]))
                    pool.RemoveAt(pool.IndexOf(currentPool[i]));
                else
                    goto Next;
            }

            validFish.AddRange(pool);

            Next:;
        }

        if(validFish.Contains(fish))
        {
            GetComponentInChildren<SpriteDisplay>().AddSprite(Fish[fish]);
            _inputSequence.Add(fish);
            Log("Input a " + Fish[fish].name + ".");
            if(_inputSequence.Count >= _solutionCount)
            {
                Log("Module solved.");
                GetComponent<KMBombModule>().HandlePass();
                State = SuperpofishinState.Solved;
            }
        }
        else
        {
            Log("That fish (" + Fish[fish].name + ") is invalid. Strike!");
            GetComponent<KMBombModule>().HandleStrike();
            _struck = true;
        }
    }

    private bool CheckUnique()
    {
        return Enumerable.Range(0, PlayerCount).AllArrangements().All(o =>
            CheckUnique(o.ToArray(), false, true)
            && CheckUnique(o.ToArray(), true, false)
            && CheckUnique(o.ToArray(), true, true));
    }

    private bool CheckUnique(int[] order, bool xor, bool swap)
    {
        int[,] heldCards = new int[PlayerCount, PlayerCount];
        int[] cardCounts = Enumerable.Repeat(4, PlayerCount).ToArray();

        for(int currentturn = 0; currentturn < _pages.Count; currentturn++)
        {
            int current = order[currentturn % 5];

            if(cardCounts[current] == 0)
                continue;

            int asked = _pages[currentturn].Asked;
            int card = _pages[currentturn].Card;
            bool response = _pages[currentturn].Response;

            if(heldCards[current, card] == 0)
                heldCards[current, card] = 1;

            if(current == asked
                || heldCards[current, card] == -1
                || response && heldCards[asked, card] == -1
                || !response && heldCards[asked, card] > 0
                || response && heldCards.Row(card).SumPositive() >= 4)
                return true;

            if(response)
            {
                heldCards[current, card]++;
                cardCounts[current]++;

                if(heldCards[asked, card] > 0)
                    heldCards[asked, card]--;

                cardCounts[asked]--;
            }
            else
            {
                heldCards[asked, card] = -1;
            }

            for(int row = 0; row < PlayerCount; ++row)
                if(heldCards.Row(row).SumPositive() == 4)
                    for(int col = 0; col < PlayerCount; ++col)
                        if(heldCards[col, row] == 0)
                            heldCards[col, row] = -1;

            for(int col = 0; col < PlayerCount; ++col)
                if(heldCards.Column(col).SumPositive() == cardCounts[col])
                    for(int row = 0; row < PlayerCount; ++row)
                        if(heldCards[col, row] == 0)
                            heldCards[col, row] = -1;
        }

        return false;
    }

    private void Log(string message, bool quiet = false)
    {
        Debug.Log((quiet ? "<" : "[") + "Superpofishin' #" + _id + (quiet ? "> " : "] ") + message);
    }

    private void Throw()
    {
        if(State != SuperpofishinState.Display)
            throw new UnreachableException("The throw button should be inaccessible.");

        if(Page == LastPage)
            State = SuperpofishinState.Input;
        else
            Page++;
    }

    private void Keep()
    {
        if(State != SuperpofishinState.Display)
            throw new UnreachableException("The keep button should be inaccessible.");

        if(Page == 0)
            State = SuperpofishinState.Start;
        else
            Page--;
    }

    private void Reel()
    {
        if(State == SuperpofishinState.Display)
            throw new UnreachableException("The reel button should be inaccessible.");

        Page = 0;
        State = SuperpofishinState.Display;
    }

    private void Sound()
    {
        Audio.PlaySoundAtTransform("Fishing_rod_cast", transform);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use ""!{0} keep|reel|throw"" to press that button once. Use ""!{0} throw 12"" to press that button twelve times. Use ""!{0} 1 2 3 4"" to press the first four input buttons in reading order.";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if(command == "reel")
        {
            if(State == SuperpofishinState.Start || State == SuperpofishinState.Input)
            {
                yield return null;
                MainButtons[1].OnInteract();
                yield break;
            }
            else
            {
                yield return "sendtochaterror That button (reel) is not pressable right now.";
                yield break;
            }
        }
        if(command == "keep")
        {
            if(State == SuperpofishinState.Display)
            {
                yield return null;
                MainButtons[0].OnInteract();
                yield break;
            }
            else
            {
                yield return "sendtochaterror That button (keep) is not pressable right now.";
                yield break;
            }
        }
        if(command == "throw")
        {
            if(State == SuperpofishinState.Display)
            {
                yield return null;
                MainButtons[2].OnInteract();
                yield break;
            }
            else
            {
                yield return "sendtochaterror That button (throw) is not pressable right now.";
                yield break;
            }
        }

        Regex re = new Regex(@"^(keep|throw)\s+([1-3]?\d)$");
        Match m = re.Match(command);
        if(m.Success)
        {
            if(State != SuperpofishinState.Display)
            {
                yield return "sendtochaterror That button (" + m.Groups[0].Value + ") is not pressable right now.";
                yield break;
            }
            yield return null;
            KMSelectable sel = m.Groups[0].Value == "keep" ? MainButtons[0] : MainButtons[2];
            int max = int.Parse(m.Groups[2].Value);
            for(int i = 0; i < max; i++)
            {
                sel.OnInteract();
                if(State != SuperpofishinState.Display)
                {
                    yield return "sendtochat The " + m.Groups[0].Value + " button was pressed " + (i + 1) + " times before it became unavailable.";
                    yield break;
                }
                if(i == max - 1)
                    yield break;
                foreach(object e in WaitWithCancel(0.1f, "The command was canceled after " + (i + 1) + "presses."))
                    yield return e;
            }
            yield break;
        }

        re = new Regex(@"^(?:[1-9]|1[012])(?:\s+(?:[1-9]|1[012])){0," + _solutionCount + "}$");
        if(re.IsMatch(command))
        {
            if(State != SuperpofishinState.Input)
            {
                yield return "sendtochaterror Those buttons are not pressable right now.";
                yield break;
            }
            yield return null;
            _struck = false;
            re = new Regex(@"\s+");
            int[] parts = re.Split(command).Select(s => int.Parse(s.Trim()) - 1).ToArray();

            for(int i = 0; i < parts.Length; i++)
            {
                InputButtons[parts[i]].OnInteract();
                if(State != SuperpofishinState.Input)
                    yield break;
                if(i == parts.Length - 1)
                    yield break;
                if(_struck)
                {
                    yield return "sendtochat " + (i + 1) + " buttons were pressed before a strike occured.";
                    yield break;
                }
                foreach(object e in WaitWithCancel(0.1f, "The command was canceled after " + (i + 1) + "presses."))
                    yield return e;
            }
            yield break;
        }

        yield break;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        Log("Autosolving...");

        while(State == SuperpofishinState.Start)
        {
            MainButtons[1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while(State == SuperpofishinState.Display)
        {
            MainButtons[2].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        while(!_isSolved)
        {
            List<int> currentPool = new List<int>();
            List<int> consumed = new List<int>();
            for(int i = 0; i < _inputSequence.Count; i++)
            {
                currentPool.Add(_inputSequence[i]);
                currentPool.Sort();
                for(int player = 0; player < _solution.Count; player++)
                {
                    if(!consumed.Contains(player) &&
                        currentPool.SequenceEqual(_solution[player]))
                    {
                        consumed.Add(player);
                        currentPool.Clear();
                    }
                }
            }

            List<int> validFish = new List<int>();
            IEnumerable<List<int>> available = Enumerable.Range(0, _solution.Count).Where(i => !consumed.Contains(i)).Select(i => _solution[i]);
            foreach(List<int> possible in available)
            {
                List<int> pool = possible.ToList();
                for(int i = 0; i < currentPool.Count; i++)
                {
                    if(pool.Contains(currentPool[i]))
                        pool.RemoveAt(pool.IndexOf(currentPool[i]));
                    else
                        goto Next;
                }

                validFish.AddRange(pool);

                Next:;
            }

            InputButtons[_fishOrder.IndexOf(validFish.PickRandom())].OnInteract();
            if(!_isSolved)
                yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerable WaitWithCancel(float time, string message)
    {
        float t = Time.time + time;
        while(Time.time < t)
            yield return "trycancel " + message;
    }

    [Serializable]
    private class UnreachableException : Exception
    {
        public UnreachableException()
        {
        }

        public UnreachableException(string message) : base(message)
        {
        }

        public UnreachableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnreachableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

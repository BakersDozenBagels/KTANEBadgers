using BadgerBoss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RNG = UnityEngine.Random;

public class BadgerBossScript : MonoBehaviour
{
    [SerializeField]
    private KMBombModule _module;
    [SerializeField]
    private Texture[] _possibleCardTextures;
    [SerializeField]
    private KMBombInfo _info;
    [SerializeField]
    private KMBossModule _boss;
    [SerializeField]
    private KMSelectable _badgerButton, _leftButton, _rightButton, _downButton, _foxButton;
    [SerializeField]
    private AudioScript _audio;
    [SerializeField]
    private GameObject _cardPrefab;
    [SerializeField]
    private Transform _cardSpawn, _cardGoal;
    [SerializeField]
    private GameObject _deck;

    private List<Rule> _rules = new List<Rule>();
    private List<Card> _possibleCards = new List<Card>();
    private List<GameObject> _cardObjects = new List<GameObject>();

    private int _id;
    private static int _idCounter;

    private static List<int> _stageActionCounts;
    private static List<List> _stageActions;
    private bool _isSolved, _hasBuzzed, _donePlaying;
    private static string[] _ignored;
    private int _cardsInStack, _selectedCard;
    private GameState _currentState;
    private List<GameState> _futureStates;
    private float _lastPressTime;
    private int _correctPlays;
    private bool _foxPressed = false;
    private const float MAXSOLVEPERCENTAGE = 0.85f;
    private const float MINSOLVEPERCENTAGE = 0.65f;
    private const int MAXACTIONSPERSTAGE = 30;
    private const int MINACTIONSPERSTAGE = 3;
    private const int MAXATTEMPTS = 1000;

    private void Start()
    {
        GetComponentInChildren<Light>().transform.localScale *= transform.lossyScale.x;
        _id = ++_idCounter;
        int attempt = 1;
        for(int i = 0; i < _possibleCardTextures.Length; i++)
            _possibleCards.Add(new Card(_possibleCardTextures[i], ((i + 1) % 13) + 1, i / 13));

        if(_ignored == null)
            _ignored = _boss.GetIgnoredModules(_module);
#if UNITY_EDITOR
        int stageCount = 3;
#else
        int stageCount = _info.GetSolvableModuleNames().Where(s => !_ignored.Contains(s)).Count();
#endif
        float targetSolvePercent = RNG.Range(MINSOLVEPERCENTAGE, MAXSOLVEPERCENTAGE);
        int maxStages = (int)(stageCount * targetSolvePercent) * MAXACTIONSPERSTAGE;
        int minStages = (int)(stageCount * targetSolvePercent) * MINACTIONSPERSTAGE;
        retry:;
        if(attempt > MAXATTEMPTS)
        {
            Debug.LogFormat("<That's The Badger #{0}> Rule generation had too many attempts! Going into error mode...", _id);
            ErrorMode();
            return;
        }
        Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: Start generation!", _id, attempt);
        _rules = new List<Rule>
        {
            RuleFactory.GetDefaultRules(),
            RuleFactory.GetNewRule()
        };
        Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: Decided on rule {2}.", _id, attempt, _rules.Last());
        GameState state = GameState.NewShuffle(Enumerable.Repeat(_possibleCards, 10).SelectMany(a => a));
        Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: Generated gamestate is: {2}.", _id, attempt, state);
        GameState initialState = state.DeepCopy();
        List<List> stages = new List<List>();
        Condition[] conds = _rules.SelectMany(r => r.GetConditions()).ToArray();
        _futureStates = new List<GameState>() { initialState };
        while(!conds.All(c => c.Count <= 0)) // Rule is currently ambiguous TODO: make this a bit better
        {
            stages.Add(AddStage(attempt, state, conds));
            if(stages.Count > maxStages)
            {
                Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: Discarding attempt as too many cards have been generated!", _id, attempt);
                attempt++;
                goto retry;
            }
        }
        if(stages.Count < minStages)
        {
            Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: Discarding attempt as too few cards have been generated!", _id, attempt);
            attempt++;
            goto retry;
        }
        // Here, we know we have a valid set of plays.
        Debug.LogFormat("[That's The Badger #{0}] Rule generation attempt #{1} succeeded!", _id, attempt);
        Debug.LogFormat("[That's The Badger #{0}] Your rule is: {1}.", _id, _rules.Last());
        float actionsPerStage = stages.Count / (stageCount * targetSolvePercent);
        List<int> actionsPerSolve = new List<int>();
        for(int i = 0; i < stageCount; i++)
            actionsPerSolve.Add(RNG.Range(Mathf.FloorToInt(actionsPerStage), Mathf.CeilToInt(actionsPerStage)));

        while(actionsPerSolve.Sum() > stages.Count)
        {
            int randIx = RNG.Range(0, actionsPerSolve.Count);
            if(actionsPerSolve[randIx] > MINACTIONSPERSTAGE)
                actionsPerSolve[randIx]--;
        }
        while(actionsPerSolve.Sum() < stages.Count)
        {
            int randIx = RNG.Range(0, actionsPerSolve.Count);
            if(actionsPerSolve[randIx] < MAXACTIONSPERSTAGE)
                actionsPerSolve[randIx]++;
        }

        int totalPossibleStages = _info.GetSolvableModuleNames().Count;
        while(actionsPerSolve.Count < totalPossibleStages)
            actionsPerSolve.Add(RNG.Range(Mathf.FloorToInt(actionsPerStage), Mathf.CeilToInt(actionsPerStage)));

        while(stages.Count < actionsPerSolve.Sum())
            stages.Add(AddStage(attempt, state, null));

        Debug.LogFormat("[That's The Badger #{0}] You're getting the following number of actions per stage: {1}.", _id, actionsPerSolve.Join(", "));
        string formatttedActions = "";
        int curIx = 0;
        for(int i = 0; i < actionsPerSolve.Count; i++)
        {
            formatttedActions += ", (" + stages.Skip(curIx).Take(actionsPerSolve[i]).Join(", ") + ")";
            curIx += actionsPerSolve[i];
        }
        Debug.LogFormat("[That's The Badger #{0}] You're getting the following actions per stage: {1}.", _id, formatttedActions);

        _stageActionCounts = actionsPerSolve;
        _stageActions = stages;

        _audio.PlayStackable(initialState.CurrentPlayOrder == GameState.PlayOrder.Clockwise ? "StartClockwise" : "StartCounterClockwise");
        _currentState = initialState;

        BuzzInMode();
    }

    private List AddStage(int attempt, GameState state, Condition[] conds)
    {
        IEnumerable<Card> validCards = state.Hands[state.CurrentPlayer].Cards.Where(c => _rules.All(r => r.IsValid(state, c)));
        List cl = new List();
        if(validCards.Count() > 0)
        {
            Card nextToPlay = validCards.PickRandom();
            Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: {2} is playing a {3}.", _id, attempt, state.CurrentPlayer, nextToPlay);
            foreach(Rule r in _rules)
                cl = r.ModifyState(state, nextToPlay, cl);
            if(nextToPlay.Rank == 1)
                state.CurrentPlayer = state.NextPlayer;
            for(int c = 0; c < conds.Length; c++)
                if(conds[c].Applies(state))
                    conds[c] = conds[c].Decrement();
        }
        else
        {
            Debug.LogFormat("<That's The Badger #{0}> Rule generation attempt #{1}: {2} is drawing a card.", _id, attempt, state.CurrentPlayer);
            foreach(Rule r in _rules)
                cl = r.ModifyState(state, cl);
        }
        state.CurrentPlayer = state.NextPlayer;
        _futureStates.Add(state.DeepCopy());
        return cl;
    }

    private void ErrorMode()
    {
        Debug.LogFormat("[That's The Badger #{0}] There was an error generating cards! Press the button at any time to solve the module.", _id);
        _deck.GetComponent<MeshRenderer>().enabled = false;
        _badgerButton.OnInteract += () => { _module.HandlePass(); return false; };
    }

    private void BuzzInMode()
    {
        StartCoroutine(WatchForSolves());
        _badgerButton.OnInteract += () => { if(_hasBuzzed || _info.GetSolvedModuleIDs().Count < 1) return false; _hasBuzzed = true; PlayingMode(); return false; };
    }

    private void PlayingMode()
    {
        Debug.LogFormat("[That's The Badger #{0}] You have entered the ring! Best of luck...", _id);
        StartCoroutine(DrawHand());
    }

    private IEnumerator DrawHand()
    {
        yield return new WaitUntil(() => _donePlaying);
        yield return new WaitForSeconds(DELAY);
        PlaySound(Phrase.ReenteringTheGame);

        List<Hand> newHands = _currentState.Hands.ToList();
        newHands.Add(new Hand("Defuser", new Card[0]));
        _currentState.Hands = newHands.ToArray();
        foreach(int i in new[] { 1, 2, 3, 4, 5 })
            _currentState.Hands[3].Cards.Add(_currentState.Deck.Pop());

        foreach(int i in new int[] { 0, 1, 2, 3, 4 })
        {
            Vector3 vec = new Vector3[] { new Vector3(0.1f, -.1f, -.11f), new Vector3(0.025f, -.1f, -.11f), new Vector3(-.05f, -.1f, -.11f), new Vector3(-.125f, -.1f, -.11f), new Vector3(-.2f, -.1f, -.11f) }[i];
            GameObject c = Instantiate(_cardPrefab, _cardSpawn);
            c.GetComponent<MeshRenderer>().material.mainTexture = _currentState.Hands[3].Cards[i].Texture;
            _cardObjects.Add(c);
            Debug.LogFormat("[That's The Badger #{0}] You drew a {1}.", _id, _currentState.Hands[3].Cards[i]);
            StartCoroutine(MoveObjectThenAction(c, vec, .75f, () => { StartCoroutine(Flip(c, 0.25f)); }));
        }
        _audio.PlayStackable("CardDraw");
        yield return new WaitForSeconds(1f);
        _badgerButton.OnInteract += () => { if(_isSolved) return false; PlayCard(); return false; };
        _leftButton.OnInteract += () => { if(_isSolved) return false; SelectNext(true); return false; };
        _rightButton.OnInteract += () => { if(_isSolved) return false; SelectNext(false); return false; };
        _downButton.OnInteract += () => { if(_isSolved) return false; Pass(); return false; };
        _foxButton.OnInteract += () => { if(_isSolved) return false; _foxPressed ^= true; _lastPressTime = Time.time; return false; };
        SelectNext(false);
        while(!_isSolved)
        {
            if(Time.time - 5f >= _lastPressTime)
            {
                OtherPlayersPlay();
            }
            if(_foxPressed)
                _foxButton.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f) * .02f;
            else
                _foxButton.transform.localScale = new Vector3(1f, 1f, 1f) * .02f;
            yield return null;
        }
    }

    private void Pass()
    {
        _lastPressTime = Time.time;
        if(_currentState.CurrentPlayer != 3)
        {
            PlaySound(Phrase.PlayingOutOfTurn);
            _module.HandleStrike();
            _foxPressed = false;
            return;
        }
        List cl = new List();
        Debug.LogFormat("[That's The Badger #{0}] You passed.", _id);
        foreach(Rule r in _rules)
            cl = r.ModifyState(_currentState, cl);

        RefreshHand();

        _currentState.CurrentPlayer = _currentState.NextPlayer;
        StartCoroutine(PlayChanges(cl).GetEnumerator());
        _foxPressed = false;
    }

    private void RefreshHand()
    {
        if(!_hasBuzzed)
            return;
        foreach(GameObject o in _cardObjects)
            Destroy(o);
        _cardObjects = new List<GameObject>();
        foreach(Card i in _currentState.Hands[3].Cards)
        {
            GameObject c = Instantiate(_cardPrefab, _cardSpawn);
            c.GetComponent<MeshRenderer>().material.mainTexture = i.Texture;
            _cardObjects.Add(c);
            c.transform.localEulerAngles = new Vector3(89f, -90f, -90f);
        }
        SelectNext(false);
    }

    private void OtherPlayersPlay()
    {
        _lastPressTime = Time.time;
        if(_currentState.CurrentPlayer == 3)
        {
            _module.HandleStrike();
            PlaySound(Phrase.FailureToPlayWithinFiveSeconds);
            _foxPressed = false;
            return;
        }
        IEnumerable<Card> validCards = _currentState.Hands[_currentState.CurrentPlayer].Cards.Where(c => _rules.All(r => r.IsValid(_currentState, c)));
        List cl = new List();
        if(validCards.Count() > 0)
        {
            Card nextToPlay = validCards.PickRandom();
            Debug.LogFormat("[That's The Badger #{0}] {1} is playing a {2}.", _id, _currentState.CurrentPlayer, nextToPlay);
            foreach(Rule r in _rules)
                cl = r.ModifyState(_currentState, nextToPlay, cl);
            if(nextToPlay.Rank == 1)
                _currentState.CurrentPlayer = _currentState.NextPlayer;
        }
        else
        {
            Debug.LogFormat("[That's The Badger #{0}] {2} is drawing a card.", _id, null, _currentState.CurrentPlayer);
            foreach(Rule r in _rules)
                cl = r.ModifyState(_currentState, cl);
        }
        _currentState.CurrentPlayer = _currentState.NextPlayer;

        StartCoroutine(PlayChanges(cl).GetEnumerator());
        _foxPressed = false;
    }

    private void PlayCard()
    {
        _lastPressTime = Time.time;
        if(_currentState.CurrentPlayer != 3)
        {
            PlaySound(Phrase.PlayingOutOfTurn);
            _module.HandleStrike();
            _foxPressed = false;
            return;
        }
        if(_rules.All(r => r.IsValid(_currentState, _currentState.Hands[3].Cards[_selectedCard])))
        {
            List cl = new List();

            Card nextToPlay = _currentState.Hands[3].Cards[_selectedCard];
            Debug.LogFormat("[That's The Badger #{0}] You are playing a {1}.", _id, nextToPlay);
            foreach(Rule r in _rules)
                cl = r.ModifyState(_currentState, nextToPlay, cl);
            if(nextToPlay.Rank == 1)
                _currentState.CurrentPlayer = _currentState.NextPlayer;

            if(_foxPressed ^ cl.Select(c => c.PhraseSaid[0]).Any(p => p == Phrase.ThatsTheFox))
            {
                if(_foxPressed)
                {
                    cl.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.ThatsTheFox, Phrase.None, Phrase.None } });
                    cl.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.Talking, Phrase.None, Phrase.None } });
                }
                else
                {
                    cl.RemoveAt(cl.Select(c => c.PhraseSaid[0] == Phrase.ThatsTheFox).IndexOf(b => b));
                    cl.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.FailureToSay, Phrase.None, Phrase.None } });
                    cl.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.ThatsTheFox, Phrase.None, Phrase.None } });
                    cl.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.ThatsTheFox, Phrase.None, Phrase.None } });
                }
            }

            _currentState.CurrentPlayer = _currentState.NextPlayer;
            StartCoroutine(PlayChanges(cl).GetEnumerator());
            Destroy(_cardObjects[_selectedCard]);
            _cardObjects.RemoveAt(_selectedCard);
            SelectNext(false);
            _correctPlays++;
            if(_correctPlays >= 7 || _currentState.Hands[3].Cards.Count == 0)
            {
                _module.HandlePass();
                _isSolved = true;
            }
            _foxPressed = false;
            return;
        }
        PlaySound(Phrase.BadCard);
        _module.HandleStrike();
        _foxPressed = false;
    }

    private void SelectNext(bool v)
    {
        _lastPressTime = Time.time;
        _selectedCard += v ? -1 : 1;
        if(_selectedCard < 0)
            _selectedCard = 0;
        if(_selectedCard > _cardObjects.Count - 1)
            _selectedCard = _cardObjects.Count - 1;
        for(int i = 0; i < _cardObjects.Count; i++)
        {
            GameObject g = _cardObjects[i];
            g.transform.localPosition = Vector3.Lerp(new Vector3(0.1f, -.1f, -.11f), new Vector3(-.2f, -.1f, -.11f), (float)i / _cardObjects.Count);
            if(i == _selectedCard)
                g.transform.localPosition = Vector3.Lerp(new Vector3(0.1f, -.1f, -.09f), new Vector3(-.2f, -.1f, -.09f), (float)i / _cardObjects.Count);
        }
    }

    private IEnumerator Flip(GameObject c, float time)
    {
        float startTime = Time.time;
        Vector3 startPos = c.transform.localEulerAngles;
        while(Time.time < startTime + time)
        {
            c.transform.localEulerAngles = Vector3.Lerp(startPos, new Vector3(80f, -90f, -90f), (Time.time - startTime) / time);
            yield return null;
        }
        c.transform.localEulerAngles = new Vector3(89f, -90f, -90f);
    }

    private IEnumerator WatchForSolves()
    {
        PlayCard(new Play(_currentState.CurrentPlayer, _currentState.LastPlayed));
        int currentlySolved = 0;
        while(!_isSolved && !_hasBuzzed)
        {
            int newSolves = _info.GetSolvedModuleNames().Where(s => !_ignored.Contains(s)).Count() - currentlySolved;
            for(int i = 0; i < newSolves; i++)
            {
                yield return new WaitForSeconds(DELAY);
                IEnumerable<List> cls = _stageActions.Skip(_stageActionCounts.Take(currentlySolved).Sum()).Take(_stageActionCounts[currentlySolved]);
                foreach(List cl in cls)
                {
                    foreach(object e in PlayChanges(cl))
                        yield return e;
                }
                currentlySolved++;
                _currentState = _futureStates[_stageActionCounts.Take(currentlySolved).Sum()];
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float PlaySound(Phrase phrase)
    {
        Debug.LogFormat("<That's The Badger #{0}> Playing phrase \"{1}\".", _id, phrase);
        if(phrase == Phrase.FailureToSay)
            _module.HandleStrike();
        IEnumerable<string> names = phrase.GetType().GetField(phrase.ToString()).GetCustomAttributes(typeof(SoundClipAttribute), false).Cast<SoundClipAttribute>().SelectMany(sca => sca.Names);
        if(names.Count() <= 0)
            return 1f;
        string name = names.PickRandom();
        _audio.PlayStackable(name);
        return _audio.AudioClips.First(a => a.name == name).length;
    }

    private const float DELAY = .75f;

    private IEnumerable PlayChanges(List changeList)
    {
        _donePlaying = false;
        foreach(Change c in changeList)
        {
            float DelayMod = 0f;
            switch(c.Type)
            {
                case Change.ChangeType.None:
                    DelayMod = .25f;
                    break;
                case Change.ChangeType.Draw:
                    DrawCard(c.CardDrawn);
                    DelayMod = .75f;
                    break;
                case Change.ChangeType.Phrase:
                    DelayMod = PlaySound(c.PhraseSaid[0] == Phrase.None ? c.PhraseSaid[1] == Phrase.None ? c.PhraseSaid[2] : c.PhraseSaid[1] : c.PhraseSaid[0]) - DELAY;
                    break;
                case Change.ChangeType.Play:
                    PlayCard(c.CardPlayed);
                    DelayMod = .75f;
                    break;
            }

            yield return new WaitForSeconds(DELAY + DelayMod);
        }
        _donePlaying = true;
    }

    private void DrawCard(int cardDrawn)
    {
        Debug.LogFormat("<That's The Badger #{0}> Player #{1} drew a card.", _id, cardDrawn);
        GameObject c = Instantiate(_cardPrefab, _cardSpawn);
        Vector3 vec;
        switch(cardDrawn)
        {
            case 0:
                vec = new Vector3(.1f, 0f, 0f);
                break;
            case 1:
                vec = new Vector3(0f, 0f, .15f);
                break;
            case 2:
                vec = new Vector3(-.2f, 0f, 0f);
                break;
            default:
                vec = new Vector3(0f, 0f, -.15f);
                break;
        }
        StartCoroutine(MoveObjectThenAction(c, vec, .75f, () => { Destroy(c); }));
        _audio.PlayStackable("CardDraw");
        if(cardDrawn == 3)
            RefreshHand();
    }

    private IEnumerator MoveObjectThenAction(GameObject c, Vector3 vec, float time, Action a)
    {
        float startTime = Time.time;
        Vector3 startPos = c.transform.localPosition;
        while(Time.time < startTime + time)
        {
            c.transform.localPosition = Vector3.Lerp(startPos, startPos + vec, (Time.time - startTime) / time);
            yield return null;
        }
        c.transform.localPosition = startPos + vec;
        a();
    }

    private void PlayCard(Play cardPlayed)
    {
        Debug.LogFormat("<That's The Badger #{0}> Player #{1} played a card: {2}.", _id, cardPlayed.Player, cardPlayed.Card);
        GameObject c = Instantiate(_cardPrefab, _cardGoal);
        Vector3 vec;
        switch(cardPlayed.Player)
        {
            case 0:
                vec = new Vector3(.2f, -.02f, 0f);
                break;
            case 1:
                vec = new Vector3(0f, -.02f, .15f);
                break;
            case 2:
                vec = new Vector3(-.1f, -.02f, 0f);
                break;
            default:
                vec = new Vector3(0f, -.02f, -.15f);
                break;
        }
        c.transform.localPosition = vec;
        c.GetComponent<MeshRenderer>().material.mainTexture = cardPlayed.Card.Texture;
        c.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        vec = new Vector3(vec.x, vec.y + 0.0003f * _cardsInStack, vec.z);
        StartCoroutine(MoveObjectThenAction(c, -vec, .75f, () => { _audio.PlayStackable("CardPlay"); }));
        _cardsInStack++;
    }
}

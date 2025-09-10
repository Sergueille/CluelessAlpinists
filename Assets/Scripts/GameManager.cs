using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PointerType
{
    normal, aim, notAllowed
}

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    [NonSerialized] public PlayerInfo[] menuInfos;
    [NonSerialized] public Player[] players;

    [NonSerialized] public BonusType bonusAtEndOfTurn = BonusType.none;

    public Map[] maps;

    public int maxPlayerCount = 6;
    public int cardsInHand = 3; // How many cards the players should draw
    public int bonusCardCount = 3; // How many cards the players should draw
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpVerticalMultiplier = 2;
    [SerializeField] private float jetpackForce = 8;
    [SerializeField] private float jetpackFuel = 2; // Seconds
    [SerializeField] private float jetPackVerticalMultiplier = 2;
    [SerializeField] private float jumpCollisionPreventionDuration = 0.3f;
    [SerializeField] private float balloonForce = 12;
    [SerializeField] private float balloonDuration = 12;
    [SerializeField] private float balloonSafeDuration = 0.3f;
    [SerializeField] private float balloonTargetVelocity = 3;
    [SerializeField] private float bombThrowForce = 10;
    [SerializeField] private float bombThrowVerticalMultiplier = 2;
    [SerializeField] private float grapplingThrowForce = 6;
    [SerializeField] private float grapplingThrowVerticalMultiplier = 2;
    [SerializeField] private float grapplingForce = 13;
    [SerializeField] private float grapplingMaxDuration = 6;
    [SerializeField] private float grapplingTargetDistance = 0.5f;
    [SerializeField] private float grapplingSafeDuration = 0.3f;
    [SerializeField] private float turnEndDelay = 2.1f;
    [SerializeField] private float turnEndVelocityThreshold = 1.0f;
    [SerializeField] private float cloudPlatformMaxDistance = 5.0f;
    [SerializeField] private float cloudPlatformSafeDuration = 0.3f;

    public Sprite[] itemsSprites;

    [NonSerialized] public int currentPlayerID;

    public bool raceStarted = false;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private float characterStartVerticalSpacing = 0.7f;
    [SerializeField] private float characterStartHorizontalShift = 0.01f;
    public float handYPosition = -305;
    public float handCardsSpacing = 100;
    public MovementDescr cardDrawMovement;
    public MovementDescr cardDrawMovementRotation;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private SpriteRenderer pointer;
    [SerializeField] private Sprite[] pointers;
    [SerializeField] private Button continueButton;
    [SerializeField] private MovementDescr continueButtonMovement;
    [SerializeField] private GameObject rankingEntryPrefab;
    [SerializeField] private Transform rankingParent;
    [SerializeField] private CanvasGroup finishScreenCanvas;
    [SerializeField] private CanvasGroup UIParent;
    [SerializeField] private float finishScreenTransitionDuration;
    [SerializeField] private Sprite[] skins;
    [SerializeField] private CanvasGroup pauseScreen;
    [SerializeField] private MovementDescr pauseScreenMovement;
    [SerializeField] private MovementDescr cardExchangeMovement;
    [SerializeField] private MovementDescr cardExchangeAppearMovement;
    [SerializeField] private MovementDescr cloudPlatformAppearMovement;
    [SerializeField] private GameObject cloudPlatformPrefab;
    public RectTransform exchangeIcon;
    public ParticleSystem exchangeIconParticles;
    public MovementDescr exchangeIconAppearMovement;
    public MovementDescr exchangeIconRotateMovement;

    [HideInInspector] public bool cursorNotAllowedOverride = false;
    [HideInInspector] public bool cursorOverPauseButton = false;

    [SerializeField] private float smallDelay = 0.1f;

    [SerializeField] private MovementDescr transitionMovement;
    [SerializeField] private Material transitionMaterial;

    [NonSerialized] private LocalizationManager.Language language = LocalizationManager.Language.systemLanguage;

    [NonSerialized] public bool shouldContinue;

    private bool finishedRace = false;

    private Card selectedExchangeNewCard = null;
    private Card selectedExchangeDeckCard = null;
    private bool grapplingTouchedSomething = false;

    private int playersFinished = 0;

    private Coroutine raceCoroutine;

    [NonSerialized] public PointerType pointerType = PointerType.normal;

    private Vector3 continueButtonStartPosition;


    public int PlayerCount
    {
        get => players.Length;
    }

    public Player CurrentPlayer
    {
        get => players[currentPlayerID];
    }

    public Character CurrentPlayerCharacter
    {
        get => players[currentPlayerID].character;
    }

    private void Awake()
    {
        i = this;
        LocalizationManager.Init();
    }

    private void Start()
    {
        infoText.text = "";
        finishScreenCanvas.alpha = 0;
        UIParent.alpha = 0;
        Cursor.visible = false;
        continueButtonStartPosition = continueButton.transform.localPosition;
        ToggleContinueButton(false, true);

        pauseScreen.blocksRaycasts = false;
        pauseScreen.alpha = 0;

        menuInfos = new PlayerInfo[maxPlayerCount];
        for (int i = 0; i < maxPlayerCount; i++)
        {
            menuInfos[i] = new PlayerInfo("");
            menuInfos[i].activated = false;
            menuInfos[i].skin = skins[i];
        }

        transitionMovement.Do(t => transitionMaterial.SetFloat("_Size", t));
        AudioListener.volume = 1;

        HideExchangeIcon();

        LocalizationManager.UpdateLanguage(language);
    }

    public void Play(int mapId)
    {
        Map mapToLoad = maps[mapId];

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(CameraController.i.gameObject);
        DontDestroyOnLoad(pointer);

        PanelsManager.i.HidePanel();

        transitionMovement.DoNormalized(t => AudioListener.volume = 1 - t);
        transitionMovement.DoReverse(t => transitionMaterial.SetFloat("_Size", t)).setOnComplete(() =>
        {
            transitionMaterial.SetFloat("_Size", 0);
            SceneManager.LoadScene(mapToLoad.sceneName);
            AudioListener.volume = 0;
            transitionMovement.Do(t => { }) // HACK: setOnUpdate overrides this
            .setOnUpdate((float t) =>
            {
                transitionMaterial.SetFloat("_Size", t);
                CameraController.i.followCharacter = false;
                Vector3 startPos = MapManager.i.finishTrigger.transform.position;
                startPos.z = CameraController.i.transform.position.z;
                CameraController.i.SetPositionEndTargetImmediate(startPos);
            })
            .setOnComplete(() =>
            {
                CameraController.i.followCharacter = true;
                StartRace(menuInfos);
            });

            transitionMovement.DoNormalized(t => AudioListener.volume = t);
        });
    }

    private void Update()
    {
        Vector3 pointerPosition = CameraController.i.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pointerPosition.z = 0;
        pointer.transform.position = pointerPosition;
        Cursor.visible = false;

        pointer.sprite = pointers[cursorNotAllowedOverride ? (int)PointerType.notAllowed : (int)pointerType];

        if (Input.GetMouseButtonDown(0) && cursorNotAllowedOverride)
        {
            SoundManager.PlaySound("buzzer");
        }
    }

    private void StartRace(PlayerInfo[] infos)
    {
        UIParent.alpha = 1;
        CreatePlayers(infos);
        currentPlayerID = 0;
        playersFinished = 0;
        cursorOverPauseButton = false;
        raceStarted = true;
        finishedRace = false;
        CameraController.i.followCharacter = true;
        HideExchangeIcon();
        raceCoroutine = StartCoroutine(RaceCoroutine());
    }

    public IEnumerator RaceCoroutine()
    {
        while (true)
        {
            bonusAtEndOfTurn = BonusType.none;
            CurrentPlayer.turns++;

            for (int i = 0; i < cardsInHand; i++)
            {
                bool couldDraw = CurrentPlayer.DrawAction();

                if (!couldDraw) break;
                yield return new WaitForSeconds(smallDelay);
            }

            // Show continue button
            ToggleContinueButton(true);

            SetInfoText(LocalizationManager.GetValue("reorder"));

            yield return new WaitUntil(() => shouldContinue || CurrentPlayer.finished);
            shouldContinue = false;

            ToggleContinueButton(false);

            // Darken cards
            foreach (Card card in CurrentPlayer.hand)
            {
                card.Dark();
                card.draggable = false;
                card.moveOnHover = false;
            }

            foreach (Card card in CurrentPlayer.hand)
            {
                if (CurrentPlayer.finished) break;

                // Highlight card
                card.Light();
                card.transform.SetAsLastSibling();

                ActionType type = card.type;

                // yield return new WaitUntil(() => !Input.GetMouseButton(0));

                if (type == ActionType.jump)
                {
                    SetPointerType(PointerType.aim);

                    yield return new WaitUntil(() => !GetValidClick());

                    while (true && !CurrentPlayer.finished)
                    {
                        if (CurrentPlayerCharacter.IsTouchingGround())
                        {
                            SetInfoText(LocalizationManager.GetValue("jump"));
                            SetPointerType(PointerType.aim);

                            Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * jumpForce;
                            force.y *= jumpVerticalMultiplier;
                            CurrentPlayerCharacter.DisplayJumpTrajectory(force, CurrentPlayerCharacter.GetComponent<Rigidbody2D>().linearDamping);

                            if (GetValidClick())
                            {
                                CurrentPlayerCharacter.AddImpulse(force);
                                SoundManager.PlaySound("boing");

                                CurrentPlayerCharacter.shouldPreventJumpCollisisons = true;

                                yield return new WaitForSeconds(jumpCollisionPreventionDuration);

                                CurrentPlayerCharacter.shouldPreventJumpCollisisons = false;

                                break;
                            }
                        }
                        else
                        {
                            SetInfoText(LocalizationManager.GetValue("jump_wait"));
                            SetPointerType(PointerType.notAllowed);
                        }

                        yield return new WaitForEndOfFrame();
                    }
                }
                else if (type == ActionType.jetpack)
                {
                    yield return new WaitForFixedUpdate();

                    SetInfoText(LocalizationManager.GetValue("jetpack"));
                    SetPointerType(PointerType.aim);

                    float fuel = jetpackFuel;

                    SoundManager.LoopSoundHandle handle = null;

                    while (fuel > 0 && !CurrentPlayer.finished)
                    {
                        if (GetValidClick())
                        {
                            Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * jetpackForce;
                            force.y *= jetPackVerticalMultiplier;

                            CurrentPlayerCharacter.AddForce(force);
                            fuel -= Time.fixedDeltaTime;

                            CurrentPlayerCharacter.ToggleJetpackParticles(true, force);

                            if (handle == null)
                            {
                                handle = SoundManager.PlayLoopSound("ext_begin", "ext_loop", "ext_end");
                            }
                        }
                        else
                        {
                            CurrentPlayerCharacter.ToggleJetpackParticles(false, Vector2.right);

                            if (handle != null)
                            {
                                handle.Stop();
                                handle = null;
                            }
                        }

                        yield return new WaitForFixedUpdate();
                    }

                    if (handle != null)
                    {
                        handle.Stop();
                    }

                    CurrentPlayerCharacter.ToggleJetpackParticles(false, Vector2.right);
                }
                else if (type == ActionType.bomb || type == ActionType.invertedBomb)
                {
                    SetInfoText(LocalizationManager.GetValue("throw"));
                    SetPointerType(PointerType.aim);

                    yield return new WaitUntil(() => !GetValidClick());

                    while (true && !CurrentPlayer.finished)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * bombThrowForce;
                        force.y *= bombThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force, 0.0f);

                        yield return new WaitForEndOfFrame();

                        if (GetValidClick())
                        {
                            CurrentPlayerCharacter.SpawnBomb(force, type == ActionType.invertedBomb);
                            SoundManager.PlaySound("throw");
                            break;
                        }
                    }
                }
                else if (type == ActionType.balloon)
                {
                    SetInfoText(LocalizationManager.GetValue("balloons"));

                    float startTime = Time.time;

                    CurrentPlayerCharacter.ShowBalloons();
                    SoundManager.PlaySound("balloon");

                    bool haveReleased = false;

                    while (Time.time < startTime + balloonDuration && !CurrentPlayer.finished
                       && GetValidClick() && haveReleased && Time.time > startTime + balloonSafeDuration)
                    {
                        if (CurrentPlayerCharacter.rb.linearVelocity.y < balloonTargetVelocity)
                        {
                            CurrentPlayerCharacter.AddForce(Vector2.up * balloonForce);
                        }

                        if (!GetValidClick())
                        {
                            haveReleased = true;
                        }

                        yield return new WaitForFixedUpdate();
                    }

                    CurrentPlayerCharacter.HideBalloons();
                    SoundManager.PlaySound("balloon_pop");
                }
                else if (type == ActionType.grappling)
                {
                    SetInfoText(LocalizationManager.GetValue("grappling"));
                    SetPointerType(PointerType.aim);

                    yield return new WaitUntil(() => !GetValidClick());

                    while (!CurrentPlayer.finished)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * grapplingThrowForce;
                        force.y *= grapplingThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force, 0.0f);

                        yield return new WaitForEndOfFrame();

                        if (GetValidClick())
                        {
                            SoundManager.PlaySound("throw");

                            grapplingTouchedSomething = false;
                            Grappling grappling = CurrentPlayerCharacter.SpawnGrappling(force, () =>
                            {
                                grapplingTouchedSomething = true;
                            });

                            float throwTime = Time.time;

                            yield return new WaitUntil(() =>
                                (Time.time - throwTime > grapplingSafeDuration && grapplingTouchedSomething && !GetValidClick())
                             || CurrentPlayer.finished || Time.time - throwTime > 10
                            );

                            yield return new WaitForFixedUpdate();

                            if (!grapplingTouchedSomething) break; // Prevent soflock if grappling went out of the map

                            SetPointerType(PointerType.normal);

                            SetInfoText(LocalizationManager.GetValue("grappling_end"));

                            SoundManager.SoundHandle handle = SoundManager.PlaySound("grap_loop", true);

                            float startTime = Time.time;
                            bool nearEnough = false;
                            while (Time.time < startTime + grapplingMaxDuration && !nearEnough && !GetValidClick() && !CurrentPlayer.finished)
                            {
                                Vector2 delta = grappling.transform.position - CurrentPlayerCharacter.transform.position;
                                CurrentPlayerCharacter.AddForce(grapplingForce * delta.normalized);

                                yield return new WaitForFixedUpdate();
                                nearEnough = delta.magnitude < grapplingTargetDistance;
                            }

                            handle.Stop();
                            grappling.RemoveRope();

                            break;
                        }
                    }
                }
                else if (type == ActionType.cloudPlatform)
                {
                    SetInfoText(LocalizationManager.GetValue("platform_cloud"));
                    SetPointerType(PointerType.aim);

                    yield return new WaitUntil(() => !GetValidClick());

                    Vector2 delta;
                    Vector2 targetPosition;
                    bool ok;

                    float startTime = Time.time;

                    while (true)
                    {
                        do
                        {
                            delta = GetPointerDelta(CurrentPlayerCharacter.transform.position);
                            targetPosition = (Vector2)CurrentPlayerCharacter.transform.position + delta;
                            ok = IsPlatformAllowedOnPoint(targetPosition);

                            SetPointerType(delta.magnitude < cloudPlatformMaxDistance && ok ? PointerType.aim : PointerType.notAllowed);

                            yield return new WaitForEndOfFrame();
                        }
                        while ((!GetValidClick() && !CurrentPlayer.finished) || Time.time - startTime < cloudPlatformSafeDuration);

                        if (CurrentPlayer.finished) { break; }

                        if (delta.magnitude > cloudPlatformMaxDistance)
                        {
                            delta = delta.normalized * cloudPlatformMaxDistance;
                        }

                        ok = IsPlatformAllowedOnPoint(targetPosition);

                        if (ok)
                        {
                            SoundManager.PlaySound("pchit");

                            GameObject platform = Instantiate(cloudPlatformPrefab);
                            platform.transform.position = CurrentPlayerCharacter.transform.position + (Vector3)delta;

                            cloudPlatformAppearMovement.Do(t =>
                            {
                                if (platform != null)
                                {
                                    platform.transform.localScale = Vector3.one * t;
                                }
                            });

                            break;
                        }
                    }
                }

                SetPointerType(PointerType.normal);
                card.Dark();
            }

            CurrentPlayer.DiscardHand();

            SetInfoText("");

            if (!CurrentPlayer.finished)
            {
                yield return new WaitForSeconds(turnEndDelay);
                yield return new WaitUntil(() => CurrentPlayerCharacter.rb.linearVelocity.magnitude < turnEndVelocityThreshold);
            }

            if (!CurrentPlayer.finished && bonusAtEndOfTurn == BonusType.exchange)
            {
                SetInfoText(LocalizationManager.GetValue("exchange"));

                selectedExchangeNewCard = null;
                selectedExchangeDeckCard = null;

                // Deck cards
                Card[] deckCards = new Card[CurrentPlayer.allActions.Count];
                for (int i = 0; i < CurrentPlayer.allActions.Count; i++)
                {
                    Card card = InstantiateCard(CurrentPlayer.allActions[i]);
                    card.moveOnHover = true;
                    deckCards[i] = card;

                    Vector3 targetPosition = new Vector3(GetHandXPosition(i, CurrentPlayer.allActions.Count), handYPosition, 0);
                    cardDrawMovement.DoReverse(t => card.transform.localPosition = targetPosition + new Vector3(0, -1, 0) * t);

                    card.clickCallback = c =>
                    {
                        selectedExchangeDeckCard = c;
                        for (int i = 0; i < deckCards.Length; i++)
                        {
                            if (deckCards[i] != c) deckCards[i].Dark();
                            else c.Light();
                        }
                    };
                }

                // New cards
                ActionType[] randomActions = GetRandomActions();
                Card[] randomCards = new Card[randomActions.Length];
                for (int i = 0; i < randomActions.Length; i++)
                {
                    Card card = InstantiateCard(randomActions[i]);
                    card.moveOnHover = true;
                    randomCards[i] = card;

                    Vector3 targetPosition = new Vector3(GetHandXPosition(i, randomActions.Length), handYPosition + 300, 0);
                    cardExchangeAppearMovement.DoReverse(t => card.transform.localPosition = targetPosition + Vector3.up * t);

                    card.clickCallback = c =>
                    {
                        selectedExchangeNewCard = c;
                        for (int i = 0; i < randomCards.Length; i++)
                        {
                            if (randomCards[i] != c) randomCards[i].Dark();
                            else c.Light();
                        }
                    };
                }

                yield return new WaitUntil(() => selectedExchangeNewCard != null && selectedExchangeDeckCard != null);

                CurrentPlayer.RemoveCard(selectedExchangeDeckCard);
                CurrentPlayer.AddAction(selectedExchangeNewCard.type);

                Vector3 newPos = selectedExchangeNewCard.transform.localPosition;
                Vector3 deckPos = selectedExchangeDeckCard.transform.localPosition;

                cardExchangeMovement.DoMovement(pos => selectedExchangeDeckCard.transform.localPosition = pos, deckPos, newPos);
                cardExchangeMovement.DoMovement(pos => selectedExchangeNewCard.transform.localPosition = pos, newPos, deckPos);

                yield return new WaitForSeconds(cardExchangeMovement.duration + smallDelay);

                for (int i = 0; i < deckCards.Length; i++)
                {
                    Card currentCard = deckCards[i];
                    if (currentCard == selectedExchangeDeckCard) currentCard = selectedExchangeNewCard; // Swap the animations for exchanged cards

                    Vector3 startPosition = currentCard.transform.localPosition;
                    cardDrawMovement.Do(t => currentCard.transform.localPosition = startPosition + Vector3.down * t)
                        .setOnComplete(() => Destroy(currentCard.gameObject));
                }

                for (int i = 0; i < randomCards.Length; i++)
                {
                    Card currentCard = randomCards[i];
                    if (currentCard == selectedExchangeNewCard) currentCard = selectedExchangeDeckCard; // Same but inverted

                    Vector3 startPosition = currentCard.transform.localPosition;
                    cardExchangeAppearMovement.Do(t => currentCard.transform.localPosition = startPosition + Vector3.up * t)
                        .setOnComplete(() => Destroy(currentCard.gameObject));
                }
            }

            HideExchangeIcon();
            SetInfoText("");
            SetPointerType(PointerType.normal);
            bonusAtEndOfTurn = BonusType.none;

            if (finishedRace)
            {
                yield break;
            }

            // Wait a little before next turn
            if (CurrentPlayer.finished)
            {
                yield return new WaitForSeconds(3);
                CameraController.i.followCharacter = true;
            }

            // Next turn! (repeat if players have finished race)
            do
            {
                currentPlayerID++;
                currentPlayerID %= PlayerCount;
            } while (CurrentPlayer.finished);
        }
    }

    public void Continue()
    {
        shouldContinue = true;
    }

    private void CreatePlayers(PlayerInfo[] infos)
    {
        int playerCount = 0;
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].activated)
                playerCount++;
        }

        players = new Player[playerCount];

        int playerID = 0;
        for (int i = 0; i < infos.Length; i++)
        {
            if (!infos[i].activated) continue;

            players[playerID] = new Player();
            players[playerID].info = infos[i];

            players[playerID].SetDefaultCards();

            players[playerID].character = InstantiateCharacter(players[playerID]); // Create character GameObject

            playerID++;
        }

        // Shuffle players afterwards instead of infos because this struct can be large
        Util.ShuffleArray(players);

        for (int i = 0; i < PlayerCount; i++)
        {
            // Move characters to start zone
            float random = UnityEngine.Random.Range(-characterStartHorizontalShift, characterStartHorizontalShift);
            Vector3 shift = Vector2.up * characterStartVerticalSpacing * i + Vector2.right * random;
            players[i].character.transform.position = MapManager.i.startZone.position + shift;
        }
    }

    private Character InstantiateCharacter(Player owner)
    {
        Character res = Instantiate(characterPrefab).GetComponent<Character>();
        res.Init(owner);
        return res;
    }

    public Card InstantiateCard(ActionType type)
    {
        Card res = Instantiate(i.cardPrefab, UIParent.transform).GetComponent<Card>();
        res.Init(type);

        return res;
    }

    public float GetHandXPosition(int cardID, int cardCount)
    {
        return (cardID + 0.5f - (float)cardCount / 2) * handCardsSpacing;
    }

    public Vector2 GetPointerDirection(Vector2 pos)
    {
        return GetPointerDelta(pos).normalized;
    }

    public Vector2 GetPointerDelta(Vector2 pos)
    {
        Vector2 worldMousePos = CameraController.i.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return worldMousePos - pos;
    }

    public void SetInfoText(string text)
    {
        infoText.text = text;
    }

    public ActionType[] GetRandomActions()
    {
        ActionType[] res = new ActionType[bonusCardCount];
        bool[] alreadyPicked = new bool[(int)ActionType.maxValue];
        for (int i = 0; i < bonusCardCount; i++)
        {
            ActionType newAction = (ActionType)UnityEngine.Random.Range(0, (int)ActionType.maxValue);

            if (alreadyPicked[(int)newAction])
            {
                i--;
                continue;
            }

            res[i] = newAction;
            alreadyPicked[(int)newAction] = true;
        }

        return res;
    }

    public void SetPointerType(PointerType type)
    {
        pointerType = type;
    }

    public void PlayerFinishesRace(Player player)
    {
        MapManager.i.finishParticles.Play();
        CameraController.i.followCharacter = false;

        playersFinished++;
        player.rank = playersFinished;
        player.finished = true;

        SoundManager.PlaySound("tadaa");
        SoundManager.PlaySound("blower");

        if (playersFinished >= PlayerCount - 1)
        {
            finishedRace = true;
            StartCoroutine(ShowRankCoroutine());
        }
    }

    private IEnumerator ShowRankCoroutine()
    {
        LeanTween.alphaCanvas(finishScreenCanvas, 1, finishScreenTransitionDuration);
        LeanTween.alphaCanvas(UIParent, 0, finishScreenTransitionDuration);

        SetPointerType(PointerType.normal);

        yield return new WaitForSeconds(finishScreenTransitionDuration);

        raceStarted = false;

        if (PlayerCount > 1)
        {
            for (int i = 1; i < PlayerCount; i++)
            {
                // Search player with rank
                foreach (Player p in players)
                {
                    if (p.rank == i)
                    {
                        InstantiateRankEntry(p);
                        break;
                    }
                }

                yield return new WaitForSeconds(smallDelay);
            }

            // Search last player
            foreach (Player p in players)
            {
                if (p.rank == -1)
                {
                    InstantiateRankEntry(p);
                    break;
                }
            }
        }
        else
        {
            InstantiateRankEntry(players[0]);
        }

        shouldContinue = false;
        ToggleContinueButton(true);
        yield return new WaitUntil(() => shouldContinue);
        shouldContinue = false;

        ReturnToMenu();
    }

    private RankingEntry InstantiateRankEntry(Player player)
    {
        RankingEntry res = Instantiate(rankingEntryPrefab, rankingParent).GetComponent<RankingEntry>();
        res.Init(player);

        return res;
    }

    private void ToggleContinueButton(bool visible, bool immediate = false)
    {
        if (immediate)
        {
            continueButton.transform.localPosition = visible ? continueButtonStartPosition : continueButtonStartPosition - Vector3.down * continueButtonMovement.amplitude;
        }

        if (visible)
            continueButtonMovement.DoReverse((t) => continueButton.transform.localPosition = continueButtonStartPosition + Vector3.down * t);
        else
            continueButtonMovement.Do((t) => continueButton.transform.localPosition = continueButtonStartPosition + Vector3.down * t);
    }

    public void TogglePauseScreen(bool enabled)
    {
        if (enabled)
        {
            Time.timeScale = 0;
            pauseScreen.blocksRaycasts = true;
            pauseScreenMovement.DoNormalized(t => pauseScreen.alpha = t).setIgnoreTimeScale(true);
        }
        else
        {
            Time.timeScale = 1;
            pauseScreen.blocksRaycasts = false;
            pauseScreenMovement.DoNormalized(t => pauseScreen.alpha = 1 - t);
        }
    }

    public void ReturnToMenu()
    {
        transitionMovement.DoReverse(t => transitionMaterial.SetFloat("_Size", t)).setIgnoreTimeScale(true).setOnComplete(() =>
        {
            Time.timeScale = 1;
            finishScreenCanvas.alpha = 0;
            AudioListener.volume = 1;
            SceneManager.LoadScene("Menu");

            Destroy(pointer.gameObject);
            Destroy(CameraController.i.gameObject);
            Destroy(gameObject);
        });
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeLanguage()
    {
        language++;
        if (language == LocalizationManager.Language.maxValue)
            language = LocalizationManager.Language.systemLanguage + 1;
        LocalizationManager.UpdateLanguage(language);
    }

    public bool IsPlatformAllowedOnPoint(Vector2 position)
    {
        foreach (PlatformPrevention prevention in FindObjectsByType<PlatformPrevention>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            BoxCollider2D coll = prevention.gameObject.GetComponent<BoxCollider2D>();

            if (coll.bounds.Contains(position))
            {
                return false;
            }
        }

        return true;
    }

    public void ShowExchangeIcon()
    {
        exchangeIcon.gameObject.SetActive(true);
        exchangeIconParticles.gameObject.SetActive(true);

        exchangeIconAppearMovement.Do(t =>
        {
            exchangeIcon.localScale = Vector3.one * t;
        });

        exchangeIconRotateMovement.Do(t =>
        {
            exchangeIcon.eulerAngles = new Vector3(0, 0, t);
        });

        exchangeIconParticles.Play();
    }

    public void HideExchangeIcon()
    {
        exchangeIcon.gameObject.SetActive(false);
        exchangeIconParticles.gameObject.SetActive(false);
    }

    public bool GetValidClickDown()
    {
        return Input.GetMouseButtonDown(0) && !cursorNotAllowedOverride && !cursorOverPauseButton;
    }

    public bool GetValidClick()
    {
        return Input.GetMouseButton(0) && !cursorNotAllowedOverride && !cursorOverPauseButton;
    }


    public void CursorEntersPauseButton()
    {
        cursorOverPauseButton = true;
    }

    public void CursorLeavesPauseButton()
    {
        cursorOverPauseButton = false;
    }
}

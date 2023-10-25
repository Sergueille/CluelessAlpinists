using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum PointerType
{
    normal, aim, notAllowed
}

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    [SerializeField] private PlayerInfo[] startInfos; // TEST
    [NonSerialized] public Player[] players;

    [NonSerialized] public BonusType bonusAtEndOfTurn = BonusType.none;

    public int cardsInHand = 3; // How many cards the players should draw
    public int bonusCardCount = 3; // How many cards the players should draw
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpVerticalMultiplier = 2;
    [SerializeField] private float jetpackForce = 8;
    [SerializeField] private float jetpackFuel = 2; // Seconds
    [SerializeField] private float jetPackVerticalMultiplier = 2;
    [SerializeField] private float balloonForce = 12;
    [SerializeField] private float balloonDuration = 12;
    [SerializeField] private float balloonTargetVelocity = 3;
    [SerializeField] private float bombThrowForce = 10;
    [SerializeField] private float bombThrowVerticalMultiplier = 2;
    [SerializeField] private float grapplingThrowForce = 6;
    [SerializeField] private float grapplingThrowVerticalMultiplier = 2;
    [SerializeField] private float grapplingForce = 13;
    [SerializeField] private float grapplingMaxDuration = 6;
    [SerializeField] private float grapplingTargetDistance = 0.5f;
    [SerializeField] private float turnEndDelay = 2.1f;
    [SerializeField] private float turnEndVelocityThreshold = 1.0f;

    public Sprite[] itemsSprites;

    [NonSerialized] public int currentPlayerID;


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

    [SerializeField] private float smallDelay = 0.1f;

    [NonSerialized] public bool shouldContinue;

    private Card selectedExchangeNewCard = null;
    private Card selectedExchangeDeckCard = null;
    private bool grapplingTouchedSomething = false;

    private Coroutine raceCoroutine;

    [NonSerialized] public PointerType pointerType = PointerType.normal;


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
    }

    private void Start()
    {
        infoText.text = "";
        StartRace(startInfos);

        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 pointerPosition = CameraController.i.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pointerPosition.z = 0;
        pointer.transform.position = pointerPosition;
        Cursor.visible = false;
    }

    private void StartRace(PlayerInfo[] infos)
    {
        CreatePlayers(infos);
        currentPlayerID = 0;
        raceCoroutine = StartCoroutine(RaceCoroutine());
    } 

    public IEnumerator RaceCoroutine()
    {
        while (true)
        {
            bonusAtEndOfTurn = BonusType.none;

            for (int i = 0; i < cardsInHand; i++)
            {
                bool couldDraw = CurrentPlayer.DrawAction();

                if (!couldDraw) break;
                yield return new WaitForSeconds(smallDelay);
            }

            SetInfoText("Réordonnez les cartes");

            yield return new WaitUntil(() => shouldContinue); // TEST
            shouldContinue = false;

            // Darken cards
            foreach (Card card in CurrentPlayer.hand)
            {
                card.Dark();
                card.draggable = false;
                card.moveOnHover = false;
            }

            foreach (Card card in CurrentPlayer.hand)
            {
                // Highlight card
                card.Light();
                card.transform.SetAsLastSibling();

                ActionType type = card.type;

                yield return new WaitUntil(() => !Input.GetMouseButton(0));

                if (type == ActionType.jump)
                {
                    SetInfoText("Cliquez pour sauter");
                    SetPointerType(PointerType.aim);

                    while (true)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * jumpForce;
                        force.y *= jumpVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (CurrentPlayerCharacter.IsTouchingGround() && Input.GetMouseButton(0))
                        {
                            CurrentPlayerCharacter.AddImpulse(force);
                            break;
                        }
                    }
                }
                else if (type == ActionType.jetpack)
                {
                    yield return new WaitForFixedUpdate();

                    SetInfoText("Maintenez pour vous propulser");
                    SetPointerType(PointerType.aim);

                    float fuel = jetpackFuel;

                    while (fuel > 0)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * jetpackForce;
                            force.y *= jetPackVerticalMultiplier;

                            CurrentPlayerCharacter.AddForce(force);
                            fuel -= Time.fixedDeltaTime;

                            CurrentPlayerCharacter.ToggleJetpackParticles(true, force);
                        }
                        else
                        {
                            CurrentPlayerCharacter.ToggleJetpackParticles(false, Vector2.right);
                        }

                        yield return new WaitForFixedUpdate();
                    }

                    CurrentPlayerCharacter.ToggleJetpackParticles(false, Vector2.right);
                }
                else if (type == ActionType.bomb || type == ActionType.invertedBomb)
                {
                    SetInfoText("Cliquez pour lancer");
                    SetPointerType(PointerType.aim);

                    while (true)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * bombThrowForce;
                        force.y *= bombThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (Input.GetMouseButton(0))
                        {
                            CurrentPlayerCharacter.SpawnBomb(force, type == ActionType.invertedBomb);
                            break;
                        }
                    }
                }
                else if (type == ActionType.balloon)
                {
                    SetInfoText("Cliquez pour éclater les ballons");

                    float startTime = Time.time;

                    CurrentPlayerCharacter.ShowBalloons();

                    while (!Input.GetMouseButton(0) && Time.time < startTime + balloonDuration)
                    {
                        if (CurrentPlayerCharacter.rb.velocity.y < balloonTargetVelocity)
                        {
                            CurrentPlayerCharacter.AddForce(Vector2.up * balloonForce);
                        }

                        yield return new WaitForFixedUpdate();
                    }

                    CurrentPlayerCharacter.HideBalloons();
                }
                else if (type == ActionType.grappling)
                {
                    SetInfoText("Cliquez pour lancer le grappin");
                    SetPointerType(PointerType.aim);

                    while (true)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * grapplingThrowForce;
                        force.y *= grapplingThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (Input.GetMouseButton(0))
                        {
                            grapplingTouchedSomething = false;
                            Grappling grappling = CurrentPlayerCharacter.SpawnGrappling(force, () => {
                                grapplingTouchedSomething = true;
                            });

                            yield return new WaitUntil(() => grapplingTouchedSomething && !Input.GetMouseButton(0));
                            yield return new WaitForFixedUpdate();
                            
                            SetPointerType(PointerType.normal);

                            SetInfoText("Cliquez pour lâcher le grappin");

                            float startTime = Time.time;
                            bool nearEnough = false;
                            while (Time.time < startTime + grapplingMaxDuration && !nearEnough && !Input.GetMouseButton(0))
                            {
                                Vector2 delta = grappling.transform.position - CurrentPlayerCharacter.transform.position;
                                CurrentPlayerCharacter.AddForce(grapplingForce * delta.normalized);

                                yield return new WaitForFixedUpdate();
                                nearEnough = delta.magnitude < grapplingTargetDistance;
                            }

                            grappling.RemoveRope();

                            break;
                        }
                    }
                }

                SetPointerType(PointerType.normal);
                card.Dark();
            }

            CurrentPlayer.DiscardHand();

            yield return new WaitForSeconds(turnEndDelay);
            yield return new WaitUntil(() => CurrentPlayerCharacter.rb.velocity.magnitude < turnEndVelocityThreshold);

            if (bonusAtEndOfTurn == BonusType.exchange)
            {
                SetInfoText("Choisissez des cartes a échanger");

                selectedExchangeNewCard = null;
                selectedExchangeDeckCard = null;

                Card[] deckCards = new Card[CurrentPlayer.allActions.Count];
                for (int i = 0; i < CurrentPlayer.allActions.Count; i++)
                {
                    Card card = InstantiateCard(CurrentPlayer.allActions[i]);
                    card.moveOnHover = true;
                    deckCards[i] = card;
                    card.transform.localPosition = new Vector3(GetHandXPosition(i, CurrentPlayer.allActions.Count), handYPosition, 0);
                    card.clickCallback = c => {
                        selectedExchangeDeckCard = c;
                        for (int i = 0; i < deckCards.Length; i++)
                        {
                            if (deckCards[i] != c) deckCards[i].Dark();
                            else c.Light(); 
                        }
                    };
                } 

                ActionType[] randomActions = GetRandomActions();
                Card[] randomCards = new Card[randomActions.Length];
                for (int i = 0; i < randomActions.Length; i++)
                {
                    Card card = InstantiateCard(randomActions[i]);
                    card.moveOnHover = true;
                    randomCards[i] = card;
                    card.transform.localPosition = new Vector3(GetHandXPosition(i, randomActions.Length), handYPosition + 300, 0); // TEST
                    card.clickCallback = c => {
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

                yield return new WaitForSeconds(smallDelay);

                for (int i = 0; i < deckCards.Length; i++)
                {
                    Destroy(deckCards[i].gameObject);
                }                
                
                for (int i = 0; i < randomCards.Length; i++)
                {
                    Destroy(randomCards[i].gameObject);
                }
            }
            else if (bonusAtEndOfTurn == BonusType.plus2)
            {

            }

            SetInfoText("");
            bonusAtEndOfTurn = BonusType.none;
            
            // Next turn! (repeat if players have finished race)
            do {
                currentPlayerID++;
                currentPlayerID %= PlayerCount;
            } while(CurrentPlayer.finished);
        }
    }

    public void Continue()
    {
        shouldContinue = true;
    }

    private void CreatePlayers(PlayerInfo[] infos)
    {
        players = new Player[infos.Length];

        for (int i = 0; i < PlayerCount; i++) 
        {
            players[i] = new Player();
            players[i].info = infos[i];

            players[i].SetDefaultCards();

            players[i].character = InstantiateCharacter(players[i]); // Create character GameObject
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
        Card res = Instantiate(i.cardPrefab, CameraController.i.canvas.transform).GetComponent<Card>();
        res.Init(type);

        return res;
    }

    public float GetHandXPosition(int cardID, int cardCount)
    {
        return (cardID + 0.5f - (float)cardCount / 2) * handCardsSpacing;
    }

    public Vector2 GetPointerDirection(Vector2 pos)
    {
        Vector2 screenPos = CameraController.i.mainCamera.WorldToScreenPoint(pos);
        return ((Vector2)Input.mousePosition - screenPos).normalized;
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
        pointer.sprite = pointers[(int)type];
    }

    public void PlayerFinishesRace(Player player)
    {
        player.finished = true;
        MapManager.i.finishParticles.Play();
        CameraController.i.followCharacter = false;
    }
}

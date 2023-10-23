using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    [SerializeField] private PlayerInfo[] startInfos; // TEST
    [NonSerialized] public Player[] players;

    public int cardsInHand = 3; // How many cards the players should draw
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

    [SerializeField] private float smallDelay = 0.1f;

    [NonSerialized] public bool shouldContinue;


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
    }

    private void StartRace(PlayerInfo[] infos)
    {
        StartCoroutine(StartRaceCoroutine(infos));
    } 

    public IEnumerator StartRaceCoroutine(PlayerInfo[] infos)
    {
        CreatePlayers(infos);

        currentPlayerID = 0;

        while (true)
        {
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
                else if (type == ActionType.bomb)
                {                    
                    SetInfoText("Cliquez pour lancer");

                    while (true)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * bombThrowForce;
                        force.y *= bombThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (Input.GetMouseButton(0))
                        {
                            CurrentPlayerCharacter.SpawnBomb(force);
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

                    while (true)
                    {
                        Vector2 force = GetPointerDirection(CurrentPlayerCharacter.transform.position) * grapplingThrowForce;
                        force.y *= grapplingThrowVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (Input.GetMouseButton(0))
                        {
                            bool touchedSomething = false;
                            Grappling grappling = CurrentPlayerCharacter.SpawnGrappling(force, () => {
                                touchedSomething = true;
                            });

                            yield return new WaitUntil(() => touchedSomething && !Input.GetMouseButton(0));
                            yield return new WaitForFixedUpdate();
                            
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

                            break;
                        }
                    }
                }

                card.Dark();
            }

            CurrentPlayer.DiscardHand();

            SetInfoText("");

            yield return new WaitForSeconds(2);
            
            // Next turn!
            currentPlayerID++;
            currentPlayerID %= PlayerCount;
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
}

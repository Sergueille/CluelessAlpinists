using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [NonSerialized] public int currentPlayerID;


    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private float characterStartVerticalSpacing = 0.7f;
    [SerializeField] private float characterStartHorizontalShift = 0.01f;
    public float handYPosition = -305;
    public float handCardsSpacing = 100;
    public MovementDescr cardDrawMovement;
    public MovementDescr cardDrawMovementRotation;

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
                    while (true)
                    {
                        Vector2 characterScreenPos = CameraController.i.mainCamera.WorldToScreenPoint(CurrentPlayerCharacter.transform.position);
                        Vector2 force = ((Vector2)Input.mousePosition - characterScreenPos).normalized * jumpForce;
                        force.y *= jumpVerticalMultiplier;
                        CurrentPlayerCharacter.DisplayJumpTrajectory(force);

                        yield return new WaitForEndOfFrame();

                        if (CurrentPlayerCharacter.IsTouchingGround() && Input.GetMouseButton(0))
                        {
                            CurrentPlayerCharacter.AddForce(force);
                            break;
                        }
                    }
                }
                else if (type == ActionType.jetpack)
                {
                    yield return new WaitForFixedUpdate();

                    float fuel = jetpackFuel;

                    while (fuel > 0)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            Vector2 characterScreenPos = CameraController.i.mainCamera.WorldToScreenPoint(CurrentPlayerCharacter.transform.position);
                            Vector2 force = ((Vector2)Input.mousePosition - characterScreenPos).normalized * jetpackForce * Time.fixedDeltaTime;
                            force.y *= jetPackVerticalMultiplier;

                            CurrentPlayerCharacter.AddForce(force);
                            fuel -= Time.fixedDeltaTime;
                        }

                        yield return new WaitForFixedUpdate();
                    }
                }
                else if (type == ActionType.bomb)
                {
                    // TODO
                }
                else if (type == ActionType.balloon)
                {
                    float startTime = Time.time;

                    while (!Input.GetMouseButton(0) && Time.time < startTime + balloonDuration)
                    {
                        CurrentPlayerCharacter.AddForce(Vector2.up * balloonForce * Time.fixedDeltaTime);
                        yield return new WaitForFixedUpdate();
                    }
                }
                else if (type == ActionType.grappling)
                {
                    // TODO
                }

                card.Dark();
            }

            CurrentPlayer.DiscardHand();

            yield return new WaitForSeconds(1);
            
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
}

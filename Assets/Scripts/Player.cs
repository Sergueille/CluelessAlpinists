using System.Collections.Generic;
using UnityEngine;
using System;

public class Player
{
    public PlayerInfo info;
    public Character character;

    public int rank = -1;
    public int turns = 0;

    public List<ActionType> allActions;
    public List<ActionType> deck;
    public List<Card> hand;
    public List<ActionType> discard;

    public bool finished = false;

    public void SetDefaultCards()
    {
        allActions = new List<ActionType> {
            ActionType.jump,
            ActionType.bomb,
            ActionType.jetpack,
            ActionType.grappling,
            ActionType.balloon,
            ActionType.balloon,
        };

        deck = new List<ActionType>(allActions);

        Util.ShuffleList(deck);

        hand = new List<Card>();
        discard = new List<ActionType>();
    }

    public void AddAction(ActionType type)
    {
        allActions.Add(type);
        discard.Add(type);
    }

    // Moves an action from deck to hand, shuffle if necessary. Returns false if there aren't enough cards
    public bool DrawAction()
    {
        if (deck.Count == 0) // If necessary, shuffle the discard pile into the deck
        {
            deck = discard;
            Util.ShuffleList(deck);
            discard = new List<ActionType>();
        }

        if (deck.Count == 0) // Sill not enough cards
        {
            return false;
        }

        // Draw card
        ActionType action = deck[0];
        deck.RemoveAt(0);
        Card card = GameManager.i.InstantiateCard(action);
        hand.Add(card);

        card.owner = this;
        card.moveOnHover = true;
        card.draggable = true;
        card.transform.position = new Vector3(0, -5000, 0); // Move out of screen to prevent it from being visible before the tween starts

        // Animate card
        Vector3 targetPosition = new Vector3(
            GameManager.i.GetHandXPosition(hand.Count - 1, GameManager.i.cardsInHand),
            GameManager.i.handYPosition,
            0
        );

        GameManager.i.cardDrawMovement.DoReverse(t => 
            card.transform.localPosition = targetPosition + new Vector3(-1, -1, 0) * t
        );
        GameManager.i.cardDrawMovementRotation.DoReverse(t => 
            card.transform.localRotation = Quaternion.Euler(0, 0, t)
        );

        return true;
    }

    public void DiscardHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            discard.Add(hand[i].type);

            Vector3 startPosition = hand[i].transform.localPosition;
            Card card = hand[i];
            GameManager.i.cardDrawMovement.Do(t => 
                card.transform.localPosition = startPosition + new Vector3(0, -1, 0) * t
            ).setOnComplete(() => GameObject.Destroy(card.gameObject));
        }

        hand.Clear();
    }

    public void RemoveCard(Card card)
    {
        int indexHand = hand.IndexOf(card);
        int indexDeck = deck.IndexOf(card.type);
        int indexDiscard = discard.IndexOf(card.type);

        if (indexHand != -1 )
        {
            hand.RemoveAt(indexHand);
            GameObject.Destroy(card);
        }
        else if (indexDiscard != -1)
        {
            discard.RemoveAt(indexDiscard);
        }
        else if (indexDeck != -1)
        {
            deck.RemoveAt(indexDeck);
        } 
        else throw new Exception("Card is not owned by player!");

        allActions.Remove(card.type);
    }
}

[Serializable]
public struct PlayerInfo
{
    public Sprite skin;
    public string name;
    public bool activated;

    public PlayerInfo(string name)
    {
        skin = null;
        this.name = name;
        activated = false;
    }
}

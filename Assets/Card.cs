using System;
using UnityEngine;

[Serializable]
public struct Card
{
    public Texture Texture;
    public int Rank;
    public Suit CardSuit;

    public Card(Texture Texture, int Rank, int Suit)
    {
        this.Texture = Texture;
        this.Rank = Rank;
        CardSuit = (Suit)Suit;
    }

    public override string ToString()
    {
        return string.Format("{0} of {1}", Rank, CardSuit);
    }

    public enum Suit
    {
        Bullets,
        Orbs,
        Spades,
        Hearts,
        Clubs,
        Diamonds,
        Stars,
        Nuts,
        None
    }

    // Only works with normal four suits
    public bool SharesColor(Card other)
    {
        return CardSuit == Suit.Spades || CardSuit == Suit.Clubs ?
            other.CardSuit == Suit.Spades || other.CardSuit == Suit.Clubs
            :
            CardSuit == Suit.Hearts || CardSuit == Suit.Diamonds ?
            other.CardSuit == Suit.Hearts || other.CardSuit == Suit.Diamonds
            : false;
    }

    internal static Card Random()
    {
        return new Card(null, UnityEngine.Random.Range(1, 14), UnityEngine.Random.Range(0, 4));
    }
}

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
        this.CardSuit = (Suit)Suit;
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

    internal static Card Random()
    {
        return new Card(null, UnityEngine.Random.Range(1, 14), UnityEngine.Random.Range(0, 4));
    }
}

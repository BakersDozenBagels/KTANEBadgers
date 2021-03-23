using UnityEngine;

public class Card
{
    public Texture Texture { get; set; }
    public int Rank { get; set; }
    public int Suit { get; set; }

    public Card(Texture Texture, int Rank, int Suit)
    {
        this.Texture = Texture;
        this.Rank = Rank;
        this.Suit = Suit;
    }

    public override string ToString()
    {
        return string.Format("{0} of {1}", Rank, Suit);
    }
}

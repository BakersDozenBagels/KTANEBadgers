using System.Collections.Generic;
using System.Linq;

namespace BadgerBoss
{
    public sealed class Hand
    {
        public readonly string PlayerName;
        public readonly List<Card> Cards;

        public Hand(string playerName, IEnumerable<Card> cards)
        {
            PlayerName = playerName;
            Cards = cards.ToList();
        }

        public override string ToString()
        {
            return string.Format("({0}:({1}))", PlayerName, Cards.Join(", "));
        }
    }
}
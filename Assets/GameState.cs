using System.Collections.Generic;
using System.Linq;

namespace BadgerBoss
{
    public sealed class GameState
    {
        public readonly Stack<Card> Deck, PlayPile;
        public Hand[] Hands;
        public int PlayerCount { get { return Hands.Length; } }
        public Card LastPlayed { get { return PlayPile.Peek(); } }
        public Card BeforeLastPlayed
        {
            get
            {
                Card x = PlayPile.Pop();
                if(PlayPile.Count == 0)
                {
                    PlayPile.Push(x);
                    return new Card();
                }
                Card r = PlayPile.Peek();
                PlayPile.Push(x);
                return r;
            }
        }

        public int NextPlayer { get { return CurrentPlayOrder == PlayOrder.Clockwise ? (CurrentPlayer + 1) % PlayerCount : (CurrentPlayer + PlayerCount - 1) % PlayerCount; } }

        public Hand CurrentHand { get { return Hands[CurrentPlayer]; } }

        public int CurrentPlayer;
        public PlayOrder CurrentPlayOrder;
        public int StackedSevens;
        public Card.Suit CalledSuit;

        public enum PlayOrder
        {
            Clockwise,
            Counterlockwise
        }

        private GameState(Stack<Card> deck, Stack<Card> playPile)
        {
            Deck = deck;
            PlayPile = playPile;
        }

        public static GameState NewShuffle(IEnumerable<Card> cards)
        {
            cards = cards.OrderBy(c => UnityEngine.Random.Range(0, int.MaxValue));
            GameState g = new GameState(new Stack<Card>(cards.Skip(16)), new Stack<Card>(cards.Take(1)));
            string[] names = GetRandomNames(3);
            g.Hands = new Hand[] { new Hand(names[0], cards.Skip(1).Take(5)), new Hand(names[1], cards.Skip(6).Take(5)), new Hand(names[2], cards.Skip(11).Take(5)) };
            g.CurrentPlayer = UnityEngine.Random.Range(0, g.PlayerCount);
            g.CurrentPlayOrder = (PlayOrder)UnityEngine.Random.Range(0, 2);
            g.CalledSuit = Card.Suit.None;
            return g;
        }

        public GameState DeepCopy()
        {
            Card[] newDeck = new Card[Deck.Count];
            Card[] newPlayPile = new Card[PlayPile.Count];
            Hand[] newHands = new Hand[Hands.Length];
            Deck.CopyTo(newDeck, 0);
            PlayPile.CopyTo(newPlayPile, 0);
            for(int h = 0; h < Hands.Length; h++)
            {
                Card[] newHand = new Card[Hands[h].Cards.Count];
                Hands[h].Cards.CopyTo(newHand, 0);
                newHands[h] = new Hand(Hands[h].PlayerName, newHand);
            }
            GameState g = new GameState(new Stack<Card>(newDeck), new Stack<Card>(newPlayPile));
            g.Hands = newHands;
            g.CurrentPlayer = CurrentPlayer;
            g.CurrentPlayOrder = CurrentPlayOrder;
            g.CalledSuit = CalledSuit;
            g.StackedSevens = StackedSevens;
            return g;
        }

        private static readonly string[] PossibleNames = new[] { "Sean", "James", "Quinn", "Jerry", "Benjamin", "John", "Emily", "Hannah", "Amy", "Bob" };

        private static string[] GetRandomNames(int count)
        {
            return PossibleNames.OrderBy(s => UnityEngine.Random.Range(0, int.MaxValue)).Take(count).ToArray();
        }

        public override string ToString()
        {
            return string.Format("(Play Pile:({0}), Deck:({1}), Hands:({6}) Current Player:({2}), PlayOrder:({3}) Stacked Sevens:({4}), Called Suit:({5}))", PlayPile.Join(", "), Deck.Join(", "), CurrentPlayer, CurrentPlayOrder, StackedSevens, CalledSuit, Hands.Join(", "));
        }
    }
}
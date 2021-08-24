using System.Collections.Generic;
using System.Linq;
using RNG = UnityEngine.Random;

namespace BadgerBoss
{
    public class PhraseRule : Rule
    {
        private Trigger _trigger;

        private PhraseRule(Trigger trigger)
        {
            _trigger = trigger;
        }

        public static PhraseRule Random()
        {
            int rankParam = RNG.Range(1, 14);
            Card.Suit suitParam = (Card.Suit)RNG.Range(0, 4);
            List<Trigger> triggers = new Trigger[] {
                new Trigger(g => g.CalledSuit == suitParam, "Called " + suitParam),
                new Trigger(g => g.CurrentHand.Cards.Count == rankParam, "Hand = " + rankParam),
                new Trigger(g => g.CurrentHand.Cards.Count == g.LastPlayed.Rank, "Hand = Played"),
                new Trigger(g => g.LastPlayed.Rank == rankParam, "Rank = " + rankParam),
                new Trigger(g => g.LastPlayed.CardSuit == suitParam, "Suit = " + suitParam),
                new Trigger(g => g.LastPlayed.CardSuit != g.BeforeLastPlayed.CardSuit, "Different Suit"),
                new Trigger(g => g.LastPlayed.Rank == g.BeforeLastPlayed.Rank, "Same Rank"),
                new Trigger(g => g.LastPlayed.Rank % 10 == g.PlayPile.Count % 10, "Rank = PlayPile Count"),
                new Trigger(g => g.LastPlayed.Rank % 10 == g.Deck.Count % 10, "Rank = Deck Count"),
                new Trigger(g => g.LastPlayed.Rank != rankParam, "Rank != " + rankParam),
                new Trigger(g => g.LastPlayed.CardSuit != suitParam, "Suit != " + suitParam),
                new Trigger(g => g.LastPlayed.Rank == rankParam && g.LastPlayed.CardSuit == suitParam, "Card = " + new Card(null, rankParam, (int)suitParam))
            }.ToList();

            return new PhraseRule(triggers.PickRandom());
        }

        public override Condition[] GetConditions()
        {
            return new Condition[] { new Condition(g => _trigger.Applies(g), 5) };
        }

        public override bool IsValid(GameState state, Card card)
        {
            return true;
        }

        public override ChangeList ModifyState(GameState state, Card card, ChangeList previous)
        {
            if(_trigger.Applies(state))
                previous.changes.Add(new Change() { PhraseSaid = new Phrase[] { Phrase.ThatsTheFox, Phrase.None, Phrase.None } });
            return previous;
        }

        public override ChangeList ModifyState(GameState state, ChangeList previous)
        {
            return previous;
        }

        public override string ToString()
        {
            return string.Format("(PhraseRule:(Trigger:{0}))", _trigger);
        }
    }
}
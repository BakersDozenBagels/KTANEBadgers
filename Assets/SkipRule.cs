using System.Collections.Generic;
using System.Linq;
using RNG = UnityEngine.Random;

namespace BadgerBoss
{
    public class SkipRule : Rule
    {
        private int _mode;
        private Trigger _triggerA, _triggerB;

        public static SkipRule Random()
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
                new Trigger(g => g.LastPlayed.Rank  ==rankParam && g.LastPlayed.CardSuit == suitParam, "Card = " + new Card(null, rankParam, (int)suitParam))
            }.ToList();
            Trigger triggerA = triggers.PickRandom();
            return new SkipRule() { _mode = RNG.Range(0, 4), _triggerA = triggerA, _triggerB = triggers.Where(t => t != triggerA).PickRandom() };
        }

        public override Condition[] GetConditions()
        {
            if(_mode == 3)
                return new[] { new Condition(g => g.CurrentHand.Cards.Count > 5, 5) };
            List<Condition> conditions = new Condition[] { new Condition(g => _triggerA.Applies(g), 5) }.ToList();
            if(_mode == 1)
                conditions.AddRange(new Condition[] { new Condition(g => _triggerB.Applies(g), 5) });
            return conditions.ToArray();
        }

        public override bool IsValid(GameState state, Card card)
        {
            return true;
        }

        public override ChangeList ModifyState(GameState state, Card card, ChangeList previous)
        {
            if(_mode == 0 && _triggerA.Applies(state))
                state.CurrentPlayer = state.NextPlayer;
            if(_mode == 1 && _triggerA.Applies(state) && !_triggerB.Applies(state))
                state.CurrentPlayer = (state.CurrentPlayer + 1) % state.PlayerCount;
            if(_mode == 1 && _triggerB.Applies(state) && !_triggerA.Applies(state))
                state.CurrentPlayer = (state.CurrentPlayer + state.PlayerCount - 1) % state.PlayerCount;
            if(_mode == 2 && _triggerA.Applies(state))
                state.CurrentPlayer = state.CurrentPlayOrder != GameState.PlayOrder.Clockwise ? (state.CurrentPlayer + 1) % state.PlayerCount : (state.CurrentPlayer + state.PlayerCount - 1) % state.PlayerCount;

            return previous;
        }

        public override ChangeList ModifyState(GameState state, ChangeList previous)
        {
            if(_mode == 3)
                state.CurrentPlayer = state.NextPlayer;
            return previous;
        }

        public override string ToString()
        {
            return string.Format("(SkipRule:(Mode:{0}, Trigger:({1}), Trigger2:({2})))", _mode, _triggerA, _triggerB);
        }
    }
}
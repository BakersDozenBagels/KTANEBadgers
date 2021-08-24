using System;
using System.Collections.Generic;
using System.Linq;
using RNG = UnityEngine.Random;

namespace BadgerBoss
{
    public class InvalidRule : Rule
    {
        private CardTrigger _triggerA;

        public static InvalidRule Random()
        {
            List<CardTrigger> triggers = new CardTrigger[] {
                new CardTrigger((g,c) => g.CurrentHand.Cards.Count == c.Rank, "Rank = Cards In Hand"),
                new CardTrigger((g,c) => g.Deck.Count == c.Rank, "Rank = Deck"),
                new CardTrigger((g,c) => g.PlayPile.Count == c.Rank, "Rank = PlayPile "),
                new CardTrigger((g,c) => (int)g.BeforeLastPlayed.CardSuit == (c.Rank - 1) % 4, "Rank = Previous Suit"),
                new CardTrigger((g,c) => (g.BeforeLastPlayed.Rank - 1) % 4 == (int)c.CardSuit, "Suit = Previous Rank"),
                new CardTrigger((g,c) => (g.BeforeLastPlayed.Rank) % 2 == c.Rank % 2 && !(g.BeforeLastPlayed.Rank == 7 && c.Rank == 7), "Same Rank Parity (Except 7s)"),
            }.ToList();
            CardTrigger triggerA = triggers.PickRandom();
            return new InvalidRule() { _triggerA = triggerA };
        }

        public override Condition[] GetConditions()
        {
            List<Condition> conditions = new Condition[] { new Condition(g => { Hand h = g.Hands.Where(hn => hn.Cards.Count > 0).PickRandom(); return _triggerA.Applies(g, h.Cards.PickRandom()); }, 10) }.ToList();
            return conditions.ToArray();
        }

        public override bool IsValid(GameState state, Card card)
        {
            return !_triggerA.Applies(state, card);
        }

        public override ChangeList ModifyState(GameState state, Card card, ChangeList previous)
        {
            return previous;
        }

        public override ChangeList ModifyState(GameState state, ChangeList previous)
        {
            return previous;
        }

        public override string ToString()
        {
            return string.Format("(InvalidRule:(Trigger:{0}))", _triggerA);
        }
    }
}
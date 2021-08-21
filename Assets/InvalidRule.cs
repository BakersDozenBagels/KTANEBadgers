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
                new CardTrigger((g,c) => { return g.CurrentHand.Cards.Count == c.Rank; }, "Rank = Cards In Hand"),
                new CardTrigger((g,c) => { return g.Deck.Count == c.Rank; }, "Rank = Deck"),
                new CardTrigger((g,c) => { return g.PlayPile.Count == c.Rank; }, "Rank = PlayPile "),
                new CardTrigger((g,c) => { return (int)g.BeforeLastPlayed.CardSuit == (c.Rank - 1) % 4; }, "Rank = Previous Suit"),
                new CardTrigger((g,c) => { return (g.BeforeLastPlayed.Rank - 1) % 4 == (int)c.CardSuit; }, "Suit = Previous Rank"),
                new CardTrigger((g,c) => { return (g.BeforeLastPlayed.Rank) % 2 == c.Rank % 2 && !(g.BeforeLastPlayed.Rank == 7 && c.Rank == 7); }, "Same Rank Parity (Except 7s)"),
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

        public override List ModifyState(GameState state, Card card, List previous)
        {
            return previous;
        }

        public override List ModifyState(GameState state, List previous)
        {
            return previous;
        }

        public override string ToString()
        {
            return string.Format("(InvalidRule:(Trigger:{0}))", _triggerA);
        }
    }
}
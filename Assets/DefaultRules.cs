using System.Collections.Generic;
using RNG = UnityEngine.Random;

namespace BadgerBoss
{
    public class DefaultRules : Rule
    {
        public override Condition[] GetConditions()
        {
#if UNITY_EDITOR
            return new[] { new Condition(g => g.PlayPile.Count > 15, 1) };
#else
            return new[] { new Condition(g => true, 1) };
#endif
        }

        public override bool IsValid(GameState state, Card card)
        {
            return (state.CalledSuit == Card.Suit.None ? state.LastPlayed.CardSuit : state.CalledSuit) == card.CardSuit || state.LastPlayed.Rank == card.Rank;
        }

        public override ChangeList ModifyState(GameState state, Card card, ChangeList previous)
        {
            state.CalledSuit = Card.Suit.None;
            ChangeList cl = new ChangeList();

            cl.changes.Add(new Change { CardPlayed = new Play(state.CurrentPlayer, card) });

            if(card.CardSuit == Card.Suit.Spades)
            {
                Change c = new Change();
                switch(card.Rank)
                {
                    case 1:
                        c.PhraseSaid[0] = Phrase.TheAceOfSpades;
                        break;
                    case 2:
                        c.PhraseSaid[0] = Phrase.TheTwoOfSpades;
                        break;
                    case 3:
                        c.PhraseSaid[0] = Phrase.TheThreeOfSpades;
                        break;
                    case 4:
                        c.PhraseSaid[0] = Phrase.TheFourOfSpades;
                        break;
                    case 5:
                        c.PhraseSaid[0] = Phrase.TheFiveOfSpades;
                        break;
                    case 6:
                        c.PhraseSaid[0] = Phrase.TheSixOfSpades;
                        break;
                    case 7:
                        c.PhraseSaid[0] = Phrase.TheSevenOfSpades;
                        break;
                    case 8:
                        c.PhraseSaid[0] = Phrase.TheEightOfSpades;
                        break;
                    case 9:
                        c.PhraseSaid[0] = Phrase.TheNineOfSpades;
                        break;
                    case 10:
                        c.PhraseSaid[0] = Phrase.TheTenOfSpades;
                        break;
                    case 11:
                        c.PhraseSaid[0] = Phrase.TheJackOfSpades;
                        break;
                    case 12:
                        c.PhraseSaid[0] = Phrase.TheQueenOfSpades;
                        break;
                    case 13:
                        c.PhraseSaid[0] = Phrase.TheKingOfSpades;
                        break;
                }
                cl.Add(c);
            }
            if(card.Rank == 8)
                state.CurrentPlayOrder = state.CurrentPlayOrder == GameState.PlayOrder.Counterlockwise ? GameState.PlayOrder.Clockwise : GameState.PlayOrder.Counterlockwise;
            if(card.Rank != 7)
            {
                for(int s = 0; s < state.StackedSevens; s++)
                {
                    cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.PenaltyCard, Phrase.None, Phrase.None } });
                    state.CurrentHand.Cards.Add(state.Deck.Pop());
                    cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                }
                state.StackedSevens = 0;
            }
            if(card.Rank == 7)
            {
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.HaveA, Phrase.None, Phrase.None } });
                for(int i = 0; i < state.StackedSevens; i++)
                    cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Very, Phrase.None, Phrase.None } });
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.NiceDay, Phrase.None, Phrase.None } });
                state.StackedSevens++;
            }
            if(card.Rank == 6 && state.PlayPile.Peek().Rank == 6)
                if(state.BeforeLastPlayed.Rank == 6)
                    cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.AllHailTheX, Phrase.None, Phrase.None } });

            if(card.Rank == 9 && card.CardSuit == Card.Suit.Diamonds)
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.ThatsTheBadger, Phrase.None, Phrase.None } });

            if(card.Rank == 11)
            {
                state.CalledSuit = (Card.Suit)RNG.Range(0, 4);
                switch(state.CalledSuit)
                {
                    case Card.Suit.Clubs:
                        cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Clubs, Phrase.None, Phrase.None } });
                        break;
                    case Card.Suit.Hearts:
                        cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Hearts, Phrase.None, Phrase.None } });
                        break;
                    case Card.Suit.Diamonds:
                        cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Diamonds, Phrase.None, Phrase.None } });
                        break;
                    case Card.Suit.Spades:
                        cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Spades, Phrase.None, Phrase.None } });
                        break;
                }
            }

            if(state.CurrentHand.Cards.Count == 2)
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.LastCard, Phrase.None, Phrase.None } });

            if(state.CurrentHand.Cards.Count == 1)
            {
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Mao, Phrase.None, Phrase.None } });
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.ReenteringTheGame, Phrase.None, Phrase.None } });
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
                state.CurrentHand.Cards.Add(state.Deck.Pop());
                state.CurrentHand.Cards.Add(state.Deck.Pop());
                state.CurrentHand.Cards.Add(state.Deck.Pop());
                state.CurrentHand.Cards.Add(state.Deck.Pop());
                state.CurrentHand.Cards.Add(state.Deck.Pop());
            }

            state.CurrentHand.Cards.Remove(card);
            state.PlayPile.Push(card);
            cl = ChangeList.CombineDefault(previous, cl);
            return cl;
        }

        public override ChangeList ModifyState(GameState state, ChangeList previous)
        {
            ChangeList cl = new ChangeList();
            for(int s = 0; s < state.StackedSevens; s++)
            {
                cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.PenaltyCard, Phrase.None, Phrase.None } });
                state.CurrentHand.Cards.Add(state.Deck.Pop());
                cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
            }
            state.StackedSevens = 0;

            state.CurrentHand.Cards.Add(state.Deck.Pop());
            cl.changes.Add(new Change { CardDrawn = state.CurrentPlayer });
            cl.changes.Add(new Change { PhraseSaid = new[] { Phrase.Pass, Phrase.None, Phrase.None } });
            return cl;
        }

        public override string ToString()
        {
            return "(Rule: Default)";
        }
    }
}
using System;

namespace BadgerBoss
{
    public class Trigger
    {
        public readonly Func<GameState, bool> Applies;
        private readonly string _name;

        public Trigger(Func<GameState,bool> applies, string name)
        {
            Applies = applies;
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    public class CardTrigger
    {
        public readonly Func<GameState, Card, bool> Applies;
        private readonly string _name;

        public CardTrigger(Func<GameState, Card, bool> applies, string name)
        {
            Applies = applies;
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
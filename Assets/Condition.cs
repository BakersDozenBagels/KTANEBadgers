using System;

namespace BadgerBoss
{
    public struct Condition
    {
        public readonly Func<GameState, bool> Applies;
        public int Count;

        public Condition(Func<GameState, bool> applies, int count)
        {
            Applies = applies;
            Count = count;
        }

        public override string ToString()
        {
            return "Cond:" + Count;
        }

        internal Condition Decrement()
        {
            return new Condition(Applies, Count - 1);
        }
    }
}
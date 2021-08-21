namespace BadgerBoss
{
    public abstract class Rule
    {
        public abstract List ModifyState(GameState state, Card card, List previous);
        public abstract List ModifyState(GameState state, List previous);

        public abstract bool IsValid(GameState state, Card card);

        public abstract Condition[] GetConditions();
    }
}
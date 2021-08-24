namespace BadgerBoss
{
    public abstract class Rule
    {
        public abstract ChangeList ModifyState(GameState state, Card card, ChangeList previous);
        public abstract ChangeList ModifyState(GameState state, ChangeList previous);

        public abstract bool IsValid(GameState state, Card card);

        public abstract Condition[] GetConditions();
    }
}
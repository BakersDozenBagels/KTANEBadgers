namespace BadgerBoss
{
    public sealed class Play
    {
        public readonly int Player;
        public readonly Card Card;

        public Play(int player, Card card)
        {
            Player = player;
            Card = card;
        }

        public override string ToString()
        {
            return string.Format("({0}:{1})", Player, Card);
        }
    }
}
using System.Linq;

namespace BadgerBoss
{
    public sealed class Change
    {
        /// <summary>
        /// Current player, next player, defuser
        /// </summary>
        public Phrase[] PhraseSaid = new Phrase[3];
        public int CardDrawn = -1;
        public Play CardPlayed;

        public ChangeType Type { get { if(PhraseSaid.Any(p => p != Phrase.None)) return ChangeType.Phrase; if(CardDrawn != -1) return ChangeType.Draw; if(CardPlayed != null) return ChangeType.Play; return ChangeType.None; } }

        public override string ToString()
        {
            string s = "(";
            if(PhraseSaid[0] != Phrase.None)
                s += "Current player says:(" + PhraseSaid[0] + ")";
            if(PhraseSaid[1] != Phrase.None)
                s += "Next player says:(" + PhraseSaid[2] + ")";
            if(PhraseSaid[2] != Phrase.None)
                s += "You say:(" + PhraseSaid[1] + ")";
            if(CardDrawn != -1)
                s += "Card drawn:(" + CardDrawn + ")";
            if(CardPlayed != null)
                s += "Card played:(" + CardPlayed + ")";
            return s + ")";
        }

        public enum ChangeType
        {
            None = 0,
            Phrase,
            Draw,
            Play
        }
    }
}
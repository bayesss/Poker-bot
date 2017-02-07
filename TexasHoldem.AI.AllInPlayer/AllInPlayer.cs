using TexasHoldem.Logic.Players;

namespace TexasHoldem.AI.AllInPlayer
{
    public class AllInPlayer : BasePlayer
    {
        public override string Name
        {
            get
            {
                return "All In Player";
            }
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            return PlayerAction.Raise(context.MoneyLeft);
        }
    }
}

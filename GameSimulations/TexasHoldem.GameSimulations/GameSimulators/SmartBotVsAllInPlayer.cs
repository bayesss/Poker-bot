using TexasHoldem.AI.AllInPlayer;
using TexasHoldem.AI.SmartBot;
using TexasHoldem.Logic.Players;
using TexasHoldem.Tests.GameSimulations.GameSimulators;

namespace TexasHoldem.GameSimulations.GameSimulators
{
    class SmartBotVsAllInPlayer : BaseGameSimulator
    {
        private readonly IPlayer firstPlayer = new SmartBot();
        private readonly IPlayer secondPlayer = new AllInPlayer();

        protected override IPlayer GetFirstPlayer()
        {
            return this.firstPlayer;
        }

        protected override IPlayer GetSecondPlayer()
        {
            return this.secondPlayer;
        }
    }
}

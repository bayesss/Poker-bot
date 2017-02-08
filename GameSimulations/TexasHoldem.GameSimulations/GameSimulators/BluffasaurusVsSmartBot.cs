using TexasHoldem.AI.Bluffasaurus;
using TexasHoldem.AI.SmartBot;
using TexasHoldem.Logic.Players;
using TexasHoldem.Tests.GameSimulations.GameSimulators;

namespace TexasHoldem.GameSimulations.GameSimulators
{
    class BluffasaurusVsSmartBot : BaseGameSimulator
    {
        private readonly IPlayer firstPlayer = new Bluffasaurus();
        private readonly IPlayer secondPlayer = new SmartBot();

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

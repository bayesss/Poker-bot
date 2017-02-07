using TexasHoldem.AI.Bluffasaurus;
using TexasHoldem.AI.RandomPlayer;
using TexasHoldem.Logic.Players;
using TexasHoldem.Tests.GameSimulations.GameSimulators;

namespace TexasHoldem.GameSimulations.GameSimulators
{
    class BluffasaurusVsRandomPlayer : BaseGameSimulator
    {
        private readonly IPlayer firstPlayer = new Bluffasaurus();
        private readonly IPlayer secondPlayer = new RandomPlayer();

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

namespace TexasHoldem.Tests.GameSimulations.GameSimulators
{
    using Logic.Players;
    using AI.Bluffasaurus;
    using AI.AllInPlayer;
    public class BluffasaurusVsAllInPlayer : BaseGameSimulator
    {
        private readonly IPlayer firstPlayer = new Bluffasaurus();
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

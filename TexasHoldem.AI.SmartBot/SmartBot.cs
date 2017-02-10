namespace TexasHoldem.AI.SmartBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logic.Players;
    using Logic;
    using Logic.Cards;
    using Bluffasaurus.Helpers;

    public class SmartBot : BasePlayer
    {
        private static bool inPosition = false;

        public override string Name { get; } = "Bluffasaurus" + Guid.NewGuid();

        private PlayerAction Fold()
        {
            return PlayerAction.Fold();
        }

        public override PlayerAction GetTurn(GetTurnContext context)
        {
            var bigBlind = context.SmallBlind * 2;

            #region Preflop
            if (context.RoundType == GameRoundType.PreFlop)
            {
                var handValue = HandStrengthValuationBluffasaurus.PreFlop(this.FirstCard, this.SecondCard);
                var agression = 0;

                var extreme = 64 - agression;
                var powerful = 60 - agression;
                var normal = 56 - agression; // top 40% of cards
                var weak = 50 - agression;
                var awful = 45 - agression; // 70% of the cards
                inPosition = true;

                // we are first to act on a small blind - we are not in position so we want to play around 70% of the cards and only raise around 40%
                if (context.MyMoneyInTheRound == context.SmallBlind)
                {
                    inPosition = false;

                    if (handValue >= extreme)
                    {
                        return PlayerAction.Raise(bigBlind * 4);
                    }
                    else if (handValue >= powerful)
                    {
                        return PlayerAction.Raise(bigBlind * 3);
                    }
                    else if (handValue >= normal)
                    {
                        return PlayerAction.Raise(bigBlind * 1);
                    }
                    else if (handValue >= awful)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else if (context.MoneyToCall < context.MoneyLeft / (double)100) // lets try our luck if it is cheap enough
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return this.Fold();
                    }
                }
                else if (context.MyMoneyInTheRound == bigBlind && context.CurrentPot == context.SmallBlind * 3)  // we are to act on big blind
                {
                    // we are in position and opp has not raised on small blind (probably hasn't a great hand) - we should make him fold most of his hands and if he calls we must be able to defend
                    if (context.MoneyToCall == 0)
                    {
                        if (handValue >= extreme)
                        {
                            return PlayerAction.Raise(bigBlind * 8);
                        }
                        else if (handValue >= powerful)
                        {
                            return PlayerAction.Raise(bigBlind * 6);
                        }
                        else if (handValue >= normal)
                        {
                            return PlayerAction.Raise(bigBlind * 4);
                        }
                        else if (handValue >= weak && bigBlind < context.MoneyLeft / (double)50) // we dont have a great hand either but we can make him sweat about it and we can always fold later
                        {
                            return PlayerAction.Raise(bigBlind * 2);
                        }
                        else // that makes around 74% of all possible hands
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }
                    else // opp has raised out of position - we can three bet him TODO: check if this logic can be combined with below else
                    {
                        // opp has raised a lot - we should call him or raise only on our best cards
                        if (context.MoneyToCall >= bigBlind * 8 && context.MoneyToCall >= context.MoneyLeft / (double)25)
                        {
                            if (handValue >= extreme)
                            {
                                return PlayerAction.Raise(context.MoneyToCall);
                            }
                            else if (handValue >= powerful)
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else // TODO: here we could open more hands if we know that our opp raises a lot on cheap hands
                            {
                                return PlayerAction.Fold();
                            }
                        }
                        else if (context.MoneyToCall >= bigBlind * 4)
                        {
                            if (handValue >= powerful)
                            {
                                return PlayerAction.Raise(context.MoneyToCall);
                            }
                            else if (handValue >= normal)
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else if (handValue >= weak && context.MoneyToCall < context.MoneyLeft / (double)30) // we dont have a great hand either but we can make him sweat about it and we can always fold later
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else // that makes around 74% of all possible hands
                            {
                                return PlayerAction.Fold();
                            }
                        }
                        else // opp raised with less than 4 big blinds
                        {
                            if (handValue >= normal)
                            {
                                return PlayerAction.Raise((int)(context.MoneyToCall * 2.5));
                            }
                            else if (handValue >= weak && bigBlind < context.MoneyLeft / (double)30) // we dont have a great hand either but we can make him sweat about it and we can always fold later
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else if (handValue >= awful && context.MoneyToCall < bigBlind * 2) // that makes around 74% of all possible hands
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else
                            {
                                return PlayerAction.Fold();
                            }
                        }
                    }
                }
                else // opp reraises us or we have checked on SB and he has raised
                {
                    if (handValue >= extreme)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    // opp has raised a lot - we should call him or raise only on our best cards
                    if (context.MoneyToCall >= bigBlind * 8 && context.MoneyToCall >= context.MoneyLeft / (double)25)
                    {
                        if (handValue >= powerful)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else if (handValue >= normal && context.MyMoneyInTheRound > context.MoneyToCall * 2) // TODO: here we could open more hands if we know that our opp raises a lot on cheap hands
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                    else if (context.MoneyToCall >= bigBlind * 4)
                    {
                        if (handValue >= normal)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else if (handValue >= weak && context.MyMoneyInTheRound > context.MoneyToCall * 2)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else // that makes around 74% of all possible hands
                        {
                            return PlayerAction.Fold();
                        }
                    }
                    else // opp raised with less than 4 big blinds
                    {
                        if (handValue >= normal)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else if (handValue >= weak && context.MyMoneyInTheRound > context.MoneyToCall * 2)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else if (handValue >= awful && context.MoneyToCall < bigBlind * 2) // that makes around 74% of all possible hands
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                }
            }
            #endregion

            #region Flop
            else if (context.RoundType == GameRoundType.Flop)
            {
                if (context.MoneyLeft == 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                var flopCardStrength = CardsStrengthEvaluation.RateCards
                    (new List<Card> { FirstCard, SecondCard, CommunityCards.ElementAt(0), CommunityCards.ElementAt(1), CommunityCards.ElementAt(2) });

                // we are first to act out of position
                if (context.MoneyToCall == 0 && !inPosition)
                {
                    if (flopCardStrength >= 4000)
                    {
                        return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4)); // we are 100% sure to win so we want to keep him in the game as long as possible
                    }
                    else if (flopCardStrength >= 3000)
                    {
                        // TODO: 3of a kind logic
                        var threeOfAKindWeHave = this.HowManyOfThreeOfAKindWeHave();
                        if (threeOfAKindWeHave == 2)
                        {
                            return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4));
                        }
                        else
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }
                    if (flopCardStrength >= 2000)
                    {
                        if (!this.IsPairInCommunity())
                        {
                            return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4));
                        }
                        else
                        {
                            if (this.HaveHighestKicker())
                            {
                                return PlayerAction.Raise(Math.Max(context.CurrentPot / 3, bigBlind * 3));
                            }
                            else
                            {
                                return PlayerAction.Raise(Math.Max(context.CurrentPot / 4, bigBlind * 2));
                            }
                        }
                    }
                    else if (flopCardStrength >= 1000)
                    {
                        var pairValue = this.GetPairValue();

                        if (pairValue == 0)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            if (this.IsHighestPair(pairValue)) // we have the highest pair possible
                            {
                                return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4));
                            }
                            else
                            {
                                return PlayerAction.Raise(Math.Max(context.CurrentPot / 3, bigBlind * 3));
                            }
                        }
                    }
                    else
                    {
                        return PlayerAction.CheckOrCall();
                    }
                }
                else if (context.MyMoneyInTheRound == 0 && inPosition) // we are to act in position
                {
                    if (context.MoneyToCall == 0) // opp has checked
                    {
                        if (flopCardStrength >= 2000)
                        {
                            return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4));
                        }
                        else if (flopCardStrength >= 1000)
                        {
                            var pairValue = this.GetPairValue();

                            if (pairValue != 0)
                            {
                                if (this.IsHighestPair(pairValue)) // we have the highest pair possible
                                {
                                    return PlayerAction.Raise(Math.Max(context.CurrentPot, bigBlind * 8));
                                }
                                else
                                {
                                    return PlayerAction.Raise(Math.Max(context.CurrentPot / 2, bigBlind * 4));
                                }
                            }
                        }

                        // we have nothing or pair is in community cards
                        if (this.HaveHighestKicker())
                        {
                            return PlayerAction.Raise(Math.Max(context.CurrentPot / 4, bigBlind * 2));
                        }
                        else
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }
                }

                // opp has raised

                // an awful lot
                if (context.MoneyToCall >= (context.CurrentPot - context.MoneyToCall) && context.MoneyToCall >= 60)
                {
                    if (flopCardStrength >= 4000)
                    {
                        return PlayerAction.Raise(context.MoneyToCall);
                    }
                    else if (flopCardStrength >= 3000)
                    {
                        // TODO: 3of a kind logic
                        var threeOfAKindWeHave = this.HowManyOfThreeOfAKindWeHave();
                        if (threeOfAKindWeHave == 2)
                        {
                            return PlayerAction.Raise(context.MoneyToCall);
                        }
                        else if (threeOfAKindWeHave == 1 || this.HaveHighestKicker())
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                    if (flopCardStrength >= 2000)
                    {
                        if (!this.IsPairInCommunity())
                        {
                            return PlayerAction.Raise(context.MoneyToCall);
                        }
                        else
                        {
                            if (this.HaveHighestKicker())
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else
                            {
                                return PlayerAction.Fold();
                            }
                        }
                    }
                    else
                    {
                        return this.Fold();
                    }
                }
                else if (context.MoneyToCall > (context.CurrentPot - context.MoneyToCall) / 2 && context.MoneyToCall >= 40) // opp has raised a reasonable amout
                {
                    if (flopCardStrength >= 4000)
                    {
                        return PlayerAction.Raise(context.CurrentPot);
                    }
                    else if (flopCardStrength >= 3000)
                    {
                        // TODO: 3of a kind logic
                        var threeOfAKindWeHave = this.HowManyOfThreeOfAKindWeHave();
                        if (threeOfAKindWeHave == 2)
                        {
                            return PlayerAction.Raise(context.CurrentPot);
                        }
                        else if (threeOfAKindWeHave == 1)
                        {
                            return PlayerAction.Raise(context.CurrentPot / 2);
                        }
                        else if (this.HaveHighestKicker())
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                    else if (flopCardStrength >= 2000)
                    {
                        if (!this.IsPairInCommunity())
                        {
                            return PlayerAction.Raise(context.MoneyToCall);
                        }
                        else
                        {
                            var pairValue = this.GetPairValue();

                            if (this.IsHighestPair(pairValue))
                            {
                                return PlayerAction.Raise(context.MoneyToCall);
                            }
                            else
                            {
                                return PlayerAction.CheckOrCall();
                            }
                        }
                    }
                    else if (flopCardStrength >= 1000)
                    {
                        var pairValue = this.GetPairValue();

                        if (pairValue == 0)
                        {
                            return this.Fold();
                        }

                        if (this.IsHighestPair(pairValue) || this.HaveHighestKicker())
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }

                    return this.Fold();
                }
                else // opp has raised a little
                {
                    if (flopCardStrength >= 4000)
                    {
                        return PlayerAction.Raise(context.MoneyToCall * 2);
                    }
                    else if (flopCardStrength >= 3000)
                    {
                        // TODO: 3of a kind logic
                        var threeOfAKindWeHave = this.HowManyOfThreeOfAKindWeHave();
                        if (threeOfAKindWeHave == 2)
                        {
                            return PlayerAction.Raise(context.MoneyToCall * 2);
                        }
                        else if (threeOfAKindWeHave == 1)
                        {
                            return PlayerAction.Raise(context.MoneyToCall);
                        }
                        else if (this.HaveHighestKicker())
                        {
                            return PlayerAction.Raise(context.MoneyToCall);
                        }
                        else
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }
                    else if (flopCardStrength >= 2000)
                    {
                        if (!this.IsPairInCommunity())
                        {
                            return PlayerAction.Raise(context.MoneyToCall * 2);
                        }
                        else
                        {
                            var pairValue = this.GetPairValue();

                            if (this.IsHighestPair(pairValue))
                            {
                                return PlayerAction.Raise(context.MoneyToCall);
                            }
                            else
                            {
                                return PlayerAction.CheckOrCall();
                            }
                        }
                    }
                    else if (flopCardStrength >= 1000)
                    {
                        var pairValue = this.GetPairValue();

                        if (pairValue == 0)
                        {
                            return PlayerAction.CheckOrCall();
                        }

                        if (this.IsHighestPair(pairValue) || this.HaveHighestKicker())
                        {
                            return PlayerAction.Raise(context.MoneyToCall / 2);
                        }
                    }

                    if (this.HaveHighestKicker())
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return this.Fold();
                    }
                }
            }
            #endregion

            #region Turn
            else if (context.RoundType == GameRoundType.Turn || context.RoundType == GameRoundType.River)
            {
                if (context.MoneyLeft == 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                var flopCardStrength = CardsStrengthEvaluation.RateCards
                    (new List<Card> { FirstCard, SecondCard, CommunityCards.ElementAt(0), CommunityCards.ElementAt(1), CommunityCards.ElementAt(2) });

                var hand = new List<Card>();
                hand.Add(this.FirstCard);
                hand.Add(this.SecondCard);

                var ehs = EffectiveHandStrenghtCalculator.CalculateEHS(hand, this.CommunityCards);

                // we are first to act out of position - we should always add a little bit of pressure on our opp
                if (context.MoneyToCall == 0 && !inPosition)
                {
                    if (ehs < 0.3)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    if (ehs < 0.5)
                    {
                        if (this.HaveHighestKicker())
                        {
                            return PlayerAction.Raise(context.CurrentPot / 5);
                        }
                        else
                        {
                            return PlayerAction.CheckOrCall();
                        }
                    }
                    else if (ehs < 0.63)
                    {
                        return PlayerAction.Raise(context.CurrentPot / 4);
                    }
                    else if (ehs < 0.75)
                    {
                        return PlayerAction.Raise(context.CurrentPot / 3);
                    }
                    else
                    {
                        return PlayerAction.Raise(context.CurrentPot / 2);
                    }
                }
                else if (context.MyMoneyInTheRound == 0 && inPosition) // we are to act in position
                {
                    if (context.MoneyToCall == 0) // opp has checked
                    {
                        // he has nothing, but neither have we - we should try to bluff him most of the time, or atleast make him sweat for that money
                        if (ehs < 0.5)
                        {
                            // he has checked as first action previous round - he has nothing => we bluff
                            if (context.PreviousRoundActions.Where(x => x.PlayerName != this.Name).FirstOrDefault().Action == PlayerAction.CheckOrCall())
                            {
                                return PlayerAction.Raise(context.CurrentPot); // bluff - we are trying to make him fold
                            }

                            if (this.HaveHighestKicker())
                            {
                                return PlayerAction.Raise(context.CurrentPot / 2); // we are trying to make him fold
                            }
                            else
                            {
                                return PlayerAction.Raise(context.CurrentPot / 5);
                            }
                        }
                        else if (ehs < 0.60)
                        {
                            return PlayerAction.Raise(context.CurrentPot / 4);
                        }
                        else if (ehs < 0.70)
                        {
                            return PlayerAction.Raise(context.CurrentPot / 2);
                        }
                        else
                        {
                            return PlayerAction.Raise(context.CurrentPot / 3);
                        }
                    }
                }

                // opp has raised

                // an awful lot
                if (context.MoneyToCall >= (context.CurrentPot - context.MoneyToCall))
                {
                    if (ehs < 0.60)
                    {
                        return this.Fold();
                    }
                    else if (ehs < 0.70)
                    {
                        if (context.MoneyToCall < context.MoneyLeft / 10 || context.MoneyToCall < 70)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else if (ehs < 0.85)
                    {
                        int moneyToBet = (int)(context.CurrentPot * 0.85);

                        if (context.MoneyToCall < context.MoneyLeft / 2 || context.MoneyToCall < 250)
                        {
                            if (context.MoneyToCall < moneyToBet && context.MyMoneyInTheRound == 0)
                            {
                                return PlayerAction.Raise(moneyToBet - context.MoneyToCall + 1);
                            }
                            else
                            {
                                return PlayerAction.CheckOrCall();
                            }
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else
                    {
                        return PlayerAction.Raise(context.MoneyToCall);
                    }
                }
                else if (context.MoneyToCall > (context.CurrentPot - context.MoneyToCall) / 2) // opp has raised a reasonable amout
                {
                    if (ehs < 0.50)
                    {
                        return this.Fold();
                    }
                    else if (ehs < 0.60)
                    {
                        if (context.MoneyToCall < context.MoneyLeft / 10 || context.MoneyToCall < 70)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else if (ehs < 0.75)
                    {
                        int moneyToBet = (int)(context.CurrentPot * 0.75);

                        if (context.MoneyToCall < context.MoneyLeft / 2 || context.MoneyToCall < 250)
                        {
                            if (context.MoneyToCall < moneyToBet && context.MyMoneyInTheRound == 0)
                            {
                                return PlayerAction.Raise(moneyToBet - context.MoneyToCall + 1);
                            }
                            else
                            {
                                return PlayerAction.CheckOrCall();
                            }
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else
                    {
                        return PlayerAction.Raise(context.MoneyToCall);
                    }
                }
                else // opp has raised a little
                {
                    if (ehs < 0.40)
                    {
                        return this.Fold();
                    }
                    else if (ehs < 0.50)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else if (ehs < 0.60)
                    {
                        if (context.MoneyToCall < context.MoneyLeft / 10 || context.MoneyToCall < 70)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else if (ehs < 0.75)
                    {
                        int moneyToBet = (int)(context.CurrentPot * 0.85);

                        if (context.MoneyToCall < context.MoneyLeft / 2 || context.MoneyToCall < 250)
                        {
                            if (context.MoneyToCall < moneyToBet && context.MyMoneyInTheRound == 0)
                            {
                                return PlayerAction.Raise(moneyToBet - context.MoneyToCall + 1);
                            }
                            else
                            {
                                return PlayerAction.CheckOrCall();
                            }
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else
                    {
                        return PlayerAction.Raise(context.CurrentPot / 2);
                    }
                }
            }
            #endregion

            return PlayerAction.CheckOrCall(); // It should never reach this point
        }

        private int GetPairValue()
        {
            if (this.FirstCard.Type == this.SecondCard.Type)
            {
                return (int)this.FirstCard.Type;
            }

            for (int i = 0; i < this.CommunityCards.Count; i++)
            {
                if (this.CommunityCards.ElementAt(i).Type == this.FirstCard.Type)
                {
                    return (int)this.FirstCard.Type;
                }

                if (this.CommunityCards.ElementAt(i).Type == this.SecondCard.Type)
                {
                    return (int)this.SecondCard.Type;
                }
            }

            return 0;
        }

        private bool IsHighestPair(int pairValue)
        {
            foreach (var card in this.CommunityCards)
            {
                if ((int)card.Type > pairValue)
                    return false;
            }

            return true;
        }

        private bool HaveHighestKicker()
        {
            var allCards = new List<Card>();
            allCards.Add(this.FirstCard);
            allCards.Add(this.SecondCard);
            allCards.AddRange(this.CommunityCards);

            var kickerGroup = allCards.GroupBy(x => x.Type).OrderByDescending(x => x.Key).Where(g => g.Count() == 1).FirstOrDefault();

            if (kickerGroup != null)
            {
                var kickerCard = kickerGroup.First();
                if (kickerCard == this.FirstCard || kickerCard == this.SecondCard)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPairInCommunity()
        {
            var firstGroup = this.CommunityCards.GroupBy(x => x.Type).OrderByDescending(x => x.Count()).First();
            return firstGroup.Count() > 1;
        }

        private int HowManyOfThreeOfAKindWeHave()
        {
            if (this.FirstCard.Type == this.SecondCard.Type)
                return 2;

            var threeOfAKindGroup = this.CommunityCards.GroupBy(x => x.Type).OrderByDescending(x => x.Count()).First();

            if (threeOfAKindGroup.Count() == 3)
                return 0;

            return 1;
        }
    }
}

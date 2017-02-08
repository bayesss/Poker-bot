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
                if (context.MoneyToCall == context.SmallBlind && context.CurrentPot == context.SmallBlind * 3)
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
                else  // we are on big blind or opp has raised
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
                    else // opponent has raised
                    {
                        //// opp has raised out of position - we can three bet him
                        //if (inPosition)
                        //{
                        //    if (handValue >= extreme)
                        //    {
                        //        return PlayerAction.Raise(bigBlind * 8);
                        //    }
                        //    else if (handValue >= powerful)
                        //    {
                        //        return PlayerAction.Raise(bigBlind * 6);
                        //    }
                        //    else if (handValue >= normal)
                        //    {
                        //        return PlayerAction.Raise(bigBlind * 4);
                        //    }
                        //    else if (handValue >= weak && bigBlind < context.MoneyLeft / (double)50) // we dont have a great hand either but we can make him sweat about it and we can always fold later
                        //    {
                        //        return PlayerAction.Raise(bigBlind * 2);
                        //    }
                        //    else if (handValue >= awful) // that makes around 74% of all possible hands
                        //    {
                        //        return PlayerAction.CheckOrCall();
                        //    }
                        //}
                        //else if (context.MyMoneyInTheRound == bigBlind) // opp has raised in position
                        //{

                        //}
                        //else // opp has reraised us
                        //{

                        //}

                        // a lot(has a strong hand)
                        if (context.MoneyToCall > context.SmallBlind * 8 && context.MoneyToCall > 40)
                        {
                            if (handValue >= extreme) // cards like AA, KK, AKs
                            {
                                return PlayerAction.Raise(context.SmallBlind * 16);
                            }
                            else if (handValue >= powerful)
                            {
                                // we have some more money and want to wait for a better shot
                                if (context.MoneyToCall > context.MoneyLeft / 4 && context.MoneyToCall > context.SmallBlind * 6)
                                {
                                    return this.Fold();
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
                        else // opponent has not raised a lot
                        {
                            if (handValue >= extreme) // cards like AA, KK, AKs
                            {
                                return PlayerAction.Raise(context.SmallBlind * 20);
                            }
                            else if (handValue >= powerful)
                            {
                                // if we have already raised enough this round
                                if (context.MyMoneyInTheRound > context.SmallBlind * 10)
                                {
                                    return PlayerAction.CheckOrCall();
                                }
                                else
                                {
                                    return PlayerAction.Raise(context.SmallBlind * 12);
                                }
                            }
                            else if (handValue >= normal)
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else if (handValue >= weak && (context.MoneyToCall <= 20 || context.MoneyToCall <= context.SmallBlind * 3))
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else if (handValue >= awful && (context.MoneyToCall <= 20 || context.MoneyToCall <= context.SmallBlind * 2))
                            {
                                return PlayerAction.CheckOrCall();
                            }
                            else
                            {
                                return this.Fold();
                            }
                        }
                    }
                }
            }
            #endregion

            #region Flop
            else if (context.RoundType == GameRoundType.Flop)
            {
                var raiseCoeff = context.SmallBlind * 0;

                if (context.MoneyLeft == 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                var flopCardStrength = CardsStrengthEvaluation.RateCards
                    (new List<Card> { FirstCard, SecondCard, CommunityCards.ElementAt(0), CommunityCards.ElementAt(1), CommunityCards.ElementAt(2) });

                if (inPosition)
                {
                    if (flopCardStrength >= 2000)
                    {
                        return PlayerAction.Raise(context.SmallBlind * 8 + raiseCoeff);
                    }
                    else if (flopCardStrength >= 1000)
                    {
                        var pairInfo = this.GetPairInfo();

                        if (pairInfo == 0)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            if (pairInfo >= 11)
                            {
                                return PlayerAction.Raise(context.SmallBlind * 12 + raiseCoeff);
                            }
                            else
                            {
                                return PlayerAction.Raise(context.SmallBlind * 8 + raiseCoeff);
                            }
                        }
                    }
                    else
                    {
                        return PlayerAction.CheckOrCall();
                    }
                }
                else
                {
                    // opponent has raised
                    if (context.MoneyToCall > 0)
                    {
                        // a lot
                        if (context.MoneyToCall > context.CurrentPot - context.MoneyToCall && context.MoneyToCall > 50)
                        {
                            if (flopCardStrength >= 3000)
                            {
                                return PlayerAction.Raise(context.SmallBlind * 30 + raiseCoeff);
                            }
                            if (flopCardStrength >= 2000)
                            {
                                return PlayerAction.Raise(context.SmallBlind * 10 + raiseCoeff);
                            }
                            else if (flopCardStrength >= 1000)
                            {
                                // is common pair logic
                                var pairInfo = this.GetPairInfo();

                                if (pairInfo == 0)
                                {
                                    return this.Fold();
                                }
                                else
                                {
                                    // money are a lot and we fold
                                    if (context.MoneyToCall > context.MoneyLeft / 3 && context.MoneyLeft > 300)
                                    {
                                        return this.Fold();
                                    }
                                    else
                                    {
                                        if (pairInfo >= 11)
                                        {
                                            return PlayerAction.CheckOrCall();
                                        }
                                        else
                                        {
                                            return this.Fold();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return this.Fold();
                            }
                        }
                        else //not a lot
                        {
                            if (flopCardStrength >= 2000)
                            {
                                return PlayerAction.Raise(context.SmallBlind * 8 + raiseCoeff);
                            }
                            else if (flopCardStrength >= 1000)
                            {
                                var pairInfo = this.GetPairInfo();

                                if (pairInfo == 0)
                                {
                                    return PlayerAction.CheckOrCall();
                                }
                                else
                                {
                                    if (pairInfo >= 11)
                                    {
                                        return PlayerAction.Raise(context.SmallBlind * 8 + raiseCoeff);
                                    }
                                    else
                                    {
                                        return PlayerAction.CheckOrCall();
                                    }
                                }
                            }
                            else
                            {
                                if (context.MoneyToCall >= 20)
                                {
                                    return this.Fold();
                                }
                                else
                                {
                                    return PlayerAction.CheckOrCall();
                                }
                            }
                        }
                    }
                    else // opp has checked (has bad hand)
                    {
                        if (flopCardStrength >= 2000)
                        {
                            return PlayerAction.Raise(context.SmallBlind * 8 + raiseCoeff);
                        }
                        else if (flopCardStrength >= 1000)
                        {
                            return PlayerAction.Raise(context.SmallBlind * 16 + raiseCoeff);
                        }

                        return PlayerAction.CheckOrCall();
                    }
                }
            }
            #endregion

            #region Turn
            else if (context.RoundType == GameRoundType.Turn || context.RoundType == GameRoundType.River)
            {
                if (context.RoundType == GameRoundType.River)
                {
                    inPosition = false;
                }

                if (context.MoneyLeft == 0)
                {
                    return PlayerAction.CheckOrCall();
                }

                var flopCardStrength = CardsStrengthEvaluation.RateCards
                    (new List<Card> { FirstCard, SecondCard, CommunityCards.ElementAt(0), CommunityCards.ElementAt(1), CommunityCards.ElementAt(2) });

                if (flopCardStrength >= 2000)
                {
                    return PlayerAction.Raise(context.CurrentPot);
                }
                else
                {
                    var hand = new List<Card>();
                    hand.Add(this.FirstCard);
                    hand.Add(this.SecondCard);

                    var ehs = EffectiveHandStrenghtCalculator.CalculateEHS(hand, this.CommunityCards);

                    if (ehs < 0.3)
                    {
                        if (context.MoneyToCall <= context.MoneyLeft / 200)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else if (ehs < 0.5)
                    {
                        if (context.MoneyToCall <= context.MoneyLeft / 40)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return this.Fold();
                        }
                    }
                    else if (ehs < 0.62)
                    {
                        var currentPot = context.CurrentPot;
                        int moneyToBet = (int)(currentPot * 0.55);

                        if (context.MoneyToCall == 0)
                        {
                            return PlayerAction.Raise(moneyToBet);
                        }
                        else if (context.MoneyToCall < context.MoneyLeft / 20 || context.MoneyToCall < 50)
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
                    else if (ehs < 0.75)
                    {
                        var currentPot = context.CurrentPot;
                        int moneyToBet = (int)(currentPot * 0.75);

                        if (context.MoneyToCall == 0)
                        {
                            return PlayerAction.Raise(moneyToBet);
                        }
                        else if (context.MoneyToCall < context.MoneyLeft / 10 || context.MoneyToCall < 70) // TODO:
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
                    else if (ehs < 0.85)
                    {
                        var currentPot = context.CurrentPot;
                        int moneyToBet = (int)(currentPot * 0.85);

                        if (context.MoneyToCall == 0)
                        {

                            if (moneyToBet < 50)
                            {
                                moneyToBet = 50;
                            }

                            return PlayerAction.Raise(moneyToBet);
                        }
                        else if (context.MoneyToCall < context.MoneyLeft / 2 || context.MoneyToCall < 250)
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
                        var currentPot = context.CurrentPot;
                        int moneyToBet = currentPot;
                        if (moneyToBet < 80)
                        {
                            moneyToBet = 80;
                        }

                        return PlayerAction.Raise(moneyToBet);
                    }
                }
            }
            #endregion

            return PlayerAction.CheckOrCall(); // It should never reach this point
        }

        private int GetPairInfo()
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
    }
}

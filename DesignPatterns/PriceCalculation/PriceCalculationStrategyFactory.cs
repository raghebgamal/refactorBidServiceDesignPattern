using Nafes.CrossCutting.Model.Enums;
using System;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Factory class that selects the appropriate pricing strategy based on bid type.
    ///
    /// This is a simple factory that returns the right strategy object.
    /// Think of it like a vending machine: you press a button (bid type),
    /// and it gives you the right product (pricing strategy).
    ///
    /// Benefits:
    /// - Centralizes strategy selection logic
    /// - Easy to add new bid types and their strategies
    /// - Client code doesn't need to know which strategy to use
    /// </summary>
    public class PriceCalculationStrategyFactory
    {
        /// <summary>
        /// Gets the appropriate pricing strategy for a given bid type
        /// </summary>
        /// <param name="bidType">The type of bid (Public, Private, Freelancing, etc.)</param>
        /// <returns>The pricing strategy to use for this bid type</returns>
        /// <exception cref="ArgumentException">Thrown when bid type is not supported</exception>
        public static IPriceCalculationStrategy GetStrategy(BidTypes bidType)
        {
            return bidType switch
            {
                // Public and Private bids use standard pricing
                BidTypes.Public => new StandardBidPriceStrategy(),
                BidTypes.Private => new StandardBidPriceStrategy(),
                BidTypes.Habilitation => new StandardBidPriceStrategy(),

                // Freelancing bids use their own strategy
                BidTypes.Freelancing => new FreelancingBidPriceStrategy(),

                // Instant bids use standard pricing (for now)
                BidTypes.Instant => new StandardBidPriceStrategy(),

                // If we encounter an unknown bid type, throw an error
                _ => throw new ArgumentException($"No pricing strategy defined for bid type: {bidType}", nameof(bidType))
            };
        }

        /// <summary>
        /// Gets the appropriate pricing strategy for a given bid type ID
        /// </summary>
        /// <param name="bidTypeId">The ID of the bid type</param>
        /// <returns>The pricing strategy to use</returns>
        public static IPriceCalculationStrategy GetStrategy(int bidTypeId)
        {
            // Convert int to enum, then get strategy
            if (!Enum.IsDefined(typeof(BidTypes), bidTypeId))
            {
                throw new ArgumentException($"Invalid bid type ID: {bidTypeId}", nameof(bidTypeId));
            }

            var bidType = (BidTypes)bidTypeId;
            return GetStrategy(bidType);
        }
    }
}

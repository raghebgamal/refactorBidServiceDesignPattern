using Nafes.CrossCutting.Common.OperationResponse;
using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Service class for calculating bid prices using Strategy pattern.
    ///
    /// This class acts as a facade (simple interface) to the pricing strategies.
    /// Instead of having calculation logic spread everywhere, we centralize it here.
    ///
    /// Usage Example:
    /// var service = new BidPriceCalculationService();
    /// var result = service.CalculatePrice(1000, settings, BidTypes.Public);
    /// if (result.IsValid)
    ///     Console.WriteLine($"Total Price: {result.TotalPrice}");
    /// </summary>
    public class BidPriceCalculationService
    {
        /// <summary>
        /// Calculates price for a bid using the appropriate strategy
        /// </summary>
        /// <param name="associationFees">Base association fees</param>
        /// <param name="settings">Application settings</param>
        /// <param name="bidType">Type of bid</param>
        /// <returns>Complete price breakdown</returns>
        public PriceCalculationResult CalculatePrice(double associationFees, ReadOnlyAppGeneralSettings settings, BidTypes bidType)
        {
            // Step 1: Get the right strategy for this bid type
            var strategy = PriceCalculationStrategyFactory.GetStrategy(bidType);

            // Step 2: Use the strategy to calculate the price
            var result = strategy.Calculate(associationFees, settings);

            return result;
        }

        /// <summary>
        /// Calculates price and updates the bid entity
        /// </summary>
        /// <param name="associationFees">Base association fees</param>
        /// <param name="settings">Application settings</param>
        /// <param name="bid">Bid entity to update</param>
        /// <returns>Operation result</returns>
        public OperationResult<bool> CalculateAndUpdateBidPrices(double associationFees, ReadOnlyAppGeneralSettings settings, Bid bid)
        {
            // Validation
            if (bid == null)
            {
                return OperationResult<bool>.Fail(HttpErrorCode.NotFound, CommonErrorCodes.NOT_FOUND);
            }

            if (settings == null)
            {
                return OperationResult<bool>.Fail(HttpErrorCode.NotFound, CommonErrorCodes.NOT_FOUND);
            }

            // Determine bid type (default to Public if not specified)
            var bidType = bid.BidTypeId.HasValue ? (BidTypes)bid.BidTypeId.Value : BidTypes.Public;

            // Calculate using the appropriate strategy
            var calculation = CalculatePrice(associationFees, settings, bidType);

            // Check if calculation was successful
            if (!calculation.IsValid)
            {
                return OperationResult<bool>.Fail(HttpErrorCode.Conflict, CommonErrorCodes.INVALID_INPUT);
            }

            // Update the bid entity with calculated values
            bid.Association_Fees = calculation.AssociationFees;
            bid.Tanafos_Fees = calculation.TanafosFees;
            bid.Bid_Documents_Price = calculation.TotalPrice;

            return OperationResult<bool>.Success(true);
        }

        /// <summary>
        /// Calculates just the total price (quick method for display purposes)
        /// </summary>
        public double CalculateTotalPrice(double associationFees, ReadOnlyAppGeneralSettings settings, BidTypes bidType)
        {
            var result = CalculatePrice(associationFees, settings, bidType);
            return result.IsValid ? result.TotalPrice : 0;
        }
    }
}

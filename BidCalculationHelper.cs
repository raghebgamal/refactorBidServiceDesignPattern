using Nafes.CrossCutting.Common.OperationResponse;
using Nafes.CrossCutting.Model.Entities;
using Nafis.Services.DTO.Bid;
using System;

namespace Nafis.Services.Implementation.Helpers
{
    /// <summary>
    /// Helper class for bid calculation logic (prices, fees, taxes)
    /// </summary>
    public static class BidCalculationHelper
    {
        /// <summary>
        /// Calculates and updates bid prices including Tanafos fees and taxes
        /// </summary>
        /// <param name="association_Fees">Association fees amount</param>
        /// <param name="settings">General application settings</param>
        /// <param name="bid">Bid entity to update</param>
        /// <returns>Operation result indicating success or failure</returns>
        public static OperationResult<bool> CalculateAndUpdateBidPrices(double association_Fees, ReadOnlyAppGeneralSettings settings, Bid bid)
        {
            if (bid is null || settings is null)
                return OperationResult<bool>.Fail(HttpErrorCode.NotFound, CommonErrorCodes.NOT_FOUND);

            // Calculate Tanafos fees without tax
            double tanafosMoneyWithoutTax = Math.Round((association_Fees * ((double)settings.TanfasPercentage / 100)), 8);
            if (tanafosMoneyWithoutTax < settings.MinTanfasOfBidDocumentPrice)
                tanafosMoneyWithoutTax = settings.MinTanfasOfBidDocumentPrice;

            // Calculate total prices
            var bidDocumentPricesWithoutTax = Math.Round((association_Fees + tanafosMoneyWithoutTax), 8);
            var bidDocumentTax = Math.Round((bidDocumentPricesWithoutTax * ((double)settings.VATPercentage / 100)), 8);
            var bidDocumentPricesWithTax = Math.Round((bidDocumentPricesWithoutTax + bidDocumentTax), 8);

            // Validate calculated prices
            if (association_Fees < 0 || bidDocumentPricesWithTax > settings.MaxBidDocumentPrice)
                return OperationResult<bool>.Fail(HttpErrorCode.Conflict, CommonErrorCodes.INVALID_INPUT);

            // Update bid with calculated values
            bid.Association_Fees = association_Fees;
            bid.Tanafos_Fees = tanafosMoneyWithoutTax;
            bid.Bid_Documents_Price = bidDocumentPricesWithTax;

            return OperationResult<bool>.Success(true);
        }

        /// <summary>
        /// Calculates Tanafos fees based on association fees
        /// </summary>
        public static double CalculateTanafos Fees(double associationFees, double tanfasPercentage, double minTanfasOfBidDocumentPrice)
        {
            double tanafosMoneyWithoutTax = Math.Round((associationFees * (tanfasPercentage / 100)), 8);
            if (tanafosMoneyWithoutTax < minTanfasOfBidDocumentPrice)
                tanafosMoneyWithoutTax = minTanfasOfBidDocumentPrice;

            return tanafosMoneyWithoutTax;
        }

        /// <summary>
        /// Calculates VAT for a given amount
        /// </summary>
        public static double CalculateVAT(double amountWithoutTax, double vatPercentage)
        {
            return Math.Round((amountWithoutTax * (vatPercentage / 100)), 8);
        }

        /// <summary>
        /// Calculates total bid document price with all fees and taxes
        /// </summary>
        public static double CalculateTotalBidDocumentPrice(double associationFees, ReadOnlyAppGeneralSettings settings)
        {
            double tanafosMoneyWithoutTax = CalculateTanafos Fees(associationFees, settings.TanfasPercentage, settings.MinTanfasOfBidDocumentPrice);
            var bidDocumentPricesWithoutTax = Math.Round((associationFees + tanafosMoneyWithoutTax), 8);
            var bidDocumentTax = CalculateVAT(bidDocumentPricesWithoutTax, settings.VATPercentage);
            return Math.Round((bidDocumentPricesWithoutTax + bidDocumentTax), 8);
        }
    }
}

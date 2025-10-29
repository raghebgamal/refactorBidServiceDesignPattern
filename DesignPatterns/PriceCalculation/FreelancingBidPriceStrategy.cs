using Nafes.CrossCutting.Model.Entities;
using System;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Pricing strategy specifically for Freelancing bids.
    ///
    /// Currently uses the same calculation as Standard bids, but having a separate
    /// strategy allows us to easily change freelancing pricing in the future without
    /// affecting other bid types.
    ///
    /// Future possibilities:
    /// - Different Tanafos percentage for freelancers
    /// - Different minimum fees
    /// - Special discounts for freelancing bids
    /// - Tiered pricing based on bid value
    /// </summary>
    public class FreelancingBidPriceStrategy : IPriceCalculationStrategy
    {
        public string StrategyName => "Freelancing Bid Pricing";

        public PriceCalculationResult Calculate(double associationFees, ReadOnlyAppGeneralSettings settings)
        {
            // Validation
            if (settings == null)
            {
                return PriceCalculationResult.Failure("Settings cannot be null");
            }

            if (associationFees < 0)
            {
                return PriceCalculationResult.Failure("Association fees cannot be negative");
            }

            // Calculate Tanafos fees (same as standard for now)
            double tanafosFees = CalculateTanafosFees(associationFees, settings.TanfasPercentage, settings.MinTanfasOfBidDocumentPrice);

            // Calculate subtotal
            double subtotalWithoutTax = Math.Round(associationFees + tanafosFees, 8);

            // Calculate VAT
            double vatAmount = CalculateVAT(subtotalWithoutTax, settings.VATPercentage);

            // Calculate total
            double totalPrice = Math.Round(subtotalWithoutTax + vatAmount, 8);

            // Validation: Check max price
            if (totalPrice > settings.MaxBidDocumentPrice)
            {
                return PriceCalculationResult.Failure(
                    $"Total price {totalPrice} exceeds maximum allowed price {settings.MaxBidDocumentPrice}");
            }

            return PriceCalculationResult.Success(
                associationFees: associationFees,
                tanafosFees: tanafosFees,
                subtotalWithoutTax: subtotalWithoutTax,
                vatAmount: vatAmount,
                totalPrice: totalPrice
            );
        }

        private double CalculateTanafosFees(double associationFees, double tanfasPercentage, double minTanfasPrice)
        {
            double calculatedFee = Math.Round((associationFees * (tanfasPercentage / 100)), 8);
            return calculatedFee < minTanfasPrice ? minTanfasPrice : calculatedFee;
        }

        private double CalculateVAT(double amountWithoutTax, double vatPercentage)
        {
            return Math.Round((amountWithoutTax * (vatPercentage / 100)), 8);
        }
    }
}

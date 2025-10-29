using Nafes.CrossCutting.Model.Entities;
using System;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Standard pricing strategy for regular bids (Public and Private bids).
    ///
    /// Calculation Steps:
    /// 1. Calculate Tanafos fees (percentage of association fees, with minimum threshold)
    /// 2. Calculate subtotal (association fees + Tanafos fees)
    /// 3. Calculate VAT on the subtotal
    /// 4. Calculate final total (subtotal + VAT)
    ///
    /// Example:
    /// Association Fees = 1000 SAR
    /// Tanafos Percentage = 5%
    /// Min Tanafos = 50 SAR
    /// VAT = 15%
    ///
    /// Step 1: Tanafos = 1000 * 5% = 50 SAR (meets minimum)
    /// Step 2: Subtotal = 1000 + 50 = 1050 SAR
    /// Step 3: VAT = 1050 * 15% = 157.5 SAR
    /// Step 4: Total = 1050 + 157.5 = 1207.5 SAR
    /// </summary>
    public class StandardBidPriceStrategy : IPriceCalculationStrategy
    {
        public string StrategyName => "Standard Bid Pricing";

        public PriceCalculationResult Calculate(double associationFees, ReadOnlyAppGeneralSettings settings)
        {
            // Validation: Check for null settings
            if (settings == null)
            {
                return PriceCalculationResult.Failure("Settings cannot be null");
            }

            // Validation: Check for negative fees
            if (associationFees < 0)
            {
                return PriceCalculationResult.Failure("Association fees cannot be negative");
            }

            // Step 1: Calculate Tanafos fees
            // Formula: (Association Fees × Tanafos Percentage) / 100
            // Then apply minimum threshold
            double tanafosFees = CalculateTanafosFees(associationFees, settings.TanfasPercentage, settings.MinTanfasOfBidDocumentPrice);

            // Step 2: Calculate subtotal (before tax)
            double subtotalWithoutTax = Math.Round(associationFees + tanafosFees, 8);

            // Step 3: Calculate VAT amount
            // Formula: (Subtotal × VAT Percentage) / 100
            double vatAmount = CalculateVAT(subtotalWithoutTax, settings.VATPercentage);

            // Step 4: Calculate final total price
            double totalPrice = Math.Round(subtotalWithoutTax + vatAmount, 8);

            // Validation: Check against maximum allowed price
            if (totalPrice > settings.MaxBidDocumentPrice)
            {
                return PriceCalculationResult.Failure(
                    $"Total price {totalPrice} exceeds maximum allowed price {settings.MaxBidDocumentPrice}");
            }

            // Return successful result with complete breakdown
            return PriceCalculationResult.Success(
                associationFees: associationFees,
                tanafosFees: tanafosFees,
                subtotalWithoutTax: subtotalWithoutTax,
                vatAmount: vatAmount,
                totalPrice: totalPrice
            );
        }

        /// <summary>
        /// Calculates Tanafos platform fees with minimum threshold
        /// </summary>
        /// <param name="associationFees">Base fees</param>
        /// <param name="tanfasPercentage">Percentage to apply (e.g., 5 for 5%)</param>
        /// <param name="minTanfasPrice">Minimum Tanafos fee amount</param>
        /// <returns>Calculated Tanafos fees (at least the minimum)</returns>
        private double CalculateTanafosFees(double associationFees, double tanfasPercentage, double minTanfasPrice)
        {
            // Calculate percentage-based fee
            double calculatedFee = Math.Round((associationFees * (tanfasPercentage / 100)), 8);

            // Apply minimum threshold: return whichever is higher
            return calculatedFee < minTanfasPrice ? minTanfasPrice : calculatedFee;
        }

        /// <summary>
        /// Calculates VAT (Value Added Tax) amount
        /// </summary>
        /// <param name="amountWithoutTax">Amount before tax</param>
        /// <param name="vatPercentage">VAT percentage (e.g., 15 for 15%)</param>
        /// <returns>Calculated VAT amount</returns>
        private double CalculateVAT(double amountWithoutTax, double vatPercentage)
        {
            return Math.Round((amountWithoutTax * (vatPercentage / 100)), 8);
        }
    }
}

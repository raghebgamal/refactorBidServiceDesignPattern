using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using System;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Example usage of the Price Calculation Strategy Pattern.
    /// This file shows you how to use the new pricing system.
    /// </summary>
    public class PriceCalculationExamples
    {
        /// <summary>
        /// Example 1: Basic price calculation
        /// </summary>
        public static void Example1_BasicCalculation()
        {
            Console.WriteLine("=== Example 1: Basic Price Calculation ===\n");

            // Create the service
            var priceService = new BidPriceCalculationService();

            // Create settings (in real code, you get this from database)
            var settings = new ReadOnlyAppGeneralSettings
            {
                TanfasPercentage = 5,                    // 5% Tanafos fee
                MinTanfasOfBidDocumentPrice = 50,        // Minimum 50 SAR
                VATPercentage = 15,                       // 15% VAT
                MaxBidDocumentPrice = 100000              // Max 100,000 SAR
            };

            // Calculate price for a Public bid with 1000 SAR association fees
            var result = priceService.CalculatePrice(
                associationFees: 1000,
                settings: settings,
                bidType: BidTypes.Public
            );

            // Display results
            if (result.IsValid)
            {
                Console.WriteLine("✓ Calculation Successful!");
                Console.WriteLine($"Association Fees:    {result.AssociationFees:N2} SAR");
                Console.WriteLine($"Tanafos Fees:        {result.TanafosFees:N2} SAR");
                Console.WriteLine($"Subtotal (no tax):   {result.SubtotalWithoutTax:N2} SAR");
                Console.WriteLine($"VAT (15%):           {result.VATAmount:N2} SAR");
                Console.WriteLine($"──────────────────────────────────");
                Console.WriteLine($"TOTAL PRICE:         {result.TotalPrice:N2} SAR");
            }
            else
            {
                Console.WriteLine($"✗ Error: {result.ErrorMessage}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 2: Calculate and update bid entity
        /// </summary>
        public static void Example2_CalculateAndUpdateBid()
        {
            Console.WriteLine("=== Example 2: Calculate and Update Bid ===\n");

            var priceService = new BidPriceCalculationService();

            var settings = new ReadOnlyAppGeneralSettings
            {
                TanfasPercentage = 5,
                MinTanfasOfBidDocumentPrice = 50,
                VATPercentage = 15,
                MaxBidDocumentPrice = 100000
            };

            // Create a bid entity
            var bid = new Bid
            {
                Id = 1,
                BidName = "Test Bid",
                BidTypeId = (int)BidTypes.Public
            };

            Console.WriteLine($"Before: Bid Prices:");
            Console.WriteLine($"  Association Fees: {bid.Association_Fees}");
            Console.WriteLine($"  Tanafos Fees:     {bid.Tanafos_Fees}");
            Console.WriteLine($"  Total Price:      {bid.Bid_Documents_Price}");

            // Calculate and update the bid
            var updateResult = priceService.CalculateAndUpdateBidPrices(
                associationFees: 2000,
                settings: settings,
                bid: bid
            );

            if (updateResult.IsSucceeded)
            {
                Console.WriteLine("\n✓ Bid Updated Successfully!");
                Console.WriteLine($"After: Bid Prices:");
                Console.WriteLine($"  Association Fees: {bid.Association_Fees:N2} SAR");
                Console.WriteLine($"  Tanafos Fees:     {bid.Tanafos_Fees:N2} SAR");
                Console.WriteLine($"  Total Price:      {bid.Bid_Documents_Price:N2} SAR");
            }
            else
            {
                Console.WriteLine($"\n✗ Update Failed: {updateResult.Code}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 3: Different bid types use different strategies
        /// </summary>
        public static void Example3_DifferentBidTypes()
        {
            Console.WriteLine("=== Example 3: Different Bid Types ===\n");

            var priceService = new BidPriceCalculationService();

            var settings = new ReadOnlyAppGeneralSettings
            {
                TanfasPercentage = 5,
                MinTanfasOfBidDocumentPrice = 50,
                VATPercentage = 15,
                MaxBidDocumentPrice = 100000
            };

            double associationFees = 1500;

            // Calculate for different bid types
            var bidTypes = new[]
            {
                BidTypes.Public,
                BidTypes.Private,
                BidTypes.Freelancing
            };

            foreach (var bidType in bidTypes)
            {
                var result = priceService.CalculatePrice(associationFees, settings, bidType);

                Console.WriteLine($"Bid Type: {bidType}");
                Console.WriteLine($"  Total Price: {result.TotalPrice:N2} SAR");
                Console.WriteLine($"  Strategy Used: {GetStrategyName(bidType)}");
                Console.WriteLine();
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 4: Testing edge cases
        /// </summary>
        public static void Example4_EdgeCases()
        {
            Console.WriteLine("=== Example 4: Edge Cases ===\n");

            var priceService = new BidPriceCalculationService();

            var settings = new ReadOnlyAppGeneralSettings
            {
                TanfasPercentage = 5,
                MinTanfasOfBidDocumentPrice = 100,       // High minimum
                VATPercentage = 15,
                MaxBidDocumentPrice = 1000               // Low maximum
            };

            // Test 1: Low fees (should use minimum Tanafos)
            Console.WriteLine("Test 1: Low Association Fees (100 SAR)");
            var result1 = priceService.CalculatePrice(100, settings, BidTypes.Public);
            Console.WriteLine($"  Tanafos Fees: {result1.TanafosFees:N2} SAR (should be minimum: 100)");
            Console.WriteLine($"  Is Valid: {result1.IsValid}");

            Console.WriteLine();

            // Test 2: High fees (should exceed maximum)
            Console.WriteLine("Test 2: High Association Fees (10,000 SAR)");
            var result2 = priceService.CalculatePrice(10000, settings, BidTypes.Public);
            Console.WriteLine($"  Is Valid: {result2.IsValid}");
            Console.WriteLine($"  Error: {result2.ErrorMessage}");

            Console.WriteLine();

            // Test 3: Negative fees (should fail)
            Console.WriteLine("Test 3: Negative Association Fees (-100 SAR)");
            var result3 = priceService.CalculatePrice(-100, settings, BidTypes.Public);
            Console.WriteLine($"  Is Valid: {result3.IsValid}");
            Console.WriteLine($"  Error: {result3.ErrorMessage}");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 5: Quick total price calculation
        /// </summary>
        public static void Example5_QuickTotal()
        {
            Console.WriteLine("=== Example 5: Quick Total Calculation ===\n");

            var priceService = new BidPriceCalculationService();

            var settings = new ReadOnlyAppGeneralSettings
            {
                TanfasPercentage = 5,
                MinTanfasOfBidDocumentPrice = 50,
                VATPercentage = 15,
                MaxBidDocumentPrice = 100000
            };

            // Just get the total price (no full breakdown)
            double total = priceService.CalculateTotalPrice(
                associationFees: 5000,
                settings: settings,
                bidType: BidTypes.Public
            );

            Console.WriteLine($"Quick Total for 5000 SAR bid: {total:N2} SAR");
            Console.WriteLine("(Use this when you only need the final price for display)");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║   PRICE CALCULATION STRATEGY PATTERN EXAMPLES      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            Example1_BasicCalculation();
            Example2_CalculateAndUpdateBid();
            Example3_DifferentBidTypes();
            Example4_EdgeCases();
            Example5_QuickTotal();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ALL EXAMPLES COMPLETED                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
        }

        // Helper method
        private static string GetStrategyName(BidTypes bidType)
        {
            var strategy = PriceCalculationStrategyFactory.GetStrategy(bidType);
            return strategy.StrategyName;
        }
    }
}

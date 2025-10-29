using Nafes.CrossCutting.Model.Entities;

namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Strategy interface for different price calculation methods.
    /// This allows us to have different pricing strategies (Standard, Freelancing, etc.)
    /// without changing the code that uses them.
    /// </summary>
    public interface IPriceCalculationStrategy
    {
        /// <summary>
        /// Calculates the complete price breakdown for a bid
        /// </summary>
        /// <param name="associationFees">Base association fees</param>
        /// <param name="settings">Application general settings (VAT %, Tanafos %, etc.)</param>
        /// <returns>Complete price breakdown</returns>
        PriceCalculationResult Calculate(double associationFees, ReadOnlyAppGeneralSettings settings);

        /// <summary>
        /// Returns the name of this pricing strategy
        /// </summary>
        string StrategyName { get; }
    }
}

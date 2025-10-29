namespace Nafis.Services.Implementation.DesignPatterns.PriceCalculation
{
    /// <summary>
    /// Holds the complete breakdown of a price calculation.
    /// This is a simple data transfer object (DTO) that contains all price components.
    /// </summary>
    public class PriceCalculationResult
    {
        /// <summary>
        /// Base association fees (the starting amount)
        /// </summary>
        public double AssociationFees { get; set; }

        /// <summary>
        /// Tanafos platform fees (percentage of association fees)
        /// </summary>
        public double TanafosFees { get; set; }

        /// <summary>
        /// Subtotal before tax (Association Fees + Tanafos Fees)
        /// </summary>
        public double SubtotalWithoutTax { get; set; }

        /// <summary>
        /// VAT amount calculated on the subtotal
        /// </summary>
        public double VATAmount { get; set; }

        /// <summary>
        /// Final total price including all fees and taxes
        /// </summary>
        public double TotalPrice { get; set; }

        /// <summary>
        /// Indicates if the calculation was successful
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if calculation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful calculation result
        /// </summary>
        public static PriceCalculationResult Success(double associationFees, double tanafosFees,
            double subtotalWithoutTax, double vatAmount, double totalPrice)
        {
            return new PriceCalculationResult
            {
                AssociationFees = associationFees,
                TanafosFees = tanafosFees,
                SubtotalWithoutTax = subtotalWithoutTax,
                VATAmount = vatAmount,
                TotalPrice = totalPrice,
                IsValid = true,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// Creates a failed calculation result
        /// </summary>
        public static PriceCalculationResult Failure(string errorMessage)
        {
            return new PriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}

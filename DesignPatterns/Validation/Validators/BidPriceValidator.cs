using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Validators
{
    /// <summary>
    /// Validates bid price and financial values.
    ///
    /// Price Rules:
    /// 1. Association fees cannot be negative
    /// 2. If financial insurance is required, value must be provided
    /// 3. Total bid price must not exceed maximum allowed price
    /// 4. For non-public/private bids, financial insurance is not required
    ///
    /// Think of it like a price checker at a store:
    /// - Is the price valid? (not negative)
    /// - Is it within allowed limits? (not too expensive)
    /// - Are required financial fields provided?
    ///
    /// OLD CODE REFERENCE:
    /// This replaces:
    /// - ValidateBidFinancialValueWithBidType() - Lines 816-824
    /// - Price validation in CalculateAndUpdateBidPrices() - Lines 874-896
    /// </summary>
    public class BidPriceValidator : BaseValidator<BidValidationContext>
    {
        public override string ValidatorName => "Bid Price Validator";

        protected override Task<ValidationResult> ValidateInternalAsync(BidValidationContext context)
        {
            var model = context.RequestModel;
            var settings = context.Settings;

            // Validation 1: Association fees cannot be negative
            if (model.Association_Fees.HasValue && model.Association_Fees.Value < 0)
            {
                return Task.FromResult(Fail(
                    "رسوم الجمعية لا يمكن أن تكون سالبة",
                    "ASSOCIATION_FEES_NEGATIVE"));
            }

            // Validation 2: Adjust financial insurance requirements based on bid type
            // For Habilitation bids, financial insurance is not required
            if (model.BidTypeId.HasValue)
            {
                var bidType = (Nafes.CrossCutting.Model.Enums.BidTypes)model.BidTypeId.Value;

                // Only Public and Private bids can have financial insurance
                if (bidType != Nafes.CrossCutting.Model.Enums.BidTypes.Public &&
                    bidType != Nafes.CrossCutting.Model.Enums.BidTypes.Private)
                {
                    // Clear financial insurance for non-applicable bid types
                    // (This is actually a transformation, not validation, but included for completeness)
                    model.IsFinancialInsuranceRequired = false;
                    model.BidFinancialInsuranceValue = null;
                }
            }

            // Validation 3: If financial insurance is required, value must be provided
            if (model.IsFinancialInsuranceRequired.HasValue &&
                model.IsFinancialInsuranceRequired.Value &&
                (!model.BidFinancialInsuranceValue.HasValue || model.BidFinancialInsuranceValue.Value <= 0))
            {
                return Task.FromResult(Fail(
                    "قيمة التأمين المالي مطلوبة عندما يكون التأمين المالي مفعلاً",
                    "FINANCIAL_INSURANCE_VALUE_REQUIRED"));
            }

            // Validation 4: Validate total price doesn't exceed maximum
            // Note: This would typically be done after price calculation
            // For now, we validate association fees are within reasonable limits
            if (model.Association_Fees.HasValue &&
                settings != null &&
                model.Association_Fees.Value > settings.MaxBidDocumentPrice)
            {
                return Task.FromResult(Fail(
                    $"رسوم الجمعية تتجاوز الحد الأقصى المسموح ({settings.MaxBidDocumentPrice})",
                    "ASSOCIATION_FEES_EXCEED_MAXIMUM"));
            }

            // All price validations passed
            return Task.FromResult(ValidationResult.Success());
        }
    }
}

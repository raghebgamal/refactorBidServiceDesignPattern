using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Validators
{
    /// <summary>
    /// Validates that all required fields are provided for a bid.
    ///
    /// This is usually the FIRST validator in the chain because there's no point
    /// validating complex business rules if basic required fields are missing.
    ///
    /// Checks:
    /// - Bid name is provided
    /// - Regions are provided (unless draft)
    /// - Dates are provided (unless draft)
    /// - Other required fields based on bid type
    ///
    /// OLD CODE REFERENCE:
    /// This replaces IsRequiredDataForNotSaveAsDraftAdded() - Lines 825-831 in BidServiceCore
    /// </summary>
    public class RequiredFieldsValidator : BaseValidator<BidValidationContext>
    {
        public override string ValidatorName => "Required Fields Validator";

        protected override Task<ValidationResult> ValidateInternalAsync(BidValidationContext context)
        {
            // If it's a draft, skip required field validation (drafts can be incomplete)
            if (context.IsDraft)
            {
                return Task.FromResult(ValidationResult.Success());
            }

            // Check bid name
            if (IsNullOrEmpty(context.RequestModel.BidName))
            {
                return Task.FromResult(Fail("اسم المنافسة مطلوب", "BID_NAME_REQUIRED"));
            }

            // Check required dates for non-draft bids
            var hasAllRequiredDates = context.RequestModel.LastDateInReceivingEnquiries.HasValue &&
                                     context.RequestModel.LastDateInOffersSubmission.HasValue &&
                                     context.RequestModel.OffersOpeningDate.HasValue;

            if (!hasAllRequiredDates)
            {
                return Task.FromResult(Fail(
                    "جميع التواريخ مطلوبة (آخر موعد لاستلام الاستفسارات، آخر موعد لتقديم العروض، موعد فتح العروض)",
                    "REQUIRED_DATES_MISSING"));
            }

            // Check regions are provided
            if (context.RequestModel.RegionsId == null || context.RequestModel.RegionsId.Count == 0)
            {
                return Task.FromResult(Fail("يجب تحديد منطقة واحدة على الأقل", "REGIONS_REQUIRED"));
            }

            // All required fields are present
            return Task.FromResult(ValidationResult.Success());
        }
    }
}

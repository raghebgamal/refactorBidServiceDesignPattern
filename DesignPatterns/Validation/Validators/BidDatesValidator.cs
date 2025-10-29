using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Validators
{
    /// <summary>
    /// Validates bid date logic and ordering.
    ///
    /// Date Rules:
    /// 1. LastDateInReceivingEnquiries must be BEFORE LastDateInOffersSubmission
    /// 2. LastDateInOffersSubmission must be BEFORE OffersOpeningDate
    /// 3. If ExpectedAnchoringDate is provided, it must be AFTER OffersOpeningDate + StoppingPeriod
    /// 4. When updating, LastDateInReceivingEnquiries can't be in the past (unless same as existing)
    ///
    /// Think of it like a timeline:
    /// [Receive Questions] → [Submit Offers] → [Open Offers] → [Stopping Period] → [Award]
    ///       Date1                Date2            Date3           X days          Date4
    ///
    /// Each date must come AFTER the previous one!
    ///
    /// OLD CODE REFERENCE:
    /// This replaces:
    /// - ValidateBidDates() - Lines 845-862
    /// - ValidateBidDatesWhileApproving() - Lines 2025-2042
    /// - checkLastReceivingEnqiryDate() - Lines 926-932
    /// </summary>
    public class BidDatesValidator : BaseValidator<BidValidationContext>
    {
        public override string ValidatorName => "Bid Dates Validator";

        protected override Task<ValidationResult> ValidateInternalAsync(BidValidationContext context)
        {
            // Skip date validation for drafts
            if (context.IsDraft)
            {
                return Task.FromResult(ValidationResult.Success());
            }

            var model = context.RequestModel;
            var settings = context.Settings;

            // Validation 1: Check if dates are in the past (for updates only)
            if (context.IsUpdate && CheckLastReceivingEnquiryDate(model, context.ExistingBid))
            {
                return Task.FromResult(Fail(
                    "آخر موعد لاستلام الاستفسارات لا يمكن أن يكون في الماضي",
                    "LAST_DATE_RECEIVING_ENQUIRIES_IN_PAST"));
            }

            // Validation 2: LastDateInReceivingEnquiries must be BEFORE LastDateInOffersSubmission
            if (model.LastDateInReceivingEnquiries > model.LastDateInOffersSubmission)
            {
                return Task.FromResult(Fail(
                    "آخر موعد لتقديم العروض يجب أن يكون بعد آخر موعد لاستلام الاستفسارات",
                    "OFFERS_SUBMISSION_DATE_INVALID"));
            }

            // Validation 3: LastDateInOffersSubmission must be BEFORE OffersOpeningDate
            if (model.LastDateInOffersSubmission > model.OffersOpeningDate)
            {
                return Task.FromResult(Fail(
                    "موعد فتح العروض يجب أن يكون بعد آخر موعد لتقديم العروض",
                    "OFFERS_OPENING_DATE_INVALID"));
            }

            // Validation 4: ExpectedAnchoringDate must be after OffersOpeningDate + StoppingPeriod
            if (model.ExpectedAnchoringDate != null &&
                model.ExpectedAnchoringDate != default(DateTime) &&
                model.OffersOpeningDate.HasValue)
            {
                var minimumAnchoringDate = model.OffersOpeningDate.Value.AddDays(settings.StoppingPeriodDays);

                if (model.ExpectedAnchoringDate < minimumAnchoringDate)
                {
                    return Task.FromResult(Fail(
                        $"تاريخ الترسية المتوقع يجب أن يكون بعد {settings.StoppingPeriodDays} يوم من موعد فتح العروض",
                        "EXPECTED_ANCHORING_DATE_INVALID"));
                }
            }

            // All date validations passed
            return Task.FromResult(ValidationResult.Success());
        }

        /// <summary>
        /// Checks if the last receiving enquiry date is being changed to a past date.
        /// This prevents users from setting dates in the past when updating bids.
        /// </summary>
        private bool CheckLastReceivingEnquiryDate(AddBidModelNew model, Bid existingBid)
        {
            // If no existing bid, this is a create operation, skip this check
            if (existingBid == null)
                return false;

            // If the date hasn't changed, it's okay
            if (existingBid.LastDateInReceivingEnquiries.HasValue &&
                model.LastDateInReceivingEnquiries.HasValue &&
                existingBid.LastDateInReceivingEnquiries.Value.Date == model.LastDateInReceivingEnquiries.Value.Date)
            {
                return false;
            }

            // If the new date is in the past, that's a problem
            if (model.LastDateInReceivingEnquiries.HasValue &&
                model.LastDateInReceivingEnquiries.Value < DateTime.UtcNow)
            {
                return true;  // Fail validation
            }

            return false;  // Pass validation
        }
    }
}

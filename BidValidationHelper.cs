using Nafes.CrossCutting.Common.OperationResponse;
using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using Nafis.Services.DTO.Bid;
using Tanafos.Main.Services.DTO.Bid;
using System;

namespace Nafis.Services.Implementation.Helpers
{
    /// <summary>
    /// Helper class for bid validation logic
    /// </summary>
    public static class BidValidationHelper
    {
        /// <summary>
        /// Validates bid financial value with bid type and adjusts model accordingly
        /// </summary>
        public static void ValidateBidFinancialValueWithBidType(AddBidModelNew model)
        {
            if (model.BidTypeId != (int)BidTypes.Public && model.BidTypeId != (int)BidTypes.Private)
            {
                model.IsFinancialInsuranceRequired = false;
                model.BidFinancialInsuranceValue = null;
            }
        }

        /// <summary>
        /// Checks if required data for non-draft bids is added
        /// </summary>
        public static bool IsRequiredDataForNotSaveAsDraftAdded(AddBidModelNew model)
        {
            var isAllRequiredDatesAdded = model.LastDateInReceivingEnquiries.HasValue &&
                 model.LastDateInOffersSubmission.HasValue && model.OffersOpeningDate.HasValue;
            return !model.IsDraft && ((!isAllRequiredDatesAdded) || (model.RegionsId is null || model.RegionsId.Count == 0));
        }

        /// <summary>
        /// Adjusts request bid addresses to the end of the day
        /// </summary>
        public static OperationResult<bool> AdjustRequestBidAddressesToTheEndOfTheDay<T>(T model) where T : BidAddressesModelRequest
        {
            if (model is null)
                return OperationResult<bool>.Fail(HttpErrorCode.InvalidInput, CommonErrorCodes.INVALID_INPUT);

            model.LastDateInReceivingEnquiries = model.LastDateInReceivingEnquiries is null ? model.LastDateInReceivingEnquiries : new DateTime(model.LastDateInReceivingEnquiries.Value.Year, model.LastDateInReceivingEnquiries.Value.Month, model.LastDateInReceivingEnquiries.Value.Day, 23, 59, 59);
            model.LastDateInOffersSubmission = model.LastDateInOffersSubmission is null ? model.LastDateInOffersSubmission : new DateTime(model.LastDateInOffersSubmission.Value.Year, model.LastDateInOffersSubmission.Value.Month, model.LastDateInOffersSubmission.Value.Day, 23, 59, 59);
            model.OffersOpeningDate = model.OffersOpeningDate is null ? model.OffersOpeningDate : new DateTime(model.OffersOpeningDate.Value.Year, model.OffersOpeningDate.Value.Month, model.OffersOpeningDate.Value.Day, 00, 00, 00);
            model.ExpectedAnchoringDate = model.ExpectedAnchoringDate.HasValue ? new DateTime(model.ExpectedAnchoringDate.Value.Year, model.ExpectedAnchoringDate.Value.Month, model.ExpectedAnchoringDate.Value.Day, 00, 00, 00) : null;

            return OperationResult<bool>.Success(true);
        }

        /// <summary>
        /// Validates bid dates for logical consistency
        /// </summary>
        public static OperationResult<AddBidResponse> ValidateBidDates(AddBidModelNew model, Bid bid, ReadOnlyAppGeneralSettings generalSettings, Func<AddBidModelNew, Bid, bool> checkLastReceivingEnqiryDate)
        {
            if (bid is not null && checkLastReceivingEnqiryDate(model, bid))
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.LAST_DATE_IN_RECEIVING_ENQUIRIES_MUST_NOT_BE_BEFORE_TODAY_DATE);

            else if (model.LastDateInReceivingEnquiries > model.LastDateInOffersSubmission)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.LAST_DATE_IN_OFFERS_SUBMISSION_MUST_BE_GREATER_THAN_LAST_DATE_IN_RECEIVING_ENQUIRIES);

            else if (model.LastDateInOffersSubmission > model.OffersOpeningDate)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.OFFERS_OPENING_DATE_MUST_BE_GREATER_THAN_LAST_DATE_IN_OFFERS_SUBMISSION);

            else if (model.ExpectedAnchoringDate != null && model.ExpectedAnchoringDate != default
                && model.OffersOpeningDate.Value.AddDays(generalSettings.StoppingPeriodDays) > model.ExpectedAnchoringDate)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.EXPECTED_ANCHORING_DATE_MUST_BE_GREATER_THAN_OFFERS_OPENING_DATE_PLUS_STOPPING_PERIOD);
            else
                return OperationResult<AddBidResponse>.Success(null);
        }

        /// <summary>
        /// Validates bid dates during bid approval
        /// </summary>
        public static OperationResult<AddBidResponse> ValidateBidDatesWhileApproving(Bid bid, ReadOnlyAppGeneralSettings generalSettings)
        {
            if (bid.LastDateInReceivingEnquiries > bid.LastDateInOffersSubmission)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.LAST_DATE_IN_OFFERS_SUBMISSION_MUST_BE_GREATER_THAN_LAST_DATE_IN_RECEIVING_ENQUIRIES);

            else if (bid.LastDateInOffersSubmission > bid.OffersOpeningDate)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.OFFERS_OPENING_DATE_MUST_BE_GREATER_THAN_LAST_DATE_IN_OFFERS_SUBMISSION);

            else if (bid.ExpectedAnchoringDate != null && bid.ExpectedAnchoringDate != default
                && bid.OffersOpeningDate.Value.AddDays(generalSettings.StoppingPeriodDays) > bid.ExpectedAnchoringDate)
                return OperationResult<AddBidResponse>.Fail(HttpErrorCode.InvalidInput, BidErrorCodes.EXPECTED_ANCHORING_DATE_MUST_BE_GREATER_THAN_OFFERS_OPENING_DATE_PLUS_STOPPING_PERIOD);
            else
                return OperationResult<AddBidResponse>.Success(null);
        }

        /// <summary>
        /// Checks if last receiving enquiry date validation passes
        /// </summary>
        public static bool CheckLastReceivingEnqiryDate(AddBidModelNew model, Bid bid)
        {
            return model.LastDateInReceivingEnquiries.HasValue
                && bid?.LastDateInReceivingEnquiries is not null
                && model.LastDateInReceivingEnquiries.Value < DateTime.UtcNow
                && bid.LastDateInReceivingEnquiries.Value.Date != model.LastDateInReceivingEnquiries.Value.Date;
        }
    }
}

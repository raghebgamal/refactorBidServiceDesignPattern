using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies
{
    /// <summary>
    /// Email strategy for when a bid is rejected by admin.
    ///
    /// WHO RECEIVES THIS EMAIL:
    /// - The bid creator (Association or Donor who created the bid)
    ///
    /// WHEN IT'S SENT:
    /// - When an admin rejects a bid during the review process
    ///
    /// CONTENT:
    /// - Bid name
    /// - Rejection reason/notes
    /// - Who rejected it (admin name)
    /// - Next steps (what they can do)
    ///
    /// REQUIRED ADDITIONAL DATA:
    /// - "RejectionNotes" (string): The reason why it was rejected
    /// - "AdminName" (string): Name of the admin who rejected it
    ///
    /// OLD CODE REFERENCE:
    /// This replaces SendAdminRejectedBidEmail() - Line 1932-1966 in BidServiceCore
    /// </summary>
    public class BidRejectedEmailStrategy : BaseBidEmailStrategy
    {
        public override string StrategyName => "Bid Rejected Email";
        public override string TemplateName => "BidRejectionEmail";

        protected override async Task<List<string>> GetRecipientsAsync(BidEmailContext context)
        {
            var recipients = new List<string>();

            // Get the email of the bid creator
            // In real implementation:
            // if (context.Bid.EntityType == UserType.Association)
            // {
            //     var association = await AssociationRepository.FindByIdAsync(context.Bid.EntityId);
            //     recipients.Add(association.Email);
            // }
            // else if (context.Bid.EntityType == UserType.Donor)
            // {
            //     var donor = await DonorRepository.FindByIdAsync(context.Bid.EntityId);
            //     recipients.Add(donor.Email);
            // }

            return await Task.FromResult(recipients);
        }

        protected override string BuildEmailSubject(BidEmailContext context)
        {
            return $"تم رفض المنافسة: {context.Bid.BidName}";
        }

        protected override async Task<object> BuildEmailContentAsync(BidEmailContext context)
        {
            // Get rejection notes from additional data
            var rejectionNotes = context.AdditionalData.ContainsKey("RejectionNotes")
                ? context.AdditionalData["RejectionNotes"]?.ToString()
                : "لم يتم تقديم ملاحظات";

            var adminName = context.AdditionalData.ContainsKey("AdminName")
                ? context.AdditionalData["AdminName"]?.ToString()
                : "المسؤول";

            var emailModel = new
            {
                BidName = context.Bid.BidName,
                BidRefNumber = context.Bid.Ref_Number,
                RejectionNotes = rejectionNotes,
                RejectedBy = adminName,
                BidUrl = GetBidUrl(context),
                NextSteps = "يرجى مراجعة الملاحظات وتعديل المنافسة وإعادة إرسالها للمراجعة"
            };

            return await Task.FromResult(emailModel);
        }

        /// <summary>
        /// Custom validation - ensure rejection notes are provided
        /// </summary>
        protected override string ValidateContext(BidEmailContext context)
        {
            var baseValidation = base.ValidateContext(context);
            if (!string.IsNullOrEmpty(baseValidation))
                return baseValidation;

            // Check if rejection notes were provided
            if (!context.AdditionalData.ContainsKey("RejectionNotes"))
                return "Rejection notes are required for rejection emails";

            return null;
        }
    }
}

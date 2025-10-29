using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies
{
    /// <summary>
    /// Email strategy for when a bid is updated after being published.
    ///
    /// WHO RECEIVES THIS EMAIL:
    /// - The bid creator (to confirm the update)
    /// - All companies that have bought the terms book (so they know about changes)
    ///
    /// WHEN IT'S SENT:
    /// - When a published bid is updated/modified
    ///
    /// CONTENT:
    /// - Bid name
    /// - What was updated (summary of changes)
    /// - Update date
    /// - Link to view updated bid
    ///
    /// OPTIONAL ADDITIONAL DATA:
    /// - "UpdateSummary" (string): Description of what changed
    /// - "UpdatedFields" (List<string>): List of field names that were updated
    ///
    /// OLD CODE REFERENCE:
    /// This replaces SendUpdatedBidEmailToCreatorAndProvidersOfThisBid() - Line 1217-1277 in BidServiceCore
    /// </summary>
    public class BidUpdatedEmailStrategy : BaseBidEmailStrategy
    {
        public override string StrategyName => "Bid Updated Email";
        public override string TemplateName => "BidUpdatedEmail";

        protected override async Task<List<string>> GetRecipientsAsync(BidEmailContext context)
        {
            var recipients = new List<string>();

            // 1. Get bid creator email
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

            // 2. Get all companies that bought terms book
            // var companiesBoughtTerms = await ProviderBidRepository
            //     .Find(pb => pb.BidId == context.Bid.Id && pb.IsPaymentConfirmed)
            //     .Include(pb => pb.Company)
            //         .ThenInclude(c => c.Provider)
            //     .ToListAsync();
            //
            // foreach (var company in companiesBoughtTerms)
            // {
            //     var email = await CompanyUserRolesService.GetEmailReceiverForProvider(
            //         company.Id,
            //         company.Provider.Email
            //     );
            //     recipients.Add(email);
            // }

            return await Task.FromResult(recipients);
        }

        protected override string BuildEmailSubject(BidEmailContext context)
        {
            return $"تحديث على المنافسة: {context.Bid.BidName}";
        }

        protected override async Task<object> BuildEmailContentAsync(BidEmailContext context)
        {
            var updateSummary = context.AdditionalData.ContainsKey("UpdateSummary")
                ? context.AdditionalData["UpdateSummary"]?.ToString()
                : "تم تحديث بيانات المنافسة";

            var emailModel = new
            {
                BidName = context.Bid.BidName,
                BidRefNumber = context.Bid.Ref_Number,
                PublisherName = context.EntityName,
                UpdateSummary = updateSummary,
                UpdateDate = context.Bid.ModificationDate,
                BidUrl = GetBidUrl(context),
                Message = "تم تحديث المنافسة. يرجى مراجعة التفاصيل الجديدة."
            };

            return await Task.FromResult(emailModel);
        }
    }
}

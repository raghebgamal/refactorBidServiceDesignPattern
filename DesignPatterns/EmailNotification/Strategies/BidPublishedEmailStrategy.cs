using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies
{
    /// <summary>
    /// Email strategy for when a bid is published.
    ///
    /// WHO RECEIVES THIS EMAIL:
    /// - All super admins
    /// - Admins with bid management permissions
    ///
    /// WHEN IT'S SENT:
    /// - When a bid moves from Draft/Reviewing to Published status
    ///
    /// CONTENT:
    /// - Bid name
    /// - Publisher name (Association/Donor)
    /// - Link to view the bid
    /// - Publication date
    ///
    /// OLD CODE REFERENCE:
    /// This replaces SendNewBidEmailToSuperAdmins() - Line 1069-1108 in BidServiceCore
    /// </summary>
    public class BidPublishedEmailStrategy : BaseBidEmailStrategy
    {
        public override string StrategyName => "Bid Published Email";
        public override string TemplateName => "BidPublishedEmail";

        protected override async Task<List<string>> GetRecipientsAsync(BidEmailContext context)
        {
            // In real implementation, this would query the database for super admins
            // Example:
            // var superAdmins = await UserManager.Users
            //     .Where(u => u.UserType == UserType.SuperAdmin)
            //     .Select(u => u.Email)
            //     .ToListAsync();

            // Also get admins with specific bid management claims:
            // var adminPermissionUsers = await CommonEmailService
            //     .GetAdminClaimOfEmails(new List<AdminClaimCodes> { AdminClaimCodes.clm_2553 });

            // For now, return placeholder
            var recipients = new List<string>();

            // In real code: recipients.AddRange(superAdmins);
            // In real code: recipients.AddRange(adminPermissionUsers);

            return await Task.FromResult(recipients);
        }

        protected override string BuildEmailSubject(BidEmailContext context)
        {
            // Arabic subject line
            return $"تم نشر منافسة جديدة: {context.Bid.BidName}";
        }

        protected override async Task<object> BuildEmailContentAsync(BidEmailContext context)
        {
            // Build the email model that will be passed to the template
            var emailModel = new
            {
                BidName = context.Bid.BidName,
                BidRefNumber = context.Bid.Ref_Number,
                PublisherName = context.EntityName,
                PublishDate = context.Bid.CreationDate,
                BidUrl = GetBidUrl(context),
                // In real implementation:
                // BaseBidData = await HelperService.GetBaseDataForBidsEmails(context.Bid)
            };

            return await Task.FromResult(emailModel);
        }

        /// <summary>
        /// Custom validation for published bid emails
        /// </summary>
        protected override string ValidateContext(BidEmailContext context)
        {
            // Call base validation first
            var baseValidation = base.ValidateContext(context);
            if (!string.IsNullOrEmpty(baseValidation))
                return baseValidation;

            // Additional validation specific to published bids
            if (string.IsNullOrEmpty(context.Bid.BidName))
                return "Bid name is required for published bid emails";

            return null; // Valid
        }
    }
}

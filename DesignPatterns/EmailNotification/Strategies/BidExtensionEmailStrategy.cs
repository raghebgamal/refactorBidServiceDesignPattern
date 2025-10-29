using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies
{
    /// <summary>
    /// Email strategy for when a bid submission deadline is extended.
    ///
    /// WHO RECEIVES THIS EMAIL:
    /// - All companies that have bought the terms book for this bid
    /// - All super admins
    /// - Admins with bid management permissions
    ///
    /// WHEN IT'S SENT:
    /// - When an association/donor extends the bid submission deadline
    ///
    /// CONTENT:
    /// - Bid name
    /// - Old submission deadline
    /// - New submission deadline
    /// - Reason for extension
    /// - Link to bid
    ///
    /// REQUIRED ADDITIONAL DATA:
    /// - "OldDeadline" (DateTime): The previous submission deadline
    /// - "NewDeadline" (DateTime): The new submission deadline
    /// - "ExtensionReason" (string): Why the deadline was extended
    ///
    /// OLD CODE REFERENCE:
    /// This replaces the complex email logic in ExtendBidAddressesTimes() - Lines 5077-5150 in BidServiceCore
    /// </summary>
    public class BidExtensionEmailStrategy : BaseBidEmailStrategy
    {
        public override string StrategyName => "Bid Extension Email";
        public override string TemplateName => "BidExtensionEmail";

        protected override async Task<List<string>> GetRecipientsAsync(BidEmailContext context)
        {
            var recipients = new List<string>();

            // Get companies that bought terms book for this bid
            // In real implementation:
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

            // Also add super admins
            // var superAdmins = await UserManager.Users
            //     .Where(u => u.UserType == UserType.SuperAdmin)
            //     .Select(u => u.Email)
            //     .ToListAsync();
            // recipients.AddRange(superAdmins);

            // And admins with bid management permission
            // var adminPermissionUsers = await CommonEmailService
            //     .GetAdminClaimOfEmails(new List<AdminClaimCodes> { AdminClaimCodes.clm_2553 });
            // recipients.AddRange(adminPermissionUsers);

            return await Task.FromResult(recipients);
        }

        protected override string BuildEmailSubject(BidEmailContext context)
        {
            return $"تمديد فترة المنافسة: {context.Bid.BidName}";
        }

        protected override async Task<object> BuildEmailContentAsync(BidEmailContext context)
        {
            // Extract data from context
            var oldDeadline = context.AdditionalData.ContainsKey("OldDeadline")
                ? (DateTime)context.AdditionalData["OldDeadline"]
                : DateTime.MinValue;

            var newDeadline = context.AdditionalData.ContainsKey("NewDeadline")
                ? (DateTime)context.AdditionalData["NewDeadline"]
                : DateTime.MinValue;

            var extensionReason = context.AdditionalData.ContainsKey("ExtensionReason")
                ? context.AdditionalData["ExtensionReason"]?.ToString()
                : "لم يتم تحديد السبب";

            var emailModel = new
            {
                BidName = context.Bid.BidName,
                BidRefNumber = context.Bid.Ref_Number,
                PublisherName = context.EntityName,
                OldDeadline = oldDeadline.ToString("d MMMM، yyyy"),
                NewDeadline = newDeadline.ToString("d MMMM، yyyy"),
                ExtensionReason = extensionReason,
                BidUrl = GetBidUrl(context),
                Message = $"تم تمديد فترة تقديم العروض ل {context.Bid.BidName} لتنتهي بتاريخ {newDeadline:d MMMM، yyyy}"
            };

            return await Task.FromResult(emailModel);
        }

        /// <summary>
        /// Custom validation for bid extension emails
        /// </summary>
        protected override string ValidateContext(BidEmailContext context)
        {
            var baseValidation = base.ValidateContext(context);
            if (!string.IsNullOrEmpty(baseValidation))
                return baseValidation;

            // Ensure old and new deadlines are provided
            if (!context.AdditionalData.ContainsKey("OldDeadline"))
                return "Old deadline is required for extension emails";

            if (!context.AdditionalData.ContainsKey("NewDeadline"))
                return "New deadline is required for extension emails";

            // Validate that new deadline is after old deadline
            if (context.AdditionalData.ContainsKey("OldDeadline") &&
                context.AdditionalData.ContainsKey("NewDeadline"))
            {
                var oldDate = (DateTime)context.AdditionalData["OldDeadline"];
                var newDate = (DateTime)context.AdditionalData["NewDeadline"];

                if (newDate <= oldDate)
                    return "New deadline must be after the old deadline";
            }

            return null;
        }
    }
}

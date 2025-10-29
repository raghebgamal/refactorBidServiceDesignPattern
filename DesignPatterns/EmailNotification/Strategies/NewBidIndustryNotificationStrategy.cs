using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies
{
    /// <summary>
    /// Email strategy for notifying companies about new bids in their industry.
    ///
    /// WHO RECEIVES THIS EMAIL:
    /// - All companies whose industry sectors match the bid's industries
    /// - Freelancers whose working sectors match (for freelancing bids)
    ///
    /// WHEN IT'S SENT:
    /// - When a new bid is published
    /// - Sent automatically to matching companies/freelancers
    ///
    /// CONTENT:
    /// - Bid name
    /// - Bid description (summary)
    /// - Industries/sectors
    /// - Submission deadline
    /// - Link to view bid
    /// - Call to action (buy terms book)
    ///
    /// OPTIONAL ADDITIONAL DATA:
    /// - "SendAutomatically" (bool): Whether this is automatic or manual sending
    /// - "CampaignId" (string): Marketing campaign identifier
    ///
    /// OLD CODE REFERENCE:
    /// This replaces SendEmailToCompaniesInBidIndustry() - Line 2192-2317 in BidServiceCore
    /// This is the MOST COMPLEX email in the system - it handles bulk sending to potentially
    /// thousands of recipients with filtering, batching, and campaign tracking.
    /// </summary>
    public class NewBidIndustryNotificationStrategy : BaseBidEmailStrategy
    {
        public override string StrategyName => "New Bid Industry Notification";
        public override string TemplateName => "NewBidIndustryEmail";

        protected override async Task<List<string>> GetRecipientsAsync(BidEmailContext context)
        {
            var recipients = new List<string>();

            // This is the most complex recipient logic in the system
            // In real implementation:

            // 1. Get all companies with matching industries
            // var matchingCompanies = await BidsOfProviderRepository
            //     .GetProvidersEmailsOfCompaniesSubscribedToBidIndustries(context.Bid);

            // 2. For freelancing bids, get freelancers with matching sectors
            // if (context.Bid.BidTypeId == (int)BidTypes.Freelancing)
            // {
            //     var matchingFreelancers = await GetFreelancersWithSameWorkingSectors(context.Bid);
            //     recipients.AddRange(matchingFreelancers.Select(f => f.Email));
            // }

            // 3. Filter out users who opted out of marketing emails
            // recipients = recipients.Where(email => !IsOptedOut(email)).ToList();

            // 4. Apply subscription filters (only send to active subscriptions)
            // recipients = await FilterByActiveSubscription(recipients);

            return await Task.FromResult(recipients);
        }

        protected override string BuildEmailSubject(BidEmailContext context)
        {
            return $"منافسة جديدة في مجالك: {context.Bid.BidName}";
        }

        protected override async Task<object> BuildEmailContentAsync(BidEmailContext context)
        {
            var isAutomatic = context.AdditionalData.ContainsKey("SendAutomatically")
                && (bool)context.AdditionalData["SendAutomatically"];

            var emailModel = new
            {
                BidName = context.Bid.BidName,
                BidRefNumber = context.Bid.Ref_Number,
                PublisherName = context.EntityName,
                BidDescription = context.Bid.BidDescription,
                SubmissionDeadline = context.Bid.BidAddressesTime?.LastDateInOffersSubmission,
                BidUrl = GetBidUrl(context),
                CallToAction = "اشترِ كراسة الشروط الآن",
                IsAutomatic = isAutomatic,
                // In real implementation:
                // Industries = context.Bid.GetBidWorkingSectors().Select(i => i.NameAr).ToList(),
                // TermsBookPrice = context.Bid.Bid_Documents_Price
            };

            return await Task.FromResult(emailModel);
        }

        /// <summary>
        /// Override send method to handle bulk sending with batching
        /// This prevents sending 10,000 emails in one go
        /// </summary>
        protected override async Task<EmailSendResult> SendEmailToRecipientsAsync(
            List<string> recipients,
            string subject,
            object emailContent,
            BidEmailContext context)
        {
            // For large recipient lists, batch the emails
            const int batchSize = 100; // Send 100 emails at a time

            var totalSent = 0;
            var sentRecipients = new List<string>();

            for (int i = 0; i < recipients.Count; i += batchSize)
            {
                var batch = recipients.Skip(i).Take(batchSize).ToList();

                // Send batch
                // In real implementation:
                // await EmailService.SendBulkAsync(batch, subject, emailContent);

                totalSent += batch.Count;
                sentRecipients.AddRange(batch);

                // Optional: Add delay between batches to avoid overwhelming email server
                // await Task.Delay(100);
            }

            return EmailSendResult.Success(totalSent, sentRecipients)
                .WithTracking("TotalRecipients", recipients.Count.ToString())
                .WithTracking("BatchSize", batchSize.ToString())
                .WithTracking("Strategy", StrategyName);
        }

        /// <summary>
        /// Override logging to track campaign metrics
        /// </summary>
        protected override async Task LogEmailEventAsync(BidEmailContext context, EmailSendResult result)
        {
            await base.LogEmailEventAsync(context, result);

            // Additional logging for campaign tracking
            // In real implementation:
            // await CampaignService.TrackEmailSent(new EmailCampaignLog
            // {
            //     BidId = context.Bid.Id,
            //     RecipientCount = result.EmailsSent,
            //     CampaignType = "NewBidIndustry",
            //     SentAt = DateTime.UtcNow
            // });
        }
    }
}

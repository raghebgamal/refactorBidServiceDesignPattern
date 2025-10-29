using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification
{
    /// <summary>
    /// Base class for all bid email strategies using the Template Method pattern.
    ///
    /// TEMPLATE METHOD PATTERN:
    /// Think of it like a recipe:
    /// - The STEPS are the same (get ingredients, mix, bake, serve)
    /// - The DETAILS change (what ingredients, how long to bake, etc.)
    ///
    /// This class defines the STEPS (algorithm):
    /// 1. Validate context
    /// 2. Build recipients list
    /// 3. Build email content
    /// 4. Send email
    /// 5. Log the event
    ///
    /// Each specific strategy (BidPublished, BidRejected, etc.) only needs to
    /// provide the DETAILS (what content, which recipients, etc.)
    ///
    /// Benefits:
    /// - No code duplication (all strategies share the same flow)
    /// - Consistent behavior (all emails follow the same steps)
    /// - Easy to maintain (change the flow once, all strategies benefit)
    /// </summary>
    public abstract class BaseBidEmailStrategy : IBidEmailStrategy
    {
        // These services would be injected in a real implementation
        // For now, they're placeholders showing what dependencies we need
        protected readonly IEmailService EmailService;
        protected readonly IHelperService HelperService;

        public abstract string StrategyName { get; }
        public abstract string TemplateName { get; }

        /// <summary>
        /// Template Method: The main algorithm that all strategies follow.
        /// This method is SEALED (cannot be overridden) to ensure all strategies
        /// follow the same process.
        /// </summary>
        public async Task<EmailSendResult> SendEmailAsync(BidEmailContext context)
        {
            try
            {
                // STEP 1: Validate the context
                var validationError = ValidateContext(context);
                if (!string.IsNullOrEmpty(validationError))
                {
                    return EmailSendResult.Failure(validationError);
                }

                // STEP 2: Get recipients (each strategy decides who receives the email)
                var recipients = await GetRecipientsAsync(context);
                if (recipients == null || recipients.Count == 0)
                {
                    return EmailSendResult.Failure("No recipients found for this email");
                }

                // STEP 3: Build email subject (each strategy provides its own subject)
                var subject = BuildEmailSubject(context);

                // STEP 4: Build email content (each strategy provides its own content/template)
                var emailContent = await BuildEmailContentAsync(context);

                // STEP 5: Send the email
                var sendResult = await SendEmailToRecipientsAsync(recipients, subject, emailContent, context);

                // STEP 6: Log the event (if needed)
                if (sendResult.IsSuccess)
                {
                    await LogEmailEventAsync(context, sendResult);
                }

                return sendResult;
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return EmailSendResult.Failure($"Email sending failed: {ex.Message}");
            }
        }

        #region Abstract Methods (Must be implemented by each strategy)

        /// <summary>
        /// Gets the list of email recipients for this email type.
        /// Each strategy decides who should receive the email.
        ///
        /// Examples:
        /// - BidPublished: Send to all super admins
        /// - BidRejected: Send to bid creator only
        /// - BidExtended: Send to companies who bought terms book
        /// </summary>
        protected abstract Task<List<string>> GetRecipientsAsync(BidEmailContext context);

        /// <summary>
        /// Builds the email subject line.
        /// Each strategy provides its own subject.
        ///
        /// Examples:
        /// - "تم نشر منافسة جديدة: {BidName}"
        /// - "تم رفض المنافسة: {BidName}"
        /// - "تمديد فترة المنافسة: {BidName}"
        /// </summary>
        protected abstract string BuildEmailSubject(BidEmailContext context);

        /// <summary>
        /// Builds the email content/body.
        /// Each strategy provides its own content structure.
        ///
        /// This typically returns an object that will be passed to a Razor view template.
        /// </summary>
        protected abstract Task<object> BuildEmailContentAsync(BidEmailContext context);

        #endregion

        #region Virtual Methods (Can be overridden by strategies if needed)

        /// <summary>
        /// Validates that the context has all required data.
        /// Can be overridden by strategies that need specific validation.
        /// </summary>
        protected virtual string ValidateContext(BidEmailContext context)
        {
            if (context == null)
                return "Email context cannot be null";

            if (context.Bid == null)
                return "Bid cannot be null";

            if (string.IsNullOrEmpty(context.EntityName))
                return "Entity name is required";

            return null; // Valid
        }

        /// <summary>
        /// Logs the email sending event.
        /// Can be overridden by strategies that need custom logging.
        /// </summary>
        protected virtual async Task LogEmailEventAsync(BidEmailContext context, EmailSendResult result)
        {
            // Default implementation: log basic info
            // In a real implementation, this would log to database or logging service
            await Task.CompletedTask;

            // Example:
            // await LogService.LogEmailEvent(new EmailLog
            // {
            //     BidId = context.Bid.Id,
            //     EmailType = StrategyName,
            //     RecipientCount = result.EmailsSent,
            //     SentAt = DateTime.UtcNow
            // });
        }

        /// <summary>
        /// Actually sends the email to recipients.
        /// Can be overridden if a strategy needs special sending logic.
        /// </summary>
        protected virtual async Task<EmailSendResult> SendEmailToRecipientsAsync(
            List<string> recipients,
            string subject,
            object emailContent,
            BidEmailContext context)
        {
            // In a real implementation, this would call the actual email service
            // For now, it's a placeholder showing the interface

            // Example:
            // await EmailService.SendAsync(new EmailRequest
            // {
            //     To = recipients,
            //     Subject = subject,
            //     TemplateName = TemplateName,
            //     TemplateData = emailContent
            // });

            return EmailSendResult.Success(recipients.Count, recipients)
                .WithTracking("SentAt", DateTime.UtcNow.ToString())
                .WithTracking("Strategy", StrategyName);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper to get bid creator name (Association or Donor)
        /// </summary>
        protected string GetBidCreatorName(BidEmailContext context)
        {
            // This would use the actual helper service in real implementation
            return context.EntityName;
        }

        /// <summary>
        /// Helper to format bid URL
        /// </summary>
        protected string GetBidUrl(BidEmailContext context)
        {
            // In real implementation:
            // return $"{Settings.OnlineUrl}/bids/{context.Bid.Id}";
            return $"/bids/{context.Bid.Id}";
        }

        #endregion
    }

    #region Placeholder Interfaces (In real code, these would be actual services)

    /// <summary>
    /// Placeholder for email service interface
    /// </summary>
    public interface IEmailService
    {
        Task SendAsync(object emailRequest);
    }

    /// <summary>
    /// Placeholder for helper service interface
    /// </summary>
    public interface IHelperService
    {
        Task<object> GetBaseDataForBidsEmails(object bid);
    }

    #endregion
}

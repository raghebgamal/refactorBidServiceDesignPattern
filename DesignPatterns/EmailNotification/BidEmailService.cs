using Nafes.CrossCutting.Model.Entities;
using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification
{
    /// <summary>
    /// Main service for sending bid-related emails using the Strategy pattern.
    ///
    /// This is a FACADE - it provides a simple, easy-to-use interface that hides
    /// all the complexity of strategies, factories, and email building.
    ///
    /// BEFORE (Old way):
    /// await SendNewBidEmailToSuperAdmins(bid);
    /// await SendAdminRejectedBidEmail(notes, bid);
    /// await SendEmailToCompaniesInBidIndustry(bid, entityName, true);
    /// // ... 8+ different methods, each with different parameters
    ///
    /// AFTER (New way):
    /// await _bidEmailService.SendEmailAsync(BidEmailType.BidPublished, bid, entityName);
    /// await _bidEmailService.SendEmailAsync(BidEmailType.BidRejected, bid, entityName, rejectionData);
    /// await _bidEmailService.SendEmailAsync(BidEmailType.NewBidIndustryNotification, bid, entityName);
    /// // One method, consistent interface!
    ///
    /// Benefits:
    /// - Simple, consistent interface
    /// - Easy to test
    /// - Easy to add new email types
    /// - Centralized email sending logic
    /// </summary>
    public class BidEmailService
    {
        /// <summary>
        /// Sends a bid-related email using the appropriate strategy
        /// </summary>
        /// <param name="emailType">Type of email to send</param>
        /// <param name="bid">The bid entity</param>
        /// <param name="entityName">Name of the bid creator (Association/Donor)</param>
        /// <param name="additionalData">Any extra data needed for this email type</param>
        /// <returns>Result indicating success/failure</returns>
        public async Task<EmailSendResult> SendEmailAsync(
            BidEmailType emailType,
            Bid bid,
            string entityName,
            Dictionary<string, object> additionalData = null)
        {
            try
            {
                // Step 1: Build the context (data package for the email)
                var context = new BidEmailContext(bid, entityName);

                // Add any additional data
                if (additionalData != null)
                {
                    foreach (var kvp in additionalData)
                    {
                        context.AdditionalData[kvp.Key] = kvp.Value;
                    }
                }

                // Step 2: Get the right strategy for this email type
                var strategy = BidEmailStrategyFactory.GetStrategy(emailType);

                // Step 3: Let the strategy handle sending the email
                var result = await strategy.SendEmailAsync(context);

                return result;
            }
            catch (Exception ex)
            {
                return EmailSendResult.Failure($"Failed to send {emailType} email: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a bid-related email with fluent API for better readability
        /// </summary>
        /// <example>
        /// await _bidEmailService
        ///     .ForBid(bid, entityName)
        ///     .WithData("RejectionNotes", notes)
        ///     .WithData("AdminName", adminName)
        ///     .SendAsync(BidEmailType.BidRejected);
        /// </example>
        public BidEmailBuilder ForBid(Bid bid, string entityName)
        {
            return new BidEmailBuilder(this, bid, entityName);
        }

        /// <summary>
        /// Internal method used by the builder
        /// </summary>
        internal async Task<EmailSendResult> SendWithContextAsync(BidEmailType emailType, BidEmailContext context)
        {
            var strategy = BidEmailStrategyFactory.GetStrategy(emailType);
            return await strategy.SendEmailAsync(context);
        }
    }

    /// <summary>
    /// Fluent builder for constructing and sending emails with a clean, readable syntax.
    ///
    /// This uses the BUILDER PATTERN to make code more readable:
    ///
    /// Instead of:
    /// var data = new Dictionary<string, object> { {"Notes", notes}, {"Admin", admin} };
    /// await service.SendEmailAsync(BidEmailType.BidRejected, bid, entityName, data);
    ///
    /// You can write:
    /// await service.ForBid(bid, entityName)
    ///     .WithData("Notes", notes)
    ///     .WithData("Admin", admin)
    ///     .SendAsync(BidEmailType.BidRejected);
    ///
    /// Much more readable!
    /// </summary>
    public class BidEmailBuilder
    {
        private readonly BidEmailService _service;
        private readonly BidEmailContext _context;

        internal BidEmailBuilder(BidEmailService service, Bid bid, string entityName)
        {
            _service = service;
            _context = new BidEmailContext(bid, entityName);
        }

        /// <summary>
        /// Adds additional data to the email context
        /// </summary>
        public BidEmailBuilder WithData(string key, object value)
        {
            _context.AdditionalData[key] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple data items at once
        /// </summary>
        public BidEmailBuilder WithData(Dictionary<string, object> data)
        {
            foreach (var kvp in data)
            {
                _context.AdditionalData[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Sets the current user
        /// </summary>
        public BidEmailBuilder WithUser(ApplicationUser user)
        {
            _context.CurrentUser = user;
            return this;
        }

        /// <summary>
        /// Adds recipients (in case you want to override the strategy's recipient logic)
        /// </summary>
        public BidEmailBuilder WithRecipients(List<string> recipients)
        {
            _context.Recipients = recipients;
            return this;
        }

        /// <summary>
        /// Sends the email with all the configured data
        /// </summary>
        public async Task<EmailSendResult> SendAsync(BidEmailType emailType)
        {
            return await _service.SendWithContextAsync(emailType, _context);
        }
    }
}

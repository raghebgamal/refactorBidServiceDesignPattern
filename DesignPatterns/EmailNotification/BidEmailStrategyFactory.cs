using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Strategies;
using System;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification
{
    /// <summary>
    /// Factory for creating the appropriate email strategy based on email type.
    ///
    /// This is like a mail sorting center:
    /// - You give it the TYPE of email you want to send
    /// - It gives you the right EMAIL STRATEGY to handle it
    ///
    /// Benefits:
    /// - Centralizes strategy selection logic
    /// - Easy to add new email types
    /// - Client code doesn't need to know which strategy to use
    /// </summary>
    public enum BidEmailType
    {
        /// <summary>
        /// Email sent when a bid is published
        /// </summary>
        BidPublished,

        /// <summary>
        /// Email sent when an admin rejects a bid
        /// </summary>
        BidRejected,

        /// <summary>
        /// Email sent when a bid deadline is extended
        /// </summary>
        BidExtended,

        /// <summary>
        /// Email sent when a bid is updated after being published
        /// </summary>
        BidUpdated,

        /// <summary>
        /// Email sent to companies/freelancers in matching industries about new bids
        /// </summary>
        NewBidIndustryNotification
    }

    public class BidEmailStrategyFactory
    {
        /// <summary>
        /// Gets the appropriate email strategy for the given email type
        /// </summary>
        /// <param name="emailType">The type of email to send</param>
        /// <returns>The strategy that handles this email type</returns>
        /// <exception cref="ArgumentException">Thrown when email type is not supported</exception>
        public static IBidEmailStrategy GetStrategy(BidEmailType emailType)
        {
            return emailType switch
            {
                BidEmailType.BidPublished => new BidPublishedEmailStrategy(),
                BidEmailType.BidRejected => new BidRejectedEmailStrategy(),
                BidEmailType.BidExtended => new BidExtensionEmailStrategy(),
                BidEmailType.BidUpdated => new BidUpdatedEmailStrategy(),
                BidEmailType.NewBidIndustryNotification => new NewBidIndustryNotificationStrategy(),

                // If we encounter an unknown email type, throw an error
                _ => throw new ArgumentException($"No email strategy defined for email type: {emailType}", nameof(emailType))
            };
        }

        /// <summary>
        /// Gets all available email strategies (useful for testing or documentation)
        /// </summary>
        public static IBidEmailStrategy[] GetAllStrategies()
        {
            return new IBidEmailStrategy[]
            {
                new BidPublishedEmailStrategy(),
                new BidRejectedEmailStrategy(),
                new BidExtensionEmailStrategy(),
                new BidUpdatedEmailStrategy(),
                new NewBidIndustryNotificationStrategy()
            };
        }

        /// <summary>
        /// Gets the strategy name for a given email type
        /// (useful for logging and UI display)
        /// </summary>
        public static string GetStrategyName(BidEmailType emailType)
        {
            var strategy = GetStrategy(emailType);
            return strategy.StrategyName;
        }
    }
}

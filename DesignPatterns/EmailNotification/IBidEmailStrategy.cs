using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification
{
    /// <summary>
    /// Strategy interface for sending different types of bid-related emails.
    ///
    /// Just like the price calculation pattern, this allows us to have different
    /// email strategies (BidPublished, BidRejected, BidExtended, etc.) without
    /// having a giant if-else statement.
    ///
    /// Each email type (strategy) implements this interface and provides its own:
    /// - Email template
    /// - Recipients list
    /// - Email content building logic
    /// - Subject line
    /// </summary>
    public interface IBidEmailStrategy
    {
        /// <summary>
        /// Sends the email for this strategy
        /// </summary>
        /// <param name="context">All the data needed to send this email</param>
        /// <returns>Result indicating success/failure and details</returns>
        Task<EmailSendResult> SendEmailAsync(BidEmailContext context);

        /// <summary>
        /// Gets the name of this email strategy (for logging and debugging)
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Gets the email template name to use
        /// Example: "BidPublishedEmail", "BidRejectionEmail"
        /// </summary>
        string TemplateName { get; }
    }
}

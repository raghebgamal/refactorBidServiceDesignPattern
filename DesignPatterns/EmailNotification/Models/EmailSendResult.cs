using System.Collections.Generic;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models
{
    /// <summary>
    /// Result of an email sending operation.
    /// Contains success status, error messages, and tracking information.
    ///
    /// Think of it like a delivery receipt:
    /// - IsSuccess = Was it delivered?
    /// - EmailsSent = How many were delivered?
    /// - ErrorMessage = What went wrong (if anything)?
    /// - TrackingInfo = Delivery details
    /// </summary>
    public class EmailSendResult
    {
        /// <summary>
        /// Indicates if the email was sent successfully
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Number of emails successfully sent
        /// </summary>
        public int EmailsSent { get; set; }

        /// <summary>
        /// Error message if sending failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// List of recipient emails that were sent to
        /// </summary>
        public List<string> SentToRecipients { get; set; }

        /// <summary>
        /// Additional tracking information (log reference, email IDs, etc.)
        /// </summary>
        public Dictionary<string, string> TrackingInfo { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EmailSendResult()
        {
            SentToRecipients = new List<string>();
            TrackingInfo = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static EmailSendResult Success(int emailsSent, List<string> recipients)
        {
            return new EmailSendResult
            {
                IsSuccess = true,
                EmailsSent = emailsSent,
                SentToRecipients = recipients,
                ErrorMessage = null
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static EmailSendResult Failure(string errorMessage)
        {
            return new EmailSendResult
            {
                IsSuccess = false,
                EmailsSent = 0,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Adds tracking information
        /// </summary>
        public EmailSendResult WithTracking(string key, string value)
        {
            TrackingInfo[key] = value;
            return this;
        }
    }
}

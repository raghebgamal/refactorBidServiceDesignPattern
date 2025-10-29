using Nafes.CrossCutting.Model.Entities;
using System.Collections.Generic;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models
{
    /// <summary>
    /// Context object containing all data needed for sending bid-related emails.
    /// This is like a "data package" that contains everything an email might need.
    ///
    /// Think of it like ordering food delivery:
    /// - Bid = The restaurant
    /// - EntityName = Restaurant name
    /// - Recipients = Delivery addresses
    /// - AdditionalData = Special instructions
    /// </summary>
    public class BidEmailContext
    {
        /// <summary>
        /// The bid entity that this email is about
        /// </summary>
        public Bid Bid { get; set; }

        /// <summary>
        /// Name of the entity creating the bid (Association or Donor name)
        /// Example: "جمعية الخيرية" or "مؤسسة التنمية"
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Email addresses to send to
        /// Example: ["admin1@example.com", "admin2@example.com"]
        /// </summary>
        public List<string> Recipients { get; set; }

        /// <summary>
        /// Current user sending the email
        /// </summary>
        public ApplicationUser CurrentUser { get; set; }

        /// <summary>
        /// Additional custom data specific to certain email types
        /// For example:
        /// - Rejection notes for rejection emails
        /// - Old dates for extension emails
        /// - Invitation details for invitation emails
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Constructor with required fields
        /// </summary>
        public BidEmailContext(Bid bid, string entityName)
        {
            Bid = bid;
            EntityName = entityName;
            Recipients = new List<string>();
            AdditionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Helper method to add additional data
        /// </summary>
        public BidEmailContext WithAdditionalData(string key, object value)
        {
            AdditionalData[key] = value;
            return this;
        }

        /// <summary>
        /// Helper method to add recipients
        /// </summary>
        public BidEmailContext WithRecipients(List<string> recipients)
        {
            Recipients = recipients;
            return this;
        }

        /// <summary>
        /// Helper method to add a single recipient
        /// </summary>
        public BidEmailContext WithRecipient(string recipient)
        {
            Recipients.Add(recipient);
            return this;
        }

        /// <summary>
        /// Helper method to set current user
        /// </summary>
        public BidEmailContext WithUser(ApplicationUser user)
        {
            CurrentUser = user;
            return this;
        }
    }
}

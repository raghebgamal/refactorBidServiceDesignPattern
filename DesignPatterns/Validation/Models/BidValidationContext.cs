using Nafes.CrossCutting.Model.Entities;
using Nafis.Services.DTO.Bid;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Models
{
    /// <summary>
    /// Context object containing all data needed for bid validation.
    ///
    /// This is like a "package" that travels through the validation chain,
    /// containing everything validators might need to check.
    ///
    /// Why use a context object?
    /// - Instead of passing 5-10 parameters to each validator
    /// - Bundle everything into ONE object
    /// - Easy to add new data without changing validator signatures
    ///
    /// Example: When adding a bid, we need to validate:
    /// - The request model (what user sent)
    /// - The existing bid (if updating)
    /// - App settings (min/max values, percentages, etc.)
    /// - Current user (permissions, user type)
    /// </summary>
    public class BidValidationContext
    {
        /// <summary>
        /// The bid request model (data from user)
        /// </summary>
        public AddBidModelNew RequestModel { get; set; }

        /// <summary>
        /// The existing bid entity (if updating, null if creating)
        /// </summary>
        public Bid ExistingBid { get; set; }

        /// <summary>
        /// Application general settings (for min/max validations)
        /// </summary>
        public ReadOnlyAppGeneralSettings Settings { get; set; }

        /// <summary>
        /// Current user making the request
        /// </summary>
        public ApplicationUser CurrentUser { get; set; }

        /// <summary>
        /// Indicates if this is a draft bid (different validation rules)
        /// </summary>
        public bool IsDraft => RequestModel?.IsDraft ?? false;

        /// <summary>
        /// Indicates if this is an update operation (vs create)
        /// </summary>
        public bool IsUpdate => ExistingBid != null;

        /// <summary>
        /// Constructor for create operations
        /// </summary>
        public BidValidationContext(AddBidModelNew requestModel, ReadOnlyAppGeneralSettings settings, ApplicationUser currentUser)
        {
            RequestModel = requestModel;
            Settings = settings;
            CurrentUser = currentUser;
            ExistingBid = null;
        }

        /// <summary>
        /// Constructor for update operations
        /// </summary>
        public BidValidationContext(AddBidModelNew requestModel, Bid existingBid, ReadOnlyAppGeneralSettings settings, ApplicationUser currentUser)
        {
            RequestModel = requestModel;
            ExistingBid = existingBid;
            Settings = settings;
            CurrentUser = currentUser;
        }

        /// <summary>
        /// Fluent setter for request model
        /// </summary>
        public BidValidationContext WithRequestModel(AddBidModelNew model)
        {
            RequestModel = model;
            return this;
        }

        /// <summary>
        /// Fluent setter for existing bid
        /// </summary>
        public BidValidationContext WithExistingBid(Bid bid)
        {
            ExistingBid = bid;
            return this;
        }

        /// <summary>
        /// Fluent setter for settings
        /// </summary>
        public BidValidationContext WithSettings(ReadOnlyAppGeneralSettings settings)
        {
            Settings = settings;
            return this;
        }

        /// <summary>
        /// Fluent setter for current user
        /// </summary>
        public BidValidationContext WithUser(ApplicationUser user)
        {
            CurrentUser = user;
            return this;
        }
    }
}

using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using Nafis.Services.Implementation.DesignPatterns.Validation.Validators;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation
{
    /// <summary>
    /// Service providing pre-configured validation chains for bid operations.
    ///
    /// This is a FACADE that hides the complexity of building validation chains.
    /// Instead of manually creating and linking validators, you just call one method.
    ///
    /// BEFORE (without this service):
    /// var chain = new ValidationChainBuilder<BidValidationContext>()
    ///     .Add(new UserAuthorizationValidator())
    ///     .Add(new RequiredFieldsValidator())
    ///     .Add(new BidDatesValidator())
    ///     .Add(new BidPriceValidator())
    ///     .Build();
    /// var result = await chain.ValidateAsync(context);
    ///
    /// AFTER (with this service):
    /// var validationService = new BidValidationService();
    /// var result = await validationService.ValidateAddBidAsync(context);
    ///
    /// Much simpler!
    /// </summary>
    public class BidValidationService
    {
        /// <summary>
        /// Validates a new bid creation or update operation.
        /// This is the COMPLETE validation chain for adding/updating a bid.
        ///
        /// Validation Order (important!):
        /// 1. User Authorization (no point validating if user can't create bids)
        /// 2. Required Fields (no point validating logic if basic fields missing)
        /// 3. Dates (validate date logic)
        /// 4. Prices (validate financial values)
        ///
        /// OLD CODE REFERENCE:
        /// This replaces all the validation logic in AddBidNew() - Lines 462-478
        /// </summary>
        public async Task<ValidationResult> ValidateAddBidAsync(BidValidationContext context)
        {
            var chain = new ValidationChainBuilder<BidValidationContext>()
                .Add(new UserAuthorizationValidator())      // Step 1: Can this user create bids?
                .Add(new RequiredFieldsValidator())         // Step 2: Are required fields present?
                .Add(new BidDatesValidator())               // Step 3: Are dates valid and in correct order?
                .Add(new BidPriceValidator())               // Step 4: Are prices valid?
                .Build();

            return await chain.ValidateAsync(context);
        }

        /// <summary>
        /// Validates a bid before approving/publishing.
        /// Similar to add validation but might have additional rules.
        ///
        /// OLD CODE REFERENCE:
        /// This replaces ValidateBidDatesWhileApproving() - Lines 2025-2042
        /// </summary>
        public async Task<ValidationResult> ValidateApproveBidAsync(BidValidationContext context)
        {
            var chain = new ValidationChainBuilder<BidValidationContext>()
                .Add(new RequiredFieldsValidator())   // All required fields must be present for approval
                .Add(new BidDatesValidator())         // Dates must be valid
                .Add(new BidPriceValidator())         // Prices must be valid
                .Build();

            return await chain.ValidateAsync(context);
        }

        /// <summary>
        /// Validates just the dates (useful when extending bid deadlines).
        ///
        /// OLD CODE REFERENCE:
        /// This replaces ValidateBidDates() - Lines 845-862
        /// </summary>
        public async Task<ValidationResult> ValidateBidDatesAsync(BidValidationContext context)
        {
            var validator = new BidDatesValidator();
            return await validator.ValidateAsync(context);
        }

        /// <summary>
        /// Validates just the prices (useful when updating bid prices).
        ///
        /// OLD CODE REFERENCE:
        /// This replaces price validation in CalculateAndUpdateBidPrices() - Lines 874-896
        /// </summary>
        public async Task<ValidationResult> ValidateBidPricesAsync(BidValidationContext context)
        {
            var validator = new BidPriceValidator();
            return await validator.ValidateAsync(context);
        }

        /// <summary>
        /// Validates just user authorization (useful when checking permissions).
        /// </summary>
        public async Task<ValidationResult> ValidateUserAuthorizationAsync(BidValidationContext context)
        {
            var validator = new UserAuthorizationValidator();
            return await validator.ValidateAsync(context);
        }

        /// <summary>
        /// Creates a custom validation chain.
        /// Use this when you need a specific combination of validators.
        /// </summary>
        /// <example>
        /// var result = await validationService.ValidateWithCustomChain(context,
        ///     new RequiredFieldsValidator(),
        ///     new BidDatesValidator()
        /// );
        /// </example>
        public async Task<ValidationResult> ValidateWithCustomChain(
            BidValidationContext context,
            params IValidator<BidValidationContext>[] validators)
        {
            if (validators == null || validators.Length == 0)
            {
                return ValidationResult.Success();
            }

            var builder = new ValidationChainBuilder<BidValidationContext>();
            foreach (var validator in validators)
            {
                builder.Add(validator);
            }

            var chain = builder.Build();
            return await chain.ValidateAsync(context);
        }
    }
}

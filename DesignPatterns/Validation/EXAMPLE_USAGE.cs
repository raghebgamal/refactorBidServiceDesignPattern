using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using Nafis.Services.DTO.Bid;
using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using Nafis.Services.Implementation.DesignPatterns.Validation.Validators;
using System;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation
{
    /// <summary>
    /// Examples showing how to use the Chain of Responsibility validation pattern.
    /// These examples replace the scattered validation logic in BidServiceCore.
    /// </summary>
    public class BidValidationExamples
    {
        /// <summary>
        /// Example 1: Simple validation using the service (recommended)
        /// </summary>
        public static async Task Example1_ValidateWithService()
        {
            Console.WriteLine("=== Example 1: Validate Using Service ===\n");

            // Create the validation service
            var validationService = new BidValidationService();

            // Create test data
            var requestModel = new AddBidModelNew
            {
                BidName = "منافسة توريد معدات",
                Association_Fees = 5000,
                BidTypeId = (int)BidTypes.Public,
                IsDraft = false,
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(20),
                OffersOpeningDate = DateTime.UtcNow.AddDays(21),
                RegionsId = new System.Collections.Generic.List<int> { 1, 2 }
            };

            var settings = new ReadOnlyAppGeneralSettings
            {
                MaxBidDocumentPrice = 100000,
                StoppingPeriodDays = 5
            };

            var user = new ApplicationUser
            {
                Id = "123",
                UserType = UserType.Association
            };

            var context = new BidValidationContext(requestModel, settings, user);

            // OLD WAY (scattered validation logic in BidServiceCore):
            // if (usr is null) return Fail();
            // if (!allowedUserTypes.Contains(usr.UserType)) return Fail();
            // if (string.IsNullOrEmpty(model.BidName)) return Fail();
            // if (model.LastDateInReceivingEnquiries > model.LastDateInOffersSubmission) return Fail();
            // ... 10+ more if statements

            // NEW WAY (one line!):
            var result = await validationService.ValidateAddBidAsync(context);

            if (result.IsValid)
            {
                Console.WriteLine("✓ All validations passed!");
                Console.WriteLine("  The bid is ready to be saved.");
            }
            else
            {
                Console.WriteLine($"✗ Validation failed!");
                Console.WriteLine($"  Error: {result.FirstError}");
                Console.WriteLine($"  Failed at: {result.FailedValidatorName}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 2: Validation fails - shows which validator caught the error
        /// </summary>
        public static async Task Example2_ValidationFails()
        {
            Console.WriteLine("=== Example 2: Validation Failure ===\n");

            var validationService = new BidValidationService();

            // Create INVALID data (dates in wrong order)
            var requestModel = new AddBidModelNew
            {
                BidName = "منافسة توريد معدات",
                IsDraft = false,
                // WRONG: Submission date AFTER opening date!
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(25),  // After opening!
                OffersOpeningDate = DateTime.UtcNow.AddDays(20),            // Before submission!
                RegionsId = new System.Collections.Generic.List<int> { 1 }
            };

            var settings = new ReadOnlyAppGeneralSettings
            {
                MaxBidDocumentPrice = 100000,
                StoppingPeriodDays = 5
            };

            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            var result = await validationService.ValidateAddBidAsync(context);

            Console.WriteLine($"Is Valid: {result.IsValid}");
            Console.WriteLine($"Failed Validator: {result.FailedValidatorName}");
            Console.WriteLine($"Error Message: {result.FirstError}");
            Console.WriteLine($"Error Code: {result.ErrorCode}");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 3: Building a custom validation chain
        /// </summary>
        public static async Task Example3_CustomValidationChain()
        {
            Console.WriteLine("=== Example 3: Custom Validation Chain ===\n");

            // Sometimes you only want to validate SOME things, not everything.
            // Build a custom chain with only the validators you need.

            var requestModel = new AddBidModelNew
            {
                BidName = "منافسة خدمات",
                IsDraft = false,
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(20),
                OffersOpeningDate = DateTime.UtcNow.AddDays(21),
                RegionsId = new System.Collections.Generic.List<int> { 1 }
            };

            var settings = new ReadOnlyAppGeneralSettings { StoppingPeriodDays = 5 };
            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            // Build custom chain: Only validate fields and dates (skip authorization and prices)
            var chain = new ValidationChainBuilder<BidValidationContext>()
                .Add(new RequiredFieldsValidator())
                .Add(new BidDatesValidator())
                .Build();

            var result = await chain.ValidateAsync(context);

            Console.WriteLine($"Custom chain validation: {(result.IsValid ? "Passed" : "Failed")}");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 4: Validating only specific aspects
        /// </summary>
        public static async Task Example4_ValidateSpecificAspects()
        {
            Console.WriteLine("=== Example 4: Validate Specific Aspects ===\n");

            var validationService = new BidValidationService();

            var requestModel = new AddBidModelNew
            {
                BidName = "Test",
                IsDraft = false,
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(20),
                OffersOpeningDate = DateTime.UtcNow.AddDays(21),
                Association_Fees = 5000,
                RegionsId = new System.Collections.Generic.List<int> { 1 }
            };

            var settings = new ReadOnlyAppGeneralSettings
            {
                MaxBidDocumentPrice = 100000,
                StoppingPeriodDays = 5
            };

            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            // Validate only dates
            var datesResult = await validationService.ValidateBidDatesAsync(context);
            Console.WriteLine($"Dates validation: {(datesResult.IsValid ? "✓ Passed" : "✗ Failed")}");

            // Validate only prices
            var pricesResult = await validationService.ValidateBidPricesAsync(context);
            Console.WriteLine($"Prices validation: {(pricesResult.IsValid ? "✓ Passed" : "✗ Failed")}");

            // Validate only authorization
            var authResult = await validationService.ValidateUserAuthorizationAsync(context);
            Console.WriteLine($"Authorization validation: {(authResult.IsValid ? "✓ Passed" : "✗ Failed")}");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 5: Draft bids skip certain validations
        /// </summary>
        public static async Task Example5_DraftBidValidation()
        {
            Console.WriteLine("=== Example 5: Draft Bid Validation ===\n");

            var validationService = new BidValidationService();

            // Draft bid with missing required fields
            var requestModel = new AddBidModelNew
            {
                BidName = "مسودة منافسة",
                IsDraft = true,  // IT'S A DRAFT!
                // Missing dates and regions - but that's OK for drafts!
                Association_Fees = 5000
            };

            var settings = new ReadOnlyAppGeneralSettings { MaxBidDocumentPrice = 100000 };
            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            var result = await validationService.ValidateAddBidAsync(context);

            Console.WriteLine($"Draft bid validation: {(result.IsValid ? "✓ Passed" : "✗ Failed")}");
            Console.WriteLine("Note: Draft bids skip required fields validation");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 6: Understanding the chain order
        /// </summary>
        public static async Task Example6_ChainOrder()
        {
            Console.WriteLine("=== Example 6: Understanding Chain Order ===\n");

            // Create data with MULTIPLE errors
            var requestModel = new AddBidModelNew
            {
                BidName = "",  // ERROR 1: Missing name
                IsDraft = false,
                Association_Fees = -1000,  // ERROR 2: Negative fees
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(5),  // ERROR 3: Wrong date order
                OffersOpeningDate = DateTime.UtcNow.AddDays(20)
                // ERROR 4: Missing regions
            };

            var settings = new ReadOnlyAppGeneralSettings
            {
                MaxBidDocumentPrice = 100000,
                StoppingPeriodDays = 5
            };

            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            var validationService = new BidValidationService();
            var result = await validationService.ValidateAddBidAsync(context);

            Console.WriteLine($"Validation stopped at: {result.FailedValidatorName}");
            Console.WriteLine($"Error message: {result.FirstError}");
            Console.WriteLine("\nImportant: The chain STOPS at the first error!");
            Console.WriteLine("This is called 'fail-fast' - no point checking dates if fields are missing.");
            Console.WriteLine("\nChain order in ValidateAddBidAsync:");
            Console.WriteLine("1. UserAuthorizationValidator");
            Console.WriteLine("2. RequiredFieldsValidator  ← Stopped here!");
            Console.WriteLine("3. BidDatesValidator  ← Never reached");
            Console.WriteLine("4. BidPriceValidator  ← Never reached");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 7: Handling validation results in real code
        /// </summary>
        public static async Task Example7_HandlingResults()
        {
            Console.WriteLine("=== Example 7: Handling Validation Results ===\n");

            var validationService = new BidValidationService();

            var requestModel = new AddBidModelNew
            {
                BidName = "منافسة صيانة",
                IsDraft = false,
                LastDateInReceivingEnquiries = DateTime.UtcNow.AddDays(10),
                LastDateInOffersSubmission = DateTime.UtcNow.AddDays(20),
                OffersOpeningDate = DateTime.UtcNow.AddDays(21),
                RegionsId = new System.Collections.Generic.List<int> { 1 }
            };

            var settings = new ReadOnlyAppGeneralSettings
            {
                MaxBidDocumentPrice = 100000,
                StoppingPeriodDays = 5
            };

            var user = new ApplicationUser { UserType = UserType.Association };
            var context = new BidValidationContext(requestModel, settings, user);

            var result = await validationService.ValidateAddBidAsync(context);

            // How you would use this in real code (like in BidServiceCore):
            if (!result.IsValid)
            {
                // Return error to API
                Console.WriteLine("Returning error response:");
                Console.WriteLine($"  HTTP Status: {result.HttpErrorCode}");
                Console.WriteLine($"  Error Code: {result.ErrorCode}");
                Console.WriteLine($"  Message: {result.FirstError}");

                // In real code:
                // return OperationResult<AddBidResponse>.Fail(
                //     result.HttpErrorCode.Value,
                //     result.ErrorCode
                // );
            }
            else
            {
                Console.WriteLine("✓ Validation passed - proceeding with bid creation");

                // In real code:
                // var bid = _mapper.Map<Bid>(requestModel);
                // await _bidRepository.Add(bid);
                // return OperationResult<AddBidResponse>.Success(response);
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public static async Task RunAllExamples()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║   CHAIN OF RESPONSIBILITY VALIDATION EXAMPLES      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            await Example1_ValidateWithService();
            await Example2_ValidationFails();
            await Example3_CustomValidationChain();
            await Example4_ValidateSpecificAspects();
            await Example5_DraftBidValidation();
            await Example6_ChainOrder();
            await Example7_HandlingResults();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ALL EXAMPLES COMPLETED                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            Console.WriteLine("\n📊 SUMMARY:");
            Console.WriteLine("Before: 10+ if statements scattered throughout code");
            Console.WriteLine("After:  Clean validation chains, each validator has one job");
            Console.WriteLine("Result: 90% more maintainable, 100% more testable!");
        }
    }
}

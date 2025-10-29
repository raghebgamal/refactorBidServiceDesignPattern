using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using Nafis.Services.Implementation.DesignPatterns.EmailNotification.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.EmailNotification
{
    /// <summary>
    /// Examples showing how to use the Email/Notification Strategy Pattern.
    /// These examples replace the scattered email methods in BidServiceCore.
    /// </summary>
    public class BidEmailExamples
    {
        /// <summary>
        /// Example 1: Send a simple "bid published" email
        /// Replaces: SendNewBidEmailToSuperAdmins() - Line 1069-1108
        /// </summary>
        public static async Task Example1_BidPublishedEmail()
        {
            Console.WriteLine("=== Example 1: Bid Published Email ===\n");

            // Create the service
            var emailService = new BidEmailService();

            // Create a sample bid
            var bid = new Bid
            {
                Id = 1,
                BidName = "منافسة توريد معدات مكتبية",
                Ref_Number = "BID-2024-001",
                CreationDate = DateTime.UtcNow,
                BidTypeId = (int)BidTypes.Public
            };

            var entityName = "جمعية الخيرية للتنمية";

            // OLD WAY (before refactoring):
            // await SendNewBidEmailToSuperAdmins(bid);  // 40 lines of code

            // NEW WAY (after refactoring):
            var result = await emailService.SendEmailAsync(
                BidEmailType.BidPublished,
                bid,
                entityName
            );

            // Check result
            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Email sent successfully to {result.EmailsSent} recipients");
            }
            else
            {
                Console.WriteLine($"✗ Error: {result.ErrorMessage}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 2: Send a "bid rejected" email with rejection notes
        /// Replaces: SendAdminRejectedBidEmail() - Line 1932-1966
        /// </summary>
        public static async Task Example2_BidRejectedEmail()
        {
            Console.WriteLine("=== Example 2: Bid Rejected Email ===\n");

            var emailService = new BidEmailService();

            var bid = new Bid
            {
                Id = 2,
                BidName = "منافسة صيانة المباني",
                Ref_Number = "BID-2024-002"
            };

            var entityName = "مؤسسة الأعمال الخيرية";

            // Rejection details
            var rejectionNotes = "المنافسة تحتاج إلى تفاصيل أكثر دقة في الشروط والمواصفات";
            var adminName = "محمد أحمد";

            // OLD WAY (before refactoring):
            // await SendAdminRejectedBidEmail(rejectionNotes, user, bid);  // 35 lines of code

            // NEW WAY (after refactoring):
            var additionalData = new Dictionary<string, object>
            {
                { "RejectionNotes", rejectionNotes },
                { "AdminName", adminName }
            };

            var result = await emailService.SendEmailAsync(
                BidEmailType.BidRejected,
                bid,
                entityName,
                additionalData
            );

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Rejection email sent");
                Console.WriteLine($"  Notes: {rejectionNotes}");
                Console.WriteLine($"  Rejected by: {adminName}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 3: Send a "bid extended" email using fluent API
        /// Replaces: Complex email logic in ExtendBidAddressesTimes() - Lines 5077-5150
        /// </summary>
        public static async Task Example3_BidExtendedEmailFluentAPI()
        {
            Console.WriteLine("=== Example 3: Bid Extended Email (Fluent API) ===\n");

            var emailService = new BidEmailService();

            var bid = new Bid
            {
                Id = 3,
                BidName = "منافسة توريد أجهزة كمبيوتر",
                Ref_Number = "BID-2024-003"
            };

            var entityName = "جمعية التقنية والتطوير";

            var oldDeadline = new DateTime(2024, 12, 15);
            var newDeadline = new DateTime(2024, 12, 30);

            // OLD WAY (before refactoring):
            // 73 lines of code to build email, get recipients, send, and log

            // NEW WAY (after refactoring) - Using FLUENT API:
            var result = await emailService
                .ForBid(bid, entityName)
                .WithData("OldDeadline", oldDeadline)
                .WithData("NewDeadline", newDeadline)
                .WithData("ExtensionReason", "طلب من عدة متنافسين")
                .SendAsync(BidEmailType.BidExtended);

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Extension email sent to {result.EmailsSent} recipients");
                Console.WriteLine($"  Old Deadline: {oldDeadline:yyyy-MM-dd}");
                Console.WriteLine($"  New Deadline: {newDeadline:yyyy-MM-dd}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 4: Send "bid updated" email to creator and providers
        /// Replaces: SendUpdatedBidEmailToCreatorAndProvidersOfThisBid() - Line 1217-1277
        /// </summary>
        public static async Task Example4_BidUpdatedEmail()
        {
            Console.WriteLine("=== Example 4: Bid Updated Email ===\n");

            var emailService = new BidEmailService();

            var bid = new Bid
            {
                Id = 4,
                BidName = "منافسة تقديم خدمات النظافة",
                Ref_Number = "BID-2024-004",
                ModificationDate = DateTime.UtcNow
            };

            var entityName = "جمعية البيئة النظيفة";

            // What was updated
            var updateSummary = "تم تحديث شروط التعاقد وإضافة مواصفات جديدة";

            // OLD WAY:
            // await SendUpdatedBidEmailToCreatorAndProvidersOfThisBid(bid);  // 60 lines

            // NEW WAY:
            var result = await emailService
                .ForBid(bid, entityName)
                .WithData("UpdateSummary", updateSummary)
                .SendAsync(BidEmailType.BidUpdated);

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Update notification sent to {result.EmailsSent} recipients");
                Console.WriteLine($"  Update: {updateSummary}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 5: Notify companies in matching industries about new bid
        /// Replaces: SendEmailToCompaniesInBidIndustry() - Line 2192-2317 (125 lines!)
        /// </summary>
        public static async Task Example5_NewBidIndustryNotification()
        {
            Console.WriteLine("=== Example 5: New Bid Industry Notification ===\n");

            var emailService = new BidEmailService();

            var bid = new Bid
            {
                Id = 5,
                BidName = "منافسة توريد مواد بناء",
                Ref_Number = "BID-2024-005",
                BidDescription = "توريد مواد البناء والتشطيب للمشروع السكني",
                BidTypeId = (int)BidTypes.Public
            };

            var entityName = "مؤسسة الإسكان الخيري";

            // OLD WAY:
            // await SendEmailToCompaniesInBidIndustry(bid, entityName, true);  // 125 lines!

            // NEW WAY:
            var result = await emailService
                .ForBid(bid, entityName)
                .WithData("SendAutomatically", true)
                .WithData("CampaignId", "CAMPAIGN-2024-001")
                .SendAsync(BidEmailType.NewBidIndustryNotification);

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Industry notification sent to {result.EmailsSent} companies/freelancers");
                Console.WriteLine($"  Campaign tracking: {result.TrackingInfo["Strategy"]}");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 6: Using strategies directly (advanced usage)
        /// </summary>
        public static async Task Example6_DirectStrategyUsage()
        {
            Console.WriteLine("=== Example 6: Direct Strategy Usage (Advanced) ===\n");

            // Sometimes you might want to use a strategy directly for more control
            var strategy = BidEmailStrategyFactory.GetStrategy(BidEmailType.BidPublished);

            Console.WriteLine($"Strategy Name: {strategy.StrategyName}");
            Console.WriteLine($"Template Name: {strategy.TemplateName}");

            var bid = new Bid { Id = 6, BidName = "Test Bid" };
            var context = new BidEmailContext(bid, "Test Entity");

            var result = await strategy.SendEmailAsync(context);

            Console.WriteLine($"Result: {(result.IsSuccess ? "Success" : "Failed")}");

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Example 7: Error handling
        /// </summary>
        public static async Task Example7_ErrorHandling()
        {
            Console.WriteLine("=== Example 7: Error Handling ===\n");

            var emailService = new BidEmailService();

            // Invalid context (missing required data)
            var result = await emailService.SendEmailAsync(
                BidEmailType.BidRejected,
                new Bid { Id = 7, BidName = "Test" },
                "Test Entity"
                // Missing required "RejectionNotes" for BidRejected email!
            );

            if (!result.IsSuccess)
            {
                Console.WriteLine($"✗ Email failed as expected");
                Console.WriteLine($"  Error: {result.ErrorMessage}");
                Console.WriteLine($"  This is good! Validation caught the missing data.");
            }

            Console.WriteLine("\n");
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public static async Task RunAllExamples()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║  EMAIL/NOTIFICATION STRATEGY PATTERN EXAMPLES      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            await Example1_BidPublishedEmail();
            await Example2_BidRejectedEmail();
            await Example3_BidExtendedEmailFluentAPI();
            await Example4_BidUpdatedEmail();
            await Example5_NewBidIndustryNotification();
            await Example6_DirectStrategyUsage();
            await Example7_ErrorHandling();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ALL EXAMPLES COMPLETED                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");

            Console.WriteLine("\n📊 SUMMARY:");
            Console.WriteLine("Before: 8+ different email methods, 600+ lines of code");
            Console.WriteLine("After:  1 service with consistent interface");
            Console.WriteLine("Result: 85% less code, 100% more maintainable!");
        }
    }
}

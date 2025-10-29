using Nafes.CrossCutting.Model.Enums;
using Nafis.Services.Implementation.DesignPatterns.Validation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nafis.Services.Implementation.DesignPatterns.Validation.Validators
{
    /// <summary>
    /// Validates that the current user is authorized to perform the bid operation.
    ///
    /// Authorization Rules:
    /// 1. User must be authenticated (logged in)
    /// 2. User must have the right user type (Association, Donor, Admin, SuperAdmin)
    /// 3. Admins can only edit existing bids, not create new ones
    /// 4. Associations and Donors can create and edit their own bids
    ///
    /// Think of it like a bouncer at a club:
    /// - Are you on the list? (authenticated)
    /// - Are you allowed in this area? (correct user type)
    /// - Do you have the right privileges? (create vs edit)
    ///
    /// OLD CODE REFERENCE:
    /// This replaces the authorization checks in AddBidNew() - Lines 462-470
    /// </summary>
    public class UserAuthorizationValidator : BaseValidator<BidValidationContext>
    {
        public override string ValidatorName => "User Authorization Validator";

        protected override Task<ValidationResult> ValidateInternalAsync(BidValidationContext context)
        {
            var user = context.CurrentUser;

            // Validation 1: User must be authenticated
            if (user == null)
            {
                return Task.FromResult(Fail(
                    "المستخدم غير مسجل دخول",
                    "NOT_AUTHENTICATED"));
            }

            // Validation 2: User must be one of the allowed types
            var allowedUserTypes = new List<UserType>
            {
                UserType.Association,
                UserType.Donor,
                UserType.SuperAdmin,
                UserType.Admin
            };

            if (!allowedUserTypes.Contains(user.UserType))
            {
                return Task.FromResult(Fail(
                    "ليس لديك صلاحية للقيام بهذا الإجراء",
                    "NOT_AUTHORIZED"));
            }

            // Validation 3: Admins can only edit existing bids (not create new ones)
            if ((user.UserType == UserType.SuperAdmin || user.UserType == UserType.Admin) &&
                context.RequestModel.Id == 0)
            {
                return Task.FromResult(Fail(
                    "المسؤولون يمكنهم فقط تعديل المنافسات الموجودة، وليس إنشاء منافسات جديدة",
                    "ADMIN_CANNOT_CREATE_BID"));
            }

            // All authorization checks passed
            return Task.FromResult(ValidationResult.Success());
        }
    }
}

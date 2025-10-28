using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using Nafis.Services.DTO.Bid;
using System;
using System.Linq;

namespace Nafis.Services.Implementation.Helpers
{
    /// <summary>
    /// Helper class for bid utility methods
    /// </summary>
    public static class BidUtilityHelper
    {
        /// <summary>
        /// Updates site map last modification date if specific bid data has changed
        /// </summary>
        public static void UpdateSiteMapLastModificationDateIfSpecificDataChanged(Bid bid, AddBidModelNew requestModel)
        {
            if (bid is null || requestModel is null)
                return;

            // Check if any relevant fields have changed
            bool hasChanged = bid.BidName != requestModel.BidName ||
                             bid.BidDescription != requestModel.BidDescription ||
                             bid.LastDateInReceivingEnquiries != requestModel.LastDateInReceivingEnquiries ||
                             bid.LastDateInOffersSubmission != requestModel.LastDateInOffersSubmission ||
                             bid.OffersOpeningDate != requestModel.OffersOpeningDate;

            if (hasChanged)
            {
                bid.SiteMapLastModificationDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Validates bid invitation attachments for new bids
        /// </summary>
        public static bool ValidateBidInvitationAttachmentsNew(AddBidModelNew model)
        {
            return model.BidVisibility == BidTypes.Habilitation &&
                (model.IsInvitationNeedAttachments.HasValue ? model.IsInvitationNeedAttachments.Value : false)
                && (model.BidInvitationsAttachments is null || model.BidInvitationsAttachments.Count == 0);
        }

        /// <summary>
        /// Checks if we need to add attachments for a new bid
        /// </summary>
        public static bool CheckIfWeNeedAddAttachmentNew(AddBidModelNew model)
        {
            return model.BidVisibility == BidTypes.Habilitation &&
                   model.IsInvitationNeedAttachments.HasValue &&
                   model.IsInvitationNeedAttachments.Value &&
                   model.BidInvitationsAttachments != null &&
                   model.BidInvitationsAttachments.Any();
        }

        /// <summary>
        /// Formats bid reference number
        /// </summary>
        public static string FormatBidRefNumber(string firstPart, string randomPart)
        {
            return $"{firstPart}{randomPart}";
        }

        /// <summary>
        /// Determines if bid is in draft status
        /// </summary>
        public static bool IsBidDraft(Bid bid)
        {
            return bid != null && bid.TenderStatusId == (int)TenderStatus.Draft;
        }

        /// <summary>
        /// Determines if bid is published
        /// </summary>
        public static bool IsBidPublished(Bid bid)
        {
            return bid != null && bid.TenderStatusId == (int)TenderStatus.Published;
        }

        /// <summary>
        /// Determines if bid is closed
        /// </summary>
        public static bool IsBidClosed(Bid bid)
        {
            return bid != null && bid.TenderStatusId == (int)TenderStatus.Closed;
        }

        /// <summary>
        /// Checks if bid visibility is private
        /// </summary>
        public static bool IsPrivateBid(Bid bid)
        {
            return bid != null && bid.BidVisibility == BidTypes.Private;
        }

        /// <summary>
        /// Checks if bid visibility is public
        /// </summary>
        public static bool IsPublicBid(Bid bid)
        {
            return bid != null && bid.BidVisibility == BidTypes.Public;
        }

        /// <summary>
        /// Checks if bid is a habilitation bid
        /// </summary>
        public static bool IsHabilitationBid(Bid bid)
        {
            return bid != null && bid.BidVisibility == BidTypes.Habilitation;
        }

        /// <summary>
        /// Gets bid type display name
        /// </summary>
        public static string GetBidTypeDisplayName(BidTypes bidType)
        {
            return bidType switch
            {
                BidTypes.Public => "عامة",
                BidTypes.Private => "خاصة",
                BidTypes.Habilitation => "تأهيل",
                _ => "غير محدد"
            };
        }

        /// <summary>
        /// Gets bid status display name in Arabic
        /// </summary>
        public static string GetBidStatusDisplayName(TenderStatus status)
        {
            return status switch
            {
                TenderStatus.Draft => "مسودة",
                TenderStatus.PendingApproval => "في انتظار الموافقة",
                TenderStatus.Published => "منشورة",
                TenderStatus.Closed => "مغلقة",
                TenderStatus.Cancelled => "ملغاة",
                _ => "غير محدد"
            };
        }
    }
}

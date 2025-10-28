using Nafes.CrossCutting.Model.Entities;
using Nafes.CrossCutting.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Nafis.Services.Implementation.Helpers
{
    /// <summary>
    /// Helper class for building common bid queries and filters
    /// </summary>
    public static class BidQueryHelper
    {
        /// <summary>
        /// Creates a filter expression for published bids
        /// </summary>
        public static Expression<Func<Bid, bool>> GetPublishedBidsFilter()
        {
            return bid => bid.TenderStatusId == (int)TenderStatus.Published;
        }

        /// <summary>
        /// Creates a filter expression for draft bids
        /// </summary>
        public static Expression<Func<Bid, bool>> GetDraftBidsFilter()
        {
            return bid => bid.TenderStatusId == (int)TenderStatus.Draft;
        }

        /// <summary>
        /// Creates a filter expression for bids by association
        /// </summary>
        public static Expression<Func<Bid, bool>> GetBidsByAssociationFilter(long associationId)
        {
            return bid => bid.AssociationId == associationId;
        }

        /// <summary>
        /// Creates a filter expression for bids by region
        /// </summary>
        public static Expression<Func<Bid, bool>> GetBidsByRegionFilter(int regionId)
        {
            return bid => bid.BidRegions.Any(br => br.RegionId == regionId);
        }

        /// <summary>
        /// Creates a filter expression for public bids
        /// </summary>
        public static Expression<Func<Bid, bool>> GetPublicBidsFilter()
        {
            return bid => bid.BidVisibility == BidTypes.Public;
        }

        /// <summary>
        /// Creates a filter expression for private bids
        /// </summary>
        public static Expression<Func<Bid, bool>> GetPrivateBidsFilter()
        {
            return bid => bid.BidVisibility == BidTypes.Private;
        }

        /// <summary>
        /// Creates a filter expression for active bids (not expired)
        /// </summary>
        public static Expression<Func<Bid, bool>> GetActiveBidsFilter(DateTime currentDate)
        {
            return bid => bid.LastDateInOffersSubmission >= currentDate &&
                         bid.TenderStatusId == (int)TenderStatus.Published;
        }

        /// <summary>
        /// Creates a filter expression for expired bids
        /// </summary>
        public static Expression<Func<Bid, bool>> GetExpiredBidsFilter(DateTime currentDate)
        {
            return bid => bid.LastDateInOffersSubmission < currentDate;
        }

        /// <summary>
        /// Creates a filter expression for bids by date range
        /// </summary>
        public static Expression<Func<Bid, bool>> GetBidsByDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            return bid => bid.CreatedDate >= startDate && bid.CreatedDate <= endDate;
        }

        /// <summary>
        /// Creates a filter expression for bids by industry
        /// </summary>
        public static Expression<Func<Bid, bool>> GetBidsByIndustryFilter(long industryId)
        {
            return bid => bid.BidIndustries.Any(bi => bi.IndustryId == industryId);
        }

        /// <summary>
        /// Creates a filter expression for bids with terms book bought
        /// </summary>
        public static Expression<Func<Bid, bool>> GetBidsWithTermsBookBoughtFilter()
        {
            return bid => bid.ProviderBids.Any(pb => pb.IsPaymentConfirmed);
        }

        /// <summary>
        /// Combines multiple bid filters with AND logic
        /// </summary>
        public static Expression<Func<Bid, bool>> CombineFiltersWithAnd(params Expression<Func<Bid, bool>>[] filters)
        {
            if (filters == null || filters.Length == 0)
                return bid => true;

            Expression<Func<Bid, bool>> combined = filters[0];

            for (int i = 1; i < filters.Length; i++)
            {
                combined = AndAlso(combined, filters[i]);
            }

            return combined;
        }

        /// <summary>
        /// Helper method to combine two expressions with AND
        /// </summary>
        private static Expression<Func<T, bool>> AndAlso<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

        /// <summary>
        /// Helper class for expression visitor pattern
        /// </summary>
        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }

        /// <summary>
        /// Gets default bid ordering (by created date descending)
        /// </summary>
        public static Func<IQueryable<Bid>, IOrderedQueryable<Bid>> GetDefaultBidOrdering()
        {
            return query => query.OrderByDescending(b => b.CreatedDate);
        }

        /// <summary>
        /// Gets bid ordering by submission deadline
        /// </summary>
        public static Func<IQueryable<Bid>, IOrderedQueryable<Bid>> GetBidOrderingByDeadline()
        {
            return query => query.OrderBy(b => b.LastDateInOffersSubmission);
        }
    }
}

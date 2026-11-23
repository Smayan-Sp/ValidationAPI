using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Models.DTO;

namespace Validation_Management_API.Helpers
{
    public static class PagingExtensions
    {
        public static async Task<PagedResult<TResult>> ToPagedResultAsync<TEntity, TResult>(
            this IOrderedQueryable<TEntity> query,
            PagedRequest paging,
            Expression<Func<TEntity, TResult>> selector)
        {
            int totalCount = await query.CountAsync();

            List<TResult> items = await query
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(selector)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Items = items,
                TotalCount = totalCount,
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }
    }
}

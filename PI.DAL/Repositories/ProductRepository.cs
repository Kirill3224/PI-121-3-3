using Microsoft.EntityFrameworkCore;
using PI.DAL.Entities.Catalog;
using PI.DAL.Interfaces;
using PI.DAL.Models;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredPagedAsync(ProductFilterParams filter, CancellationToken cancellationToken = default)
    {
        // 1. Create a query
        var query = _dbSet.AsNoTracking();

        // 2. Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filter.SearchTerm) || p.Description.Contains(filter.SearchTerm));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // 3. Sort using OrderBy/OrderByDescending based on filter.SortBy
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name) // Default sorting
            };
        }
        else
        {
            // Default sort is required before using Skip/Take in EF Core
            query = query.OrderBy(p => p.Name);
        }

        // 4. Get the total count
        int totalCount = await query.CountAsync(cancellationToken);

        // 5. Apply pagination & 6. Return the result
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Added .AsNoTracking() to satisfy the technical requirement for read methods
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
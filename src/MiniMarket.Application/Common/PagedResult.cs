namespace MiniMarket.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        : this(items.ToList(), totalCount, pageNumber, pageSize)
    {
    }

    public static PagedResult<T> Empty(int pageSize = 10)
        => new([], 0, 1, pageSize);
}

public class PagedQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }

    public int Skip => (PageNumber - 1) * PageSize;
}

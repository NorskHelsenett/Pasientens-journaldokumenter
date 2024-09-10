public class Pagination
{
    public int TotalCount { get; set; }
	public int PageSize { get; set; }
	public int CurrentPageIndex { get; set; } = 0;
	public int PrevPageIndex { get; set; } = 0;
	public int NextPageIndex { get; set; } = 0;
	public int FirstPageIndex { get; set; } = 0;
	public int LastPageIndex { get; set; } = 0;
	public int PageCount { get; set; } = 0;

	public static Pagination Create(int totalCount, int pageIndex, int pageSize)
	{
		var pagination = new Pagination();

		pagination.TotalCount = totalCount;
		pagination.PageSize = pageSize; 
		pagination.CurrentPageIndex = pageIndex;
		pagination.FirstPageIndex = 1;

		pagination.PageCount = (totalCount / pageSize) + 1;

		pagination.PrevPageIndex = pageIndex - 1;
		pagination.NextPageIndex = pageIndex + 1;

		if (pagination.PrevPageIndex < 1)
		{
			pagination.PrevPageIndex = 1;
		}

        if (pagination.NextPageIndex > pagination.PageCount)
        {
            pagination.NextPageIndex = pagination.PageCount;
        }

        if (pagination.LastPageIndex > pagination.PageCount)
		{
			pagination.LastPageIndex = pagination.PageCount;
		}

		pagination.LastPageIndex = pagination.PageCount; 
		
		return pagination; 
	}
}
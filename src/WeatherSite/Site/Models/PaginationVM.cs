using System.Collections.Generic;

namespace WeatherSite.Models;

public interface IPaginationVM
{
    bool IsSuccess { get; set; }
    string ElementId { get; set; }
    string Url { get; set; }
    int PageNumber { get; set; }
    int NumberOfEntitiesOnPage { get; set; }
    int NumberOfPages { get; set; }
}

public class PaginationVM<T> : IPaginationVM where T : class
{
    public bool IsSuccess { get; set; }
    public List<T>? Values { get; set; } = [];
    public required string ElementId { get; set; }
    public required string Url { get; set; }
    public int PageNumber { get; set; }
    public int NumberOfEntitiesOnPage { get; set; }
    public int NumberOfPages { get; set; }
}

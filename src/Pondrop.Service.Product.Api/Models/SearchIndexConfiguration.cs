namespace Pondrop.Service.Product.Api.Models;

public class SearchIndexConfiguration
{
    public const string Key = nameof(SearchIndexConfiguration);
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ProductIndexName { get; set; } = string.Empty;
    public string ProductCategoryIndexName { get; set; } = string.Empty;
    public string CategoryIndexName { get; set; } = string.Empty;
    public string CategoryGroupingIndexName { get; set; } = string.Empty;
}

namespace Pondrop.Service.Product.Api.Models;

public class CategorySearchIndexConfiguration
{
    public const string Key = nameof(CategorySearchIndexConfiguration);

    public string BaseUrl { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
namespace Pondrop.Service.Product.Api.Models;

public class ProductCategorySearchIndexConfiguration
{
    public const string Key = nameof(ProductCategorySearchIndexConfiguration);

    public string BaseUrl { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
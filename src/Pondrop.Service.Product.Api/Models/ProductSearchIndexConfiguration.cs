namespace Pondrop.Service.Product.Api.Models;

public class ProductSearchIndexConfiguration
{
    public const string Key = nameof(ProductSearchIndexConfiguration);

    public string BaseUrl { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
using Bogus;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Events.ProductCategory;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Product.Tests.Faker;

public static class ProductCategoryFaker
{
    private static readonly Guid[] CategoryIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly Guid[] ProductIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly string[] Ids = new[] { "12", "1213", "2132", "13", "3012300", "1232" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<ProductCategoryRecord> GetProductCategoryRecords(int count = 5)
    {
        var faker = new Faker<ProductCategoryRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<ProductCategoryEntity> GetProductCategoryEntities(int count = 5)
    {
        var faker = new Faker<ProductCategoryEntity>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    

    public static ProductCategoryEntity GetProductCategoryEntity()
    {
        var faker = new Faker<ProductCategoryEntity>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static CreateProductCategoryCommand GetCreateProductCategoryCommand()
    {
        var faker = new Faker<CreateProductCategoryCommand>()
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }
    public static UpdateProductCategoryCommand GetUpdateProductCategoryCommand()
    {
        var faker = new Faker<UpdateProductCategoryCommand>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }

    public static ProductCategoryRecord GetProductCategoryRecord(CreateProductCategoryCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<ProductCategoryRecord>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static ProductCategoryRecord GetProductCategoryRecord(UpdateProductCategoryCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<ProductCategoryRecord>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryId, f => f.PickRandom(CategoryIds))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}
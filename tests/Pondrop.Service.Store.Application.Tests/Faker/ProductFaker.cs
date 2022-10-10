using Bogus;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Events.Product;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Product.Tests.Faker;

public static class ProductFaker
{
    private static readonly string[] Names = new[] { "The Local", "The Far Away", "The Just Right", "Test" };
    private static readonly string[] Variants = new[] { "Variants 1", "Variants 2", "Variants 3" };
    private static readonly string[] AltNames = new[] { "Alt Name 1", "Alt Name 2", "Alt Name 3" };
    private static readonly string[] ShortDescriptions = new[] { "Description 1", "Description 2", "Description 3" };
    private static readonly string[] ExternalReferenceIds = new[] { "1", "2", "3" };
    private static readonly string[] NetContentUoms = new[] { "ML", "L" };
    private static readonly string[] PossibleCategories = new[] { "Category 1", "Category 2" };
    private static readonly double[] NetContents = new[] { 1d, 10d, 100d };
    private static readonly Guid[] BrandIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly Guid[] ProductIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly string[] Ids = new[] { "12", "1213", "2132", "13", "3012300", "1232" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

   

    public static List<ProductRecord> GetProductRecords(int count = 5)
    {
        var faker = new Faker<ProductRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<ProductEntity> GetProductEntities(int count = 5)
    {
        var faker = new Faker<ProductEntity>()
          .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }


    public static ProductEntity GetProductEntity()
    {
        var faker = new Faker<ProductEntity>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static List<ProductViewRecord> GetProductViewRecords(int count = 5)
    {

        var faker = new Faker<ProductViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions));

        return faker.Generate(Math.Max(0, count));
    }

    public static CreateProductCommand GetCreateProductCommand()
    {
        var faker = new Faker<CreateProductCommand>()
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }
    public static UpdateProductCommand GetUpdateProductCommand()
    {
        var faker = new Faker<UpdateProductCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }

    public static ProductRecord GetProductRecord(CreateProductCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<ProductRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }

    public static ProductRecord GetProductRecord(UpdateProductCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<ProductRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Name, f => f.PickRandom(Names))
            .RuleFor(x => x.Variant, f => f.PickRandom(Variants))
            .RuleFor(x => x.AltName, f => f.PickRandom(AltNames))
            .RuleFor(x => x.ShortDescription, f => f.PickRandom(ShortDescriptions))
            .RuleFor(x => x.ExternalReferenceId, f => f.PickRandom(ExternalReferenceIds))
            .RuleFor(x => x.NetContentUom, f => f.PickRandom(NetContentUoms))
            .RuleFor(x => x.NetContent, f => f.PickRandom(NetContents))
            .RuleFor(x => x.PossibleCategories, f => f.PickRandom(PossibleCategories))
            .RuleFor(x => x.BrandId, f => f.PickRandom(BrandIds))
            .RuleFor(x => x.ChildProductId, f => ProductIds.ToList())
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}
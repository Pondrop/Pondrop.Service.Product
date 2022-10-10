using Bogus;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Events.Product;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Product.Tests.Faker;

public static class BarcodeFaker
{
    private static readonly string[] BarcodeNumbers = new[] { "Number 123", "Number 234", "Number 345" };
    private static readonly string[] BarcodeTexts = new[] { "Text 1", "Text 2", "Text 3" };
    private static readonly string[] BarcodeTypes = new[] { "Type 1", "Type 2", "Type 3" };
    private static readonly Guid[] ProductIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly Guid[] RetailerIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly Guid[] CompanyIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly string[] Ids = new[] { "12", "1213", "2132", "13", "3012300", "1232" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };

    public static List<BarcodeRecord> GetBarcodeRecords(int count = 5)
    {
        var faker = new Faker<BarcodeRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductID, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerID, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyID, f => f.PickRandom(CompanyIds))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<BarcodeEntity> GetBarcodeEntities(int count = 5)
    {
        var faker = new Faker<BarcodeEntity>()
          .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerId, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyId, f => f.PickRandom(CompanyIds))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }


    public static BarcodeEntity GetBarcodeEntity()
    {
        var faker = new Faker<BarcodeEntity>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerId, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyId, f => f.PickRandom(CompanyIds))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static CreateBarcodeCommand GetCreateBarcodeCommand()
    {
        var faker = new Faker<CreateBarcodeCommand>()
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerId, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyId, f => f.PickRandom(CompanyIds));
        return faker.Generate();
    }
    public static UpdateBarcodeCommand GetUpdateBarcodeCommand()
    {
        var faker = new Faker<UpdateBarcodeCommand>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductId, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerId, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyId, f => f.PickRandom(CompanyIds));
        return faker.Generate();
    }

    public static BarcodeRecord GetBarcodeRecord(CreateBarcodeCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<BarcodeRecord>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductID, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerID, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyID, f => f.PickRandom(CompanyIds))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }

    public static BarcodeRecord GetBarcodeRecord(UpdateBarcodeCommand command)
    {
        var utcNow = DateTime.UtcNow;

        var faker = new Faker<BarcodeRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.BarcodeNumber, f => f.PickRandom(BarcodeNumbers))
            .RuleFor(x => x.BarcodeText, f => f.PickRandom(BarcodeTexts))
            .RuleFor(x => x.BarcodeType, f => f.PickRandom(BarcodeTypes))
            .RuleFor(x => x.ProductID, f => f.PickRandom(ProductIds))
            .RuleFor(x => x.RetailerID, f => f.PickRandom(RetailerIds))
            .RuleFor(x => x.CompanyID, f => f.PickRandom(CompanyIds))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}
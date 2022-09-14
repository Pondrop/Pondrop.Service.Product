using Bogus;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Events.Category;
using Pondrop.Service.Product.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Product.Tests.Faker;

public static class CategoryFaker
{
    private static readonly string[] Names = new[] { "The Local", "The Far Away", "The Just Right", "Test" };
    private static readonly string[] Descriptions = new[] { "Lakes", "Rivers", "Seaside" };
    private static readonly string[] Ids = new[] { "12", "1213", "2132", "13", "3012300", "1232" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<CategoryRecord> GetCategoryRecords(int count = 5)
    {
        var faker = new Faker<CategoryRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static List<CategoryViewRecord> GetCategoryViewRecords(int count = 5)
    {
        
        var faker = new Faker<CategoryViewRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }
    
    public static CreateCategoryCommand GetCreateCategoryCommand()
    {
        var faker = new Faker<CreateCategoryCommand>()
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }
    public static UpdateCategoryCommand GetUpdateCategoryCommand()
    {
        var faker = new Faker<UpdateCategoryCommand>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }

    public static CategoryRecord GetCategoryRecord(CreateCategoryCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<CategoryRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static CategoryRecord GetCategoryRecord(UpdateCategoryCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<CategoryRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names))
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}
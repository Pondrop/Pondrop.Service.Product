using Bogus;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Events.CategoryGrouping;
using Pondrop.Service.Product.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pondrop.Service.Product.Tests.Faker;

public static class CategoryGroupingFaker
{
    private static readonly string[] Names = new[] { "Name 1", "Name 2", "Name 3" };
    private static readonly string[] Descriptions = new[] { "Description 1", "Description 2", "Description 3" };
    private static readonly Guid[] HigherLevelCategoryIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly Guid[] LowerLevelCategoryIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
    private static readonly string[] Ids = new[] { "12", "1213", "2132", "13", "3012300", "1232" };
    private static readonly string[] UserNames = new[] { "test/faker", "test/jsmith", "test/bsmith", "test/asmith", "test/account" };
    
    public static List<CategoryGroupingRecord> GetCategoryGroupingRecords(int count = 5)
    {
        var faker = new Faker<CategoryGroupingRecord>()
            .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<CategoryGroupingEntity> GetCategoryGroupingEntities(int count = 5)
    {
        var faker = new Faker<CategoryGroupingEntity>()
          .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate(Math.Max(0, count));
    }

    public static List<CategoryGroupingViewRecord> GetCategoryGroupingViewRecords(int count = 5)
    {
        var faker = new Faker<CategoryGroupingViewRecord>()
          .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.ParentName, f => f.PickRandom(Names))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.CategoryName, f => f.PickRandom(Names));

        return faker.Generate(Math.Max(0, count));
    }


    public static CategoryGroupingEntity GetCategoryGroupingEntity()
    {
        var faker = new Faker<CategoryGroupingEntity>()
             .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.CreatedUtc, f => DateTime.UtcNow.AddSeconds(-1 * f.Random.Int(5000, 10000)))
            .RuleFor(x => x.UpdatedBy, f => f.PickRandom(UserNames))
            .RuleFor(x => x.UpdatedUtc, f => DateTime.UtcNow);

        return faker.Generate();
    }

    public static CreateCategoryGroupingCommand GetCreateCategoryGroupingCommand()
    {
        var faker = new Faker<CreateCategoryGroupingCommand>()
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }
    public static UpdateCategoryGroupingCommand GetUpdateCategoryGroupingCommand()
    {
        var faker = new Faker<UpdateCategoryGroupingCommand>()
         .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids));
        return faker.Generate();
    }

    public static CategoryGroupingRecord GetCategoryGroupingRecord(CreateCategoryGroupingCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<CategoryGroupingRecord>()
        .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
    
    public static CategoryGroupingRecord GetCategoryGroupingRecord(UpdateCategoryGroupingCommand command)
    {
        var utcNow = DateTime.UtcNow;
        
        var faker = new Faker<CategoryGroupingRecord>()
         .RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.Description, f => f.PickRandom(Descriptions))
            .RuleFor(x => x.HigherLevelCategoryId, f => f.PickRandom(HigherLevelCategoryIds))
            .RuleFor(x => x.LowerLevelCategoryId, f => f.PickRandom(LowerLevelCategoryIds))
            .RuleFor(x => x.PublicationLifecycleId, f => f.PickRandom(Ids))
            .RuleFor(x => x.CreatedBy, f => UserNames.First())
            .RuleFor(x => x.CreatedUtc, f => utcNow)
            .RuleFor(x => x.UpdatedBy, f => UserNames.First())
            .RuleFor(x => x.UpdatedUtc, f => utcNow);

        return faker.Generate();
    }
}
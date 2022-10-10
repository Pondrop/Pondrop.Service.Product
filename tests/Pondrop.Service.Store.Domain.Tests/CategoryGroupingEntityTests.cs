using Pondrop.Service.Product.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Product.Domain.Tests;

public class CategoryGroupingEntityTests
{
    private const string Description = "Test description";
    private const string PublicationLifecycleId = "121323442";
    private Guid HigherLevelCategoryId = Guid.NewGuid();
    private Guid LowerLevelCategoryId = Guid.NewGuid();
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    [Fact]
    public void CategoryGrouping_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new CategoryGroupingEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void CategoryGrouping_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewCategoryGrouping();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(HigherLevelCategoryId, entity.HigherLevelCategoryId);
        Assert.Equal(LowerLevelCategoryId, entity.LowerLevelCategoryId);
        Assert.Equal(Description, entity.Description);
        Assert.Equal(PublicationLifecycleId, entity.PublicationLifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    private CategoryGroupingEntity GetNewCategoryGrouping() => new CategoryGroupingEntity(
        HigherLevelCategoryId,
        LowerLevelCategoryId,
        Description,
        PublicationLifecycleId,
        CreatedBy);

}
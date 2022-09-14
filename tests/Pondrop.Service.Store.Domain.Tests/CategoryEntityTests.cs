using Pondrop.Service.Product.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Product.Domain.Tests;

public class CategoryEntityTests
{
    private const string Name = "My Category";
    private const string Description = "Test description";
    private const string PublicationLifecycleId = "121323442";
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    [Fact]
    public void Category_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new CategoryEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Category_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewCategory();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.CategoryName);
        Assert.Equal(Description, entity.Description);
        Assert.Equal(PublicationLifecycleId, entity.PublicationLifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    private CategoryEntity GetNewCategory() => new CategoryEntity(
        Name,
        Description,
        PublicationLifecycleId,
        CreatedBy);

}
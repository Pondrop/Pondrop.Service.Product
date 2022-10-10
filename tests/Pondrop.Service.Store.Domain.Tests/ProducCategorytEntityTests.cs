using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.ProductCategory.Domain.Tests;

public class ProductCategoryEntityTests
{
    private const string PublicationLifecycleId = "121323442";
    private Guid ProductId = Guid.NewGuid();
    private Guid CategoryId = Guid.NewGuid();
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    [Fact]
    public void ProductCategory_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new ProductCategoryEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void ProductCategory_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewProductCategory();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(CategoryId, entity.CategoryId);
        Assert.Equal(ProductId, entity.ProductId);
        Assert.Equal(PublicationLifecycleId, entity.PublicationLifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    private ProductCategoryEntity GetNewProductCategory() => new ProductCategoryEntity(
        CategoryId,
        ProductId,
        PublicationLifecycleId,
        CreatedBy);

}
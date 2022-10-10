using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Product.Domain.Tests;

public class ProductEntityTests
{
    private const string Name = "Test Name";
    private const string AltName = "Test Alt name";
    private const string Variant = "Test variant";
    private const string ShortDescription = "Test description";
    private const double NetContent = 123;
    private const string NetContentUom = "Test UOM";
    private const string PublicationLifecycleId = "121323442";
    private const string PossibleCategories = "Category 1";
    private const string ExternalReferenceId = "Ref Id";
    private List<Guid> ChildProductId = new List<Guid> { Guid.NewGuid() };
    private Guid BrandId = Guid.NewGuid();
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    [Fact]
    public void Product_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new ProductEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Product_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewProduct();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(Name, entity.Name);
        Assert.Equal(ShortDescription, entity.ShortDescription);
        Assert.Equal(AltName, entity.AltName);
        Assert.Equal(NetContent, entity.NetContent);
        Assert.Equal(NetContentUom, entity.NetContentUom);
        Assert.Equal(Variant, entity.Variant);
        Assert.Equal(PossibleCategories, entity.PossibleCategories);
        Assert.Equal(BrandId, entity.BrandId);
        Assert.Equal(ExternalReferenceId, entity.ExternalReferenceId);
        Assert.Equal(ChildProductId, entity.ChildProductId);
        Assert.Equal(PublicationLifecycleId, entity.PublicationLifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    private ProductEntity GetNewProduct() => new ProductEntity(
        Name,
        BrandId,
        ExternalReferenceId,
        Variant,
        AltName,
        ShortDescription,
        NetContent,
        NetContentUom,
        PossibleCategories,
        PublicationLifecycleId,
        ChildProductId,
        CreatedBy);

}
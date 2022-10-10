using Pondrop.Service.Product.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Barcode.Domain.Tests;

public class BarcodeEntityTests
{
    private const string BarcodeNumber = "Test Name";
    private const string BarcodeText = "Test Alt name";
    private const string BarcodeType = "Test variant";
    private const string PublicationLifecycleId = "121323442";
    private Guid ProductId = Guid.NewGuid();
    private Guid RetailerId = Guid.NewGuid();
    private Guid CompanyId = Guid.NewGuid();
    private const string CreatedBy = "user/admin1";
    private const string UpdatedBy = "user/admin2";

    

    [Fact]
    public void Barcode_Ctor_ShouldCreateEmpty()
    {
        // arrange

        // act
        var entity = new BarcodeEntity();

        // assert
        Assert.NotNull(entity);
        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(0, entity.EventsCount);
    }

    [Fact]
    public void Barcode_Ctor_ShouldCreateEvent()
    {
        // arrange

        // act
        var entity = GetNewBarcode();

        // assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(BarcodeNumber, entity.BarcodeNumber);
        Assert.Equal(BarcodeText, entity.BarcodeText);
        Assert.Equal(BarcodeType, entity.BarcodeType);
        Assert.Equal(ProductId, entity.ProductId);
        Assert.Equal(RetailerId, entity.RetailerId);
        Assert.Equal(CompanyId, entity.CompanyId);
        Assert.Equal(PublicationLifecycleId, entity.PublicationLifecycleId);
        Assert.Equal(CreatedBy, entity.CreatedBy);
        Assert.Equal(1, entity.EventsCount);
    }

    private BarcodeEntity GetNewBarcode() => new BarcodeEntity(
        BarcodeNumber,
        BarcodeText,
        BarcodeType,
        ProductId,
        RetailerId,
        CompanyId,
        PublicationLifecycleId,
        CreatedBy);

  
}
using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Microsoft.AspNetCore.Mvc;

namespace Its.Onix.Api.Test.Controllers;

public class ItemControllerTest
{
    [Theory]
    [InlineData("org1")]
    public void AddItemSuccessTest(string orgId)
    {
        var mv = new MVItem() { Status = "OK" };
        var m = new MItem() { };

        var itemImageSvc = new Mock<IItemImageService>();

        var service = new Mock<IItemService>();
        service.Setup(s => s.AddItem(orgId, m)).Returns(mv);

        var ic = new ItemController(service.Object, itemImageSvc.Object);
        var t = ic.AddItem(orgId, m);

        Assert.IsType<MVItem>(t);
        Assert.Equal("OK", t.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void AddItemImageSuccessTest(string orgId)
    {
        var itemId = Guid.NewGuid();

        var mv = new MVItemImage() { Status = "OK" };
        var m = new MItemImage() { ItemId = itemId };

        var itemImageSvc = new Mock<IItemImageService>();
        itemImageSvc.Setup(s => s.AddItemImage(orgId, m)).Returns(mv);

        var service = new Mock<IItemService>();

        var ic = new ItemController(service.Object, itemImageSvc.Object);
        var t = ic.AddItemImage(orgId, itemId, m);

        Assert.IsType<MVItemImage>(t);
        Assert.Equal("OK", t.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemByIdSuccessTest(string orgId)
    {
        var itemId = Guid.NewGuid().ToString();
        var mv = new MVItem() { Status = "OK" };

        var itemImageSvc = new Mock<IItemImageService>();

        var service = new Mock<IItemService>();
        service.Setup(s => s.DeleteItemById(orgId, itemId)).Returns(mv);

        var ic = new ItemController(service.Object, itemImageSvc.Object);
        var t = ic.DeleteItemById(orgId, itemId);

        var r = Assert.IsType<OkObjectResult>(t);
        var i = Assert.IsType<MVItem>(r.Value);
        Assert.Equal("OK", i.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemCascadeByIdSuccessTest(string orgId)
    {
        var itemId = Guid.NewGuid().ToString();
        var mv = new MVItem() { Status = "OK" };

        var itemImageSvc = new Mock<IItemImageService>();
        itemImageSvc.Setup(s => s.DeleteItemImageByItemId(orgId, itemId)).Returns((MVItemImage?)null);

        var service = new Mock<IItemService>();
        service.Setup(s => s.DeleteItemById(orgId, itemId)).Returns(mv);

        var ic = new ItemController(service.Object, itemImageSvc.Object);
        var t = ic.DeleteItemCascadeById(orgId, itemId);

        var r = Assert.IsType<OkObjectResult>(t);
        var i = Assert.IsType<MVItem>(r.Value);
        Assert.Equal("OK", i.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void UpdateItemByIdSuccessTest(string orgId)
    {
        var itemId = Guid.NewGuid().ToString();
        var m = new MItem() { };
        var mv = new MVItem() { Status = "OK" };

        var itemImageSvc = new Mock<IItemImageService>();

        var service = new Mock<IItemService>();
        service.Setup(s => s.UpdateItemById(orgId, itemId, m)).Returns(mv);

        var ic = new ItemController(service.Object, itemImageSvc.Object);
        var t = ic.UpdateItemById(orgId, itemId, m);

        var r = Assert.IsType<OkObjectResult>(t);
        var i = Assert.IsType<MVItem>(r.Value);
        Assert.Equal("OK", i.Status);
    }
}

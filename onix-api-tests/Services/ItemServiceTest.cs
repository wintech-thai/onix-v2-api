using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Test.Services;

public class ItemServiceTest
{
    [Theory]
    [InlineData("org1")]
    public void GetItemByIdOkTest(string orgId)
    {
        string jsonStr = """
        {
            "DimensionUnit": "cm",
            "WeightUnit": "gram",
            "Category": "XXXX",
            "SupplierUrl": "https://xxxx",
            "ProductUrl": "https://yyyy"
        }
        """;

        var itemId = Guid.NewGuid().ToString();

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.GetItemById(itemId)).Returns(new MItem()
        {
            Properties = jsonStr,
            Narrative = "XXXX|SSS",
        });

        var itemSvc = new ItemService(repo.Object);
        var item = itemSvc.GetItemById(orgId, itemId);

        Assert.NotNull(item);

        // ฟีลด์ Properties จะต้องเป็น empty เสมอ เพราะฟีลด์นี้คือ internal field ที่เก็บ json string
        Assert.NotNull(item.Properties);
        Assert.Empty(item.Properties);

        // ทดสอบว่า PropertiesObj จะต้องถูก deserialize จาก JSON string
        Assert.NotNull(item.PropertiesObj);
        Assert.Equal("XXXX", item.PropertiesObj.Category);
    }

    [Theory]
    [InlineData("org1")]
    public void AddItemCodeDuplicateTest(string orgId)
    {
        var code = "duplicate-code";
        var item = new MItem()
        {
            Code = code,
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.IsItemCodeExist(code)).Returns(true);

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.AddItem(orgId, item);

        Assert.NotNull(result);
        Assert.Equal("DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "sss|xx")]
    [InlineData("org1", null)]
    public void AddItemOkTest(string orgId, string narrative)
    {
        string[] narratives = null!;
        if (narrative != null)
        {
            narratives = narrative.Split('|');
        }

        string jsonStr = """
        {
            "DimensionUnit": "cm",
            "WeightUnit": "gram",
            "Category": "XXXX",
            "SupplierUrl": "https://xxxx",
            "ProductUrl": "https://yyyy"
        }
        """;

        var code = "new-code";
        var item = new MItem()
        {
            Code = code,
            Properties = "anything-does-not-matter",
            PropertiesObj = new MItemProperties()
            {
                Category = "test"
            },

            Narratives = narratives,
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.IsItemCodeExist(code)).Returns(false);

        repo.Setup(s => s.AddItem(item)).Returns(new MItem()
        {
            Properties = jsonStr,
        });

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.AddItem(orgId, item);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);

        // ฟีลด์ Properties จะต้องเป็น empty เสมอ เพราะฟีลด์นี้คือ internal field ที่เก็บ json string
        Assert.NotNull(result.Item);
        Assert.NotNull(result.Item.Properties);
        Assert.Empty(result.Item.Properties);
    }

    [Theory]
    [InlineData("org1")]
    public void AddItemPropertiesNullOkTest(string orgId)
    {
        var code = "new-code";
        var item = new MItem()
        {
            Code = code,
            Properties = "anything-does-not-matter",
            PropertiesObj = null
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.IsItemCodeExist(code)).Returns(false);

        repo.Setup(s => s.AddItem(item)).Returns(new MItem()
        {
            Properties = null,
        });

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.AddItem(orgId, item);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);

        // ฟีลด์ Properties จะต้องเป็น empty เสมอ เพราะฟีลด์นี้คือ internal field ที่เก็บ json string
        Assert.NotNull(result.Item);
        Assert.NotNull(result.Item.Properties);
        Assert.Empty(result.Item.Properties);
    }

    [Theory]
    [InlineData("org1")]
    public void UpdateItemIdNotFoundTest(string orgId)
    {
        var code = "not-found-code";
        var item = new MItem()
        {
            Id = Guid.NewGuid(),
            Code = code,
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.UpdateItemById(item.Id.ToString()!, item)).Returns((MItem?)null);

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.UpdateItemById(orgId, item.Id.ToString()!, item);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aa|aaa")]
    [InlineData("org1", null)]
    public void UpdateItemIdOkTest(string orgId, string narrative)
    {
        string[] narratives = null!;
        if (narrative != null)
        {
            narratives = narrative.Split('|');
        }

        var code = "code1";
        var item = new MItem()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Narratives = narratives,
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.UpdateItemById(item.Id.ToString()!, item)).Returns(new MItem()
        {
            Code = code,
        });

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.UpdateItemById(orgId, item.Id.ToString()!, item);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);

        Assert.NotNull(result.Item);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemIdInvalidIdTest(string orgId)
    {
        var itemId = "this-is-invalid-id";
        var repo = new Mock<IItemRepository>();

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.DeleteItemById(orgId, itemId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemIdNotFoundTest(string orgId)
    {
        var itemId = Guid.NewGuid().ToString();

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.DeleteItemById(itemId)).Returns((MItem?)null);

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.DeleteItemById(orgId, itemId);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemOkTest(string orgId)
    {
        var id = Guid.NewGuid();
        var itemId = id.ToString();

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.DeleteItemById(itemId)).Returns(new MItem()
        {
            Id = id
        });

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.DeleteItemById(orgId, itemId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);

        //ทดสอบว่ามีการ return object เดิมออกมาด้วยหลังจากลบไปแล้ว
        Assert.NotNull(result.Item);
        Assert.NotNull(result.Item.Id);
        Assert.Equal(itemId, result.Item.Id.ToString());
    }

    [Theory]
    [InlineData("org1")]
    public void GetItemOkTest(string orgId)
    {
        string jsonStr = """
        {
            "DimensionUnit": "cm",
            "WeightUnit": "gram",
            "Category": "XXXX",
            "SupplierUrl": "https://xxxx",
            "ProductUrl": "https://yyyy"
        }
        """;

        var itemQuery = new VMItem()
        {
            FullTextSearch = "",
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.GetItems(itemQuery)).Returns(
        [
            new MItem { Code = "001", Properties = jsonStr },
            new MItem { Code = "002", Properties = jsonStr },
        ]);

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.GetItems(orgId, itemQuery);

        foreach (var item in result)
        {
            Assert.Empty(item.Narrative!);
        }

        Assert.Equal(2, result.ToArray().Length);
    }

    [Theory]
    [InlineData("org1")]
    public void GetItemCountOkTest(string orgId)
    {
        var itemCount = 5;
        var itemQuery = new VMItem()
        {
            FullTextSearch = "",
        };

        var repo = new Mock<IItemRepository>();
        repo.Setup(s => s.GetItemCount(itemQuery)).Returns(itemCount);

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.GetItemCount(orgId, itemQuery);

        Assert.Equal(itemCount, result);
    }

    //====
    [Theory]
    [InlineData("org1")]
    public void GetAllowItemPropertyNamesTest(string orgId)
    {
        var repo = new Mock<IItemRepository>();

        var itemSvc = new ItemService(repo.Object);
        var result = itemSvc.GetAllowItemPropertyNames(orgId);

        Assert.Equal(8, result.ToArray().Length);
    }
    //====
}
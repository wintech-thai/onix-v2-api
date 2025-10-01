using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using Microsoft.EntityFrameworkCore;
using Its.Onix.Api.Database;

namespace Its.Onix.Api.Test.Services;

public class ScanItemRepositoryTest
{
    private DataContext GetDataContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new DataContext(null!, options);
        return context;
    }

    private MScanItem[] AddMockedData(DataContext ctx)
    {
        MScanItem[] items = [
            new() { OrgId = "org1", Serial = "001", Pin = "001", RunId = "AA" },
            new() { OrgId = "org2", Serial = "001", Pin = "001", RunId = "BB" },
            new() { OrgId = "org2", Serial = "001", Pin = "002", RunId = "BB", ScanCount = 5 },
        ];

        foreach (var item in items)
        {
            ctx.ScanItems!.Add(item);
        }

        ctx.SaveChanges();
        return items;
    }

    //===== GetJobById() ====
    [Theory]
    [InlineData("org1", "serial1", "pin1", false)]
    [InlineData("org1", "001", "002", false)]
    [InlineData("org1", "001", "001", true)]
    [InlineData("org2", "001", "001", true)]
    [InlineData("org3", "001", "001", false)]
    [InlineData("", "001", "001", false)]
    public void GetScanItemBySerialPinTest(string orgId, string serial, string pin, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.GetScanItemBySerialPin(serial, pin);
        var isExist = scanItem != null;

        Assert.Equal(found, isExist);
    }
    //=====

    //===== RegisterScanItem() ====
    [Theory]
    [InlineData("org1", 0, true)]
    [InlineData("org1", 1, false)] //org ไม่ตรงเลยไม่เจอ
    public void RegisterScanItemTest(string orgId, int idx, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.RegisterScanItem(item.Id.ToString()!);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal("TRUE", scanItem.RegisteredFlag);
            Assert.Equal(1, scanItem.ScanCount);
        }

        Assert.Equal(found, isExist);
    }
    //=====


    //===== IncreaseScanCount() ====
    [Theory]
    [InlineData("org1", 0, true)]
    [InlineData("org1", 1, false)] //org ไม่ตรงเลยไม่เจอ
    [InlineData("", 1, false)] //org ไม่ตรงเลยไม่เจอ
    [InlineData("org2", 2, true)]
    public void IncreaseScanCountTest(string orgId, int idx, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        int previousScanCnt = 0;
        if ((item != null) && (item.ScanCount != null))
        {
            previousScanCnt = (int)item.ScanCount;
        }

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.IncreaseScanCount(item!.Id.ToString()!);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal(previousScanCnt + 1, scanItem.ScanCount);
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== AttachScanItemToProduct() ====
    [Theory]
    [InlineData("org1", 0, true, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("org1", 1, false, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeebb")] //org ไม่ตรงเลยไม่เจอ
    public void AttachScanItemToProductTest(string orgId, int idx, bool found, string productId)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.AttachScanItemToProduct(item.Id.ToString()!, productId);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal("TRUE", scanItem.UsedFlag);
            Assert.Equal(productId, scanItem.ItemId.ToString());
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== AttachScanItemToCustomer() ====
    [Theory]
    [InlineData("org1", 0, true, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("org1", 1, false, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeebb")] //org ไม่ตรงเลยไม่เจอ
    public void AttachScanItemToCustomerTest(string orgId, int idx, bool found, string customerId)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.AttachScanItemToCustomer(item.Id.ToString()!, customerId);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal("TRUE", scanItem.AppliedFlag);
            Assert.Equal(customerId, scanItem.CustomerId.ToString());
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== IsPinExist() ====
    [Theory]
    [InlineData("org1", "001", true)] //เจอ
    [InlineData("org1", "002", false)] //ไม่เจอ
    [InlineData("org3", "002", false)] //ไม่เจอ
    [InlineData("", "001", false)] //ไม่เจอ
    public void IsPinExistTest(string orgId, string pin, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var isExist = repo.IsPinExist(pin);

        Assert.Equal(found, isExist);
    }
    //=====

    //===== IsSerialExist() ====
    [Theory]
    [InlineData("org1", "001", true)] //เจอ
    [InlineData("org1", "002", false)] //ไม่เจอ
    [InlineData("org3", "002", false)] //ไม่เจอ
    [InlineData("", "001", false)] //ไม่เจอ
    public void IsSerialExistTest(string orgId, string serial, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var isExist = repo.IsSerialExist(serial);

        Assert.Equal(found, isExist);
    }
    //=====

    //===== UnVerifyScanItemById() ====
    [Theory]
    [InlineData("org1", 0, true)]
    [InlineData("org1", 1, false)] //org ไม่ตรงเลยไม่เจอ
    public void UnVerifyScanItemByIdTest(string orgId, int idx, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.UnVerifyScanItemById(item.Id.ToString()!);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal("NO", scanItem.RegisteredFlag);
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== DeleteScanItemById() ====
    [Theory]
    [InlineData("org1", 0, true)]
    [InlineData("org1", 1, false)] //org ไม่ตรงเลยไม่เจอ
    public void DeleteScanItemByIdTest(string orgId, int idx, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.DeleteScanItemById(item.Id.ToString()!);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal(item.Id, scanItem.Id);
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== GetScanItemById() ====
    [Theory]
    [InlineData("org1", 0, true)]
    [InlineData("org1", 1, false)] //org ไม่ตรงเลยไม่เจอ
    public void GetScanItemByIdTest(string orgId, int idx, bool found)
    {
        var ctx = GetDataContext(Guid.NewGuid().ToString());
        var items = AddMockedData(ctx);
        var item = items[idx];

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var scanItem = repo.GetScanItemById(item.Id.ToString()!);
        var isExist = scanItem != null;

        if (scanItem != null)
        {
            Assert.Equal(item.Id, scanItem.Id);
            Assert.Equal(item.Serial, scanItem.Serial);
            Assert.Equal(item.Pin, scanItem.Pin);
            Assert.Equal(item.OrgId, scanItem.OrgId);
        }

        Assert.Equal(found, isExist);
    }
    //=====

    //===== GetScanItemCount() ====
    [Theory]
    [InlineData("org1", "XXX", 0)]
    [InlineData("org1", "XX", 0)]
    [InlineData("org1", "", 1)]
    [InlineData("org2", "", 2)]
    [InlineData("", "", 0)]
    public void GetScanItemCountTest(string orgId, string txtSearch, int expectedCnt)
    {
        var param = new VMScanItem() { FullTextSearch = txtSearch };

        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var cnt = repo.GetScanItemCount(param);

        Assert.Equal(expectedCnt, cnt);
    }
    //=====

    //===== GetScanItems() ====
    [Theory]
    [InlineData("org1", "XXX", 0, 0)]
    [InlineData("org1", "XX", 0, 0)]
    [InlineData("org1", "", 1, 0)]
    [InlineData("org2", "", 2, 0)]
    [InlineData("", "", 0, 1)]
    public void GetScanItemsTest(string orgId, string txtSearch, int expectedCnt, int offset)
    {
        var param = new VMScanItem() { FullTextSearch = txtSearch, Offset = offset };

        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var items = repo.GetScanItems(param);

        Assert.Equal(expectedCnt, items.ToArray().Length);
    }
    //=====

    //===== AddScanItem() ====
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void AddScanItemTest(string orgId)
    {
        var m = new MScanItem() { Serial = "1111", Pin = "AAAAA" };

        var ctx = GetDataContext(Guid.NewGuid().ToString());
        AddMockedData(ctx);

        var repo = new ScanItemRepository(ctx);

        repo.SetCustomOrgId(orgId);
        var item = repo.AddScanItem(m);

        Assert.NotNull(item.Id);
        Assert.Equal(item.Pin, m.Pin);
    }
    //=====
}

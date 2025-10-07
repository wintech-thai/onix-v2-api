using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Test.Services;

public class ScanItemServiceTest
{
    //==== AttachScanItemToProduct()
    [Theory]
    [InlineData("org1")]
    public void AttachScanItemToProductOkTest(string orgId)
    {
        var scanItemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var m = new MScanItem() { ItemId = productId };
        var product = new MItem() { Code = "This is code" };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.AttachScanItemToProduct(scanItemId.ToString(), productId.ToString(), It.IsAny<MItem>())).Returns(m);

        var itemRepo = new Mock<IItemRepository>();
        itemRepo.Setup(s => s.GetItemById(productId.ToString())).Returns(product);

        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var sciTplService = new Mock<IScanItemTemplateService>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToProduct(orgId, scanItemId.ToString(), productId.ToString());

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaa-bbbb")]
    public void AttachScanItemToProductInvalidScanItemIdTest(string orgId, string scanItemId)
    {
        var productId = Guid.NewGuid();

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToProduct(orgId, scanItemId.ToString(), productId.ToString());

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaa-bbbb")]
    public void AttachScanItemToProductInvalidProductIdTest(string orgId, string productId)
    {
        var scanItemId = Guid.NewGuid();

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToProduct(orgId, scanItemId.ToString(), productId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void AttachScanItemToProductNotFoundTest(string orgId)
    {
        var scanItemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var m = new MScanItem() { ItemId = productId };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.AttachScanItemToProduct(scanItemId.ToString(), productId.ToString(), It.IsAny<MItem>())).Returns(m);

        var itemRepo = new Mock<IItemRepository>();
        itemRepo.Setup(s => s.GetItemById(productId.ToString())).Returns((MItem?) null!);

        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToProduct(orgId, scanItemId.ToString(), productId.ToString());

        Assert.NotNull(result);
        Assert.Equal("PRODUCT_NOTFOUND", result.Status);
    }
    //===

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemOtpNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync((MOtp?)null);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("OTP_NOTFOUND_OR_EXPIRE", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemOtpInvalidTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = "99999999" };

        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("OTP_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemSerialPinNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemProductNotAttachTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };
        // Simulate product not attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, ItemId = null };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("PRODUCT_NOT_ATTACH", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemProductNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };
        // Simulate product attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, ItemId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee") };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        // Simulate ว่าไม่มี product ใน DB
        itemRepo.Setup(s => s.GetItemById(scanItem.ItemId.ToString()!)).Returns((MItem)null!);

        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("PRODUCT_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemProductOkTest(string orgId, string serial, string pin, string otp)
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

        var itemId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        //var imageItem = new VMItemImage() { ItemId = itemId.ToString() };
        var product = new MItem() { Id = itemId, Properties = jsonStr };

        var motp = new MOtp() { Otp = otp };
        // Simulate product attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, ItemId = itemId };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        // Simulate ว่ามี product ใน DB
        itemRepo.Setup(s => s.GetItemById(itemId.ToString()!)).Returns(product);

        var itemImageRepo = new Mock<IItemImageRepository>();
        itemImageRepo.Setup(s => s.GetImages(It.IsAny<VMItemImage>())).Returns([
            new MImage() { ImagePath = "this/is/path1" },
            new MImage() { ImagePath = "this/is/path2" },
        ]);

        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemProduct(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE")]
    public void VerifyScanItemNotFoundTest(string orgId, string serial, string pin)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.VerifyScanItem(orgId, serial, pin);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE")]
    public void VerifyScanItemAlreadyRegisteredTest(string orgId, string serial, string pin)
    {
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, RegisteredFlag = "TRUE" };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.VerifyScanItem(orgId, serial, pin);

        Assert.NotNull(result);
        Assert.Equal("ALREADY_REGISTERED", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "FALSE")]
    [InlineData("org1", "A000001", "42AIDS3SeE", "NO")]
    [InlineData("org1", "A000001", "42AIDS3SeE", "")]
    [InlineData("org1", "A000001", "42AIDS3SeE", null)]
    //[InlineData("org1", "A000001", "42AIDS3SeE", null)] --> อันนี้ยังไม่ผ่านเพราะ code ไม่ได้ handle null
    public void VerifyScanItemOkTest(string orgId, string serial, string pin, string registerFlag)
    {
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, RegisteredFlag = registerFlag };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.VerifyScanItem(orgId, serial, pin);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222", "test@gmail.com")]
    public void GetOtpViaEmailNotFoundTest(string orgId, string serial, string pin, string otp, string email)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync((MOtp?)null);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetOtpViaEmail(orgId, serial, pin, otp, email);

        Assert.NotNull(result);
        Assert.Equal("OTP_NOTFOUND_OR_EXPIRE", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222", "test@gmail.com")]
    public void GetOtpViaEmailInvalidOtpTest(string orgId, string serial, string pin, string otp, string email)
    {
        var motp = new MOtp() { Otp = "0000000" };

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetOtpViaEmail(orgId, serial, pin, otp, email);

        Assert.NotNull(result);
        Assert.Equal("OTP_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222", "test@gmail.com")]
    public void GetOtpViaEmailOkTest(string orgId, string serial, string pin, string otp, string email)
    {
        var motp = new MOtp() { Otp = otp };

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetOtpViaEmail(orgId, serial, pin, otp, email);

        Assert.NotNull(result);
        Assert.NotNull(result.OTP);
        Assert.NotEmpty(result.OTP);
        Assert.Equal("SUCCESS", result.Status);
    }

    //=== AttachScanItemToCustomer()
    [Theory]
    [InlineData("org1")]
    public void AttachScanItemToCustomerTest(string orgId)
    {
        var scanItemId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = new MEntity() { };

        var scanItem = new MScanItem() { Serial = "yyyy", Pin = "xxxx", CustomerId = customerId, Id = scanItemId };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.AttachScanItemToCustomer(scanItemId.ToString(), customerId.ToString(), customer)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetEntityById(customerId.ToString())).Returns(customer);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToCustomer(orgId, scanItemId.ToString(), customerId.ToString());

        Assert.NotNull(result);
        Assert.NotNull(result.ScanItem);
        Assert.Equal("SUCCESS", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaa-bbbbb")]
    public void AttachScanItemToCustomerInvalidIdTest(string orgId, string scanItemId)
    {
        var customerId = Guid.NewGuid();
        var customer = new MEntity() { };

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetEntityById(customerId.ToString())).Returns(customer);

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToCustomer(orgId, scanItemId, customerId.ToString());

        Assert.NotNull(result);
        Assert.Null(result.ScanItem);
        Assert.Equal("UUID_INVALID", result.Status);
    }


    [Theory]
    [InlineData("org1", "aaaaa-bbbbb")]
    public void AttachScanItemToCustomerInvalidCustIdTest(string orgId, string customerId)
    {
        var scanItemId = Guid.NewGuid();
        var customer = new MEntity() { };

        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetEntityById(customerId.ToString())).Returns(customer);

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AttachScanItemToCustomer(orgId, scanItemId.ToString(), customerId);

        Assert.NotNull(result);
        Assert.Null(result.ScanItem);
        Assert.Equal("UUID_INVALID", result.Status);
    }
    //===

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerOtpNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ไม่มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync((MOtp?)null);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, null!);

        Assert.NotNull(result);
        Assert.Equal("OTP_NOTFOUND_OR_EXPIRE", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerOtpInvalidTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = "999999" };
        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(It.IsAny<string>())).ReturnsAsync(motp);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, null!);

        Assert.NotNull(result);
        Assert.Equal("OTP_INVALID", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerOtpEmailOtpNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var cust = new MCustomerRegister() { EmailOtp = "000000" };
        var motp = new MOtp() { Otp = otp };
        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:RegisterCustomer:{serial}:{pin}")).ReturnsAsync(motp);
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:ReceivedOtpViaEmail:{serial}:{pin}")).ReturnsAsync((MOtp?)null);
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, cust);

        Assert.NotNull(result);
        Assert.Equal("CUSTOMER_OTP_NOTFOUND", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerOtpEmailOtpInvalidTest(string orgId, string serial, string pin, string otp)
    {
        var cust = new MCustomerRegister() { EmailOtp = "000000" };
        var motp = new MOtp() { Otp = otp };
        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:RegisterCustomer:{serial}:{pin}")).ReturnsAsync(motp);
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:ReceivedOtpViaEmail:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, cust);

        Assert.NotNull(result);
        Assert.Equal("CUSTOMER_OTP_INVALID", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerSerialPinNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var cust = new MCustomerRegister() { EmailOtp = otp };
        var motp = new MOtp() { Otp = otp };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:RegisterCustomer:{serial}:{pin}")).ReturnsAsync(motp);
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:ReceivedOtpViaEmail:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, cust);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerScanItemInUsedTest(string orgId, string serial, string pin, string otp)
    {
        var cust = new MCustomerRegister() { EmailOtp = otp };
        var motp = new MOtp() { Otp = otp };
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, CustomerId = Guid.NewGuid() };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:RegisterCustomer:{serial}:{pin}")).ReturnsAsync(motp);
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:ReceivedOtpViaEmail:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, cust);

        Assert.NotNull(result);
        Assert.Equal("SCAN_ITEM_IS_ALREADY_OCCUPIED", result.Status);
    }


    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "222222")]
    public void RegisterCustomerOkTest(string orgId, string serial, string pin, string otp)
    {
        var cust = new MCustomerRegister() { EmailOtp = otp };
        var motp = new MOtp() { Otp = otp };
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, CustomerId = null, Id = Guid.NewGuid() };
        var customer = new MEntity() { Id = Guid.NewGuid() };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetOrCreateEntityByEmail(It.IsAny<MEntity>())).Returns(customer);

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:RegisterCustomer:{serial}:{pin}")).ReturnsAsync(motp);
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:ReceivedOtpViaEmail:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.RegisterCustomer(orgId, serial, pin, otp, cust);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }

    //=== GetScanItemCustomer() ===

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemCustomerOtpNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync((MOtp?)null);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("OTP_NOTFOUND_OR_EXPIRE", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemCustomerOtpInvalidTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = "99999999" };

        var scanItemRepo = new Mock<IScanItemRepository>();

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("OTP_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemProductSerialPinNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemCustomerNotAttachTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };
        // Simulate customer not attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, CustomerId = null };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("CUSTOMER_NOT_ATTACH", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemCustomerNotFoundTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };
        // Simulate product attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, CustomerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee") };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetEntityById(scanItem.CustomerId.ToString()!)).Returns((MEntity)null!);

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("CUSTOMER_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "A000001", "42AIDS3SeE", "091234")]
    public void GetScanItemCustomerOkTest(string orgId, string serial, string pin, string otp)
    {
        var motp = new MOtp() { Otp = otp };
        // Simulate product attached
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, CustomerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee") };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemBySerialPin(serial, pin)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();

        var entityRepo = new Mock<IEntityRepository>();
        entityRepo.Setup(s => s.GetEntityById(scanItem.CustomerId.ToString()!)).Returns(new MEntity());

        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate มีข้อมูล ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>($"{orgId}:Local:GetCustomer:{serial}:{pin}")).ReturnsAsync(motp);

        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCustomer(orgId, serial, pin, otp);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }
    //===

    //=== GetScanItemById()
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeff")]
    public void GetScanItemByIdOkTest(string orgId, string scanItemId)
    {
        var scanItem = new MScanItem() { Serial = "xxx", Pin = "thisispin", Id = Guid.Parse(scanItemId) };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemById(scanItemId)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);

        Assert.NotNull(result.ScanItem);
        Assert.Equal("xxx", result.ScanItem.Serial);
        //Test masking PIN
        Assert.Equal("t*******n", result.ScanItem.Pin);
    }
    //===

    //=== AddScanItem()
    [Theory]
    [InlineData("org1", "S099011", "ADESEESSS")]
    [InlineData("org2", "S090011", "ADESDESSS")]
    public void AddScanItemPinExistTest(string orgId, string serial, string pin)
    {
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, Url = pin };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.IsPinExist(pin)).Returns(true);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AddScanItem(orgId, scanItem);

        Assert.NotNull(result);
        Assert.Equal("PIN_ALREADY_EXIST", result.Status);
    }

    [Theory]
    [InlineData("org1", "S099011", "ADESEESSS")]
    [InlineData("org2", "S090011", "ADESDESSS")]
    public void AddScanItemSerialExistTest(string orgId, string serial, string pin)
    {
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, Url = pin };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.IsSerialExist(serial)).Returns(true);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AddScanItem(orgId, scanItem);

        Assert.NotNull(result);
        Assert.Equal("SERIAL_ALREADY_EXIST", result.Status);
    }

    [Theory]
    [InlineData("org1", "S099011", "ADESEESSS")]
    [InlineData("org2", "S090011", "ADESDESSS")]
    public void AddScanItemTemplateNotFoundTest(string orgId, string serial, string pin)
    {
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, Url = pin };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.IsPinExist(pin)).Returns(false);
        scanItemRepo.Setup(s => s.IsSerialExist(serial)).Returns(false);
        scanItemRepo.Setup(s => s.AddScanItem(scanItem)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AddScanItem(orgId, scanItem);

        Assert.NotNull(result);
        Assert.Equal("NO_SCAN_ITEM_TEMPLATE_FOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "S099011", "ADESEESSS")]
    [InlineData("org2", "S090011", "ADESDESSS")]
    public void AddScanItemTemplateUrlEmptyTest(string orgId, string serial, string pin)
    {
        var sciTpl = new MScanItemTemplate() { UrlTemplate = "" };
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, Url = pin };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.IsPinExist(pin)).Returns(false);
        scanItemRepo.Setup(s => s.IsSerialExist(serial)).Returns(false);
        scanItemRepo.Setup(s => s.AddScanItem(scanItem)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();
        sciTplService.Setup(s => s.GetScanItemTemplate(orgId)).Returns(sciTpl);

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AddScanItem(orgId, scanItem);

        Assert.NotNull(result);
        Assert.Equal("URL_TEMPLATE_EMPTY", result.Status);
    }


    [Theory]
    [InlineData("org1", "S099011", "ADESEESSS")]
    [InlineData("org2", "S090011", "ADESDESSS")]
    public void AddScanItemOkTest(string orgId, string serial, string pin)
    {
        var sciTpl = new MScanItemTemplate() { UrlTemplate = "https://scan-dev.please-scan.com/org/{VAR_ORG}/Verify/{VAR_SERIAL}/{VAR_PIN}" };
        var scanItem = new MScanItem() { Serial = serial, Pin = pin, Url = pin };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.IsPinExist(pin)).Returns(false);
        scanItemRepo.Setup(s => s.IsSerialExist(serial)).Returns(false);
        scanItemRepo.Setup(s => s.AddScanItem(scanItem)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();
        sciTplService.Setup(s => s.GetScanItemTemplate(orgId)).Returns(sciTpl);

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.AddScanItem(orgId, scanItem);

        Assert.NotNull(result);
        Assert.NotNull(result.ScanItem);
        Assert.NotNull(result.ScanItem.Url);

        var url = $"https://scan-dev.please-scan.com/org/{orgId}/Verify/{serial}/{pin}";
        Assert.Equal(url, result.ScanItem.Url);

        Assert.Equal("OK", result.Status);
    }
    //===

    //=== DeleteScanItemById()
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeex")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeay")]
    public void DeleteScanItemByIdInvalidTest(string orgId, string scanItemId)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.DeleteScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeab")]
    public void DeleteScanItemByIdNotFoundTest(string orgId, string scanItemId)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.DeleteScanItemById(scanItemId)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.DeleteScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeab")]
    public void DeleteScanItemByIdOkTest(string orgId, string scanItemId)
    {
        var scanItem = new MScanItem() { Serial = "", Pin = "", Url = "" };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.DeleteScanItemById(scanItemId)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.DeleteScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //===

    //=== UnVerifyScanItemById()
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeex")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeay")]
    public void UnVerifyScanItemByIdInvalidTest(string orgId, string scanItemId)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.UnVerifyScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeab")]
    public void UnVerifyScanItemByIdNotFoundTest(string orgId, string scanItemId)
    {
        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.UnVerifyScanItemById(scanItemId)).Returns((MScanItem?)null);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.UnVerifyScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea")]
    [InlineData("org2", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeab")]
    public void UnVerifyScanItemByIdOkTest(string orgId, string scanItemId)
    {
        var scanItem = new MScanItem() { Serial = "", Pin = "" };

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.UnVerifyScanItemById(scanItemId)).Returns(scanItem);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.UnVerifyScanItemById(orgId, scanItemId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //===

    //=== GetScanItemCount()
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void GetScanItemCountOkTest(string orgId)
    {
        var query = new VMScanItem();
        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItemCount(query)).Returns(5);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItemCount(orgId, query);

        Assert.Equal(5, result);
    }
    //===

    //=== GetScanItems()
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void GetScanItemsOkTest(string orgId)
    {
        var maskingPin = "T********1";
        var query = new VMScanItem();

        var scanItemRepo = new Mock<IScanItemRepository>();
        scanItemRepo.Setup(s => s.GetScanItems(query)).Returns([
            new MScanItem() { Serial = "S33333333", Pin = "THISISPIN1", Url = "THISISPIN1" },
            new MScanItem() { Serial = "S33333333", Pin = "THISISPIN1", Url = "THISISPIN1" }
        ]);

        var itemRepo = new Mock<IItemRepository>();
        var itemImageRepo = new Mock<IItemImageRepository>();
        var entityRepo = new Mock<IEntityRepository>();
        var storageUtil = new Mock<IStorageUtils>();
        var jobService = new Mock<IJobService>();
        var redisHelper = new Mock<IRedisHelper>();
        var sciTplService = new Mock<IScanItemTemplateService>();

        var sciService = new ScanItemService(
            scanItemRepo.Object,
            itemRepo.Object,
            itemImageRepo.Object,
            entityRepo.Object,
            storageUtil.Object,
            jobService.Object,
            sciTplService.Object,
            redisHelper.Object);

        var result = sciService.GetScanItems(orgId, query);

        int errorCnt = 0;
        foreach (var item in result.ToArray())
        {
            if (item.Pin != maskingPin) errorCnt++;

            //ทดสอบ URL ก็ต้องถูก masking ด้วย
            Assert.Contains(item.Pin!, item.Url);
        }

        Assert.Equal(0, errorCnt);
        Assert.Equal(2, result.ToArray().Length);
    }
    //===
}
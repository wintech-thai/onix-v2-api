using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using System.Security.Cryptography.X509Certificates;

namespace Its.Onix.Api.Test.Controllers;

public class VerifyScanItemControllerTest
{
    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI")]
    public void VerifyScanItemOkTest(string orgId, string serial, string pin)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.VerifyScanItem(orgId, serial, pin)).Returns(new MVScanItemResult()
        {
            Status = pin, //เอา pin มาใส่เพื่อเช็คว่าค่าถูกส่งมา เพื่อ unit test ได้ง่าย ๆ
            ScanItem = new MScanItem()
            {
                Url = $"https://verify.its.com/verify/{orgId}/{serial}/{pin}",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        var redisHelper = new Mock<IRedisHelper>();

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.VerifyScanItem(orgId, serial, pin);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(pin, custStatus.ToString());

        var scanResult = Assert.IsType<MVScanItemResult>(t);

        var prdUrl = scanResult.GetProductUrl;
        var cusUrl = scanResult.GetCustomerUrl;
        var regUrl = scanResult.RegisterCustomerUrl;
        var otpUrl = scanResult.RequestOtpViaEmailUrl;

        //ดูว่าต้องมีการ return url ออกมาด้วย
        Assert.Contains($"verify/{orgId}/{serial}/{pin}", prdUrl);
        Assert.Contains($"verify/{orgId}/{serial}/{pin}", cusUrl);
        Assert.Contains($"verify/{orgId}/{serial}/{pin}", regUrl);
        Assert.Contains($"verify/{orgId}/{serial}/{pin}", otpUrl);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "123456")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "654321")]
    public void GetProductTest(string orgId, string serial, string pin, string otp)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.GetScanItemProduct(orgId, serial, pin, otp)).Returns(new MVItem()
        {
            Status = pin, //เอา pin มาใส่เพื่อเช็คว่าค่าถูกส่งมา เพื่อ unit test ได้ง่าย ๆ
        });

        var actionService = new Mock<IScanItemActionService>();
        var redisHelper = new Mock<IRedisHelper>();

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.GetProduct(orgId, serial, pin, otp);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(pin, custStatus.ToString());

        var scanResult = Assert.IsType<MVItem>(t);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "123456")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "654321")]
    public void GetOtpViaEmailTest(string orgId, string serial, string pin, string otp)
    {
        var email = "xxx@gmail.com";
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.GetOtpViaEmail(orgId, serial, pin, otp, email)).Returns(new MVOtp()
        {
            Status = pin, //เอา pin มาใส่เพื่อเช็คว่าค่าถูกส่งมา เพื่อ unit test ได้ง่าย ๆ
        });

        var actionService = new Mock<IScanItemActionService>();
        var redisHelper = new Mock<IRedisHelper>();

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.GetOtpViaEmail(orgId, serial, pin, otp, email);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(pin, custStatus.ToString());
        Assert.IsType<MVOtp>(t);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "123456")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "654321")]
    public void RegisterCustomerTest(string orgId, string serial, string pin, string otp)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.RegisterCustomer(orgId, serial, pin, otp, It.IsAny<MCustomerRegister>())).Returns(new MVEntity()
        {
            Status = pin, //เอา pin มาใส่เพื่อเช็คว่าค่าถูกส่งมา เพื่อ unit test ได้ง่าย ๆ
            Entity = new MEntity()
            {
                PrimaryEmail = "aasdssssdsdfsdf@gmail.com",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        var redisHelper = new Mock<IRedisHelper>();

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.RegisterCustomer(orgId, serial, pin, otp, new MCustomerRegister());

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(pin, custStatus.ToString());
        var entity = Assert.IsType<MVEntityRestrictedInfo>(t);

        Assert.Equal("a*************f@gmail.com", entity.MaskingEmail);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "123456")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "654321")]
    public void GetCustomerTest(string orgId, string serial, string pin, string otp)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.GetScanItemCustomer(orgId, serial, pin, otp)).Returns(new MVEntity()
        {
            Status = pin, //เอา pin มาใส่เพื่อเช็คว่าค่าถูกส่งมา เพื่อ unit test ได้ง่าย ๆ
            Entity = new MEntity()
            {
                PrimaryEmail = "name.last@gmail.com",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        var redisHelper = new Mock<IRedisHelper>();

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.GetCustomer(orgId, serial, pin, otp);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(pin, custStatus.ToString());
        var entity = Assert.IsType<MVEntityRestrictedInfo>(t);

        Assert.Equal("n*******t@gmail.com", entity.MaskingEmail);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI")]
    public void VerifyWithNoScanItemActionTest(string orgId, string serial, string pin)
    {
        var service = new Mock<IScanItemService>();

        var actionService = new Mock<IScanItemActionService>();
        // Simulate ว่าไม่มีข้อมูล action ใน DB (user ไม่เคยตั้งค่า action ไว้)
        actionService.Setup(s => s.GetScanItemAction(orgId)).Returns((MScanItemAction)null!);

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล action ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MScanItemAction>(It.IsAny<string>())).ReturnsAsync((MScanItemAction?)null);

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.Verify(orgId, serial, pin);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal("NO_SCAN_ITEM_ACTION", custStatus.ToString());
        Assert.IsType<BadRequestObjectResult>(t);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "greenlight")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "darkangle")]
    public void VerifyWithScanItemActionInCacheTest(string orgId, string serial, string pin, string themeVerify)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.VerifyScanItem(orgId, serial, pin)).Returns(new MVScanItemResult()
        {
            Status = "SUCCESS",
            ScanItem = new MScanItem()
            {
                Url = $"https://scan.its.com/verify/{orgId}/{serial}/{pin}",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        actionService.Setup(s => s.GetScanItemAction(orgId)).Returns((MScanItemAction)null!);

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่ามีข้อมูล action ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MScanItemAction>(It.IsAny<string>())).ReturnsAsync(new MScanItemAction()
        {
            OrgId = orgId,
            RedirectUrl = $"https://verify.its.com/verify",
            EncryptionKey = "1234567890123456",
            EncryptionIV = "1234567890123456",
            ThemeVerify = themeVerify,
            RegisteredAwareFlag = "YES",
        });

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.Verify(orgId, serial, pin);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal("SUCCESS", custStatus.ToString());
        var redirect = Assert.IsType<RedirectResult>(t);

        var uri = new Uri(redirect.Url!);

        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        //Console.WriteLine($"@@@@@@@@@@@@@@@@@@ [{redirect.Url}] @@@@@@@@@@@@@@@@@@@@@@");

        var theme = queryParams["theme"];
        Assert.NotNull(theme);
        Assert.NotEmpty(theme);
        Assert.Equal(theme, themeVerify);

        var org = queryParams["org"];
        Assert.NotNull(org);
        Assert.NotEmpty(org);
        //ต้อง redirect ไปที่ orgId ที่ส่งมา
        Assert.Equal(orgId, org);

        //TODO : test ต่อว่า data มีการเข้ารหัสถูกต้อง
        var data = queryParams["data"];
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "greenlight", "SUCCESS")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "darkangle", "ALREADY_REGISTERED")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", null, "SUCCESS")] //ทดสอบกรณีที่ user ไม่ได้ตั้งค่า theme มา
    public void VerifyWithScanItemActionInDbTest(string orgId, string serial, string pin, string themeVerify, string scanStatus)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.VerifyScanItem(orgId, serial, pin)).Returns(new MVScanItemResult()
        {
            Status = scanStatus,
            ScanItem = new MScanItem()
            {
                Url = $"https://scan.its.com/verify/{orgId}/{serial}/{pin}",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        actionService.Setup(s => s.GetScanItemAction(orgId)).Returns(new MScanItemAction()
        {
            OrgId = orgId,
            RedirectUrl = $"https://verify.its.com/verify",
            EncryptionKey = "1234567890123456",
            EncryptionIV = "1234567890123456",
            ThemeVerify = themeVerify,
            RegisteredAwareFlag = "YES",
        });

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล action ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MScanItemAction>(It.IsAny<string>())).ReturnsAsync((MScanItemAction?)null);

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.Verify(orgId, serial, pin);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal(scanStatus, custStatus.ToString());
        var redirect = Assert.IsType<RedirectResult>(t);

        var uri = new Uri(redirect.Url!);

        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        //Console.WriteLine($"@@@@@@@@@@@@@@@@@@ [{redirect.Url}] @@@@@@@@@@@@@@@@@@@@@@");

        var theme = queryParams["theme"];
        Assert.NotNull(theme);
        Assert.NotEmpty(theme);
        if (themeVerify == null)
        {
            //ถ้า user ไม่ได้ตั้งค่า theme มา จะต้องเป็น default
            Assert.Equal("default", theme);
        }
        else
        {
            Assert.Equal(theme, themeVerify);
        }

        var org = queryParams["org"];
        Assert.NotNull(org);
        Assert.NotEmpty(org);
        //ต้อง redirect ไปที่ orgId ที่ส่งมา
        Assert.Equal(orgId, org);

        //TODO : test ต่อว่า data มีการเข้ารหัสถูกต้อง
        var data = queryParams["data"];
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    [Theory]
    [InlineData("default", "A0000001", "EKEOSKDIDLSI", "greenlight", "SUCCESS")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", "darkangle", "ALREADY_REGISTERED")]
    [InlineData("abcdefg", "A0000002", "EKEOSK124LSI", null, "SUCCESS")] //ทดสอบกรณีที่ user ไม่ได้ตั้งค่า theme มา
    public void VerifyWithRegisteredAwareTest(string orgId, string serial, string pin, string themeVerify, string scanStatus)
    {
        var service = new Mock<IScanItemService>();
        service.Setup(s => s.VerifyScanItem(orgId, serial, pin)).Returns(new MVScanItemResult()
        {
            Status = scanStatus,
            ScanItem = new MScanItem()
            {
                Url = $"https://scan.its.com/verify/{orgId}/{serial}/{pin}",
            },
        });

        var actionService = new Mock<IScanItemActionService>();
        actionService.Setup(s => s.GetScanItemAction(orgId)).Returns(new MScanItemAction()
        {
            OrgId = orgId,
            RedirectUrl = $"https://verify.its.com/verify",
            EncryptionKey = "1234567890123456",
            EncryptionIV = "1234567890123456",
            ThemeVerify = themeVerify,
            RegisteredAwareFlag = "NO",
        });

        var redisHelper = new Mock<IRedisHelper>();
        // Simulate ว่าไม่มีข้อมูล action ใน redis
        redisHelper.Setup(s => s.GetObjectAsync<MScanItemAction>(It.IsAny<string>())).ReturnsAsync((MScanItemAction?)null);

        var vc = new VerifyScanItemController(service.Object, actionService.Object, redisHelper.Object);
        vc.ControllerContext.HttpContext = new DefaultHttpContext();
        var t = vc.Verify(orgId, serial, pin);

        var custStatus = vc.Response.Headers["CUST_STATUS"];

        Assert.NotEmpty(custStatus);
        Assert.Equal("SUCCESS", custStatus.ToString());
        var redirect = Assert.IsType<RedirectResult>(t);

        var uri = new Uri(redirect.Url!);

        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        //Console.WriteLine($"@@@@@@@@@@@@@@@@@@ [{redirect.Url}] @@@@@@@@@@@@@@@@@@@@@@");

        var theme = queryParams["theme"];
        Assert.NotNull(theme);
        Assert.NotEmpty(theme);
        if (themeVerify == null)
        {
            //ถ้า user ไม่ได้ตั้งค่า theme มา จะต้องเป็น default
            Assert.Equal("default", theme);
        }
        else
        {
            Assert.Equal(theme, themeVerify);
        }

        var org = queryParams["org"];
        Assert.NotNull(org);
        Assert.NotEmpty(org);
        //ต้อง redirect ไปที่ orgId ที่ส่งมา
        Assert.Equal(orgId, org);

        //TODO : test ต่อว่า data มีการเข้ารหัสถูกต้อง
        var data = queryParams["data"];
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }
}

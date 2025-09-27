using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

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
}

using System.Text.Json;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Services
{
    // ข้อ 3c : กดเติม point => สร้าง AccountDoc "PurchaseCash" + ขอ QR จาก payment gateway, แล้วอัปเดตสถานะตามผล
    public class PointTopUpService : IPointTopUpService
    {
        private readonly IAccountDocService _accountDocService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IPaymentGatewayRepository _paymentGatewayRepository;
        private readonly IMerchantService _merchantService;

        public PointTopUpService(
            IAccountDocService accountDocService,
            IPaymentGatewayService paymentGatewayService,
            IPaymentGatewayRepository paymentGatewayRepository,
            IMerchantService merchantService)
        {
            _accountDocService = accountDocService;
            _paymentGatewayService = paymentGatewayService;
            _paymentGatewayRepository = paymentGatewayRepository;
            _merchantService = merchantService;
        }

        public async Task<MVAccountDoc> CreatePointTopUp(string orgId, decimal amount)
        {
            if (amount <= 0)
            {
                return new MVAccountDoc { Status = "INVALID_AMOUNT", Description = "Amount must be greater than 0" };
            }

            var merchants = await _merchantService.GetMerchants(orgId, new VMMerchant { Limit = 100 });
            var merchant = merchants.FirstOrDefault(m => m.OrgId == orgId);
            if (merchant == null)
            {
                return new MVAccountDoc { Status = "MERCHANT_NOTFOUND", Description = $"Merchant not found for organization [{orgId}]" };
            }

            if (merchant.PaymentGatewayId == null)
            {
                return new MVAccountDoc { Status = "PAYMENT_GATEWAY_NOTCONFIGURED", Description = $"Point top-up payment gateway for merchant [{merchant.Code}] is not configured" };
            }

            _paymentGatewayRepository.SetCustomOrgId("global");
            var gateway = await _paymentGatewayRepository.GetPaymentGatewayById(merchant.PaymentGatewayId.ToString()!);
            if (gateway?.Id == null)
            {
                return new MVAccountDoc { Status = "PAYMENT_GATEWAY_NOTCONFIGURED", Description = $"Point top-up payment gateway for merchant [{merchant.Code}] is not configured" };
            }

            var amountDouble = (double)amount;
            var doc = new MAccountDoc
            {
                DocumentType = "PurchaseCash",
                DocumentSubType = "PointTopUp",
                Description = "เติม point",
                TotalPrice = amountDouble,
                NetAmount = amountDouble,
                TotalWithVat = amountDouble,
                GrandTotal = amountDouble,
                Items = new List<MAccountDocItem>
                {
                    new MAccountDocItem
                    {
                        ProductType = "Custom",
                        ProductDesc = "เติม point",
                        Quantity = 1,
                        UnitPrice = amountDouble,
                        TotalPrice = amountDouble,
                        NetAmount = amountDouble,
                    },
                },
            };

            var addResult = await _accountDocService.AddAccountDoc(orgId, doc);
            if (addResult.Status != "OK" || addResult.AccountDoc?.Id == null)
            {
                return addResult;
            }

            var accountDocId = addResult.AccountDoc.Id.ToString()!;

            var qrResult = await _paymentGatewayService.GenerateQR("global", gateway.Id.ToString()!, new PaymentGatewayQrRequest
            {
                Ref1 = accountDocId,
                Ref2 = "Point Purchase",
                Ref3 = orgId,
                Payer = merchant.ContactEmail,
                Amount = amount,
            });

            if (qrResult.Status != "OK")
            {
                return await _accountDocService.RejectAccountDocById(orgId, accountDocId, qrResult.Description);
            }

            // ระบุ key เป็น camelCase ตรงๆ เพราะเรียก JsonSerializer.Serialize() ตรงๆ แบบนี้ไม่ใช้ค่า camelCase ของ Program.cs (ใช้ได้แค่ตอนผ่าน MVC pipeline)
            var paramsJson = JsonSerializer.Serialize(new
            {
                qrCode = qrResult.QrCode,
                qrCodeImageBase64 = qrResult.QrCodeImageBase64,
                paymentUrl = qrResult.PaymentUrl,
                externalPaymentRequestId = qrResult.ExternalPaymentRequestId,
                generatedAmount = qrResult.GeneratedAmount,
                payinBankCode = qrResult.PayInBankCode,
                payinBankAccountNo = qrResult.PayInBankAccountNo,
                payinBankAccountName = qrResult.PayInBankAccountName,
                payinPromptPayId = qrResult.PayInPromptPayId,
                slipUploadUrl = qrResult.SlipUploadUrl,
                paymentStatusUrl = qrResult.PaymentStatusUrl,
                expireAt = qrResult.ExpireAt,
                websocketPath = qrResult.WebsocketPath,
                sessionId = qrResult.SessionId,
                gatewayBaseUrl = qrResult.GatewayBaseUrl,
            });

            return await _accountDocService.UpdateAccountDocParamsById(orgId, accountDocId, paramsJson);
        }
    }
}

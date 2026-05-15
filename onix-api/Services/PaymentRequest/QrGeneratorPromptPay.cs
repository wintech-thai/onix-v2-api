using System.Globalization;
using System.Text;
using Its.Onix.Api.Models;
using QRCoder;

namespace Its.Onix.Api.Services
{
    public class QrGeneratorPromptPay : IQrGenerator
    {
        private MPaymentRequest _pqymentRequest;
        private MBankAccount _bankAccount;

        public QrGeneratorPromptPay(MPaymentRequest pmr, MBankAccount ba)
        {
            _pqymentRequest = pmr;
            _bankAccount = ba;
        }

        private string FormatTag(string id, string value)
        {
            return $"{id}{value.Length:D2}{value}";
        }

        private string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }

        private string CalculateCRC16(string input)
        {
            ushort crc = 0xFFFF;
            byte[] bytes = Encoding.ASCII.GetBytes(input);

            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }

            return crc.ToString("X4");
        }

        private string BuildPayInAccount(string target, string proxyType)
        {
            var gui = FormatTag("00", "A000000677010111");
            var memberId = FormatTag(proxyType, target);

            return gui + memberId;
        }

        private (string,string) FormatTarget(string targetId)
        {
            var digits = targetId.Replace("-", "").Trim();
            var target = "00";

            if (digits.StartsWith("0") && digits.Length == 10)
            {
                // Mobile
                digits = "0066" + digits.Substring(1);
                target = "01";
            }
            else if (digits.Length == 13)
            {
                // Tax ID
                target = "02";
            }
            else if (digits.Length == 15)
            {
                // E-Wallet
                target = "03";
            }

            return (digits, target);
        }

        private string GenerateQrBarcode(QrGeneratorPayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.TargetId))
                throw new ArgumentException("TargetId is required.");

            var (target, proxyType) = FormatTarget(payload.TargetId);

            var sb = new StringBuilder();

            sb.Append("000201"); // Payload Format Indicator
            sb.Append("010212"); // Dynamic QR

            // Merchant Account Info
            var merchantAccount = BuildPayInAccount(target, proxyType);
            sb.Append(FormatTag("29", merchantAccount));

            sb.Append(FormatTag("52", payload.MerchantCategoryCode));
            sb.Append(FormatTag("53", payload.CurrencyCode));

            if (payload.Amount.HasValue)
            {
                sb.Append(FormatTag("54",
                    payload.Amount.Value.ToString("0.00", CultureInfo.InvariantCulture)));
            }

            sb.Append(FormatTag("58", payload.CountryCode));
            sb.Append(FormatTag("59",
                Truncate(payload.AccountName ?? "PROMPTPAY", 25)));
            sb.Append(FormatTag("60", "Bangkok"));

            // Additional Data
            if (!string.IsNullOrWhiteSpace(payload.Reference1))
            {
                var additional = FormatTag("01", payload.Reference1);

                if (!string.IsNullOrWhiteSpace(payload.Reference2))
                    additional += FormatTag("02", payload.Reference2);

                sb.Append(FormatTag("62", additional));
            }

            // CRC placeholder
            sb.Append("6304");

            var rawPayload = sb.ToString();
            var crc = CalculateCRC16(rawPayload);

            return rawPayload + crc;
        }

        private QrGeneratorPayload GeneratePayload()
        {
Console.WriteLine($"DEBUG A targetId=[{_bankAccount.PromptPayId}]"); 
            var result = new QrGeneratorPayload()
            {
                TargetId = _bankAccount.PromptPayId!,
                Amount = _pqymentRequest.GeneratedAmount,
                Reference1 = _pqymentRequest.RefId,
                Reference2 = "",
                AccountName = _bankAccount.AccountName, //ต้องเป็นภาษาอังกฤษ (ถ้าเป็นภาษาไทยต้องหาวิธีคำนวณ string lenght อีกที)
            };
            
            return result;
        }

        public QrGeneratorResult Generate()
        {
            var payload = GeneratePayload();
            var qrBarcodeStr = GenerateQrBarcode(payload);

            // สร้าง QR image bytes จาก payload string
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrBarcodeStr, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrData);

            byte[] imageBytes = qrCode.GetGraphic(20); // ขนาดภาพ

            var result = new QrGeneratorResult()
            {
                QrPayload = qrBarcodeStr,
                ImageBytes = imageBytes
            };

            return result;
        }
    }
}

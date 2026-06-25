using System.Globalization;
using System.Text;
using Its.Onix.Api.Models;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class QrGeneratorSCB : IQrGenerator
    {
        private readonly MPaymentRequest _pqymentRequest;
        private readonly MBankAccount _bankAccount;
        private readonly IRedisHelper _redis;

        public QrGeneratorSCB(MPaymentRequest pmr, MBankAccount ba, IRedisHelper redis)
        {
            _pqymentRequest = pmr;
            _bankAccount = ba;
            _redis = redis;
        }

        public async Task<QrGeneratorResult> GenerateAsync()
        {
            var result = new QrGeneratorResult()
            {
                Status = "OK",
                Description = "Success",
            };

            //TODO : Implement here
            //1. ให้มี config ว่า SCB ตัว Sandbox กับ Prod จะต้องยิงไปคนละ domain, แต่ API path มันจะเหมือนกัน
            //2. เลือกใช้ว่าจะยิงไปที่ sandbox หรือ prod ขึ้นอยู่กับ IsSandbox ใน _bankAccount.BankConfigObj
            
            //3. ให้เก็บ JWT token ของ SCB ไว้ที่ Redis โดยที่เอา BankAccountId มาเป็น cache key ด้วย
            //ตัวอย่าง -->  var cacheKey1 = CacheHelper.CreateBankApiTokenKey(orgId, "SCBToken");
            //ตัวอย่าง -->   _ = _redis.SetObjectAsync($"{cacheKey1}:{_bankAccount.Id.ToString()}", jwt, TimeSpan.FromMinutes(30));
            //สาเหตุที่ให้กับใน Redis เพราะว่า อาจจะมี request จากการเรียก API ตัวเดียวกันนี้มาจากที่อื่น ๆ ด้วย ก็จะได้แชร์ JWT ตรงนี้ร่วมกัน ไม่ต้องยิงไป authen
            //กับ SCB บ่อย ๆ จนติด rate limit

            //4. ทุก ๆ การเรียก API ให้ดึง JWT มาจาก Redis ก่อน แล้วดูว่า expire หรือยัง เช่นถ้าอีก 5 วินาที expires ก็ถือว่าให้ไป refresh token มาใหม่เลยก็ได้
            //  ถ้ายังไม่ expire ก็เอา JWT นั้นไปใช้เลย
            //  ถ้า expire แล้วก็ไป refresh มาใหม่ แล้วเก็บตัวใหม่ไว้ใน Redis ด้วย
            //  เวลายิง rest API ไปหา SCB ให้ใช้วิธีการยิง http แบบ Async (มีการใช้ await ด้วยนะตอนยิงไปที่ SCB) นะจะได้ไม่ block รอ

            //5. การจะได้มาซึ่ง JWT token ก็ต้องไปเรียก /v1/oauth/token ของ SCB ก่อน โดยใช้ SCBO API key มาจาก _bankAccount.BankConfigObj นั่นแหละ
            //   ไม่แน่ใจว่าต้องใช้ API secret ด้วยมั้ย ในตอนที่ยิง API ไปหา SCB

            //6. เลือก data ที่ได้จาก SCB เอาไปใส่ใน QrGeneratorResult แล้ว return ออกไป

            //7. พวก error ต่าง ๆ จากการเรียก SCB ควรต้องใส่ใน QrGeneratorResult.Status และ QrGeneratorResult.Description ด้วยเพื่อ response กลับไป

            //เอาโค้ดตรงนี้ออกด้วยเวลา implement logic แล้วนะ, ตรงนี้ใส่ไว้ไม่ให้ compiler มัน warning น่ะ
            await Task.CompletedTask;

            return result;
        }

        public QrGeneratorResult Generate()
        {
            throw new NotImplementedException();
        }
    }
}

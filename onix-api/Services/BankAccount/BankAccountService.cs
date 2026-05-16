using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class BankAccountService : BaseService, IBankAccountService
    {
        private readonly IBankAccountRepository? repository = null;
        private readonly List<MBank> _banks;

        public BankAccountService(IBankAccountRepository repo) : base()
        {
            repository = repo;
            _banks = [
                new() 
                { 
                    BankCode = "PP", 
                    BankNameTh = "บริการพร้อมเพย์", 
                    BankNameEng = "Prompt Pay",
                    Type = "PromptPay",
                    QrSupportFlag = true,
                },

                new() 
                { 
                    BankCode = "BAY", 
                    BankNameTh = "ธนาคารกรุงศรีอยุธยา", 
                    BankNameEng = "Bank of Ayudhya",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new () 
                { 
                    BankCode = "KBANK", 
                    BankNameTh = "ธนาคารกสิกรไทย", 
                    BankNameEng = "Kasikorn Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "KTB",
                    BankNameTh = "ธนาคารกรุงไทย",
                    BankNameEng = "Krung Thai Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "SCB", 
                    BankNameTh = "ธนาคารไทยพาณิชย์", 
                    BankNameEng = "Siam Commercial Bank",
                    Type = "Native",
                    QrSupportFlag = false,  
                },

                new ()
                { 
                    BankCode = "BBL", 
                    BankNameTh = "ธนาคารกรุงเทพ", 
                    BankNameEng = "Bank of Bangkok",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new () 
                { 
                    BankCode = "TMB", 
                    BankNameTh = "ธนาคารทหารไทย", 
                    BankNameEng = "Bank of Thailand",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "GSB", 
                    BankNameTh = "ธนาคารออมสิน", 
                    BankNameEng = "Government Savings Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                {
                    BankCode = "UOB", 
                    BankNameTh = "ธนาคารยูโอบี", 
                    BankNameEng = "United Overseas Bank",
                    Type = "Native",
                    QrSupportFlag = false,                
                },

                new ()
                { 
                    BankCode = "CIMBT", 
                    BankNameTh = "ธนาคารซีไอเอ็มบี ไทย", 
                    BankNameEng = "Citibank (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false, 
                },

                new ()
                {
                    BankCode = "SCBT", 
                    BankNameTh = "ธนาคารสแตนดาร์ดชาร์เตอร์ด (ไทย)", 
                    BankNameEng = "Standard Chartered Bank (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "TISCO", 
                    BankNameTh = "ธนาคารทิสโก้", 
                    BankNameEng = "Tisco Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "LHFG", 
                    BankNameTh = "ธนาคารแลนด์ แอนด์ เฮ้าส์", 
                    BankNameEng = "Land and Houses Bank",
                    Type = "Native",
                    QrSupportFlag = false,
                },
                
                new ()
                {
                    BankCode = "ICBC",
                    BankNameTh = "ธนาคารไอซีบีซี (ไทย)", 
                    BankNameEng = "Industrial and Commercial Bank of China (Thailand)",
                    Type = "Native",
                    QrSupportFlag = false,
                },

                new ()
                { 
                    BankCode = "CITI", 
                    BankNameTh = "ธนาคารซิตี้แบงก์", 
                    BankNameEng = "Citibank",
                    Type = "Native",
                    QrSupportFlag = false,  
                },

                new ()
                { 
                    BankCode = "EXIM", 
                    BankNameTh = "ธนาคารเพื่อการส่งออกและนำเข้าแห่งประเทศไทย", 
                    BankNameEng = "Export-Import_BANK_OF_THAILAND", 
                    Type = "Native",
                    QrSupportFlag = false,  
                }
            ];
        }

        public async Task<MVBankAccount> GetBankAccountById(string orgId, string bankAccountId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            var result = await repository!.GetBankAccountById(bankAccountId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            r.BankAccount = result;

            return r;
        }

        public async Task<MVBankAccount> AddBankAccount(string orgId, MBankAccount bankAccount)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(bankAccount.BankCode))
            {
                r.Status = "BANK_CODE_MISSING";
                r.Description = $"Bank Code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(bankAccount.AccountName))
            {
                r.Status = "ACCOUNT_NAME_MISSING";
                r.Description = $"Bank Account name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(bankAccount.AccountNumber))
            {
                r.Status = "ACCOUNT_NUMBER_MISSING";
                r.Description = $"Bank Account number is missing!!!";

                return r;
            }

            var isAccountNoExist = await repository!.IsBankAccountNoExist(bankAccount.AccountNumber);
            if (isAccountNoExist)
            {
                r.Status = "ACCOUNT_NUMBER_DUPLICATE";
                r.Description = $"Bank Account number [{bankAccount.AccountNumber}] already exist!!!";

                return r;
            }

            var isNameExist = await repository!.IsBankAccountNameExist(bankAccount.BankCode, bankAccount.AccountName);
            if (isNameExist)
            {
                r.Status = "ACCOUNT_NAME_DUPLICATE";
                r.Description = $"Bank Account name [{bankAccount.AccountName}] already exist!!!";

                return r;
            }

            bankAccount.Status = "Pending";
            var result = await repository!.AddBankAccount(bankAccount);

            r.BankAccount = result;

            return r;
        }

        public async Task<MVBankAccount> DeleteBankAccountById(string orgId, string bankAccountId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteBankAccountById(bankAccountId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            r.BankAccount = m;
            return r;
        }

        public async Task<List<MBankAccount>> GetBankAccounts(string orgId, VMBankAccount param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetBankAccounts(param);

            return result;
        }

        public async Task<int> GetBankAccountCount(string orgId, VMBankAccount param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetBankAccountCount(param);

            return result;
        }

        public async Task<MVBankAccount> UpdateBankAccountById(string orgId, string bankAccountId, MBankAccount bankAccount)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(bankAccount.BankCode))
            {
                r.Status = "BANK_CODE_MISSING";
                r.Description = $"Bank Code is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(bankAccount.AccountName))
            {
                r.Status = "ACCOUNT_NAME_MISSING";
                r.Description = $"Bank Account name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(bankAccount.AccountNumber))
            {
                r.Status = "ACCOUNT_NUMBER_MISSING";
                r.Description = $"Bank Account number is missing!!!";

                return r;
            }

            var code = bankAccount.AccountNumber;
            var br1 = await repository!.GetBankAccountByAccountNo(code!);
            if ((br1 != null) && (br1.Id.ToString() != bankAccountId))
            {
                r.Status = "BANK_ACCOUNT_NUMBER_DUPLICATE";
                r.Description = $"Bank Account number [{code}] already exist!!!";

                return r;
            }

            var br2 = await repository!.GetBankAccountByAccountName(bankAccount.BankCode!, bankAccount.AccountName!);
            if ((br2 != null) && (br2.Id.ToString() != bankAccountId))
            {
                r.Status = "BANK_ACCOUNT_NAME_DUPLICATE";
                r.Description = $"Bank Account name [{bankAccount.AccountName}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateBankAccountById(bankAccountId, bankAccount);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] not found for the organization [{orgId}]";

                return r;
            }

            r.BankAccount = result;

            return r;
        }

        public async Task<MVBankAccount?> UpdateBankAccountStatusById(string bankAccountId, string status)
        {
            var r = new MVBankAccount()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdateBankAccountStatusById(bankAccountId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] not found";

                return r;
            }

            r.BankAccount = result;

            return r;
        }

        public List<MBank> GetAvailableBanks()
        {
            var banks = _banks
                .Where(b => b.Type.Equals("Native", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return banks;
        }

        public List<MBank> GetAvailableSupportQrBanks()
        {
            var banks = _banks
                .Where(b => b.Type.Equals("PromptPay", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return banks;
        }

        public async Task<List<MBankAccountMerchant>> GetMerchantsForBankAccount(string orgId, string bankAccountId)
        {
            repository!.SetCustomOrgId(orgId);

            var result = await repository.GetMerchantsForBankAccount(bankAccountId);
            return result;
        }

        public async Task<List<MBankAccountMerchant>> GetPayInBankAccountsForMerchant(string orgId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);

            var result = await repository.GetPayInBankAccountsForMerchant(merchantId);
            return result;
        }

        public async Task<MVBankAccountMerchant?> SelectMerchant(string orgId, string bankAccountId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVBankAccountMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            var result = await repository.SelectMerchant(bankAccountId, merchantId);
            if (result == null)            
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] or Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }
            
            r.BankAccountMerchant = result;
            return r;
        }

        public async Task<MVBankAccountMerchant?> UnSelectMerchant(string orgId,string bankAccountId, string merchantId)
        {
            repository!.SetCustomOrgId(orgId);
            var r = new MVBankAccountMerchant()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(bankAccountId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Bank Account ID [{bankAccountId}] format is invalid";

                return r;
            }

            var result = await repository.UnSelectMerchant(bankAccountId, merchantId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Bank Account ID [{bankAccountId}] or Merchant ID [{merchantId}] not found for the organization [{orgId}]";

                return r;
            }

            r.BankAccountMerchant = result;
            return r;
        }
    }
}

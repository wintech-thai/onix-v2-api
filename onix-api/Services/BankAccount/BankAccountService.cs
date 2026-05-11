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

        public BankAccountService(IBankAccountRepository repo) : base()
        {
            repository = repo;
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

            var isNoExist = await repository!.IsBankAccountNoExist(bankAccount.AccountNumber);
            if (!isNoExist)
            {
                r.Status = "ACCOUNT_NUMBER_DUPLICATE";
                r.Description = $"Bank Account number [{bankAccount.AccountNumber}] already exist!!!";

                return r;
            }

            var isNameExist = await repository!.IsBankAccountNameExist(bankAccount.BankCode, bankAccount.AccountName);
            if (!isNameExist)
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
            var banks = new List<MBank>()
            {
                new MBank() { BankCode = "BAY", BankNameTh = "ธนาคารกรุงศรีอยุธยา", BankNameEng = "Bank of Ayudhya" },
                new MBank() { BankCode = "KBANK", BankNameTh = "ธนาคารกสิกรไทย", BankNameEng = "Kasikorn Bank" },
                new MBank() { BankCode = "KTB", BankNameTh = "ธนาคารกรุงไทย", BankNameEng = "Krung Thai Bank" },
                new MBank() { BankCode = "SCB", BankNameTh = "ธนาคารไทยพาณิชย์", BankNameEng = "Siam Commercial Bank" },
                new MBank() { BankCode = "BBL", BankNameTh = "ธนาคารกรุงเทพ", BankNameEng = "Bank of Bangkok" },
                new MBank() { BankCode = "TMB", BankNameTh = "ธนาคารทหารไทย", BankNameEng = "Bank of Thailand" },
                new MBank() { BankCode = "GSB", BankNameTh = "ธนาคารออมสิน", BankNameEng = "Government Savings Bank" },
                new MBank() { BankCode = "UOB", BankNameTh = "ธนาคารยูโอบี", BankNameEng = "United Overseas Bank" },
                new MBank() { BankCode = "CIMBT", BankNameTh = "ธนาคารซีไอเอ็มบี ไทย", BankNameEng = "Citibank (Thailand)" },
                new MBank() { BankCode = "SCBT", BankNameTh = "ธนาคารสแตนดาร์ดชาร์เตอร์ด (ไทย)", BankNameEng = "Standard Chartered Bank (Thailand)" },
                new MBank() { BankCode = "TISCO", BankNameTh = "ธนาคารทิสโก้", BankNameEng = "Tisco Bank" },
                new MBank() { BankCode = "LHFG", BankNameTh = "ธนาคารแลนด์ แอนด์ เฮ้าส์", BankNameEng = "Land and Houses Bank" },
                new MBank() { BankCode = "ICBC", BankNameTh = "ธนาคารไอซีบีซี (ไทย)", BankNameEng = "Industrial and Commercial Bank of China (Thailand)" },
                new MBank() { BankCode = "CITI", BankNameTh = "ธนาคารซิตี้แบงก์", BankNameEng = "Citibank" },
                new MBank() { BankCode = "EXIM", BankNameTh = "ธนาคารเพื่อการส่งออกและนำเข้าแห่งประเทศไทย", BankNameEng = "Export-Import_BANK_OF_THAILAND" }
            };

            return banks;
        }
    }
}

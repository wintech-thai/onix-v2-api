using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PointService : BaseService, IPointService
    {
        private readonly IPointRepository repository = null!;
        private readonly IEntityRepository _entityRepo;

        public PointService(
            IPointRepository repo,
            IEntityRepository entityRepo) : base()
        {
            repository = repo;
            _entityRepo = entityRepo;
        }

        private async Task<MPointBalance> GetCurrentBalance(string walletId)
        {
            var param = new VMPointBalance()
            {
                WalletId = walletId,
                BalanceType = "PointBalanceCurrent",
            };

            var bal = await repository.GetPointBalanceByWalletId(param);

            if (bal == null)
            {
                bal = new MPointBalance()
                {
                    StatCode = param.BalanceType,
                    WalletId = walletId,
                    BalanceDate = DateTime.UtcNow,
                    BalanceDateKey = "000000",
                    TxIn = 0,
                    TxOut = 0,
                    BalanceBegin = 0,
                    BalanceEnd = 0,

                    TxInDecimal = 0,
                    TxOutDecimal = 0,
                    BalanceBeginDecimal = 0,
                    BalanceEndDecimal = 0,

                    IsNew = true,
                };
            }
            else
            {
                bal.IsNew = false;
            }

            return bal;
        }

        private async Task<MPointBalance> GetDailyBalance(string walletId, long? currentAmt, decimal? currentAmtDecimal)
        {
            var amt = currentAmt;
            if (amt == null)
            {
                amt = 0;
            }

            var amtDecimal = currentAmtDecimal;
            if (amtDecimal == null)
            {
                amtDecimal = 0;
            }

            var dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            var param = new VMPointBalance()
            {
                WalletId = walletId,
                BalanceType = "PointBalanceDaily",
                DateKey = dateString,
            };

            var bal = await repository.GetPointBalanceByWalletId(param);

            if (bal == null)
            {
                bal = new MPointBalance()
                {
                    StatCode = param.BalanceType,
                    WalletId = walletId,
                    BalanceDate = DateTime.UtcNow,
                    BalanceDateKey = param.DateKey,
                    TxIn = 0,
                    TxOut = 0,
                    BalanceBegin = amt,
                    BalanceEnd = amt,

                    TxInDecimal = 0,
                    TxOutDecimal = 0,
                    BalanceBeginDecimal = amtDecimal,
                    BalanceEndDecimal = amtDecimal,

                    IsNew = true,
                };
            }
            else
            {
                bal.IsNew = false;    
            }

            return bal;
        }

        public async Task<MVPointTx> AddPoint(string orgId, MPointTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (tx.TxAmount == null || tx.TxAmount < 0)
            {
                r.Status = "INVALID_TX_AMOUNT";
                r.Description = "TxAmount must be greater than 0 for point addition!!!";

                return r;
            }

            if (tx.TxAmountDecimal == null || tx.TxAmountDecimal < 0)
            {
                r.Status = "INVALID_TX_AMOUNT_DECIMAL";
                r.Description = "TxAmount must be greater than 0 for point addition!!!";

                return r;
            }


            var txAmt = tx.TxAmount;
            var txAmtDecimal = tx.TxAmountDecimal;
            var currBal = await GetCurrentBalance(tx.WalletId!);

            var previousBal = currBal.BalanceEnd;
            var previousBalDecimal = currBal.BalanceEndDecimal;

            //เอา balance ปัจจุบันมาก่อน
            var dailyBal = await GetDailyBalance(tx.WalletId!, previousBal, previousBalDecimal);

            // Legacy type Long
            tx.PreviousBalance = previousBal;
            currBal.TxIn = currBal.TxIn + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd + txAmt;
            currBal.BalanceDate = DateTime.UtcNow;

            dailyBal.TxIn = dailyBal.TxIn + txAmt;
            dailyBal.BalanceEnd = dailyBal.BalanceEnd + txAmt;
            dailyBal.BalanceDate = DateTime.UtcNow;

            // New type Decimal
            tx.PreviousBalanceDecimal = previousBalDecimal;
            currBal.TxInDecimal = currBal.TxInDecimal + txAmtDecimal;
            currBal.BalanceEndDecimal = currBal.BalanceEndDecimal + txAmtDecimal;
            currBal.BalanceDate = DateTime.UtcNow;
            
            dailyBal.TxInDecimal = dailyBal.TxInDecimal + txAmtDecimal;
            dailyBal.BalanceEndDecimal = dailyBal.BalanceEndDecimal + txAmtDecimal;
            dailyBal.BalanceDate = DateTime.UtcNow;


            tx.TxType = 1;

            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            tx.CurrentBalanceDecimal = currBal.BalanceEndDecimal;
            var result = await repository!.AddPointTxWithBalance(tx, currBal, dailyBal);

            r.Status = "OK";
            r.Description = "Success";
            r.PointTx = result;

            return r;
        }

        public async Task<MVPointTx> DeductPoint(string orgId, MPointTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (tx.TxAmount == null || tx.TxAmount < 0)
            {
                r.Status = "INVALID_TX_AMOUNT";
                r.Description = "TxAmount must be greater than 0 for point deduction!!!";

                return r;
            }

            if (tx.TxAmountDecimal == null || tx.TxAmountDecimal < 0)
            {
                r.Status = "INVALID_TX_AMOUNT_DECIMAL";
                r.Description = "TxAmountDecimal must be greater than 0 for point deduction!!!";

                return r;
            }


            var txAmt = tx.TxAmount;
            var txAmtDecimal = tx.TxAmountDecimal;
            var currBal = await GetCurrentBalance(tx.WalletId!);

            //เอา balance ปัจจุบันมาก่อน
            var previousBal = currBal.BalanceEnd;
            var previousBalDecimal = currBal.BalanceEndDecimal;
            var dailyBal = await GetDailyBalance(tx.WalletId!, previousBal, previousBalDecimal);

            tx.PreviousBalance = previousBal;
            tx.PreviousBalanceDecimal = previousBalDecimal;
            currBal.BalanceDate = DateTime.UtcNow;
            dailyBal.BalanceDate = DateTime.UtcNow;

            //Legacy type Long
            currBal.TxOut = currBal.TxOut + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd - txAmt;
            if (currBal.BalanceEnd < 0)
            {
                r.Status = "INVALID_BALANCE_LESS_THAN_ZERO";
                r.Description = "Balance cannot be less than 0!!!";
                return r;
            }
            dailyBal.TxOut = dailyBal.TxOut + txAmt;
            dailyBal.BalanceEnd = dailyBal.BalanceEnd - txAmt;
            

            //New type Decimal
            currBal.TxOutDecimal = currBal.TxOutDecimal + txAmtDecimal;
            currBal.BalanceEndDecimal = currBal.BalanceEndDecimal - txAmtDecimal;
            if (currBal.BalanceEndDecimal < 0)
            {
                r.Status = "INVALID_BALANCE_LESS_THAN_ZERO_DECIMAL";
                r.Description = "Balance (decimal) cannot be less than 0!!!";
                return r;
            }
            dailyBal.TxOutDecimal = dailyBal.TxOutDecimal + txAmtDecimal;
            dailyBal.BalanceEndDecimal = dailyBal.BalanceEndDecimal - txAmtDecimal;


            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            tx.CurrentBalanceDecimal = currBal.BalanceEndDecimal;
            tx.TxType = -1;
            var result = await repository!.AddPointTxWithBalance(tx, currBal, dailyBal);

            r.Status = "OK";
            r.Description = "Success";
            r.PointTx = result;

            return r;
        }

        // ─── V2 : Inventory (quantity + ราคาเฉลี่ยถ่วงน้ำหนัก) ─────────────────────────

        public async Task<MWallet> GetOrCreateInventoryWallet(string orgId, string locationId, string itemId)
        {
            repository!.SetCustomOrgId(orgId);

            var wallet = await repository!.GetWalletByLocationItemId(locationId, itemId);
            if (wallet != null)
            {
                return wallet;
            }

            var newWallet = new MWallet()
            {
                OrgId = orgId,
                WalletType = "InventoryItem",
                InventoryLocationId = locationId,
                InventoryItemId = itemId,
                Name = $"Inventory-{locationId}-{itemId}",
                PointBalanceDecimal = 0,
                PointQuantityBalanceDecimal = 0,
                PointAveragePriceDecimal = 0,
            };

            return await repository!.AddWallet(newWallet);
        }

        public async Task<MWallet?> GetInventoryWalletByLocationItemId(string orgId, string locationId, string itemId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetWalletByLocationItemId(locationId, itemId);
        }

        public async Task<MVPointTx> AddPointV2(string orgId, string locationId, string itemId, decimal quantity, decimal amount, string? tags, string? description)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (quantity <= 0)
            {
                r.Status = "INVALID_QUANTITY";
                r.Description = "Quantity must be greater than 0 for inventory add!!!";

                return r;
            }

            if (amount < 0)
            {
                r.Status = "INVALID_TX_AMOUNT_DECIMAL";
                r.Description = "Amount must not be negative!!!";

                return r;
            }

            var wallet = await GetOrCreateInventoryWallet(orgId, locationId, itemId);
            var walletId = wallet.Id.ToString()!;

            var currBal = await GetCurrentBalance(walletId);
            var previousBalDecimal = currBal.BalanceEndDecimal ?? 0;
            var dailyBal = await GetDailyBalance(walletId, currBal.BalanceEnd, previousBalDecimal);

            var previousQty = wallet.PointQuantityBalanceDecimal ?? 0;
            var previousAvg = wallet.PointAveragePriceDecimal ?? 0;

            var newQty = previousQty + quantity;
            var newValueDecimal = previousBalDecimal + amount;
            var newAvg = newQty == 0 ? 0 : newValueDecimal / newQty;

            currBal.TxInDecimal = (currBal.TxInDecimal ?? 0) + amount;
            currBal.BalanceEndDecimal = newValueDecimal;
            currBal.BalanceDate = DateTime.UtcNow;

            dailyBal.TxInDecimal = (dailyBal.TxInDecimal ?? 0) + amount;
            dailyBal.BalanceEndDecimal = (dailyBal.BalanceEndDecimal ?? 0) + amount;
            dailyBal.BalanceDate = DateTime.UtcNow;

            var tx = new MPointTx()
            {
                WalletId = walletId,
                Tags = tags,
                Description = description,
                TxAmountDecimal = amount,
                TxQuantityDecimal = quantity,
                TxAveragePriceDecimal = quantity == 0 ? 0 : amount / quantity,
                TxType = 1,
                PreviousBalanceDecimal = previousBalDecimal,
                PreviousBalanceQuantityDecimal = previousQty,
                PreviousAveragePriceDecimal = previousAvg,
                CurrentBalanceDecimal = newValueDecimal,
                CurrentBalanceQuantityDecimal = newQty,
                CurrentAveragePriceDecimal = newAvg,
            };

            var result = await repository!.AddPointTxWithBalanceV2(tx, currBal, dailyBal, newQty, newAvg);

            r.PointTx = result;

            return r;
        }

        public async Task<MVPointTx> DeductPointV2(string orgId, string locationId, string itemId, decimal quantity, string? tags, string? description)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointTx()
            {
                Status = "OK",
                Description = "Success",
            };

            if (quantity <= 0)
            {
                r.Status = "INVALID_QUANTITY";
                r.Description = "Quantity must be greater than 0 for inventory deduction!!!";

                return r;
            }

            var wallet = await repository!.GetWalletByLocationItemId(locationId, itemId);
            if (wallet == null)
            {
                r.Status = "WALLET_NOTFOUND";
                r.Description = $"No inventory balance found for location [{locationId}], item [{itemId}]";

                return r;
            }

            var walletId = wallet.Id.ToString()!;
            var previousQty = wallet.PointQuantityBalanceDecimal ?? 0;
            var previousAvg = wallet.PointAveragePriceDecimal ?? 0;

            var newQty = previousQty - quantity;
            if (newQty < 0)
            {
                r.Status = "INVALID_BALANCE_LESS_THAN_ZERO";
                r.Description = $"Insufficient quantity balance: available [{previousQty}], requested [{quantity}]";

                return r;
            }

            //ราคาเฉลี่ยของที่เบิก คือราคาเฉลี่ย ณ ตอนนั้น (ไม่เปลี่ยนหลังเบิก)
            var deductAmount = quantity * previousAvg;

            var currBal = await GetCurrentBalance(walletId);
            var previousBalDecimal = currBal.BalanceEndDecimal ?? 0;
            var dailyBal = await GetDailyBalance(walletId, currBal.BalanceEnd, previousBalDecimal);

            var newValueDecimal = previousBalDecimal - deductAmount;
            if (newValueDecimal < 0)
            {
                newValueDecimal = 0; //กันเศษปัดเล็กน้อยติดลบ
            }

            currBal.TxOutDecimal = (currBal.TxOutDecimal ?? 0) + deductAmount;
            currBal.BalanceEndDecimal = newValueDecimal;
            currBal.BalanceDate = DateTime.UtcNow;

            dailyBal.TxOutDecimal = (dailyBal.TxOutDecimal ?? 0) + deductAmount;
            dailyBal.BalanceEndDecimal = (dailyBal.BalanceEndDecimal ?? 0) - deductAmount;
            dailyBal.BalanceDate = DateTime.UtcNow;

            var tx = new MPointTx()
            {
                WalletId = walletId,
                Tags = tags,
                Description = description,
                TxAmountDecimal = deductAmount,
                TxQuantityDecimal = quantity,
                TxAveragePriceDecimal = previousAvg,
                TxType = -1,
                PreviousBalanceDecimal = previousBalDecimal,
                PreviousBalanceQuantityDecimal = previousQty,
                PreviousAveragePriceDecimal = previousAvg,
                CurrentBalanceDecimal = newValueDecimal,
                CurrentBalanceQuantityDecimal = newQty,
                CurrentAveragePriceDecimal = previousAvg,
            };

            var result = await repository!.AddPointTxWithBalanceV2(tx, currBal, dailyBal, newQty, previousAvg);

            r.PointTx = result;

            return r;
        }

        public async Task<List<MPointTx>> GetPointTxsByLocationItemId(string orgId, string locationId, string itemId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);

            var wallet = await repository!.GetWalletByLocationItemId(locationId, itemId);
            if (wallet == null)
            {
                return [];
            }

            param.WalletId = wallet.Id.ToString();
            var pointTxs = await repository.GetPointTxsByWalletId(param);

            return pointTxs;
        }

        public async Task<List<MWallet>> GetWalletsByLocationId(string orgId, string locationId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetWalletsByLocationId(locationId);
        }

        public async Task<List<MWallet>> GetWalletsByItemId(string orgId, string itemId)
        {
            repository!.SetCustomOrgId(orgId);
            return await repository!.GetWalletsByItemId(itemId);
        }

        public async Task<List<MPointTx>> GetPointTxsByCustomerId(string orgId, string custId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);

            var wallet = await repository!.GetWalletByCustomerId(custId);
            if (wallet == null)
            {
                return [];
            }

            param.WalletId = wallet.Id.ToString();
            var pointTxs = await repository.GetPointTxsByWalletId(param);

            return pointTxs;
        }

        public async Task<List<MPointTx>> GetPointTxsByWalletId(string orgId, VMPointTx param)
        {

            repository.SetCustomOrgId(orgId);
            var result = await repository.GetPointTxsByWalletId(param);
            return result;
        }

        public async Task<int> GetPointTxsCountByWalletId(string orgId, VMPointTx param)
        {
            repository.SetCustomOrgId(orgId);
            var result = await repository.GetPointTxsCountByWalletId(param);
            return result;
        }

        public async Task<MVPointBalance?> GetPointBalanceByWalletId(string orgId, VMPointBalance param)
        {
            repository.SetCustomOrgId(orgId);

            var r = new MVPointBalance()
            {
                Status = "OK",
                Description = "Success",
            };

            var balanceType = param.BalanceType;
            var dateKey = param.DateKey;

            if (string.IsNullOrEmpty(balanceType))
            {
                r.Status = "BALANCE_TYPE_MISSING";
                r.Description = "BalanceType need to be PointBalanceDaily or PointBalanceCurrent";

                return r;
            }

            if (balanceType == "PointBalanceDaily")
            {
                if (string.IsNullOrEmpty(dateKey))
                {
                    r.Status = "BALANCE_DATEKEY_MISSING";
                    r.Description = "DateKey is empty!!!";

                    return r;
                }
            }

            var result = await repository.GetPointBalanceByWalletId(param);
            r.PointBalance = result;

            return r;
        }

        public async Task<MVWallet> AddWallet(string orgId, MWallet wallet)
        {
            var r = new MVWallet
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            if (string.IsNullOrEmpty(wallet.Name))
            {
                r.Status = "INVALID_WALLET_NAME";
                r.Description = "Wallet name must not be blank!!!";

                return r;
            }

            var result = await repository!.AddWallet(wallet);
            r.Wallet = result;

            return r;
        }

        public async Task<List<MWallet>> GetWallets(string orgId, VMWallet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWallets(param);

            return result;
        }

        public async Task<int> GetWalletsCount(string orgId, VMWallet param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletsCount(param);

            return result;
        }

        public async Task<MVWallet?> GetWalletByCustomerId(string orgId, string customerId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(customerId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Customer ID [{customerId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletByCustomerId(customerId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet not found for customer ID [{customerId}], organization [{orgId}]";
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> GetWalletByBankAccountId(string orgId, string bankAccountId)
        {
            var r = new MVWallet()
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

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletByBankAccountId(bankAccountId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet not found for Bank Account ID [{bankAccountId}], organization [{orgId}]";
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> GetWalletByMerchantId(string orgId, string merchantId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(merchantId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Merchant ID [{merchantId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletByMerchantId(merchantId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet not found for merchant ID [{merchantId}], organization [{orgId}]";
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> GetWalletByRefIdAndType(string orgId, string refId, string walletType)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(refId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Ref ID [{refId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletByRefIdAndType(refId, walletType);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet not found for Ref ID [{refId}], Wallet Type [{walletType}], organization [{orgId}]";
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> GetWalletById(string orgId, string walletId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetWalletById(walletId);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";
            }

            r.Wallet = result;

            return r;
        }

        public async Task<MVWallet?> UpdateWalletById(string orgId, string walletId, MWallet wallet)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVWallet()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(wallet.Name))
            {
                r.Status = "INVALID_WALLET_NAME";
                r.Description = "Wallet name must not be blank!!!";

                return r;
            }

            var result = await repository!.UpdateWalletById(walletId, wallet);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Wallet = result;
            return r;
        }

        public async Task<MVWallet?> AttachCustomerToWalletById(string orgId, string walletId, string custId)
        {
            repository!.SetCustomOrgId(orgId);
            _entityRepo.SetCustomOrgId(orgId);

            var r = new MVWallet()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            if (!ServiceUtils.IsGuidValid(custId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Customer ID [{custId}] format is invalid";

                return r;
            }

            var customer = _entityRepo.GetEntityById(custId!);
            if (customer == null)
            {
                r.Status = "CUSTOMER_NOTFOUND";
                r.Description = $"Customer ID [{custId}] not found!!!";

                return r;
            }

            var result = await repository!.AttachCustomerToWalletById(walletId, custId, customer);
            r.Wallet = result;

            return r;
        }

        public async Task<MVWallet?> DeleteWalletById(string orgId, string walletId)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(walletId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Wallet ID [{walletId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = await repository!.DeleteWalletById(walletId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Wallet ID [{walletId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Wallet = m;

            return r;
        }

        public MVWallet ValidateResponseData(string orgId, string customerId, List<MPointTx> items)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            var wallet = repository!.GetWalletByCustomerId(customerId).Result;

            foreach (var pointTx in items)
            {
                var responseOrgId = pointTx.OrgId;
                var responseWalletId = pointTx.WalletId;

                if (responseOrgId != orgId)
                {
                    r.Status = "ERROR_ORG_ID_MISMATCH";
                    r.Description = "Organization ID of data is different from requested!!!";
                    return r;
                }

                if (responseWalletId != wallet!.Id.ToString())
                {
                    r.Status = "ERROR_CUST_ID_MISMATCH";
                    r.Description = "Customer ID of data is different from requested!!!";
                    return r;
                }
            }

            return r;
        }

        public MVWallet ValidateResponseData(string orgId, string customerId, MVWallet responseData)
        {
            var r = new MVWallet()
            {
                Status = "OK",
                Description = "Success"
            };

            var responseWallet = responseData.Wallet!;
            var responseOrgId = responseWallet.OrgId;
            var responseCustomerId = responseWallet.CustomerId;

            if (responseOrgId != orgId)
            {
                r.Status = "ERROR_ORG_ID_MISMATCH";
                r.Description = "Organization ID of data is different from requested!!!";
                return r;
            }

            if (responseCustomerId != customerId)
            {
                r.Status = "ERROR_CUST_ID_MISMATCH";
                r.Description = "Customer ID of data is different from requested!!!";
                return r;
            }

            return r;
        }
    }
}

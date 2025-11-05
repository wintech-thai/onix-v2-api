using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;

namespace Its.Onix.Api.Services
{
    public class ItemService : BaseService, IItemService
    {
        private readonly IItemRepository? repository = null;

        public ItemService(IItemRepository repo) : base()
        {
            repository = repo;
        }

        public MItem GetItemById(string orgId, string itemId)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemById(itemId);

            if (!string.IsNullOrEmpty(result.Properties))
            {
                result.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(result.Properties!);
            }

            if (!string.IsNullOrEmpty(result.Narrative))
            {
                result.Narratives = [.. result.Narrative.Split('|')];
            }

            result.Properties = "";
            result.Narrative = "";

            return result;
        }

        public MVItem? AddItem(string orgId, MItem item)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVItem();

            var isExist = repository!.IsItemCodeExist(item.Code!);

            if (isExist)
            {
                r.Status = "DUPLICATE";
                r.Description = $"Item code [{item.Code}] is duplicate";

                return r;
            }

            if (item.PropertiesObj == null)
            {
                item.PropertiesObj = new MItemProperties();
            }
            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            item.Narrative = string.Join("|", item.Narratives ?? Array.Empty<string>());

            var result = repository!.AddItem(item);
            result.Properties = "";

            r.Status = "OK";
            r.Description = "Success";
            r.Item = result;

            return r;
        }

        public MVItem? ApproveItemById(string orgId, string itemId)
        {
            //TODO : ต้องเช็คเบื้องต้นด้วยว่า ก่อนหน้านั้นเป็น Pending เท่านั้น
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            var result = repository!.ApproveItemById(itemId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            result.Properties = "";
            r.Item = result;

            return r;
        }

        public MVItem? DisableItemById(string orgId, string itemId)
        {
            //TODO : ต้องเช็คเบื้องต้นด้วยว่า ก่อนหน้านั้นเป็น Approved เท่านั้น
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            var result = repository!.DisableItemById(itemId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            result.Properties = "";
            r.Item = result;

            return r;
        }

        public MVItem? UpdateItemById(string orgId, string itemId, MItem item)
        {
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            if (item.PropertiesObj == null)
            {
                item.PropertiesObj = new MItemProperties();
            }

            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            item.Narrative = string.Join("|", item.Narratives ?? Array.Empty<string>());

            var result = repository!.UpdateItemById(itemId, item);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            result.Properties = "";
            r.Item = result;

            return r;
        }

        public MVItem? UpdatePrivilegeById(string orgId, string itemId, MItem item)
        {
            //TODO : เช็คว่าต้อง Pending เท่า่นั้นถึงจะแก้ไขได้
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            repository!.SetCustomOrgId(orgId);

            if (item.PropertiesObj == null)
            {
                item.PropertiesObj = new MItemProperties();
            }

            item.Properties = JsonSerializer.Serialize(item.PropertiesObj);
            item.Narrative = string.Join("|", item.Narratives ?? Array.Empty<string>());

            var result = repository!.UpdateItemById(itemId, item);

            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";

                return r;
            }

            result.Properties = "";
            r.Item = result;

            return r;
        }

        public MVItem? DeletePrivilegeById(string orgId, string itemId)
        {
            //TODO : ต้องเช็คก่อนว่า Status เป็น Pending กับ Disabled เท่านั้นถึงจะลบได้
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(itemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item ID [{itemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteItemById(itemId);

            r.Item = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public MVItem? DeleteItemById(string orgId, string itemId)
        {
            var r = new MVItem()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(itemId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Item ID [{itemId}] format is invalid";

                return r;
            }

            repository!.SetCustomOrgId(orgId);
            var m = repository!.DeleteItemById(itemId);

            r.Item = m;
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Item ID [{itemId}] not found for the organization [{orgId}]";
            }

            return r;
        }

        public IEnumerable<MItem> GetItems(string orgId, VMItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItems(param);

            foreach (var item in result)
            {
                // ไม่จำเป็นต้องส่งออกไป
                //if (!string.IsNullOrEmpty(item.Properties))
                //{
                //    item.PropertiesObj = JsonSerializer.Deserialize<MItemProperties>(item.Properties!);
                //}

                item.Properties = "";

                //เพื่อไม่ให้ข้อมูลที่ response กลับไปใหญ่จนเกินไป
                item.Narrative = "";
                item.Content = "";
            }

            return result;
        }

        public int GetItemCount(string orgId, VMItem param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository!.GetItemCount(param);

            return result;
        }

        public IEnumerable<NameValue> GetAllowItemPropertyNames(string orgId)
        {
            repository!.SetCustomOrgId(orgId);

            var props = new NameValue[]
            {
                new() { Name = "DimensionUnit", Value = "cm" },
                new() { Name = "WeightUnit", Value = "gram" },
                new() { Name = "Category", Value = "" },
                new() { Name = "SupplierUrl", Value = "" },
                new() { Name = "ProductUrl", Value = "" },
                new() { Name = "Width", Value = "" },
                new() { Name = "Height", Value = "" },
                new() { Name = "Weight", Value = "" },
            };

            return props;
        }

        private MItemBalance GetCurrentBalance(string itemId)
        {
            var param = new VMItemBalance()
            {
                ItemId = itemId,
                BalanceType = "ItemBalanceCurrent",
            };

            var bal = repository!.GetItemBalanceByItemId(param);

            if (bal == null)
            {
                bal = new MItemBalance()
                {
                    StatCode = param.BalanceType,
                    ItemId = itemId,
                    BalanceDate = DateTime.UtcNow,
                    BalanceDateKey = "000000",
                    TxIn = 0,
                    TxOut = 0,
                    BalanceBegin = 0,
                    BalanceEnd = 0,
                    IsNew = true,
                };
            }
            else
            {
                bal.IsNew = false;
            }

            return bal;
        }

        public MVItemTx AddItemQuantity(string orgId, MItemTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository!.SetCustomOrgId(orgId);

            var r = new MVItemTx()
            {
                Status = "OK",
                Description = "Success",
            };

            var txAmt = tx.TxAmount;
            var currBal = GetCurrentBalance(tx.ItemId!);

            //เอา balance ปัจจุบันมาก่อน

            var previousBal = currBal.BalanceEnd;
            tx.PreviousBalance = previousBal;

            currBal.TxIn = currBal.TxIn + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd + txAmt;
            currBal.BalanceDate = DateTime.UtcNow;

            tx.TxType = 1;

            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            var result = repository!.AddItemTxWithBalance(tx, currBal);

            r.Status = "OK";
            r.Description = "Success";
            r.ItemTx = result;

            return r;
        }

        public MVItemTx DeductItemQuantity(string orgId, MItemTx tx)
        {
            //ใช้วิธี : Optimistic Concurrency ในการแก้ race condition
            repository!.SetCustomOrgId(orgId);

            var r = new MVItemTx()
            {
                Status = "OK",
                Description = "Success",
            };

            var txAmt = tx.TxAmount;
            var currBal = GetCurrentBalance(tx.ItemId!);

            //เอา balance ปัจจุบันมาก่อน

            var previousBal = currBal.BalanceEnd;
            tx.PreviousBalance = previousBal;

            currBal.TxOut = currBal.TxOut + txAmt;
            currBal.BalanceEnd = currBal.BalanceEnd - txAmt;
            currBal.BalanceDate = DateTime.UtcNow;

            if (currBal.BalanceEnd < 0)
            {
                r.Status = "INVALID_BALANCE_LESS_THAN_ZERO";
                r.Description = "Balance cannot be less than 0!!!";
                return r;
            }

            tx.TxType = -1;

            //ตรงนี้จะได้ balance ที่ลดหรือเพิ่มแล้ว
            tx.CurrentBalance = currBal.BalanceEnd;
            var result = repository!.AddItemTxWithBalance(tx, currBal);

            r.Status = "OK";
            r.Description = "Success";
            r.ItemTx = result;

            return r;
        }

        public MVItemBalance? GetItemBalanceByItemId(string orgId, VMItemBalance param)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVItemBalance()
            {
                Status = "OK",
                Description = "Success",
            };

            var balanceType = param.BalanceType;

            if (string.IsNullOrEmpty(balanceType))
            {
                r.Status = "BALANCE_TYPE_MISSING";
                r.Description = "BalanceType need to be ItemBalanceCurrent";

                return r;
            }

            var result = repository.GetItemBalanceByItemId(param);
            r.ItemBalance = result;

            return r;
        }

        public List<MItemTx> GetItemTxsByItemId(string orgId, VMItemTx param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository.GetItemTxsByItemId(param);
            return result;
        }
        
        public int GetItemTxsCountByItemId(string orgId, VMItemTx param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = repository.GetItemTxsCountByItemId(param);
            return result;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Models;
using Its.Onix.Api.Services;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/api/[controller]")]
    public class PrivilegeController : ControllerBase
    {
        private readonly IItemService svc;
        private readonly IItemImageService _itemImgService;

        public PrivilegeController(IItemService service, IItemImageService itemImgService)
        {
            svc = service;
            _itemImgService = itemImgService;
        }

        [HttpPost]
        [Route("org/{id}/action/AddPrivilege")]
        public MVItem? AddPrivilege(string id, [FromBody] MItem request)
        {
            request.ItemType = 2;
            request.Status = "Pending";
            var result = svc.AddItem(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/AddPrivilegeImage/{itemId}")]
        public MVItemImage? AddPrivilegeImage(string id, Guid itemId, [FromBody] MItemImage request)
        {
            request.ItemId = itemId;
            var result = _itemImgService.AddItemImage(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [HttpDelete]
        [Route("org/{id}/action/DeletePrivilegeById/{itemId}")]
        public IActionResult DeletePrivilegeById(string id, string itemId)
        {
            var result = svc.DeletePrivilegeById(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/ApprovedPrivilegeById/{itemId}")]
        public IActionResult ApprovedPrivilegeById(string id, string itemId)
        {
            var result = svc.ApproveItemById(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/DisablePrivilegeById/{itemId}")]
        public IActionResult DisablePrivilegeById(string id, string itemId)
        {
            var result = svc.DisableItemById(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeletePrivilegeCascadeById/{itemId}")]
        public IActionResult DeletePrivilegeCascadeById(string id, string itemId)
        {
            _itemImgService.DeleteItemImageByItemId(id, itemId);
            var result = svc.DeletePrivilegeById(id, itemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeletePrivilegeImagesByPrivilegeId/{itemId}")]
        public IActionResult DeletePrivilegeImagesByPrivilegeId(string id, string itemId)
        {
            var result = _itemImgService.DeleteItemImageByItemId(id, itemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeletePrivilegeImageByPrivilegeImageId/{itemImageId}")]
        public IActionResult DeletePrivilegeImageByPrivilegeImageId(string id, string itemImageId)
        {
            var result = _itemImgService.DeleteItemImageById(id, itemImageId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdatePrivilegeById/{itemId}")]
        public IActionResult UpdatePrivilegeById(string id, string itemId, [FromBody] MItem request)
        {
            var result = svc.UpdatePrivilegeById(id, itemId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdatePrivilegeImageByPrivilegeImageId/{itemImageId}")]
        public IActionResult UpdatePrivilegeImageByPrivilegeImageId(string id, string itemImageId, [FromBody] MItemImage request)
        {
            var result = _itemImgService.UpdateItemImageById(id, itemImageId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdatePrivilegeImagesSortingOrder/{itemId}")]
        public IActionResult UpdatePrivilegeImagesSortingOrder(string id, string itemId, [FromBody] IEnumerable<string> itemImageIdList)
        {
            var result = _itemImgService.UpdateItemImagesSortingOrder(id, itemId, itemImageIdList);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPrivilegeById/{itemId}")]
        public MItem GetPrivilegeById(string id, string itemId)
        {
            var result = svc.GetItemById(id, itemId);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPrivilegeImagesByPrivilegeId/{itemId}")]
        public IActionResult GetPrivilegeImagesByPrivilegeId(string id, string itemId)
        {
            var param = new VMItemImage();
            param.ItemId = itemId;

            var result = _itemImgService.GetItemImages(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetPrivilegeImageUploadPresignedUrl/{itemId}")]
        public IActionResult GetPrivilegeImageUploadPresignedUrl(string id, string itemId)
        {
            var result = _itemImgService.GetItemImageUploadPresignedUrl(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPrivileges")]
        public IActionResult GetPrivileges(string id, [FromBody] VMItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            param.ItemType = 2;
            var result = svc.GetItems(id, param);

            return Ok(result);
        }


        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetRedeemablePrivileges")]
        public IActionResult GetRedeemablePrivileges(string id, [FromBody] VMItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            param.ItemType = 2;
            param.Status = "Approved";
            var result = svc.GetItems(id, param);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPrivilegeCount")]
        public IActionResult GetPrivilegeCount(string id, [FromBody] VMItem param)
        {
            param.ItemType = 2;
            var result = svc.GetItemCount(id, param);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddPrivilegeQuantity/{itemId}")]
        public IActionResult AddPrivilegeQuantity(string id, string itemId, [FromBody] MItemTx request)
        {
            request.ItemId = itemId;
            var result = svc.AddItemQuantity(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/DeductPrivilegeQuantity/{itemId}")]
        public IActionResult DeductPrivilegeQuantity(string id, string itemId, [FromBody] MItemTx request)
        {
            request.ItemId = itemId;
            var result = svc.DeductItemQuantity(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPrivilegeBalanceById/{itemId}")]
        public IActionResult GetPrivilegeBalanceById(string id, string itemId, [FromBody] VMItemBalance request)
        {
            request.ItemId = itemId;
            var result = svc.GetItemBalanceByItemId(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPrivilegeTxsById/{itemId}")]
        public IActionResult GetPrivilegeTxsById(string id, string itemId, [FromBody] VMItemTx request)
        {
            request.ItemId = itemId;
            var result = svc.GetItemTxsByItemId(id, request);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetPrivilegeTxsCountById/{itemId}")]
        public IActionResult GetPrivilegeTxsCountById(string id, string itemId, [FromBody] VMItemTx request)
        {
            request.ItemId = itemId;
            var result = svc.GetItemTxsCountByItemId(id, request);

            return Ok(result);
        }
    }
}

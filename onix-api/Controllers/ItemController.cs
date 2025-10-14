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
    public class ItemController : ControllerBase
    {
        private readonly IItemService svc;
        private readonly IItemImageService _itemImgService;

        public ItemController(IItemService service, IItemImageService itemImgService)
        {
            svc = service;
            _itemImgService = itemImgService;
        }

        [HttpPost]
        [Route("org/{id}/action/AddItem")]
        public MVItem? AddItem(string id, [FromBody] MItem request)
        {
            var result = svc.AddItem(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [HttpPost]
        [Route("org/{id}/action/AddItemImage/{itemId}")]
        public MVItemImage? AddItemImage(string id, Guid itemId, [FromBody] MItemImage request)
        {
            request.ItemId = itemId;
            var result = _itemImgService.AddItemImage(id, request);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return result;
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteItemById/{itemId}")]
        public IActionResult DeleteItemById(string id, string itemId)
        {
            var result = svc.DeleteItemById(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteItemCascadeById/{itemId}")]
        public IActionResult DeleteItemCascadeById(string id, string itemId)
        {
            _itemImgService.DeleteItemImageByItemId(id, itemId);
            var result = svc.DeleteItemById(id, itemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteItemImagesByItemId/{itemId}")]
        public IActionResult DeleteItemImagesByItemId(string id, string itemId)
        {
            var result = _itemImgService.DeleteItemImageByItemId(id, itemId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpDelete]
        [Route("org/{id}/action/DeleteItemImageByItemImageId/{itemImageId}")]
        public IActionResult DeleteItemImageByItemImageId(string id, string itemImageId)
        {
            var result = _itemImgService.DeleteItemImageById(id, itemImageId);

            Response.Headers.Append("CUST_STATUS", result!.Status);
            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateItemById/{itemId}")]
        public IActionResult UpdateItemById(string id, string itemId, [FromBody] MItem request)
        {
            var result = svc.UpdateItemById(id, itemId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateItemImageByItemImageId/{itemImageId}")]
        public IActionResult UpdateItemImageByItemImageId(string id, string itemImageId, [FromBody] MItemImage request)
        {
            var result = _itemImgService.UpdateItemImageById(id, itemImageId, request);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [HttpPost]
        [Route("org/{id}/action/UpdateItemImagesSortingOrder/{itemId}")]
        public IActionResult UpdateItemImagesSortingOrder(string id, string itemId, [FromBody] IEnumerable<string> itemImageIdList)
        {
            var result = _itemImgService.UpdateItemImagesSortingOrder(id, itemId, itemImageIdList);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetItemById/{itemId}")]
        public MItem GetItemById(string id, string itemId)
        {
            var result = svc.GetItemById(id, itemId);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetItemImagesByItemId/{itemId}")]
        public IActionResult GetItemImagesByItemId(string id, string itemId)
        {
            var param = new VMItemImage();
            param.ItemId = itemId;

            var result = _itemImgService.GetItemImages(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetAllowItemPropertyNames")]
        public IActionResult GetAllowItemPropertyNames(string id)
        {
            var result = svc.GetAllowItemPropertyNames(id);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GetItemImageUploadPresignedUrl/{itemId}")]
        public IActionResult GetItemImageUploadPresignedUrl(string id, string itemId)
        {
            var result = _itemImgService.GetItemImageUploadPresignedUrl(id, itemId);
            Response.Headers.Append("CUST_STATUS", result!.Status);

            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetItems")]
        public IActionResult GetItems(string id, [FromBody] VMItem param)
        {
            if (param.Limit <= 0)
            {
                param.Limit = 100;
            }

            var result = svc.GetItems(id, param);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/GetItemCount")]
        public IActionResult GetItemCount(string id, [FromBody] VMItem param)
        {
            var result = svc.GetItemCount(id, param);
            return Ok(result);
        }
    }
}

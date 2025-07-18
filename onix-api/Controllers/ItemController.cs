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

        [ExcludeFromCodeCoverage]
        public ItemController(IItemService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/AddItem")]
        public MVItem? AddItem(string id, [FromBody] MItem request)
        {
            var result = svc.AddItem(id, request);
            return result;
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteItemById/{itemId}")]
        public IActionResult DeleteItemById(string id, string itemId)
        {
            var result = svc.DeleteItemById(id, itemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpDelete]
        [Route("org/{id}/action/DeleteItemCascadeById/{itemId}")]
        public IActionResult DeleteItemCascadeById(string id, string itemId)
        {
            //TODO : Change the implementiation here...
            var result = svc.DeleteItemById(id, itemId);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpPost]
        [Route("org/{id}/action/UpdateItemById/{itemId}")]
        public IActionResult UpdateItemById(string id, string itemId, [FromBody] MItem request)
        {
            var result = svc.UpdateItemById(id, itemId, request);
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

        [HttpPost]
        [Route("org/{id}/action/GetItemCount")]
        public IActionResult GetItemCount(string id, [FromBody] VMItem param)
        {
            var result = svc.GetItemCount(id, param);
            return Ok(result);
        }
    }
}

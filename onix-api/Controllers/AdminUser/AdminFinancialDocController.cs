using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.ViewsModels;
using System.Text.Json;
using Its.Onix.Api.ModelsViews;
using System.Text.RegularExpressions;
using QRCoder;

namespace Its.Onix.Api.Controllers
{
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminFinancialDocController : ControllerBase
    {
        private readonly IFinancialDocService svc;

        [ExcludeFromCodeCoverage]
        public AdminFinancialDocController(IFinancialDocService service)
        {
            svc = service;
        }
    }
}

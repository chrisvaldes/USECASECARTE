using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // GET ALL
        [HttpGet]
        //[Authorize(Policy = "VOIR_ROLE")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _permissionService.GetAllAsync();

            return Ok(response);
        }

        // GET ALL 
        [HttpPost]
        //[Authorize(Policy = "VOIR_ROLE")]
        public async Task<IActionResult> SavePermission(Permission permission)
        {
            var response = await _permissionService.PostAsync(permission);

            Console.WriteLine($"response : {response.Data!.Code}");

            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}

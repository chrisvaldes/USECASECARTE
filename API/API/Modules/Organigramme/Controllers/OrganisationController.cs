using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organigramme.Application.DTO;
using Organigramme.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Organigramme.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationController : ControllerBase
    {
        private readonly IOrganisationService _service;

        public OrganisationController(IOrganisationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id) => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create(OrganisationDto dto)
        {
            var response = await _service.CreateAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, OrganisationDto dto)
        {
            var response = await _service.UpdateAsync(id, dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _service.DeleteAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

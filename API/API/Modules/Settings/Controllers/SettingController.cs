using Settings.Application.DTO;
using Settings.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Settings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _service;

        public SettingController(ISettingService service)
        {
            _service = service;
        }

        [HttpGet]
        //[Authorize(Policy = "VOIR_USER")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        //[Authorize(Policy = "VOIR_USER")]
        public async Task<IActionResult> GetById(Guid id) => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        //[Authorize(Policy = "CREER_USER")]
        public async Task<IActionResult> Create(SettingDto dto)
        {
            var response = await _service.CreateAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{uid}")]
        //[Authorize(Policy = "MODIFIER_USER")]
        public async Task<IActionResult> Update(Guid uid, SettingDto dto)
        {
            var response = await _service.UpdateAsync(uid, dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "SUPPRIMER_USER")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _service.DeleteAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

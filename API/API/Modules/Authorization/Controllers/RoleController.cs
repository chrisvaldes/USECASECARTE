using Authorization.Application.DTO;
using Authorization.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET ALL
        [HttpGet]
        //[Authorize(Policy = "VOIR_ROLE")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _roleService.GetAllAsync();

            return Ok(response);
        }

        // GET BY ID
        [HttpGet("{id:Guid}")]
        //[Authorize(Policy = "VOIR_ROLE")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _roleService.GetByIdAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        // CREATE
        [HttpPost]
        //[Authorize(Policy = "CREER_ROLE")]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var response = await _roleService.CreateAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // UPDATE
        [HttpPut("{id}")]
        //[Authorize(Policy = "MODIFIER_ROLE")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
        {
            var response = await _roleService.UpdateAsync(id, dto);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        // DELETE
        [HttpDelete("{id}")]
        //[Authorize(Policy = "SUPPRIMER_ROLE")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _roleService.DeleteAsync(id);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

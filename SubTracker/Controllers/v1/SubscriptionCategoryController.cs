using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubTracker.Core.Application.Dtos.SubscriptionCategory;
using SubTracker.Core.Application.Interfaces;
using System.Security.Claims;

namespace SubTracker.Controllers.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    public class SubscriptionCategoryController : BaseApiController
    {
        private readonly ISubscriptionCategoryService _categoryService;

        public SubscriptionCategoryController(ISubscriptionCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SubscriptionCategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            var result = await _categoryService.GetAllByUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SubscriptionCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _categoryService.GetByIdAsync(id, userId);

            if (result == null) return NotFound(new { message = "Categoría no encontrada o no autorizada." });
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SubscriptionCategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SaveSubscriptionCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _categoryService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(SubscriptionCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(int id, [FromBody] SaveSubscriptionCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _categoryService.UpdateAsync(id, dto, userId);

            if (result == null) return NotFound(new { message = "No se pudo editar. Categoría no encontrada o sin permisos." });
            return Ok(result);
        }

        [HttpPatch("{id:int}/toggle-active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _categoryService.ToggleActiveAsync(id, userId);

            if (!success) return NotFound(new { message = "Categoría no encontrada o sin permisos." });
            return Ok(new { message = "Estado de la categoría actualizado correctamente." });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _categoryService.DeleteAsync(id, userId);

            if (!success) return NotFound(new { message = "Categoría no encontrada o sin permisos." });
            return Ok(new { message = "Categoría eliminada correctamente." });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("uid")?.Value
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuario no autenticado.");
        }
    }
}
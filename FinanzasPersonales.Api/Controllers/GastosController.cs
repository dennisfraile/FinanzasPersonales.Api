using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestionar todos los gastos de la aplicación.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class GastosController : ControllerBase
    {
        private readonly IGastosService _gastosService;

        public GastosController(IGastosService gastosService)
        {
            _gastosService = gastosService;
        }

        /// <summary>
        /// Obtiene una lista de gastos con filtros y paginación.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<GastoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<GastoDto>>> GetGastos(
            [FromQuery] int? categoriaId = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] decimal? montoMin = null,
            [FromQuery] decimal? montoMax = null,
            [FromQuery] string? descripcionContiene = null,
            [FromQuery] string ordenarPor = "fecha",
            [FromQuery] string ordenDireccion = "desc",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 50,
            [FromQuery] List<int>? tagIds = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _gastosService.GetGastosAsync(
                userId!, categoriaId, tipo, desde, hasta, montoMin, montoMax,
                descripcionContiene, ordenarPor, ordenDireccion, pagina, tamañoPagina, tagIds);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene un gasto específico por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Gasto>> GetGasto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var gasto = await _gastosService.GetGastoAsync(userId!, id);

            if (gasto == null)
                return NotFound();

            return Ok(gasto);
        }

        /// <summary>
        /// Registra un nuevo gasto en el sistema.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GastoDto>> PostGasto(CreateGastoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("No se pudo obtener el UserId del token JWT");

            var gasto = await _gastosService.CreateGastoAsync(userId, dto);

            return CreatedAtAction("GetGasto", new { id = gasto.Id }, gasto);
        }

        /// <summary>
        /// Actualiza un gasto existente usando su ID.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutGasto(int id, UpdateGastoDto dto)
        {
            if (id != dto.Id)
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _gastosService.UpdateGastoAsync(userId!, id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un gasto del sistema por su ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _gastosService.DeleteGastoAsync(userId!, id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

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
    /// API para gestionar todos los ingresos de la aplicación.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class IngresosController : Controller
    {
        private readonly IIngresosService _ingresosService;

        public IngresosController(IIngresosService ingresosService)
        {
            _ingresosService = ingresosService;
        }

        /// <summary>
        /// Obtiene una lista de ingresos con filtros y paginación.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<IngresoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<IngresoDto>>> GetIngresos(
            [FromQuery] int? categoriaId = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] decimal? montoMin = null,
            [FromQuery] decimal? montoMax = null,
            [FromQuery] string ordenarPor = "fecha",
            [FromQuery] string ordenDireccion = "desc",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 50,
            [FromQuery] List<int>? tagIds = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _ingresosService.GetIngresosAsync(
                userId!, categoriaId, desde, hasta, montoMin, montoMax,
                ordenarPor, ordenDireccion, pagina, tamañoPagina, tagIds);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene un ingreso específico por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ingreso>> GetIngreso(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ingreso = await _ingresosService.GetIngresoAsync(userId!, id);

            if (ingreso == null)
                return NotFound();

            return Ok(ingreso);
        }

        /// <summary>
        /// Registra un nuevo ingreso en el sistema.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IngresoDto>> PostIngreso(CreateIngresoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("No se pudo obtener el UserId del token JWT");

            var ingreso = await _ingresosService.CreateIngresoAsync(userId, dto);

            return CreatedAtAction("GetIngreso", new { id = ingreso.Id }, ingreso);
        }

        /// <summary>
        /// Actualiza un ingreso existente usando su ID.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutIngreso(int id, UpdateIngresoDto dto)
        {
            if (id != dto.Id)
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _ingresosService.UpdateIngresoAsync(userId!, id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un ingreso del sistema por su ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIngreso(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _ingresosService.DeleteIngresoAsync(userId!, id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

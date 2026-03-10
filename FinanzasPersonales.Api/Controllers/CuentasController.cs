using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly ICuentasService _cuentasService;

        public CuentasController(ICuentasService cuentasService)
        {
            _cuentasService = cuentasService;
        }

        // GET: api/Cuentas
        [HttpGet]
        public async Task<ActionResult<List<CuentaDto>>> GetCuentas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuentas = await _cuentasService.GetCuentasAsync(userId!);

            return Ok(cuentas);
        }

        // GET: api/Cuentas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaDto>> GetCuenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cuenta = await _cuentasService.GetCuentaAsync(userId!, id);

            if (cuenta == null)
                return NotFound();

            return Ok(cuenta);
        }

        // POST: api/Cuentas
        [HttpPost]
        public async Task<ActionResult<CuentaDto>> PostCuenta(CuentaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _cuentasService.CreateCuentaAsync(userId!, dto);

            return CreatedAtAction(nameof(GetCuenta), new { id = result.Id }, result);
        }

        // PUT: api/Cuentas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuenta(int id, CuentaUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _cuentasService.UpdateCuentaAsync(userId!, id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/Cuentas/{id} (Soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _cuentasService.DeleteCuentaAsync(userId!, id);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // GET: api/Cuentas/balance-total
        [HttpGet("balance-total")]
        public async Task<ActionResult<decimal>> GetBalanceTotal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var balanceTotal = await _cuentasService.GetBalanceTotalAsync(userId!);

            return Ok(balanceTotal);
        }
    }
}

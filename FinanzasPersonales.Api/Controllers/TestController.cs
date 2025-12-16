using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinanzasPersonales.Api.Data;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public TestController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-user")]
        public async Task<IActionResult> CheckUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new
                {
                    success = false,
                    message = "UserId is null or empty",
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

            return Ok(new
            {
                success = true,
                userId = userId,
                userExists = userExists,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public TagsController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tags = await _context.Tags
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Nombre)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Color = t.Color,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();

            return Ok(tags);
        }

        [HttpPost]
        public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tag = new Tag
            {
                Nombre = dto.Nombre,
                Color = dto.Color,
                UserId = userId
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            var result = new TagDto
            {
                Id = tag.Id,
                Nombre = tag.Nombre,
                Color = tag.Color,
                FechaCreacion = tag.FechaCreacion
            };

            return CreatedAtAction(nameof(GetAll), new { id = tag.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (tag == null) return NotFound();

            tag.Nombre = dto.Nombre;
            tag.Color = dto.Color;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

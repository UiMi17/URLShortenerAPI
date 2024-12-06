using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using URLShortenerAPI.Models;
using URLShortenerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("api/url")]
    public class UrlController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UrlController(AppDbContext context) { 
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateShortUrl([FromBody] UrlModel urlModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Urls.Any(u => u.OriginalUrl == urlModel.OriginalUrl))
            {
                return BadRequest("This URL is already shortened.");
            }

            urlModel.ShortenedUrl = GenerateShortUrl();

            urlModel.CreatedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            _context.Urls.Add(urlModel);
            await _context.SaveChangesAsync();

            return Created("", urlModel);
        }

        [HttpGet]
        public IActionResult GetAllUrls()
        {
            var urls = _context.Urls.ToList();
            return Ok(urls);
        }

        [HttpGet("{shortenedUrl}")]
        public async Task<IActionResult> RedirectToShortUrl(string shortenedUrl)
        {
            var urlToRedirect = await _context.Urls.FirstOrDefaultAsync(u => 
            u.ShortenedUrl == shortenedUrl);

            if (urlToRedirect == null) return NotFound();

            return Redirect(urlToRedirect.OriginalUrl);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUrlById(int id)
        {
            var urlToDelete = await _context.Urls.FindAsync(id);
            if (urlToDelete == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserId != urlToDelete.CreatedBy && currentUserRole != "Admin")
            {
                return Forbid();
            }

            _context.Urls.Remove(urlToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }



        private string GenerateShortUrl()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

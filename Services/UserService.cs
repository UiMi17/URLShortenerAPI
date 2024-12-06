using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using URLShortenerAPI.Data;
using URLShortenerAPI.Enums;
using URLShortenerAPI.Models;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<UserModel> _passwordHasher;

    public UserService(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<UserModel>();
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found.");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7); 
        await _context.SaveChangesAsync();
    }

    public async Task<int?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryDate > DateTime.UtcNow);

        if (user == null)
            return null; 

        return user.Id;
    }

    public async Task RemoveRefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiryDate = null;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> RegisterAsync(string email, string username, string password)
    {
        if (_context.Users.Any(u => u.Email == email))
            return false; 

        var user = new UserModel
        {
            Email = email,
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(null, password),
            Role = UserRole.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserModel?> AuthenticateAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return verificationResult == PasswordVerificationResult.Success ? user : null;
    }
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Badminton_BE.Configuration;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Badminton_BE.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _jwtOptions = jwtOptions.Value;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
        {
            var normalizedUsername = NormalizeUsername(dto.Username);
            var existing = await _userRepository.GetByNormalizedUsernameAsync(normalizedUsername);
            if (existing != null)
            {
                return null;
            }

            var user = new AppUser
            {
                Username = dto.Username.Trim(),
                NormalizedUsername = normalizedUsername
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var normalizedUsername = NormalizeUsername(dto.Username);
            var user = await _userRepository.GetByNormalizedUsernameAsync(normalizedUsername);
            if (user == null)
            {
                return null;
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(AppUser user)
        {
            if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
            {
                throw new InvalidOperationException("JWT secret is not configured.");
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt,
                Username = user.Username
            };
        }

        private static string NormalizeUsername(string username)
        {
            return username.Trim().ToUpperInvariant();
        }
    }
}

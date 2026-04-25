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
        private readonly IMemberRepository _memberRepository;
        private readonly IRevokedTokenRepository _revokedTokenRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public AuthService(
            IUserRepository userRepository, 
            IMemberRepository memberRepository,
            IRevokedTokenRepository revokedTokenRepository, 
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _memberRepository = memberRepository;
            _revokedTokenRepository = revokedTokenRepository;
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

        public async Task LogoutAsync(int userId, string jti, DateTime expiresAt)
        {
            var existing = await _revokedTokenRepository.GetByJtiAsync(jti);
            if (existing != null)
            {
                return;
            }

            var revokedToken = new RevokedToken
            {
                UserId = userId,
                Jti = jti,
                ExpiresAt = expiresAt
            };

            await _revokedTokenRepository.AddAsync(revokedToken);
            await _revokedTokenRepository.SaveChangesAsync();
        }

        public async Task<UserProfileReadDto?> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return MapToProfileDto(user);
        }

        public async Task<bool> UpdateProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.Name = dto.Name?.Trim();
            user.AvatarUrl = dto.AvatarUrl?.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.Email = dto.Email?.Trim();
            user.Facebook = dto.Facebook?.Trim();
            user.BankAccountNumber = dto.BankAccountNumber?.Trim();
            user.BankOwnerName = dto.BankOwnerName?.Trim();
            user.BankName = dto.BankName?.Trim();

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
            return true;
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
                Username = user.Username,
                Name = user.Name,
                IsPlayer = user.Username.StartsWith("phone_")
            };
        }

        private static string NormalizeUsername(string username)
        {
            return username.Trim().ToUpperInvariant();
        }

        private static UserProfileReadDto MapToProfileDto(AppUser user)
        {
            return new UserProfileReadDto
            {
                Username = user.Username,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Facebook = user.Facebook,
                BankAccountNumber = user.BankAccountNumber,
                BankOwnerName = user.BankOwnerName,
                BankName = user.BankName
            };
        }
    }
}

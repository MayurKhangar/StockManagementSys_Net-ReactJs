using Microsoft.Extensions.Options;
using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Auth;
using SmartStock.Application.Interfaces;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;

namespace SmartStock.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ResultModel<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.Users.FindAsync(u => u.Email == normalizedEmail);
        if (existing.Count > 0)
        {
            return ResultModel<AuthResponseDto>.Fail("A user with this email already exists.");
        }

        var customerRole = (await _unitOfWork.Roles.FindAsync(r => r.Name == RoleType.Customer)).FirstOrDefault();
        if (customerRole == null)
        {
            return ResultModel<AuthResponseDto>.Fail("Customer role is not configured.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            RoleId = customerRole.Id,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        user.Role = customerRole;

        return ResultModel<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user), "Registration successful.");
    }

    public async Task<ResultModel<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = _unitOfWork.Users.Query()
            .Where(u => u.Email == normalizedEmail)
            .Select(u => u)
            .FirstOrDefault();

        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ResultModel<AuthResponseDto>.Fail("Invalid email or password.");
        }

        user.Role = _unitOfWork.Roles.Query().First(r => r.Id == user.RoleId);

        return ResultModel<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user), "Login successful.");
    }

    public async Task<ResultModel<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return ResultModel<AuthResponseDto>.Fail("Invalid access token.");
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return ResultModel<AuthResponseDto>.Fail("Invalid access token.");
        }

        var user = _unitOfWork.Users.Query().FirstOrDefault(u => u.Id == userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ResultModel<AuthResponseDto>.Fail("Invalid or expired refresh token.");
        }

        user.Role = _unitOfWork.Roles.Query().First(r => r.Id == user.RoleId);

        return ResultModel<AuthResponseDto>.Ok(await BuildAuthResponseAsync(user), "Token refreshed.");
    }

    public async Task<ResultModel<bool>> RevokeTokenAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return ResultModel<bool>.Fail("User not found.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return ResultModel<bool>.Ok(true, "Token revoked.");
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = _tokenService.GetAccessTokenExpiry(),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.Name.ToString()
            }
        };
    }
}

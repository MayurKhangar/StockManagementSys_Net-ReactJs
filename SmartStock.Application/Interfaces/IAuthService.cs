using SmartStock.Application.Common;
using SmartStock.Application.DTOs.Auth;

namespace SmartStock.Application.Interfaces;

public interface IAuthService
{
    Task<ResultModel<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ResultModel<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ResultModel<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ResultModel<bool>> RevokeTokenAsync(int userId);
}

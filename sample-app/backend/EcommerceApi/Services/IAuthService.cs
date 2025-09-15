using EcommerceApi.DTOs;
using EcommerceApi.Models;

namespace EcommerceApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto> GetUserProfileAsync(string userId);
    Task<UserDto> UpdateUserProfileAsync(string userId, UserDto userDto);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task LogoutAsync(string userId);
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
}

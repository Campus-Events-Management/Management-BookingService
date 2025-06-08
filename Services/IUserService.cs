using System.Security.Claims;

namespace EventManagement.BookingService.Services
{
    public interface IUserService
    {
        string GetUserId(ClaimsPrincipal user);
        bool IsAdmin(ClaimsPrincipal user);
    }
} 
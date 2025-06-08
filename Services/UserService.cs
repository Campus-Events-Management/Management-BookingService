using System.Security.Claims;
using System.Linq;

namespace EventManagement.BookingService.Services
{
    public class UserService : IUserService
    {
        public string GetUserId(ClaimsPrincipal user)
        {
            // Try to get the user ID from different possible claim types
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? user.FindFirstValue("sub")  // JWT standard claim for subject/user ID
                ?? user.FindFirstValue("userId")
                ?? string.Empty;
            
            return userId;
        }
        
        public bool IsAdmin(ClaimsPrincipal user)
        {
            // Check for admin role in different possible formats
            if (user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.ToLower() == "admin"))
                return true;
                
            if (user.HasClaim(c => c.Type == "role" && c.Value.ToLower() == "admin"))
                return true;
                
            if (user.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value.ToLower() == "admin"))
                return true;
                
            // Check if the email contains "admin"
            var email = user.FindFirstValue(ClaimTypes.Email) ?? 
                        user.FindFirstValue("email") ?? 
                        string.Empty;
                        
            if (email.ToLower().Contains("admin"))
                return true;
                
            // Check if the name contains "admin"
            var name = user.FindFirstValue(ClaimTypes.Name) ?? 
                       user.FindFirstValue("name") ?? 
                       user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") ?? 
                       string.Empty;
                       
            if (name.ToLower().Contains("admin"))
                return true;
                
            return false;
        }
    }
} 
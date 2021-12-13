using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Suitex.Models;

namespace Suitex.Extensions
{
    public static class RoleClaimsExtension
    {
        public static IEnumerable<Claim> GetClaims(this User user)
        {
            var result = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email), // User.Identity.Name
            };
            result.AddRange(
                user.Roles.Select(role => 
                    new Claim(ClaimTypes.Role, role.Slug))
                );
            return result;
        }
    }
}
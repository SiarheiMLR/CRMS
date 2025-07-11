using System.Linq;
using CRMS.Domain.Entities;

namespace CRMS.Business.Services.UserService
{
    public static class RoleMapper
    {
        public static UserRole ResolveRole(User user)
        {
            if (user.GroupMembers == null || user.GroupMembers.Count == 0)
                return UserRole.User;

            // Собираем все роли, назначенные через GroupRoleMapping
            var roles = user.GroupMembers
                .Where(gm => gm.Group?.GroupRoleMapping != null)
                .Select(gm => gm.Group.GroupRoleMapping!.Role)
                .Distinct()
                .ToList();

            if (roles.Contains(UserRole.Admin))
                return UserRole.Admin;

            if (roles.Contains(UserRole.Support))
                return UserRole.Support;

            if (roles.Contains(UserRole.User))
                return UserRole.User;

            return UserRole.User; // fallback
        }
    }
}



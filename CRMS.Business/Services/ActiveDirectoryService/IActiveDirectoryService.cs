using System.Collections.Generic;
using System.Threading.Tasks;
using CRMS.Business.Models;

namespace CRMS.Business.ActiveDirectoryService
{
    public interface IActiveDirectoryService
    {
        Task<List<ADUserDto>> GetAllUsersAsync(string domain, string login, string password);
    }
}



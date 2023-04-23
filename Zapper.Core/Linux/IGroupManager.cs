using System.Threading.Tasks;

namespace Zapper.Core.Linux;

public interface ILinuxGroupManager
{
    Task<bool> IsInInputGroup();
    
    Task AddUserToInputGroup(string password);
    
    Task RebootSystem(string password);
}
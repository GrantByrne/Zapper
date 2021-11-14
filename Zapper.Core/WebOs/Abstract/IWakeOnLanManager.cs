namespace Zapper.Core.WebOs.Abstract
{
    public interface IWakeOnLanManager
    {
        void Send(string ipAddress, string macAddress);
    }
}
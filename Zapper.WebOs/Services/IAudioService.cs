using System.Threading.Tasks;

namespace Zapper.WebOs.Services
{
    public interface IAudioService
    {
        Task<AudioService.VolumeDetails> VolumeGetAsync();
        Task VolumeDownAsync(int by = 1);
        Task VolumeUpAsync(int by = 1);
        Task MuteAsync();
        Task UnmuteAsync();
        Task SetVolumeAsync(int volume);
    }
}
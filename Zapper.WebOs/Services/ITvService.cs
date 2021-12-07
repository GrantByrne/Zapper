using System.Threading.Tasks;
using Zapper.WebOs.Responses.Tv;

namespace Zapper.WebOs.Services
{
    public interface ITvService
    {
        Task ChannelDownAsync();
        Task<ChannelListResponse.Channel[]> ListChannelsAsync();
        Task ChannelUpAsync();
        Task<ExternalInputListResponse.Device[]> ListInputsAsync();
        Task<GetChannelProgramInfoResponse.ProgramItem[]> GetProgramInfoAsync();
        Task<string> GetCurrentChannelAsync();
        Task SetInputAsync(string inputId);
        Task TurnOn3dAsync();
        Task TurnOff3dAsync();
        Task<ThreeDimensionStatusResponse.Status3d> Get3dStatusAsync();
    }
}
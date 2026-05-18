using AntGateBarrier.Domain.Model;
using AntGateBarrier.Entity;

namespace AntGateBarrier.Driver
{
    public interface IRfid
    {
        public Task ConnectAsync();
        public Task<bool> DisconnectAsync();
        public Task ServicesAsync();
        public Task<bool> UpdateAsync(AnntModel anntConfig);
        public Task<bool> IsConnectedAsync();
    }
}

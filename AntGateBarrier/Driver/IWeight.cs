using AntGateBarrier.Domain.Model;

namespace AntGateBarrier.Driver
{
    public interface IWeight
    {
        public Task ConnectAsync();
        public Task<bool> DisconnectAsync();
        public Task ServicesAsync();
        public Task<bool> IsConnectedAsync();
        public Task<int> GetWeightAsync();
        public Task<bool> UpdateAsync(WeigthModel weigthConfig);
    }
}

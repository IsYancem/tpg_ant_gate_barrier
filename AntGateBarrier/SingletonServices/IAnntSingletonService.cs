using AntGateBarrier.Domain.Model;
using AntGateBarrier.Driver;

namespace AntGateBarrier.SingletonServices
{
    public interface IAnntSingletonService
    {
        public Task<bool> AddAsync(string ipaddress, CRfid rfid);
        public Task<bool> IsFoundAsync(string ipaddress);
        public Task<bool> DeleteAsync(string ipadddres);
        public Task<bool> UpdateAsync(string ipadddres, AnntModel anntConfig);
    }
}

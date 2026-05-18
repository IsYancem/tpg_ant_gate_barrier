using AntGateBarrier.Domain.Model;
using AntGateBarrier.Driver;

namespace AntGateBarrier.SingletonServices
{
    public interface IWeightSingletonService
    {

        public Task<bool> AddAsync(int gate, CWeight cWweight);
        public Task<bool> IsFoundAsync(int gate);
        public Task<bool> DeleteAsync(int gate);
        public Task<bool> UpdateAsync(int gate, WeigthModel weigtConfig);
        public Task<int> GetWeightAsync(int gate);
    }
}

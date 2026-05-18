using AntGateBarrier.Domain.Model;

namespace AntGateBarrier.Domain.Controller
{
    public interface ISqlServerController
    {
        public Task<List<VehicleModel>> GetVehicles(int all);
        public Task<List<AnntModel>> GetAnnts(int all,int group);
        public Task<List<WeigthModel>> GetWeigths(int all, int group);
    }
}

using AntGateBarrier.Domain.Model;

namespace AntGateBarrier.Domain.Controller
{
    public interface ISqlMemoryController
    {
        public Task<bool> AddVehicle(VehicleModel vehicle);
        public Task<VehicleModel> GetVehicle(string hexa, int last);
        public Task<bool> AddAnntRfid(AnntRfidModel anntRfid);
        public Task<List<AnntRfidModel>> GetAnntRfids();


    }
}

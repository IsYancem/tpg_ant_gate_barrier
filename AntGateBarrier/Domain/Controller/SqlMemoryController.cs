using AntGateBarrier.Domain.Factory;
using AntGateBarrier.Domain.Model;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace AntGateBarrier.Domain.Controller
{
    public class SqlMemoryController : ISqlMemoryController
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMemoryContextFactory _memoryContextFactory;
        public SqlMemoryController(IMemoryContextFactory memoryContextFactory) { _memoryContextFactory = memoryContextFactory; }
        
        public async Task<bool> AddVehicle(VehicleModel vehicle)
        {
            var _state = false;
            try
            {
                var _memoryContext = _memoryContextFactory.CreateDbContext();
                if (await _memoryContext.Vehicles.FindAsync(vehicle.REGNUMBER) is VehicleModel found)
                {
                    found.RFID = vehicle.RFID;
                    found.HEXASTRING = vehicle.HEXASTRING;
                    found.BRAND = vehicle.BRAND;
                    found.MODEL = vehicle.MODEL;
                    found.COLOR = vehicle.COLOR;
                    found.COMPANY = vehicle.COMPANY;
                    found.MESSAGE = vehicle.MESSAGE;
                    found.STATE = vehicle.STATE;
                }
                else { await _memoryContext.Vehicles.AddAsync(vehicle); }
                await _memoryContext.SaveChangesAsync();
                _state = true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message}");
            }
            return _state;

        }

        public async Task<bool> AddAnntRfid(AnntRfidModel anntRfid)
        {
            var _state = false;
            try
            {
                var _memoryContext = _memoryContextFactory.CreateDbContext();
                if (await _memoryContext.AnntRfids.FindAsync(anntRfid.IpAddress) is AnntRfidModel found)
                {
                    found.gate = anntRfid.gate;
                    found.Name = anntRfid.Name;
                    found.Plate = anntRfid.Plate;
                    found.Number = anntRfid.Number; 
                    found.Hexa = anntRfid.Hexa;
                    found.Side = anntRfid.Side;
                    found.Site = anntRfid.Site;
                    found.Code = anntRfid.Code;
                    found.Message = anntRfid.Message;
                    found.Now = anntRfid.Now;
                    if (anntRfid.IsRegistered == 1)
                    {
                        found.Registered = anntRfid.Registered;
                    }
                }
                else { await _memoryContext.AnntRfids.AddAsync(anntRfid); }
                await _memoryContext.SaveChangesAsync();
                _state = true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message}");
            }
            return _state;

        }

        public async Task<VehicleModel> GetVehicle(string hexa,int last)
        {
            VehicleModel? _vehicle = null;
            try
            {
                var _memoryContext = _memoryContextFactory.CreateDbContext();
                _vehicle = await _memoryContext.Vehicles.Where(row => row.HEXASTRING == hexa).SingleOrDefaultAsync();
                if (_vehicle == null) {
                    _vehicle = await _memoryContext.Vehicles.Where(row => row.HEXASTRING == hexa.Substring(0,last)).SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex.Message}");
            }

            return _vehicle;
        }

        public async Task<List<AnntRfidModel>> GetAnntRfids()
        {
            var _anntRfids = new List<AnntRfidModel>();
            try
            {
                var _memoryContext = _memoryContextFactory.CreateDbContext();
                _anntRfids = await _memoryContext.AnntRfids.OrderBy(row => row.gate).ToListAsync();

            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _anntRfids;
        }
    }
}

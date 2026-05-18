using AntGateBarrier.Domain.Factory;
using AntGateBarrier.Domain.Model;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AntGateBarrier.Domain.Controller
{
    public class SqlServerController : ISqlServerController
    {
        private readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISqlServerContextFactory _sqlServerContextFactory;

        public SqlServerController(ISqlServerContextFactory sqlServerContextFactory) 
        {
            _sqlServerContextFactory = sqlServerContextFactory; 
        }

        public async  Task<List<AnntModel>> GetAnnts(int all, int group)
        {
            var _result =  new List<AnntModel>();
            try
            {
                var _sqlServerContext = _sqlServerContextFactory.CreateDbContext();
                _result = await _sqlServerContext.Annts.FromSql($"EXEC atack.dbo.atk_get_ant_group_plc {all},{group}").AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex}");
            }
            return _result;
        }

        public async  Task<List<WeigthModel>> GetWeigths(int all, int group)
        {
            var _result = new List<WeigthModel>();
            try
            {
                var _sqlServerContext = _sqlServerContextFactory.CreateDbContext();
                _result = await _sqlServerContext.Weigths.FromSql($"EXEC atack.dbo.atk_get_weight_gate {all},{group}").AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex}");
            }
            return _result;
        }

        public async Task<List<VehicleModel>> GetVehicles(int all)
        {
            var _result = new List<VehicleModel>();
            try
            {
                var _sqlServerContext = _sqlServerContextFactory.CreateDbContext();
                _result = await _sqlServerContext.Vehicles.FromSql($"EXEC atack.dbo.atk_get_vehicle_mid {all}").AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _log.Error($"Exception:{ex}");
            }
            return _result;
        }
    }

}

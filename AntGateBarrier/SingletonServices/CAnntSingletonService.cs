using AntGateBarrier.Domain.Model;
using AntGateBarrier.Driver;
using log4net;
using System.Net;

namespace AntGateBarrier.SingletonServices
{
    public class CAnntSingletonService : IAnntSingletonService
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string,CRfid> _rfids { get; set; }
        public CAnntSingletonService()
        {
            _rfids = new Dictionary<string, CRfid>();
        }

        public async Task<bool> AddAsync(string ipaddress, CRfid rfid)
        {
            var _state = false;
            try
            {
                _rfids.Add(ipaddress, rfid);
                _state = true;
            }
            catch (Exception ex){_log.Error($"Exception:{ex.Message}");}
            return _state;
        }

        public async Task<bool> IsFoundAsync(string ipaddress)
        {
            var _state = false;
            try
            {
                _state = _rfids.ContainsKey(ipaddress);
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;

        }

        public async Task<bool> DeleteAsync(string ipadddres)
        {
            var _state = false;
            try
            {
                _state = _rfids.Remove(ipadddres);
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task<bool> UpdateAsync(string ipadddres, AnntModel anntConfig)
        {
            var _state = false;
            var _rfid = new CRfid();
            try
            {
                _state = _rfids.TryGetValue(ipadddres,out _rfid);
                if (_state)
                {
                    _state = await _rfid.UpdateAsync(anntConfig);
                }
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }
    }

}

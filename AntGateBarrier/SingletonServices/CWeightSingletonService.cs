using AntGateBarrier.Domain.Model;
using AntGateBarrier.Driver;
using log4net;

namespace AntGateBarrier.SingletonServices
{
    public class CWeightSingletonService : IWeightSingletonService
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<int, CWeight> _weights;

        public CWeightSingletonService()
        {
            _weights = new Dictionary<int, CWeight>();
        }

        public async Task<bool> AddAsync(int gate, CWeight cWweight)
        {
            var _state = false;
            try
            {
                _weights.Add(gate, cWweight);
                _state = true;
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task<bool> DeleteAsync(int gate)
        {
            var _state = false;
            try
            {
                _state = _weights.Remove(gate);
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task<int> GetWeightAsync(int gate)
        {
            var _weight = 0; // FIX: si el gate no tiene báscula registrada, peso neutro (no -1)
            var _cWeigth = new CWeight();
            try
            {
                var _state = _weights.TryGetValue(gate, out _cWeigth);
                if (_state)
                {
                    _weight = await _cWeigth.GetWeightAsync();
                }
                else
                {
                    _log.Error("Equipment is not assigned to scale");
                }
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _weight;
        }

        public async Task<bool> IsFoundAsync(int gate)
        {
            var _state = false;
            try
            {
                _state = _weights.ContainsKey(gate);
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }

        public async Task<bool> UpdateAsync(int gate, WeigthModel weigthConfig)
        {
            var _state = false;
            var _weigth = new CWeight();
            try
            {
                _state = _weights.TryGetValue(gate, out _weigth);
                if (_state)
                {
                    _state = await _weigth.UpdateAsync(weigthConfig);
                }
            }
            catch (Exception ex) { _log.Error($"Exception:{ex.Message}"); }
            return _state;
        }
    }
}
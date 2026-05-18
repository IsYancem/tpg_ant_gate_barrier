using AntGateBarrier.Entity;
using Newtonsoft.Json;



namespace AntGateBarrier.SingletonServices
{
    public class CAppSettings : IAppSettings
    {

        public ConnectionStrings? GetConnectionStrings()
        {
            ConnectionStrings _connectionStrings = new();
            IAppSettings._config.Bind("ConnectionStrings", _connectionStrings);
            return _connectionStrings;
        }

        public Settings? GetSettings()
        {
            Settings _settings = new();
            IAppSettings._config.Bind("Settings", _settings);
            return _settings;
        }

        public string Encode(string text)
        {
            byte[] mybyte = System.Text.Encoding.UTF8.GetBytes(text);
            string returntext = Convert.ToBase64String(mybyte);
            return returntext;
        }

        public string Decode(string text)
        {
            byte[] mybyte = Convert.FromBase64String(text);
            string returntext = System.Text.Encoding.UTF8.GetString(mybyte);
            return returntext;
        }

        public Cors? GetCors()
        {
            Cors _cors = new();
            IAppSettings._config.Bind("Cors", _cors);
            return _cors;
        }

        public async Task<ResponseMethod> PostAuth(string plate, int gate, int side, int site, string tag, string ipaddress_annt, string ipaddress_barrier,string url_auth,string key_auth)
        {
            ResponseMethod _responseMethod = new();
            _responseMethod.Code = 1;
            _responseMethod.Msg = "INIT PostAuth";
            try
            {
                HttpClient _client = new HttpClient();
                var _settings = GetSettings();
                var _access_token = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                _access_token = Encode(_access_token);

                var content = new FormUrlEncodedContent(new[]
                 {
                    new KeyValuePair<string, string>("plate", plate.ToString()),
                    new KeyValuePair<string, string>("gate", gate.ToString()),
                    new KeyValuePair<string, string>("side", side.ToString()),
                    new KeyValuePair<string, string>("site", site.ToString()),
                    new KeyValuePair<string, string>("tag", tag.ToString()),
                    new KeyValuePair<string, string>("ipaddress_annt", ipaddress_annt.ToString()),
                    new KeyValuePair<string, string>("ipaddress_barrier", ipaddress_barrier.ToString()),
                });

                _client.DefaultRequestHeaders.Add("x-api-key", $"{key_auth}:{_access_token}");
                var _response = await _client.PostAsync(url_auth, content);
                if (!_response.IsSuccessStatusCode)
                {
                    _responseMethod.Msg = _response.ReasonPhrase;

                }
                else
                {
                    _responseMethod = await _response.Content.ReadFromJsonAsync<ResponseMethod>();
                    _responseMethod.Msg = $"{_responseMethod.Msg} -- SIDE:{await SideToString(side)} -- PLATE:{plate}";
                }
            }
            catch (Exception ex) { _responseMethod.Msg = ex.Message; }
            return _responseMethod;
        }

        public async Task<string> SideToString(int side)
        {
            string ToString = $"THERE IS NO SIDE {side}";
            if (side == 1) { ToString = "STREET"; }
            if (side == 2) { ToString = "YARD"; }
            if (side == 3) { ToString = "OTHERS RELAY 3"; }
            if (side == 4) { ToString = "OTHERS RELAY 4"; }
            return ToString;
        }

        public async Task<string> SiteToString(int site)
        {
            string ToString = $"TPG {site} ";

            return ToString;
        }

        public async Task<ResponseMethod> PostUnLockGateC3(string plate, int gate, int side, string ipaddress_barrier, string url_barrier, string key_barrier)
        {
            ResponseMethod _responseMethod = new();

            try
            {
                HttpClient _client = new HttpClient();
                var _settings = GetSettings();

                var _data = new CData();
                _data.device = "0";
                _data.gate = ipaddress_barrier;
                _data.relay = side;
                _data.key_id = key_barrier;
                _data.plate = plate;
                var jsonData = JsonConvert.SerializeObject(_data);
                var jsonEncode = Encode(jsonData);

                var parameter = new FormUrlEncodedContent(new[]
                 {
                    new KeyValuePair<string, string>("key",jsonEncode)
                });
                var _response = await _client.PostAsync(url_barrier, parameter);
                if (!_response.IsSuccessStatusCode)
                {
                    _responseMethod.Msg = _response.ReasonPhrase;
                }
                else
                {
                    var _cKey = await _response.Content.ReadFromJsonAsync<CKey>();
                    if (_cKey.key.Length <= 0)
                    {
                        _responseMethod.Code = 1;
                        _responseMethod.Msg = $"PostUnLockGateC3 Request not defined";
                    }
                    else
                    {
                        var valueJson = Decode(_cKey.key);
                        _responseMethod = JsonConvert.DeserializeObject<ResponseMethod>(valueJson);
                    }

                }
            }
            catch (Exception ex) { _responseMethod.Msg = ex.Message; }
            return _responseMethod;
        }

        public async Task<ResponsePlcMethod> PostUnLockGatePLC(string plate, int gate, int side, string url_barrier, string key_barrier)
        {
            ResponsePlcMethod _responseMethod = new();
            try
            {
                HttpClient _client = new HttpClient();
                var _settings = GetSettings();

                var _data = new CDataPlc();
                _data.gate = gate;
                _data.api_key = key_barrier;
                _data.side = side;
                _data.plate = plate;
                var jsonData = JsonConvert.SerializeObject(_data);
                var jsonEncode = Encode(jsonData);

                var parameter = new FormUrlEncodedContent(new[]
                 {
                    new KeyValuePair<string, string>("token",jsonEncode)
                });
                var _response = await _client.PostAsync(url_barrier, parameter);
                if (!_response.IsSuccessStatusCode)
                {
                    _responseMethod.Msg = _response.ReasonPhrase;
                }
                else
                {
                    _responseMethod = await _response.Content.ReadFromJsonAsync<ResponsePlcMethod>();
                }
            }
            catch (Exception ex) { _responseMethod.Msg = $"{ex.Message}/{url_barrier}"; }
            return _responseMethod;

        }
    }
}

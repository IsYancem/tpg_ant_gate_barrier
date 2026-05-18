using AntGateBarrier.Entity;

namespace AntGateBarrier.SingletonServices
{
    public interface IAppSettings
    {
        protected static readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public ConnectionStrings? GetConnectionStrings();
        public Settings? GetSettings();
        public Cors? GetCors();
        public string Encode(string value);
        public string Decode(string value);
        public Task<ResponseMethod> PostAuth(string plate, int gate, int side, int site, string tag, string ipaddress_annt, string ipaddress_barrier, string url_auth, string key_auth);
        public Task<ResponseMethod> PostUnLockGateC3(string plate, int gate, int side,string ipaddress_barrier,string url_barrier,string key_barrier);
        public Task<ResponsePlcMethod> PostUnLockGatePLC(string plate, int gate, int side, string url_barrier, string key_barrier);
        public Task<string> SideToString(int side);
        public Task<string> SiteToString(int site);
    }
}

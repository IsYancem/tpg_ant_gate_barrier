using System.ComponentModel.DataAnnotations;

namespace AntGateBarrier.Domain.Model
{
    public class AnntModel
    {
        [Key]
        public string ip_address { set; get; }
        public int port { set; get; }
        public string name { set; get; }
        public int gate { set; get; }
        public int side { set; get; }
        public string ipaddress_barrier { set; get; }
        public string search { set; get; }
        public int first { set; get; }
        public int last { set; get; }
        public int max { set; get; }
        public int wait { set; get; }
        public int sensor { set; get; }
        public int isplc { set; get; }
        public string url_auth { set; get; }
        public string key_auth { set; get; }
        public string url_barrier { set; get; }
        public string key_barrier { set; get; }
        public int group_gate { set; get; }
        public int site { set; get; }
        public int state { set; get; }
    }
}

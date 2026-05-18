using System.ComponentModel.DataAnnotations;

namespace AntGateBarrier.Domain.Model
{
    public class WeigthModel
    {
        [Key]
        public int gate { set; get; }
        public string ip_address { set; get; }
        public int port { set; get; }
        public string name { set; get; }
        public int wait { set; get; }
        public int group_gate { set; get; }
        public int state { set; get; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AntGateBarrier.Domain.Model
{
    public class VehicleModel
    {
        [Key]
        public string REGNUMBER { set; get; }
        public string RFID { set; get; }
        public string BRAND { set; get; }
        public string MODEL { set; get; }
        public string COLOR { set; get; }
        public string COMPANY { set; get; }
        public string HEXASTRING { set; get; }
        public string MESSAGE { set; get; }
        public int STATE { set; get; }
        public int IDVEHICLE { set; get; }   
        public int IDCOMPANY { set; get; }  
    }
}
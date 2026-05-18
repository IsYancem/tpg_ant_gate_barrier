using System.ComponentModel.DataAnnotations;

namespace AntGateBarrier.Domain.Model
{
    public class AnntRfidModel
    {
        [Key]
        public string? IpAddress { get; set; } = ""; 
        public int gate { get; set; } = 0;
        public string? Name { get; set; } = "";
        public string? Plate { get; set; } = "";
        public string? Number { get; set; } = "";
        public string? Hexa { get; set; } = "";
        public string? Side { get; set; } = "";
        public string? Site { get; set; } = "TPG";
        public int Code { get; set; } = 1;
        public string? Message { get; set; } = "Conectando ServicesAsync";
        public int IsRegistered { get; set; } = 0;
        public string? Registered { get; set; } = "";
        public string? Now { get; set; } = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
    }
}

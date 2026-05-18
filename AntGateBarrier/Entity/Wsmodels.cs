namespace AntGateBarrier.Entity
{
    // ── RFID WS ──────────────────────────────────────────────
    public class RfidWsMessage
    {
        public int code { get; set; }
        public string message { get; set; }
        public int gate { get; set; }
        public int side { get; set; }
        public RfidWsRecord record { get; set; }
    }

    public class RfidWsRecord
    {
        public string RegNumber { get; set; }
        public string Rfid { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Company { get; set; }
        public string Hexastring { get; set; }
        public string Message { get; set; }
        public int IdVehicle { get; set; }
        public int IdCompany { get; set; }
        public int State { get; set; }
    }

    // ── WEIGHT WS ─────────────────────────────────────────────
    public class WeightWsMessage
    {
        public int code { get; set; }
        public string message { get; set; }
        public int gate { get; set; }
        public int side { get; set; }   // siempre 0 para peso
        public WeightWsRecord record { get; set; }
    }

    public class WeightWsRecord
    {
        public int Weight { get; set; }
    }
}
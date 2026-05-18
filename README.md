<!DOCTYPE html>
<html lang="es">
<body>

<h1>🚧 AntGateBarrier</h1>

<p>
<b>AntGateBarrier</b> es un backend industrial en <b>.NET</b> que integra:
</p>

<ul>
    <li>📡 Antenas RFID por <b>TCP</b></li>
    <li>⚖️ Básculas de peso por <b>TCP</b></li>
    <li>🔌 WebSockets en tiempo real</li>
    <li>🚪 Control de barreras (PLC / C3)</li>
    <li>🧠 Estado centralizado en memoria</li>
</ul>

---

<h2>🧠 Arquitectura General</h2>

<pre>
BÁSCULA (TCP) ──▶ CWeight ──▶ WEIGHT WS
                     │
RFID (TCP) ─────▶ CRfid ───▶ REALTIME WS
                     │
                     └──▶ KIOSKO WS
</pre>

---

<h2>🔐 Seguridad</h2>

<div class="box">
<ul>
    <li>🔑 Todas las APIs críticas requieren <b>ApiKey</b></li>
    <li>📩 Se envía por Header HTTP</li>
</ul>

<pre>
ApiKey: TU_API_KEY
</pre>
</div>

---

<h2>⚙️ Configuración</h2>

<h4>appsettings.json</h4>

<pre>
{
  "Settings": {
    "ServiceName": "AntennaGroup",
    "Group": 4,
    "ApiKey": "TU_API_KEY"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
</pre>

---

<h2>▶️ Ejecución</h2>

<pre>
dotnet restore
dotnet build
dotnet run
</pre>

---

<h2>🌐 WebSockets Disponibles</h2>

<h3>1️⃣ WebSocket General (RFID + Vehículo)</h3>

<div class="box">
<p><span class="tag">URL</span></p>
<pre>ws://localhost:8080/api/ws</pre>

<p><span class="tag">Servicio</span></p>
<pre>IRealtimeWs → CRealtimeWs</pre>

<p><span class="tag">Payload</span></p>
<pre>
{
  "code": 0,
  "message": "Record Found",
  "gate": 92,
  "side": "STREET",
  "ip_address": "172.16.102.254",
  "record": {
    "regNumber": "SIS0001",
    "rfid": "001769",
    "company": "TPG",
    "state": 1
  }
}
</pre>
</div>

---

<h3>2️⃣ WebSocket Kiosko</h3>

<div class="box">
<pre>ws://localhost:8080/api/kiosko</pre>

<p>Header requerido:</p>
<pre>ApiKey: TU_API_KEY</pre>

<p>Mensajes simplificados para pantallas y kioskos</p>
</div>

---

<h3>3️⃣ WebSocket RFID</h3>

<div class="box">
<pre>ws://localhost:8080/api/rfid/ws</pre>
<p>Eventos RFID en tiempo real</p>
</div>

---

<h3>4️⃣ WebSocket Báscula de Peso</h3>

<div class="box">
<pre>ws://localhost:8080/api/weight/ws</pre>

<p>Header:</p>
<pre>ApiKey: TU_API_KEY</pre>

<p>Payload:</p>
<pre>
{
  "gate": 92,
  "ip": "172.16.101.102",
  "weight": 18400,
  "timestamp": "2026-01-19 09:45:11"
}
</pre>
</div>

---

<h2>🔁 TCP Internos</h2>

<h3>📡 RFID TCP</h3>

<pre>
Driver/CRfid.cs
</pre>

<ul>
    <li>Lectura HEX</li>
    <li>Validación de vehículo</li>
    <li>Apertura automática de barrera</li>
    <li>Publica eventos a WebSocket</li>
</ul>

---

<h3>⚖️ Báscula TCP</h3>

<pre>
Driver/CWeight.cs
</pre>

<pre>
Comando: r wt0101\n
Respuesta: 00R405~18400~
</pre>

<ul>
    <li>Lectura continua</li>
    <li>No bloquea TCP</li>
    <li>Replica datos al WS</li>
</ul>

---

<h2>🌍 REST APIs</h2>

<h3>🔓 Abrir Barrera Manual</h3>

<pre>
POST http://localhost:8080/api/barrier/open
</pre>

<p>Header:</p>
<pre>x-api-key: TU_API_KEY</pre>

<p>Body (Base64):</p>
<pre>
{
  "ip": "MTcyLjE2LjEwMi4yNTQ=",
  "plate": "U0lTMDAwMQ=="
}
</pre>

---

<h3>🧪 Inyección Manual RFID</h3>

<pre>
POST http://localhost:8080/api/rfid/inject
</pre>

<pre>
{
  "ip": "172.16.102.254",
  "plate": "SIS0001"
}
</pre>

---

<h3>📊 Eventos RFID en memoria</h3>

<pre>
GET http://localhost:8080/api/rfid
</pre>

---

<h2>🧵 Servicios Background</h2>

<pre>
WeightBackgroundServices
VehicleBackgroundServices
AnntBackgroundServices
</pre>

---

<h2>🧩 Componentes Clave</h2>

<pre>
CRfid            → RFID TCP
CWeight          → Báscula TCP
IRealtimeWs      → WS General
IKioskoWs        → WS Kiosko
IWeightWs        → WS Peso
CBarrierService  → Control de barreras
</pre>

---

<h2>🧠 Principios</h2>

<ul>
    <li>🔁 TCP nunca se bloquea</li>
    <li>🔌 WS desacoplados</li>
    <li>🧠 Estado en memoria</li>
    <li>🔐 Seguridad por ApiKey</li>
    <li>🧪 Simulación sin hardware</li>
</ul>

---

<h2>👤 Autor</h2>

<p>
<b>Abraham Yance</b><br>
Arquitectura industrial · RFID · PLC · TCP · WebSockets · .NET
</p>

</body>
</html>


**Estado:** ✅ **v1.0.0 (Stable)**  
**Último release/tag:** `v1.0.0`  
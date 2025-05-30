using ShipSimulatorBackend.Models; // For � f� tilgang til Ship-klassen
using System.Collections.Concurrent; // For tr�dsikker samling (ConcurrentDictionary)
using System.Threading;

    


namespace ShipSimulatorBackend.Services
{
    public class SimulationManager
    {
        // ConcurrentDictionary er en tr�dsikker samling, noe som betyr at flere tr�der
        // trygt kan lese og skrive til den samtidig. Dette er viktig n�r vi har en
        // bakgrunnstr�d som oppdaterer skip og en web-tr�d som leser/skriver.
        private readonly ConcurrentDictionary<string, Ship> _ships = new();

        // System.Timers.Timer er en timer som kan kj�res i en egen tr�d
        // og kaller en spesifikk metode periodisk.
        private System.Threading.Timer? _simulationTimer;

        // Konstant for hvor ofte simuleringen skal oppdateres (hvert 0.1 sekund).
        private const double SimulationIntervalSeconds = 0.1;
        // Konstant for hvor raskt skipene kan svinge (grader per sekund).
        private const double ShipTurningSpeedDegreesPerSecond = 20.0;

        // Konstrukt�r for SimulationManager. Den kj�res n�r en instans av klassen opprettes.
        public SimulationManager()
        {
            // Initialiserer et par skip for � starte med i simuleringen.
            // Du kan endre startposisjoner, retninger og hastigheter her.
            AddShip(new Ship(100, 100, 90, 50)); // Skip 1: starter p� (100,100), peker �st, fart 50
            AddShip(new Ship(500, 500, 270, 70)); // Skip 2: starter p� (500,500), peker Vest, fart 70
        }

        /// <summary>
        /// Legger til et nytt skip i simuleringen.
        /// </summary>
        /// <param name="ship">Skipsobjektet som skal legges til.</param>
        public void AddShip(Ship ship)
        {
            _ships.TryAdd(ship.Id, ship); // TryAdd sikrer at det ikke legges til hvis ID allerede finnes
        }

        /// <summary>
        /// Returnerer en samling av alle skipene i simuleringen.
        /// </summary>
        public IEnumerable<Ship> GetAllShips()
        {
            return _ships.Values;
        }

        /// <summary>
        /// Setter en ny m�lposisjon for et spesifikt skip.
        /// </summary>
        /// <param name="shipId">ID-en til skipet.</param>
        /// <param name="targetX">Ny X-koordinat for m�let.</param>
        /// <param name="targetY">Ny Y-koordinat for m�let.</param>
        /// <returns>True hvis m�let ble satt, false hvis skipet ikke ble funnet.</returns>
        public bool SetShipTarget(string shipId, double targetX, double targetY)
        {
            if (_ships.TryGetValue(shipId, out var ship))
            {
                // Hvis skipet finnes, oppdaterer vi dets m�lposisjon.
                ship.TargetX = targetX;
                ship.TargetY = targetY;
                return true;
            }
            return false; // Skipet ble ikke funnet
        }

        /// <summary>
        /// Starter simuleringstimeren, som periodisk kaller Tick-metoden.
        /// </summary>
        public void StartSimulation()
        {
            if (_simulationTimer == null)
            {
                // Oppretter en ny timer:
                // 1. Tick er metoden som kalles.
                // 2. null er state-objektet (trenger ikke her).
                // 3. TimeSpan.Zero betyr start med en gang.
                // 4. TimeSpan.FromSeconds(SimulationIntervalSeconds) er intervallet mellom kallene.
                _simulationTimer = new System.Threading.Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(SimulationIntervalSeconds));
                // System.Threading.Timer starter automatisk n�r den opprettes med intervall
                Console.WriteLine("Simulation started."); // Skriver til Visual Studios Output-vindu
            }
        }

        /// <summary>
        /// Stopper simuleringstimeren og frigj�r ressurser.
        /// </summary>
        public void StopSimulation()
        {
            _simulationTimer?.Dispose(); // Dispose() stopper og frigj�r ressurser for System.Threading.Timer
            _simulationTimer = null;
            Console.WriteLine("Simulation stopped.");
        }

        /// <summary>
        /// Hovedmetoden for simuleringen. Kalles periodisk av _simulationTimer.
        /// </summary>
        /// <param name="state">Objekt som sendes med timeren (ikke brukt her).</param>
        private void Tick(object? state)
        {
            // Itererer gjennom alle skipene i simuleringen.
            foreach (var shipEntry in _ships)
            {
                var ship = shipEntry.Value; // Henter ut Ship-objektet

                // Sjekker om skipet har n�dd sitt n�v�rende m�l.
                if (!ship.HasReachedTarget())
                {
                    // Hvis ikke, styrer vi skipet mot m�let.
                    ship.SteerTowardsTarget(SimulationIntervalSeconds, ShipTurningSpeedDegreesPerSecond);
                    // Og oppdaterer deretter skipets posisjon basert p� ny retning og hastighet.
                    ship.UpdatePosition(SimulationIntervalSeconds);
                }
                // Hvis skipet har n�dd m�let, gj�r det ingenting inntil et nytt m�l blir satt.
                // Senere kan du legge til logikk for � sette nye tilfeldige m�l her!
            }

            // Viktig for senere: Her kommer logikken for � sende
            // oppdaterte skipsposisjoner til de tilkoblede klientene (frontend)
            // n�r vi implementerer SignalR!
        }
    }
}
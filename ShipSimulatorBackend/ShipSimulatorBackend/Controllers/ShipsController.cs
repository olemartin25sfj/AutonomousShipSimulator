using Microsoft.AspNetCore.Mvc; // N�dvendig for API-kontrollere
using ShipSimulatorBackend.Models; // For � f� tilgang til Ship-klassen
using ShipSimulatorBackend.Services; // For � f� tilgang til SimulationManager

namespace ShipSimulatorBackend.Controllers
{
	[ApiController] // Dette attributtet forteller ASP.NET Core at dette er en API-kontroller
	[Route("api/[controller]")] // Definerer ruten for denne kontrolleren. [controller] blir erstattet med "Ships", alts� /api/Ships
	public class ShipsController : ControllerBase // Arver fra ControllerBase, som er grunnlaget for API-kontrollere
	{
		private readonly SimulationManager _simulationManager;

		// Dette er kontrollerens konstrukt�r.
		// ASP.NET Core bruker "Dependency Injection" (DI) til automatisk � "injisere" (gi oss)
		// en instans av SimulationManager her, fordi vi registrerte den som en Singleton i Program.cs.
		public ShipsController(SimulationManager simulationManager)
		{
			_simulationManager = simulationManager;
		}

		/// <summary>
		/// Henter alle skipenes n�v�rende posisjoner og tilstand.
		/// Dette svarer p� HTTP GET-foresp�rsler til /api/Ships.
		/// </summary>
		/// <returns>En liste av Ship-objekter.</returns>
		[HttpGet] // Angir at denne metoden h�ndterer HTTP GET-foresp�rsler
		public ActionResult<IEnumerable<Ship>> GetShips()
		{
			// Returnerer en HTTP 200 OK-status med listen over skip som JSON.
			return Ok(_simulationManager.GetAllShips());
		}

		/// <summary>
		/// Setter en ny m�lposisjon for et spesifikt skip.
		/// Dette svarer p� HTTP POST-foresp�rsler til /api/Ships/{shipId}/settarget.
		/// </summary>
		/// <param name="shipId">Den unike ID-en til skipet som skal ha nytt m�l.</param>
		/// <param name="request">Et objekt som inneholder de nye X- og Y-koordinatene for m�let.</param>
		/// <returns>En bekreftelse p� at m�let er satt, eller en feilmelding hvis skipet ikke ble funnet.</returns>
		[HttpPost("{shipId}/settarget")] // Angir at denne metoden h�ndterer HTTP POST-foresp�rsler
										 // {shipId} i ruten betyr at en del av URL-en vil v�re skipets ID.
		public IActionResult SetShipTarget(string shipId, [FromBody] TargetRequest request)
		{
			// Bruker SimulationManager til � fors�ke � sette m�let.
			if (_simulationManager.SetShipTarget(shipId, request.TargetX, request.TargetY))
			{
				// Hvis det lykkes, returnerer vi HTTP 200 OK med en bekreftelsesmelding.
				return Ok(new { Message = $"Target set for ship {shipId} to ({request.TargetX},{request.TargetY})" });
			}
			// Hvis skipet ikke ble funnet, returnerer vi HTTP 404 Not Found.
			return NotFound($"Ship with ID {shipId} not found.");
		}
	}

	// Dette er en enkel "hjelpeklasse" (Data Transfer Object - DTO)
	// for � representere dataen som forventes i "body" av POST-foresp�rselen
	// n�r vi setter et nytt m�l.
	public class TargetRequest
	{
		public double TargetX { get; set; }
		public double TargetY { get; set; }
	}
}
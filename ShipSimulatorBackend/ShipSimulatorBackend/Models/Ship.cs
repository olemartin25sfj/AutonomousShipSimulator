namespace ShipSimulatorBackend.Models
{
    public class Ship
    {
        // En unik identifikator for hvert skip.
        // Guid.NewGuid().ToString() genererer en helt unik streng automatisk.
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Skipets nåværende posisjon på kartet.
        // Vi bruker double for presise koordinater.
        public double PositionX { get; set; }
        public double PositionY { get; set; }

        // Skipets retning i grader (0-360).
        // Konvensjonen vi bruker: 0 er Nord (rett opp), 90 er Øst (rett høyre), osv.
        public double Heading { get; set; }

        // Skipets hastighet. Vi antar en konstant hastighet for enkelhetens skyld.
        public double Speed { get; set; } // F.eks. enheter per sekund

        // Målposisjonen skipet skal styre mot.
        public double TargetX { get; set; }
        public double TargetY { get; set; }

        // Konstruktør for å opprette et nytt Ship-objekt.
        // Den tar inn startverdier for posisjon, retning og hastighet.
        public Ship(double initialX, double initialY, double initialHeading, double speed)
        {
            PositionX = initialX;
            PositionY = initialY;
            Heading = initialHeading;
            Speed = speed;
            // Ved opprettelse er målet der skipet er, så det ikke beveger seg med en gang.
            TargetX = initialX;
            TargetY = initialY;
        }

        /// <summary>
        /// Oppdaterer skipets posisjon basert på dets nåværende hastighet og retning.
        /// </summary>
        /// <param name="deltaTimeSeconds">Hvor mye tid (i sekunder) som har gått siden sist oppdatering.</param>
        public void UpdatePosition(double deltaTimeSeconds)
        {
            // Beregn hvor langt skipet beveger seg i løpet av dette tidsintervallet.
            double distance = Speed * deltaTimeSeconds;

            // Konverter heading fra grader til radianer.
            // Math.Cos og Math.Sin forventer radianer.
            // Standard trigonometri har 0 radianer langs den positive X-aksen (øst).
            // Vi justerer for vår konvensjon hvor 0 grader er Nord (positiv Y-akse).
            double headingRadians = (Heading - 90) * (Math.PI / 180);

            // Beregn endring i X- og Y-posisjon basert på avstand og retning.
            PositionX += distance * Math.Cos(headingRadians);
            PositionY += distance * Math.Sin(headingRadians);
        }

        /// <summary>
        /// Justerer skipets retning (Heading) for å styre mot TargetX/Y.
        /// </summary>
        /// <param name="deltaTimeSeconds">Hvor mye tid (i sekunder) som har gått.</param>
        /// <param name="turningSpeedDegreesPerSecond">Hvor raskt skipet kan svinge i grader per sekund.</param>
        public void SteerTowardsTarget(double deltaTimeSeconds, double turningSpeedDegreesPerSecond)
        {
            // Beregn vinkelen (i radianer) fra skipets nåværende posisjon til målposisjonen.
            // Math.Atan2 gir vinkel mellom -PI og PI (-180 til 180 grader), hvor 0 er øst.
            double angleToTargetRadians = Math.Atan2(TargetY - PositionY, TargetX - PositionX);
            double angleToTargetDegrees = angleToTargetRadians * (180 / Math.PI);

            // Juster vinkelen til vår 0=Nord konvensjon (0-360 grader med klokken).
            angleToTargetDegrees = (angleToTargetDegrees + 360 + 90) % 360;

            // Beregn differansen mellom nåværende heading og vinkel til mål.
            // Vi vil finne den korteste svingen (med eller mot klokken).
            double headingDifference = angleToTargetDegrees - Heading;
            if (headingDifference > 180)
            {
                headingDifference -= 360;
            }
            else if (headingDifference < -180)
            {
                headingDifference += 360;
            }

            // Bestem hvor mye skipet kan svinge i dette tidssteget.
            double turnAmount = turningSpeedDegreesPerSecond * deltaTimeSeconds;

            // Hvis vi er nærme nok til å svinge direkte til målvinkel, gjør det.
            if (Math.Abs(headingDifference) <= turnAmount)
            {
                Heading = angleToTargetDegrees;
            }
            else
            {
                // Sving i riktig retning (med eller mot klokken).
                if (headingDifference > 0)
                {
                    Heading += turnAmount; // Sving med klokken
                }
                else
                {
                    Heading -= turnAmount; // Sving mot klokken
                }
            }

            // Normaliser headingen slik at den alltid er mellom 0 og 360 grader.
            Heading = (Heading + 360) % 360;
        }

        /// <summary>
        /// Sjekker om skipet har nådd sitt mål (innenfor en spesifisert toleranse).
        /// </summary>
        /// <param name="tolerance">Maksimal avstand for å bli ansett som "nådd målet".</param>
        /// <returns>True hvis skipet er innenfor toleransen til målet, ellers false.</returns>
        public bool HasReachedTarget(double tolerance = 5.0)
        {
            double distance = Math.Sqrt(Math.Pow(TargetX - PositionX, 2) + Math.Pow(TargetY - PositionY, 2));
            return distance < tolerance;
        }
    }
}
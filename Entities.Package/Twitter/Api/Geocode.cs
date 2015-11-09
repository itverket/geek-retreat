namespace Entities.Twitter.Api
{
    public class Geocode
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Radius { get; set; }
        public GeocodeUnit Unit { get; set; }

        public override string ToString() => $"{ Latitude },{ Longitude },{ Radius }{ Unit.ToString().ToLower() }";
    }
}
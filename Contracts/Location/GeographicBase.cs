namespace Contracts.Location
{
    public class GeographicBase : CoordinateBase, IGeographicBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
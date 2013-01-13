using Mono.Contracts.Location.Extensions;

namespace Mono.Contracts.Location.Mappings
{
    public class GeographicDataMapper : IMapToString<GeographicData>
    {
        public string MapToString(GeographicData data)
        {
            return data.MapToString();
        }
    }
}
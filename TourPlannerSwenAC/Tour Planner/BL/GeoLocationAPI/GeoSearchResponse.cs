using Newtonsoft.Json;

namespace Tour_Planner.BL.GeoLocationAPI
{
    public class GeoSearchResponse
    {
        [JsonProperty("features")]
        public List<GeocodeFeature> Features { get; set; }

        public class GeocodeFeature
        {
            [JsonProperty("geometry")]
            public Geometry Geometry { get; set; }
        }

        public class Geometry
        {
            [JsonProperty("coordinates")]
            public List<double> Coordinates { get; set; }
        }
    }
}

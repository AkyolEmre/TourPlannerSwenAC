using Newtonsoft.Json;

namespace Tour_Planner.BL.GeoLocationAPI
{
    public class CalculatedRouteResponse
    {
        [JsonProperty("features")]
        public List<Feature> Features { get; set; }

        public class Feature
        {
            [JsonProperty("geometry")]
            public Geometry Geometry { get; set; }
            [JsonProperty("properties")]
            public Properties Properties { get; set; }
        }

        public class Properties
        {
            [JsonProperty("summary")]
            public Summary Summary { get; set; }
        }
        public class Summary
        {
            [JsonProperty("distance")]
            public double Distance { get; set; }
            [JsonProperty("duration")]
            public double Duration { get; set; }
        }

        public class Geometry
        {
            [JsonProperty("coordinates")]
            public List<List<double>> Coordinates { get; set; }
        }
    }
}

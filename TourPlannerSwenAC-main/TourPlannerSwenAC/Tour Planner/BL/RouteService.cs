using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;
using Tour_Planner.BL.GeoLocationAPI;

namespace Tour_Planner.BL
{
    public class RouteService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "5b3ce3597851110001cf6248d157c6099b64438e81765a43f9ecc90b";
        private readonly string _urlSearchGeocode = "https://api.openrouteservice.org/geocode/search";
        private readonly string _urlDirections = "https://api.openrouteservice.org/v2/directions/";

        public RouteService() { }

        static Dictionary<Models.TransportType, string> TransportDictionary = new()
        {
            { Models.TransportType.Walk, "foot-walking" },
            { Models.TransportType.Bicycle, "cycling-regular" },
            { Models.TransportType.Car, "driving-car" }
        };

        private async Task<string?> GetGeocode(string url, string apiKey, string address)
        {
            string request = $"{url}?api_key={apiKey}&text={Uri.EscapeDataString(address)}";

            HttpResponseMessage response = await _httpClient.GetAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> GetCoordinates(string address)
        {
            var coordinates = await GetGeocode(_urlSearchGeocode, _apiKey, address);

            if (!string.IsNullOrEmpty(coordinates))
            {
                var geocodeData = JsonConvert.DeserializeObject<GeoSearchResponse>(coordinates);

                if (geocodeData?.Features != null && geocodeData.Features.Count > 0)
                {
                    var coordinatesList = geocodeData.Features[0].Geometry?.Coordinates;

                    if (coordinatesList != null && coordinatesList.Count == 2)
                    {
                        return $"{coordinatesList[0].ToString(CultureInfo.InvariantCulture)}," +
                               $"{coordinatesList[1].ToString(CultureInfo.InvariantCulture)}";
                    }
                }
            }

            return null;
        }

        public async Task<(string? raw, CalculatedRouteResponse? parsed)> GetRoute(string address1, string address2, Models.TransportType transportType)
        {
            var startCoordinates = await GetCoordinates(address1);
            var endCoordinates = await GetCoordinates(address2);

            if (startCoordinates == null || endCoordinates == null)
                return (null, null);

            string profile = TransportDictionary.TryGetValue(transportType, out var p)
                ? p
                : TransportDictionary[Models.TransportType.Car];

            string requestUrl = $"{_urlDirections}{profile}?api_key={_apiKey}&start={startCoordinates}&end={endCoordinates}";

            try
            {
                using var response = await _httpClient.GetAsync(requestUrl);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null);
                }

                var data = JsonConvert.DeserializeObject<CalculatedRouteResponse>(body);
                if (data == null)
                {
                    return (body, null);
                }

                return (body, data);
            }
            catch (HttpRequestException)
            {
                return (null, null);
            }
        }
    }
}

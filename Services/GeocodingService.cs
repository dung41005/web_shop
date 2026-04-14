using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UC.eComm.Publish.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;

        public GeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Lấy tọa độ (lat, lng) từ địa chỉ văn bản
        /// </summary>
        /// <param name="address">Địa chỉ đầy đủ</param>
        /// <returns>Tuple (Lat, Lng) hoặc (null, null) nếu không tìm thấy</returns>
        public async Task<(double? Lat, double? Lng)> GetCoordinatesAsync(string address)
        {
            try
            {
                // Sử dụng OpenStreetMap Nominatim (miễn phí)
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

                // Nominatim yêu cầu User-Agent
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UCeComm/1.0 (nicholasneed@gmail.com)");

                var response = await _httpClient.GetStringAsync(url);
                var json = JArray.Parse(response);

                if (json.Count > 0)
                {
                    double lat = double.Parse(json[0]["lat"].ToString());
                    double lng = double.Parse(json[0]["lon"].ToString());
                    return (lat, lng);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Geocoding error: {ex.Message}");
            }

            return (null, null);
        }
    }
}
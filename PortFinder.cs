using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShipPortFinder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShipPortController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public ShipPortController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public ShipPortController()
        {
           
        }

        [HttpGet("closest")]
        public async Task<ActionResult<Port>> GetClosestPort(double shipLatitude, double shipLongitude, double shipVelocity)
        {
            // Get all ports from an external API
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync("https://externalapi.com/ports");
            var content = await response.Content.ReadAsStringAsync();
            var ports = JsonSerializer.Deserialize<List<Port>>(content);

            // Calculate distances between the ship and all ports
            var shipLocation = new GeoCoordinate(shipLatitude, shipLongitude);
            var portDistances = new Dictionary<Port, double>();
            foreach (var port in ports)
            {
                var portLocation = new GeoCoordinate(port.Latitude, port.Longitude);
                var distance = shipLocation.GetDistanceTo(portLocation);
                portDistances.Add(port, distance);
            }

            // Find the port with the shortest distance
            var closestPort = portDistances.OrderBy(p => p.Value).FirstOrDefault().Key;

            // Calculate the estimated arrival time based on the distance and velocity of the ship
            var distanceToPort = portDistances[closestPort];
            var estimatedArrivalTime = distanceToPort / shipVelocity;

            return closestPort;
        }
    }

    public class Port
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

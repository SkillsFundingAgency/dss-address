using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IAzureMapService _azureMapService;
        private readonly ILogger<GeoCodingService> _logger;

        public GeoCodingService(IAzureMapService azureMapService, ILogger<GeoCodingService> logger)
        {
            _azureMapService = azureMapService;
            _logger = logger;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            _logger.LogInformation("Retrieving Position for postcode: {Postcode}.", postcode);

            if (string.IsNullOrEmpty(postcode))
            {
                _logger.LogWarning("Invalid postcode provided: {Postcode}.", postcode);
                return null;
            }

            var position = await _azureMapService.GetPositionForAddress(postcode);

            if (position == null)
            {
                _logger.LogInformation("Position not found for postcode: {Postcode}.", postcode);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved Position for postcode: {Postcode}.", postcode);
            }

            return position;
        }
    }
}

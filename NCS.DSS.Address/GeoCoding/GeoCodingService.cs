using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;
using DFC.GeoCoding.Standard.OrdnanceSurvey.Services;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IOSService _OSService;
        private readonly ILogger<GeoCodingService> _logger;

        public GeoCodingService(IOSService OSService, ILogger<GeoCodingService> logger)
        {
            _OSService = OSService;
            _logger = logger;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            _logger.LogTrace("Retrieving Position for postcode: {Postcode}.", postcode);

            if (string.IsNullOrEmpty(postcode))
            {
                _logger.LogInformation("Invalid postcode provided: {Postcode}.", postcode);
                return null;
            }

            var position = await _OSService.GetPositionForPostcodeAsync(postcode);

            if (position == null)
            {
                _logger.LogInformation("Position not found for postcode: {Postcode}.", postcode);
            }
            else
            {
                _logger.LogTrace("Successfully retrieved Position for postcode: {Postcode}.", postcode);
            }

            return position;
        }
    }
}

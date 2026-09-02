using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;

namespace NCS.DSS.Address.GeoCoding
{
    public interface IGeoCodingService
    {
        Task<Position> GetPositionForPostcodeAsync(string postcode);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Destinations;

namespace BookingTravel.Application.Interfaces
{
    public interface IDestinationService
    {
        Task<IReadOnlyList<DestinationDto>> GetAllDestinationsAsync();
        Task<DestinationDto?> GetDestinationByIdAsync(int id);
        Task<DestinationDto> CreateDestinationAsync(CreateDestinationRequest request);
        Task<bool> UpdateDestinationAsync(int id, UpdateDestinationRequest request);
        Task<bool> DeleteDestinationAsync(int id);
    }
}

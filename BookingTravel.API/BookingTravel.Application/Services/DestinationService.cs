using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingTravel.Application.DTOs.Destinations;
using BookingTravel.Application.Interfaces;
using BookingTravel.Domain.Entities;

namespace BookingTravel.Application.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly IRepository<Destination> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DestinationService(IRepository<Destination> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<DestinationDto>> GetAllDestinationsAsync()
        {
            var data = await _repository.GetAllAsync();
            return data.Select(MapToDto).ToList();
        }

        public async Task<DestinationDto?> GetDestinationByIdAsync(int id)
        {
            var destination = await _repository.GetByIdAsync(id);
            return destination == null ? null : MapToDto(destination);
        }

        public async Task<DestinationDto> CreateDestinationAsync(CreateDestinationRequest request)
        {
            var newDestination = new Destination
            {
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-").Replace("đ", "d"),
                Description = request.Description,
                Country = request.Country,
                Province = request.Province,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CoverImageUrl = request.CoverImageUrl
            };

            await _repository.AddAsync(newDestination);
            await _unitOfWork.CompleteAsync();

            return MapToDto(newDestination);
        }

        public async Task<bool> UpdateDestinationAsync(int id, UpdateDestinationRequest request)
        {
            var destination = await _repository.GetByIdAsync(id);
            if (destination == null) return false;

            destination.Name = request.Name;
            destination.Slug = request.Name.ToLower().Replace(" ", "-").Replace("đ", "d");
            destination.Description = request.Description;
            destination.Country = request.Country;
            destination.Province = request.Province;
            destination.Latitude = request.Latitude;
            destination.Longitude = request.Longitude;
            destination.CoverImageUrl = request.CoverImageUrl;

            _repository.Update(destination);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            var destination = await _repository.GetByIdAsync(id);
            if (destination == null) return false;

            // Xoá cứng
            _repository.Delete(destination);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private DestinationDto MapToDto(Destination entity)
        {
            return new DestinationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                Country = entity.Country,
                Province = entity.Province,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                CoverImageUrl = entity.CoverImageUrl
            };
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using BookingTravel.Application.Interfaces;
using BookingTravel.Infrastructure.Data;

namespace BookingTravel.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookingTravelDbContext _context;

        public UnitOfWork(BookingTravelDbContext context)
        {
            _context = context;
        }

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

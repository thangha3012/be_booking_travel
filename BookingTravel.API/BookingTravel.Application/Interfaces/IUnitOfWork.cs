using System.Threading;
using System.Threading.Tasks;

namespace BookingTravel.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        void Dispose();
    }
}

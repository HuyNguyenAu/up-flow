using System.Threading.Tasks;

namespace UpFlow.API.Services
{
    /// <summary>
    /// Defines an interface for transaction CRUD operations.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Get new transactions from Up Bank API.
        /// </summary>
        Task<Dto.PagedResultDto<Dto.Empty>> Update();

        /// <summary>
        /// Get transactions.
        /// </summary>
        /// <param name="skip">The page offset.</param>
        /// <param name="take">The page size.</param>
        /// <returns>A paged result of transactions.</returns>
        Task<Dto.PagedResultDto<Dto.Transaction>> ListAsync(int skip, int take);
    }
}

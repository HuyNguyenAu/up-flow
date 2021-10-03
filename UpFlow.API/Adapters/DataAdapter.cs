namespace UpFlow.API.Adapters
{
    /// <summary>
    /// Provides conversion between EF entities and DTOs.
    /// </summary>
    public class DataAdapter
    {
        /// <summary>
        /// Convert an EF device entity to a DTO.
        /// </summary>
        /// <param name="dbTransaction">The EF transaction entity.</param>
        /// <returns>The device DTO.</returns>
        public Dto.Transaction ToDto(Data.Transaction dbTransaction)
        {
            return new Dto.Transaction()
            {
               Id = dbTransaction.Id,
               Value = dbTransaction.Value,
               SettledAt = dbTransaction.SettledAt,
            };
        }
    }
}

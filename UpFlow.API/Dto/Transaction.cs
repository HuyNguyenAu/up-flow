using System;

namespace UpFlow.API.Dto
{
    /// <summary>
    /// The Transaction DTO.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The transaction ID.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// The value in base units.
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// The settled date of the transaction.
        /// </summary>
        public DateTime SettledAt { get; set; }
    }
}

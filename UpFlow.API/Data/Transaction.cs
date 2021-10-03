using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpFlow.API.Data
{
    /// <summary>
    /// Represents a simplified Up Transaction entity.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The internal transaction ID.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// The unique identifier of the transaction set by Up.
        /// </summary>
        public Guid UpId { get; set; }
        /// <summary>
        /// The value in base units.
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// The settled date of the transaction.
        /// </summary>
        public DateTime SettledAt { get; set; }
        /// <summary>
        /// The DB generated ceated at date and time.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// The DB generated modified at date and time.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// The rowversion/timestamp for concurrency checks.
        /// https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations#timestamprowversion
        /// </summary>
        [Timestamp, Required]
        public byte[] Timestamp { get; set; }
    }
}

using System.Collections.Generic;

namespace UpFlow.API.Dto
{
    /// <summary>
    /// Represents paged results DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// The paged data rows.
        /// </summary>
        public IEnumerable<T> Rows { get; set; }
        /// <summary>
        /// The total count of rows.
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// The indicator that an error has occurred.
        /// </summary>
        public bool Error { get; set; }
        /// <summary>
        /// The error message to communicate to the user if an error has occured.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UpFlow.API.Services;

namespace UpFlow.API.Controllers
{
    /// <summary>
    /// The Up API controller.
    /// </summary>
    [ApiController]
    [Route("api/up")]
    public class UpController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        private readonly ILogger<UpController> _logger;

        /// <summary>
        /// The <see cref="UpController"/> constructor.
        /// </summary>
        /// <param name="transactionService">The transaction service via DI.</param>
        /// <param name="logger">The logging provider via DI.</param>
        public UpController(ITransactionService transactionService, ILogger<UpController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Get new transactions from Up Bank API.
        /// </summary>
        [HttpGet("update")]
        public async Task<Dto.PagedResultDto<Dto.Empty>> Update()
        {
            _logger.LogDebug("Processing get update request...");

            Dto.PagedResultDto<Dto.Empty> result;

            try
            {
                result = await _transactionService.Update();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get new transactions from Up Bank API. Error: {0}", ex);

                throw new Exception("An internal server error has occured.");
            }

            _logger.LogInformation("Processed get transactions request.");

            return result;
        }

        /// <summary>
        /// Get transactions.
        /// </summary>
        /// <param name="skip">The page offset.</param>
        /// <param name="take">The page size.</param>
        /// <returns>A paged result of transactions.</returns>
        [HttpGet("transactions")]
        public async Task<Dto.PagedResultDto<Dto.Transaction>> Get(int skip = 0, int take = 20)
        {
            _logger.LogDebug("Processing get transactions request...");

            Dto.PagedResultDto<Dto.Transaction> result;

            try
            {
                result = await _transactionService.ListAsync(skip, take);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get transactions. Error: {0}", ex);

                throw new Exception("An internal server error has occured.");
            }

            _logger.LogInformation("Processed get transactions request.");

            return result;
        }
    }
}

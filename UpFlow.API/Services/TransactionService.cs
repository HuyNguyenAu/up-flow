using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System;
using UpFlow.API.Data;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace UpFlow.API.Services
{
    /// <summary>
    /// This class provides CRUD actions for transaction data and implements caching.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        /// <summary>
        /// The memory cache provider (DI).
        /// </summary>
        private readonly IMemoryCache _cache;
        /// <summary>
        /// The EF core Transaction DB context (DI).
        /// </summary>
        private readonly TransactionDbContext _transactionDbContext;
        /// <summary>
        /// The logging provider (DI).
        /// </summary>
        private readonly ILogger<TransactionService> _logger;
        /// <summary>
        /// The logging provider (DI).
        /// </summary>
        private readonly IHttpClientFactory _clientFactory;
        /// <summary>
        /// The logging provider (DI).
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The parameterized <see cref="TransactionService"/> constructor.
        /// </summary>
        /// <param name="cache">The memory cache provider (DI).</param>
        /// <param name="transactionDbContext">The EF core Transaction DB context (DI).</param>
        /// <param name="logger">The logging provider (DI).</param>
        /// <param name="clientFactory">The HTTP client provider (DI).</param>
        /// <param name="configuration">The configuration provider (DI).</param>
        public TransactionService(IMemoryCache cache, TransactionDbContext transactionDbContext, ILogger<TransactionService> logger, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _cache = cache;
            _transactionDbContext = transactionDbContext;
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Get new transactions and add it to the database.
        /// </summary>
        /// <returns>A paged result that shows how many new added transactions..</returns>
        public async Task<Dto.PagedResultDto<Dto.Empty>> Update()
        {
            _logger.LogTrace("Pulling new transactions from Up Bank.");

            var page = new Dto.PagedResultDto<Dto.Empty>()
            {
                Rows = new List<Dto.Empty>(),
                TotalCount = 0,
                Error = false
            };

            // Get the lastest settled at transcation date.
            DateTime lastTransactionDateTime = _transactionDbContext.Transactions.Select(x => x.SettledAt).OrderByDescending(x => x).FirstOrDefault();
            string nextLink = $"https://api.up.com.au/api/v1/transactions?filter[status]=SETTLED";
            using HttpClient client = _clientFactory.CreateClient();

            while (nextLink != null && nextLink.Length > 0)
            {
                _logger.LogTrace("Retrieving transactions from {0}.", nextLink);

                // Create the request and get the response.
                HttpRequestMessage request = new(HttpMethod.Get, nextLink);
                request.Headers.Add("Authorization", $"Bearer {_configuration["UpAPIToken"]}");
                HttpResponseMessage response = await client.SendAsync(request);

                // Check if the request is successful.
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve transactions from Up Bank.", nextLink);

                    page.Error = true;
                    page.ErrorMessage = $"Failed to get Up Bank transactions. Status code {response.StatusCode}. Reason: {response.ReasonPhrase}.";

                    return page;
                }

                // Deserialise the response.
                _logger.LogTrace("Deserialising response...");
                dynamic json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                IQueryable<Guid> guids = _transactionDbContext.Transactions.Select(x => x.UpId);

                // Parse all new transactions and add it to the database.
                _logger.LogTrace("Parsing data...");
                foreach (dynamic item in json["data"])
                {
                    /* Since we converted the response into a dynamic object (because I'm lazy), 
                     * we'll need to catch any errors (most likely a property not existing) in order to at least send the user a response.
                     */
                    try
                    {
                        // Get transaction fields.
                        Guid id = Guid.Parse(item["id"].ToString());
                        double value = double.Parse(item["attributes"]["amount"]["value"].ToString());
                        DateTime settledAt = DateTime.Parse(item["attributes"]["settledAt"].ToString());

                        _logger.LogTrace("Parsing transaction {0}...", id);

                        // Stop parsing transaction if we find a transaction that has a settled date that is earlier or equal to the last transaction settled date in the database. 
                        if (settledAt <= lastTransactionDateTime)
                        {
                            _logger.LogInformation("Skipping transaction {0} because its settled date is earlier or equal to the last transaction settled date in the database.", id);
                            nextLink = null;
                            break;
                        }

                        // Skip current transaction if it's identifier already exists in the database.
                        if (guids.Any(x => x == id))
                        {
                            _logger.LogInformation("Skipping transaction {0} because it already exists.", id);
                            continue;
                        }

                        // Add the transaction to the database.
                        await _transactionDbContext.Transactions.AddAsync(new Transaction
                        {
                            UpId = id,
                            Value = value,
                            SettledAt = settledAt
                        });

                        // Keep track of how many new transactions we've added.
                        page.TotalCount++;

                        // Get the link that points to the next set of transactions.
                        nextLink = json["links"]["next"];
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to parse transaction with error: {0}", e);

                        page.Error = true;
                        page.ErrorMessage = $"Failed to parse Up Bank transactions.";
                        page.TotalCount = 0;

                        return page;
                    }
                }
            }

            // Save any changes made to the database.
            _logger.LogError("Saving database changes.");
            await _transactionDbContext.SaveChangesAsync();

            return page;
        }

        /// <summary>
        /// Get transactions.
        /// </summary>
        /// <param name="skip">The page offset.</param>
        /// <param name="take">The page size.</param>
        /// <returns>A paged result of transactions.</returns>
        public async Task<Dto.PagedResultDto<Dto.Transaction>> ListAsync(int skip, int take)
        {
            if (skip < 0)
            {
                _logger.LogError("The skip parameter must be at greater than or equal to 0.");

                return new Dto.PagedResultDto<Dto.Transaction>()
                {
                    Rows = new List<Dto.Transaction>(),
                    TotalCount = 0,
                    Error = true,
                    ErrorMessage = "The skip parameter must be at greater than or equal to 0."
                };
            }

            if (take <= 0)
            {
                _logger.LogError("The take parameter must be at greater than 0.");

                return new Dto.PagedResultDto<Dto.Transaction>()
                {
                    Rows = new List<Dto.Transaction>(),
                    TotalCount = 0,
                    Error = true,
                    ErrorMessage = "The take parameter must be at greater than 0."
                };
            }

            _logger.LogTrace("Retrieving transaction page with skip: {0} and take: {1}", skip, take);

            string key = $"transactions_{skip}_{take}";

            if (!_cache.TryGetValue(key, out Dto.PagedResultDto<Dto.Transaction> page))
            {
                Adapters.DataAdapter dataAdapter = new();

                page = new Dto.PagedResultDto<Dto.Transaction>()
                {
                    Rows = await _transactionDbContext.Transactions.OrderBy(x => x.Id).Skip(skip).Take(take).Select(x => dataAdapter.ToDto(x)).ToArrayAsync(),
                    TotalCount = await _transactionDbContext.Transactions.CountAsync(),
                    Error = false
                };

                _cache.Set(key, page);

                _logger.LogDebug("Retrieved transactions page with skip: {0} and take: {1} from DB.", skip, take);
            }
            else
            {
                _logger.LogDebug("Retrieved transactions page with skip: {0} and take: {1} from cache.", skip, take);
            }

            return page;
        }
    }
}

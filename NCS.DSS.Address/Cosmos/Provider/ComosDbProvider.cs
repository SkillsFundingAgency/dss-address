using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Address.Models;
using Newtonsoft.Json;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public class CosmosDbProvider : ICosmosDbProvider
    {
        private readonly Container _addressContainer;
        private readonly Container _customerContainer;
        private readonly ILogger<CosmosDbProvider> _logger;

        public CosmosDbProvider(CosmosClient cosmosClient,
            IOptions<AddressConfigurationSettings> configOptions,
            ILogger<CosmosDbProvider> logger)
        {
            var config = configOptions.Value;

            _addressContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for customer resource. Customer ID: {CustomerId}", customerId);

                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                if (response.Resource != null)
                {
                    _logger.LogInformation("Customer exists. Customer ID: {CustomerId}", customerId);
                    return true;
                }

                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<string> GetAddressByIdForUpdateAsync(Guid customerId, Guid addressId)
        {
            var address = await GetAddressForCustomerAsync(customerId, addressId);

            return JsonConvert.SerializeObject(address);
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            _logger.LogInformation("Retrieving Address for Customer. Customer ID: {CustomerId}. Address ID: {AddressId}.", customerId, addressId);

            try
            {
                var query = _addressContainer.GetItemLinqQueryable<Models.Address>()
                    .Where(x => x.CustomerId == customerId && x.AddressId == addressId)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                if (response.Any())
                {
                    _logger.LogInformation("Address retrieved successfully. Customer ID: {CustomerId}. Address ID: {AddressId}.", customerId, addressId);
                    return response?.FirstOrDefault();
                }

                _logger.LogWarning("Address not found. Customer ID: {CustomerId}. Address ID: {AddressId}.", customerId, addressId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Address. Customer ID: {CustomerId}. Address ID: {AddressId}.", customerId, addressId);
                throw;
            }
        }


        public async Task<List<Models.Address>> GetAddressesForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving Addresses for Customer. Customer ID: {CustomerId}.", customerId);

            try
            {
                var addresses = new List<Models.Address>();
                var query = _addressContainer.GetItemLinqQueryable<Models.Address>()
                    .Where(x => x.CustomerId == customerId)
                    .ToFeedIterator();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    addresses.AddRange(response);
                }

                _logger.LogInformation("Retrieved {Count} Address(es) for Customer with ID: {CustomerId}.", addresses.Count, customerId);
                return addresses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Addresses. Customer ID: {CustomerId}.", customerId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Address>> CreateAddressAsync(Models.Address address)
        {
            if (address == null)
            {
                _logger.LogError("Address object is null. Creation aborted.");
                throw new ArgumentNullException(nameof(Address), "Address cannot be null.");
            }

            _logger.LogInformation("Creating Address with ID: {AddressId}", address.AddressId);

            try
            {
                var response = await _addressContainer.CreateItemAsync(address, PartitionKey.None);
                _logger.LogInformation("Successfully created Address with ID: {AddressID}", address.AddressId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Address with ID: {AddressId}", address.AddressId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Address>> UpdateAddressAsync(string addressJson, Guid addressId)
        {
            if (string.IsNullOrEmpty(addressJson))
            {
                _logger.LogError("addressJson object is null. Update aborted.");
                throw new ArgumentNullException(nameof(addressJson), "Address cannot be null.");
            }

            var address = JsonConvert.DeserializeObject<Models.Address>(addressJson);

            _logger.LogInformation("Updating Address with ID: {AddressId}", addressId);

            try
            {
                var response = await _addressContainer.ReplaceItemAsync(address, addressId.ToString());
                _logger.LogInformation("Successfully updated Address with ID: {AddressId}", addressId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Address with ID: {AddressId}", addressId);
                throw;
            }
        }
    }
}
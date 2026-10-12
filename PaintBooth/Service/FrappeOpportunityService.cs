using PaintBooth.Models.ERP;
using System.Text.Json;

namespace PaintBooth.Service
{
    public class FrappeOpportunityService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<FrappeOpportunityService> _logger;

        public FrappeOpportunityService(HttpClient httpClient, IConfiguration config,
            ILogger<FrappeOpportunityService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }
        // Get all Enquiry ID from Frappe API, with only the fields we need
        public async Task<List<FrappeOpportunityData>> GetAllOpportunitiesAsync()
        {
            try
            {
                var baseUrl = _config["FrappeApi:BaseUrl"];
                var authToken = _config["FrappeApi:AuthToken"];

                // Request only the fields you need + pull all records (limit_page_length=0)
                var fields = "[\"name\",\"party_name\",\"customer_name\",\"opportunity_from\"," +
                   "\"status\",\"transaction_date\",\"opportunity_amount\"," +
                   "\"creation\",\"modified\"]";   // ← ADDED

                var url = $"{baseUrl}/api/resource/Opportunity?fields={Uri.EscapeDataString(fields)}&limit_page_length=0";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Frappe Opportunity API returned {StatusCode}", response.StatusCode);
                    return new List<FrappeOpportunityData>();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Get All Enquiry ID Response: {Response}", json);
                var result = JsonSerializer.Deserialize<FrappeOpportunityListResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.data ?? new List<FrappeOpportunityData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Opportunities from Frappe API");
                return new List<FrappeOpportunityData>();
            }
        }
        // Get Latest Enquiry ids 
        public async Task<List<FrappeOpportunityData>> GetLatestOpportunitiesAsync(int count = 1)
        {
            try
            {
                var baseUrl = _config["FrappeApi:BaseUrl"];
                var authToken = _config["FrappeApi:AuthToken"];

                var fields = "[\"name\",\"party_name\",\"customer_name\",\"opportunity_from\"," +
                     "\"status\",\"transaction_date\",\"opportunity_amount\"," +
                     "\"creation\",\"modified\"]";   // ← ADDED

                var url = $"{baseUrl}/api/resource/Opportunity" +
                          $"?fields={Uri.EscapeDataString(fields)}" +
                          $"&order_by=creation desc" +      // sort by creation timestamp (most reliable for "latest")
                          $"&limit_page_length={count}";    // ← only fetch what's needed

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Frappe Opportunity API returned {StatusCode}", response.StatusCode);
                    return new List<FrappeOpportunityData>();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Lastest Enquiry ID Response: {Response}", json);
                var result = JsonSerializer.Deserialize<FrappeOpportunityListResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.data ?? new List<FrappeOpportunityData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest Opportunities from Frappe API");
                return new List<FrappeOpportunityData>();
            }
        }
        // Get Enquiry by ENquiry ID 
        public async Task<FrappeOpportunityData> GetOpportunityByNameAsync(string name)
        {
            try
            {
                var baseUrl = _config["FrappeApi:BaseUrl"];
                var authToken = _config["FrappeApi:AuthToken"];
                var url = $"{baseUrl}/api/resource/Opportunity/{Uri.EscapeDataString(name)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Enquiry ID Response: {Response}", json);

                var wrapper = JsonSerializer.Deserialize<Dictionary<string, FrappeOpportunityData>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return wrapper != null && wrapper.ContainsKey("data") ? wrapper["data"] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching single Opportunity {Name}", name);
                return null;
            }
        }

        // Enquiries Filter by Dates 
        public async Task<List<FrappeOpportunityData>> GetOpportunitiesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var baseUrl = _config["FrappeApi:BaseUrl"];
                var authToken = _config["FrappeApi:AuthToken"];

                var fields = "[\"name\",\"party_name\",\"customer_name\",\"opportunity_from\"," +
                     "\"status\",\"transaction_date\",\"opportunity_amount\"," +
                     "\"creation\",\"modified\"]";

                // Filter by creation date range (inclusive) — end date pushed to end-of-day
                var startStr = startDate.ToString("yyyy-MM-dd 00:00:00");
                var endStr = endDate.ToString("yyyy-MM-dd 23:59:59");

                var filters = $"[[\"creation\",\">=\",\"{startStr}\"],[\"creation\",\"<=\",\"{endStr}\"]]";

                var url = $"{baseUrl}/api/resource/Opportunity" +
                          $"?fields={Uri.EscapeDataString(fields)}" +
                          $"&filters={Uri.EscapeDataString(filters)}" +
                          $"&order_by=creation desc" +
                          $"&limit_page_length=0";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Frappe Opportunity API returned {StatusCode}", response.StatusCode);
                    return new List<FrappeOpportunityData>();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Date Range Opportunities Response: {Response}", json);

                var result = JsonSerializer.Deserialize<FrappeOpportunityListResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.data ?? new List<FrappeOpportunityData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Opportunities by date range from Frappe API");
                return new List<FrappeOpportunityData>();
            }
        }

        #region Frappe Item Rate From API 
        public async Task<FrappeItemData?> GetItemByCodeAsync(string itemCode)
        {
            if (string.IsNullOrWhiteSpace(itemCode))
                return null;

            itemCode = itemCode.Trim();
            var cacheKey = $"frappe_item_{itemCode}";

            //if (_cache.TryGetValue(cacheKey, out FrappeItemData? cached))
            //    return cached;

            try
            {
                var baseUrl = _config["FrappeApi:BaseUrl"];
                var authToken = _config["FrappeApi:AuthToken"];

                var url = $"{baseUrl}/api/resource/Item/{Uri.EscapeDataString(itemCode)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", authToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Frappe API returned {StatusCode} for item {ItemCode}",
                        response.StatusCode, itemCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FrappeItemResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var data = result?.data;

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Frappe Item API for item {ItemCode}", itemCode);
                return null; // caller falls back to last known rate
            }
        }
        #endregion
    }
}

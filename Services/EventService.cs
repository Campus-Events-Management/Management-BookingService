using EventManagement.BookingService.Models.DTOs;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EventManagement.BookingService.Services
{
    public class EventService : IEventService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EventService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        public EventService(HttpClient httpClient, ILogger<EventService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<EventDto?> GetEventByIdAsync(string eventId)
        {
            try
            {
                // Log the full URL being called
                var requestUrl = $"api/events/{eventId}";
                _logger.LogInformation("Calling Event Service at: {BaseAddress}{RequestUrl}", _httpClient.BaseAddress, requestUrl);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var eventDto = await response.Content.ReadFromJsonAsync<EventDto>(_jsonOptions);
                    _logger.LogInformation("Successfully retrieved event with ID: {EventId}, Title: {Title}", 
                        eventDto?.Id, eventDto?.Title);
                    return eventDto;
                }
                
                _logger.LogWarning("Failed to get event with ID {EventId}. Status code: {StatusCode}", 
                    eventId, response.StatusCode);
                    
                // Try to read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting event with ID {EventId}", eventId);
                return null;
            }
        }
        
        public async Task<bool> UpdateEventBookingCountAsync(string eventId, bool isIncrement)
        {
            try
            {
                var requestData = new { IsIncrement = isIncrement };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");
                
                var requestUrl = $"api/events/{eventId}/bookings";
                _logger.LogInformation("Calling Event Service to update booking count at: {BaseAddress}{RequestUrl}, IsIncrement: {IsIncrement}", 
                    _httpClient.BaseAddress, requestUrl, isIncrement);
                    
                var response = await _httpClient.PutAsync(requestUrl, content);
                
                _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated booking count for event ID: {EventId}", eventId);
                    return true;
                }
                
                _logger.LogWarning("Failed to update booking count for event ID {EventId}. Status code: {StatusCode}", 
                    eventId, response.StatusCode);
                    
                // Try to read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking count for event ID {EventId}", eventId);
                return false;
            }
        }
    }
} 
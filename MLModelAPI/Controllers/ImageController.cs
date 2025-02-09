using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace MLModelAPI.Controllers
{
    [ApiController]
    [Route("api/Image")]
    public class ImageController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ImageController()
        {
            _httpClient = new HttpClient();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image file uploaded.");
            
            string filePath = Path.Combine(Path.GetTempPath(), image.FileName);

            try
            {
                // Save the uploaded file temporarily
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Forward image to ML model
                var mlApiUrl = "http://159.223.85.131:5000/predict";
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
                    content.Add(streamContent, "image", image.FileName);

                    var response = await _httpClient.PostAsync(mlApiUrl, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, errorMessage);
                    }

                    // Read and parse the response
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var mlResponse = JsonSerializer.Deserialize<MlResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (mlResponse?.Detections == null || mlResponse.Detections.Count == 0)
                        return NotFound("No species detected.");

                    // Extract uniqle class names
                    var speciesList = mlResponse.Detections
                        .Select(d => d.ClassName)
                        .Distinct()
                        .ToList();

                    return Ok(new { species = speciesList });  // Return ML model response to mobile app
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with ML model: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing ML model response: {ex.Message}");
            }
            finally
            {
                // Delete the temporary file
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private class MlResponse
        {
            [JsonPropertyName("detections")]
            public List<Detection> Detections { get; set; }
        }

        private class Detection
        {
            [JsonPropertyName("class_name")]
            public string ClassName { get; set; }
            [JsonPropertyName("confidence")]
            public float Confidence { get; set; }
        }
    }
}
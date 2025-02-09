using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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

            try
            {
                // Save the uploaded file temporarily
                var filePath = Path.Combine(Path.GetTempPath(), image.FileName);
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

                    var result = await response.Content.ReadAsStringAsync();
                    return Ok(result);  // Return ML model response to mobile app
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with ML model: {ex.Message}");
            }
        }
    }
}
using FitStackDBL.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;


namespace FitStackDBL.Services
{
    public class NutritionService : INutritionService
    {
        private readonly ILogger<NutritionService> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public NutritionService(IConfiguration configuration, ILogger<NutritionService> logger)
        {
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        }

        public async Task<NutritionResult?> AnalyzeFoodImageAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                var base64Image = Convert.ToBase64String(imageBytes);
                return await AnalyzeFoodImageFromBase64Async(base64Image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing food image");
                return null;
            }
        }



        private string GetNutritionAnalysisPrompt()
        {
            return @"You are a world-class AI Nutrition Analyst. Analyze the food in this image and provide detailed nutritional information.

                Please identify all visible food items and estimate:
                1. Total calories
                2. Protein (grams)
                3. Carbohydrates (grams)
                4. Fat (grams)
                5. Fiber (grams)
                6. Sugar (grams)

                Also provide:
                - A confidence score (0.0 to 1.0) based on how clearly the food is visible
                - A health score (0-10, where 10 is healthiest)
                - A brief rationale explaining how you estimated the nutrition
                - A breakdown of individual food components if multiple items are present

                Return ONLY valid JSON in this exact format, no additional text:
                {
                  ""mealName"": ""Name of the meal"",
                  ""calories"": 0,
                  ""protein"": 0,
                  ""carbs"": 0,
                  ""fat"": 0,
                  ""fiber"": 0,
                  ""sugar"": 0,
                  ""confidenceScore"": 0.0,
                  ""healthScore"": 0,
                  ""rationale"": ""Explanation of estimation"",
                  ""components"": [
                    {
                      ""name"": ""Component name"",
                      ""calories"": 0,
                      ""protein"": 0,
                      ""carbs"": 0,
                      ""fat"": 0
                    }
                  ]
                }

                If the image does not contain food, return confidenceScore: 0.0 and an appropriate message.";
        }

        private NutritionResult? ParseGeminiResponse(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var candidate = root.GetProperty("candidates")[0];
                var content = candidate.GetProperty("content");
                var parts = content.GetProperty("parts");
                var text = parts[0].GetProperty("text").GetString();

                if (!string.IsNullOrEmpty(text))
                {
                    // Clean up the response (remove markdown code blocks if present)
                    var cleanedText = text.Replace("```json", "").Replace("```", "").Trim();
                    return JsonSerializer.Deserialize<NutritionResult>(cleanedText);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing Gemini response: {responseJson}");
                return null;
            }
        }
        public async Task<NutritionResult?> AnalyzeFoodImageFromBase64Async(string base64Image)
        {
            try
            {
                Console.WriteLine("=== GEMINI API DEBUG ===");
                Console.WriteLine($"API Key configured: {!string.IsNullOrEmpty(_apiKey)}");
                Console.WriteLine($"Image base64 length: {base64Image?.Length ?? 0}");

                var prompt = GetNutritionAnalysisPrompt();

                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "image/jpeg",
                                data = base64Image
                            }
                        }
                    }
                }
            },
                    generationConfig = new
                    {
                        temperature = 0.4,
                        topP = 0.95,
                        topK = 40,
                        maxOutputTokens = 2048
                    }
                };

                using var httpClient = new HttpClient();
                var json = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"Request JSON length: {json.Length}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Gemini API error: {responseJson}");
                    return null;
                }

                return ParseGeminiResponse(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error calling Gemini API");
                return null;
            }
        }
    }
}


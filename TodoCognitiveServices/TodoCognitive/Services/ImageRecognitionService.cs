using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using TodoCognitive.Services;

[assembly: Dependency(typeof(ImageRecognitionService))]
namespace TodoCognitive.Services
{
    public class ImageRecognitionService
    {
        /// <summary>
        /// The Azure Cognitive Services Computer Vision API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Parameterless constructor so Dependency Service can create an instance.
        /// </summary>
        public ImageRecognitionService()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReptileTracker.Services.ImageRecognitionService"/> class.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        public ImageRecognitionService(string apiKey)
        {

            ApiKey = apiKey;
        }

        /// <summary>
        /// Analyzes the image.
        /// </summary>
        /// <returns>The image.</returns>
        /// <param name="imageStream">Image stream.</param>
        public async Task<OcrResult> AnalyzeImage(Stream imageStream)
        {
            const string funcName = nameof(AnalyzeImage);

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new ArgumentException("API Key must be provided.");
            }

            var features = new List<VisualFeatureTypes> {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags
            };

            var credentials = new ApiKeyServiceClientCredentials(ApiKey);
            var handler = new System.Net.Http.DelegatingHandler[] { };
            using (var visionClient = new ComputerVisionClient(credentials, handler))
            {
                try
                {
                    imageStream.Position = 0;
                    visionClient.Endpoint = "https://eastus.api.cognitive.microsoft.com/";
                    var result = await visionClient.RecognizePrintedTextInStreamAsync(true, imageStream);
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{funcName}: {ex.GetBaseException().Message}");
                    return null;
                }
            }
        }

    }
}


using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using TodoCognitive.Exceptions;
using TodoCognitive.Models;
using TodoCognitive.Services;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TodoCognitive
{
    public partial class RateAppPage : ContentPage
    {		
        MediaFile photo;

        public RateAppPage()
        {
            InitializeComponent();
			
        }

        async void OnTakePhotoButtonClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            // Take photo
            if (CrossMedia.Current.IsCameraAvailable || CrossMedia.Current.IsTakePhotoSupported)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    Name = "emotion.jpg",
                    SaveToAlbum = true,
                    PhotoSize = PhotoSize.Small
                });

                if (photo != null)
                {
                    image.Source = ImageSource.FromStream(photo.GetStream);
                }
            }
            else
            {
                await DisplayAlert("No Camera", "Camera unavailable.", "OK");
            }

            ((Button)sender).IsEnabled = false;
            activityIndicator.IsRunning = true;

            // Recognize emotion
            try
            {
                if (photo != null)
                {                    
                    using (var photoStream = photo.GetStream())
                    {
                        var imageRecognizer = DependencyService.Get<ImageRecognitionService>();
                        imageRecognizer.ApiKey = "5ce7d40fb6334cb5864530608b766def";

                        var image = new Image { Source = "testing.jepg" };
                        var details = await imageRecognizer.AnalyzeImage(photoStream);
                        var scannedText = "";
                        List<string> listOfWords = new List<string>();
                        if (details?.Regions != null)
                        {
                            foreach (var algo in details?.Regions.FirstOrDefault().Lines)
                            {
                                foreach (var palabra in algo.Words)
                                {
                                    var aux = Regex.Replace(palabra.Text, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.0));
                                    aux = Regex.Replace(aux, @"[\:]", "", RegexOptions.None, TimeSpan.FromSeconds(1.0));
                                    if (aux != "")
                                    {
                                        scannedText += aux + ",";
                                        listOfWords.Add(aux);
                                    }
                                }
                            }
                            emotionResultLabel.Text = scannedText;

                            // Serializing the array to JSON
                            string jsonData = JsonConvert.SerializeObject(listOfWords);

                            // Sending http request
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://hackathoncr.azure-api.net/ingredients/consult"))
                                {
                                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");                                   
                                    request.Headers.TryAddWithoutValidation("Host", "hackathoncr.azure-api.net");
                                    //request.Content = new StringContent("[\"WHEAT\"]", Encoding.UTF8, "application/json");
                                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = await httpClient.SendAsync(request);                                    
                                    string result = await response.Content.ReadAsStringAsync();

                                    //var content = await response.Content.ReadAsStringAsync();
                                    //List<Allergen> Items = JsonConvert.DeserializeObject<List<Allergen>>(content);

                                    //string test = Regex.Replace(result, @"[\\]", "", RegexOptions.None, TimeSpan.FromSeconds(1.0));
                                    // Working on this part 
                                    
                                    JObject token = JObject.Parse(result);

                                    JToken values = token["value"];
                                    // Allergen is an object that I created to store the results 
                                    List<Allergen> allergens = new List<Allergen>();

                                    // This part has to be tested
                                    foreach (var item in values.Children())
                                    {
                                        Allergen allergen = new Allergen();
                                        var itemProperties = item.Children<JProperty>();

                                        allergen.Key = itemProperties.FirstOrDefault(x => x.Name == "Key").Value.ToString();
                                        allergen.CommonName = itemProperties.FirstOrDefault(x => x.Name == "CommonName").Value.ToString();
                                        allergen.Group = itemProperties.FirstOrDefault(x => x.Name == "Group").Value.ToString();
                                        allergen.IUISAAllergen = itemProperties.FirstOrDefault(x => x.Name == "IUISAAllergen").Value.ToString();
                                        allergen.LastFileSync = itemProperties.FirstOrDefault(x => x.Name == "LastFileSync").Value.ToString();
                                        allergen.Source = itemProperties.FirstOrDefault(x => x.Name == "Source").Value.ToString();
                                        allergen.Species = itemProperties.FirstOrDefault(x => x.Name == "Species").Value.ToString();
                                        allergen.Type = itemProperties.FirstOrDefault(x => x.Name == "Type").Value.ToString();

                                        allergens.Add(allergen);
                                    }

                                    resultsgrid.RowDefinitions.Add(new RowDefinition());
                                    resultsgrid.RowDefinitions.Add(new RowDefinition());
                                    resultsgrid.ColumnDefinitions.Add(new ColumnDefinition());
                                    resultsgrid.ColumnDefinitions.Add(new ColumnDefinition());
                                    resultsgrid.ColumnDefinitions.Add(new ColumnDefinition());

                                    var allergenIndex = 0;
                                    for (int rowIndex = 0; rowIndex < allergens.Count(); rowIndex++)
                                    {
                                        for (int columnIndex = 0; columnIndex < 1; columnIndex++)
                                        {
                                            if (allergens.Count == 0)
                                            {
                                                break;
                                            }
                                            var allergenaux = allergens[allergenIndex];
                                            allergenIndex += 1;
                                            var label = new Label
                                            {
                                                Text = "Name: " + allergenaux.CommonName + " Group: " +allergenaux.Group 
                                                + " Type: " + allergenaux.Type + " IUUISAAllergen: " + allergenaux.IUISAAllergen,
                                                //VerticalOptions = LayoutOptions.Center,
                                                //HorizontalOptions = LayoutOptions.Center
                                            };
                                            resultsgrid.Children.Add(label, columnIndex, rowIndex);
                                        }
                                    }

                                    emotionResultLabel.Text = "";
                                    label.Text = "";

                                    //........................................
                                }
                            }
                        }

                        photo.Dispose();
                    }
                }
            }
            catch (FaceAPIException fx)
            {
                Debug.WriteLine(fx.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            activityIndicator.IsRunning = false;
            ((Button)sender).IsEnabled = true;
        }
    }
}

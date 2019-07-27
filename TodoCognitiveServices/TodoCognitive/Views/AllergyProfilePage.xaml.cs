using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;



namespace TodoCognitive.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AllergyProfilePage : ContentPage
    {
        public static readonly BindableProperty TodoItemProperty =
            BindableProperty.Create("TodoItem", typeof(TodoItem), typeof(TodoItemPage), null);
        public TodoItem TodoItem
        {
            get { return (TodoItem)GetValue(TodoItemProperty); }
            set { SetValue(TodoItemProperty, value); }
        }
        public AllergyProfilePage()
        {
            InitializeComponent();



            LoadDataAsync();
        }



        public async Task LoadDataAsync()
        {
            activityIndicator.IsRunning = true;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://hackathoncr.azure-api.net/ingredients/consult"))
                {
                    request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    request.Headers.TryAddWithoutValidation("Host", "hackathoncr.azure-api.net");
                    request.Content = new StringContent("[]", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    string result = await response.Content.ReadAsStringAsync();
                    JObject token = JObject.Parse(result);
                    JToken values = token["value"];



                    for (int i = 0; i < 1000; i++)
                    {
                        var itemProperties = values[i].Children<JProperty>();
                        var aller = itemProperties.FirstOrDefault(x => x.Name == "CommonName").Value.ToString();



                        var label = new Label
                        {
                            Text = (i + 1) + ". " + itemProperties.FirstOrDefault(x => x.Name == "CommonName").Value.ToString()
                        };



                        resultsgrid.Children.Add(label, 0, i);
                        activityIndicator.IsRunning = true;
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MagazineStoreApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        } 

        static HttpClient client = new HttpClient();
        static Uri apiBaseAddress = new Uri("http://magazinestore.azurewebsites.net");
                   
        static async Task RunAsync()
        {
            client.BaseAddress = apiBaseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                //Get api token
                string token = await GetToken();
                Console.WriteLine("Token = " + token);

                //Get list of subscribers
                List<Subscriber> subscribers = await GetSubscribers(token);

                //Get list of categories
                List<string> categories = await GetCategories(token);                

                Console.WriteLine("*********Categories*****************");              
               
                foreach(string category in categories){
                   
                    Console.WriteLine(category);
                }
                

                Console.WriteLine("************************************");                        
  
                //Get the list of all magazines of all categories
                List<Magazine> magazines = await GetAllMagazines(token, categories);

                Console.WriteLine("*********************************************************************************");
                Console.WriteLine("List of subscribers that are subscirbed to at least one magazine in each category");
                Console.WriteLine("*********************************************************************************");

                Answer answer = new Answer();
                answer.Subscribers = new List<string>();

                //Loop through subcriber list to check the list of subscribers that are subscribed to at least one mangazine in each category
                foreach (Subscriber subscriber in subscribers)
                {
     
                    bool isFound = (from m in magazines
                                  join c in categories on m.Category equals c
                                  join sm in subscriber.MagazineIds on m.Id equals sm
                                  group sm by m.Category into g
                                  select g).ToList().Count == 3;

                    if (isFound) {  
                        answer.Subscribers.Add(subscriber.Id);
                        Console.WriteLine("Subcriber :  " + subscriber.Id);
                    }
                    
                }

                Console.WriteLine("*********************************************************************************");
             
                AnswerResponse answerResponse = await PostAnswer(token, answer);

                Console.WriteLine("*********************************************************************************");
                Console.WriteLine("                     Answer Response");
                Console.WriteLine("*********************************************************************************");
                Console.WriteLine("Total Time     : " + answerResponse.Data.TotalTime);
                Console.WriteLine("Answer Correct : " + answerResponse.Data.AnswerCorrect);
                Console.WriteLine("Should Be      : ");
                if (answerResponse.Data.ShouldBe != null)
                {
                    foreach (string item in answerResponse.Data.ShouldBe)
                    {
                        Console.WriteLine(item);
                    }
                }
                Console.WriteLine("*********************************************************************************");

                Console.WriteLine(">Press any key to exit.");
                Console.ReadLine();

            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<List<Magazine>> GetAllMagazines(string token, List<string> categories)
        {
            Magazines magazines = new Magazines();
            magazines.Data = new List<Magazine>();
            foreach (string category in categories)
            {
                List<Magazine> _magazines = await GetMagazines(token, category.ToString());
                magazines.Data.AddRange(_magazines);
            }

            return magazines.Data;
        }

        static async Task<string> GetToken()
        {
            HttpResponseMessage response = await client.GetAsync(apiBaseAddress + "api/token");
            
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Tokens token = JsonConvert.DeserializeObject<Tokens>(result);
                if (token.Success)
                {
                    return token.Token;
                }
            }

            return null;
        }

        static async Task<List<string>> GetCategories(string token)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync(apiBaseAddress + "api/categories/"+ token);

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Categories categories = JsonConvert.DeserializeObject<Categories>(result);
                return categories.Data;
              
            }
            return null;
        }

        static async Task<List<Magazine>> GetMagazines(string token, string category)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync(apiBaseAddress + "api/magazines/" + token +"/" + category);

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Magazines magazines = JsonConvert.DeserializeObject<Magazines>(result);
                return magazines.Data;

            }
            return null;
        }

        static async Task<List<Subscriber>> GetSubscribers(string token)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync(apiBaseAddress + "api/subscribers/" + token);

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Subscribers subscribers = JsonConvert.DeserializeObject<Subscribers>(result);
                return subscribers.Data;

            }
            return null;
        }

        static async Task<AnswerResponse> PostAnswer(string token, Answer answer)
        {
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

             HttpContent content = new StringContent(JsonConvert.SerializeObject(answer), Encoding.UTF8, "application/json");

             HttpResponseMessage response = await client.PostAsync(apiBaseAddress + "api/answer/" + token,  content);

            if (response.IsSuccessStatusCode)            {
                string result = response.Content.ReadAsStringAsync().Result;
                AnswerResponse answerResponse = JsonConvert.DeserializeObject<AnswerResponse>(result);
                return answerResponse;

            }
            return null;
        }
    }

    public class Categories
    {
        public List<string> Data { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class Magazine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }

    public class Magazines
    {
        public List<Magazine> Data { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class Subscriber
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> MagazineIds { get; set; }
    }

    public class Subscribers
    {
        public List<Subscriber> Data { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class Tokens
    {
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class Answer
    {
        public List<string> Subscribers { get; set; }
    }
    
    public class AnswerResponse
    {
        public Data Data { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
    }

    public class Data
    {
        public string TotalTime { get; set; }
        public bool AnswerCorrect { get; set; }
        public List<string> ShouldBe { get; set; }
    }
}

using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNetFramework.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("Press Enter to continue calling the API, or any other key to exit..");
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                    await CallApi();
                else
                {
                    return;
                }
            }
        }

        private static async Task CallApi()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("[ENTER_THE_IDENTITY_AUTHORITY_PROVIDED_BY_AUTOCRIB]");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            Console.WriteLine("Calling IDP for Token...");
            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "[ENTER_THE_IDENTITY_CLIENT_ID_PROVIDED_BY_AUTOCRIB]",
                ClientSecret = "[ENTER_THE_IDENTITY_CLIENT_SECRET_PROVIDED_BY_AUTOCRIB]",
                Scope = "arcturuswebapi"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Error calling API");
                Console.WriteLine("\n\n");
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine("Token retrieved by IDP");
            Console.WriteLine("\n\n");
            Console.WriteLine(tokenResponse.Json);

            // call api
            var apiClient = new HttpClient();
            apiClient.DefaultRequestHeaders.Add("dbId", "[ENTER_THE_DBID_PROVIDED_BY_AUTOCRIB]");
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            Console.WriteLine("Set Bearer Token Header on Request to API...");
            Console.WriteLine("Calling API with Bearer Token Set in Header...");

            var response = await apiClient.GetAsync("[ENTER_THE_ARCTURUS_WEB_API_URL_PROVIDED_BY_AUTOCRIB]");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error calling API");
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            else
            {
                Console.WriteLine("API call was successful, writing out response contents:");
                Console.WriteLine("\n\n");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            Console.ReadKey();
        }
    }
}

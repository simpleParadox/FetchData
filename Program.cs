using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
namespace FetchData
{
    class Radiation
    {
        public string Id{ get; set; }
        public int Strength { get; set; }

    }
    class Program
    {
        private const string EndpointUri = "https://signal.documents.azure.com:443/";
        private const string PrimaryKey = "";
        private DocumentClient client;


        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, Radiation radiation)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, radiation.Id));
                Console.WriteLine("Found {0}", radiation.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), radiation);
                    Console.WriteLine("Created Radiation {0}", radiation.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            try
            {
                Program p = new Program();
                p.Start().Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private async void UpdateDocument(String databaseName, string collectionName, string id, Radiation updatedRadiation)
        {
            await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, id), updatedRadiation);
            Console.WriteLine("Document updated: ", updatedRadiation);
        }

        private async Task Start()
        {
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = "Strength" });

            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Strength"),
                new DocumentCollection { Id = "Values" });


            Random random = new Random();
            while (true)
            {
                string id = RandomString(10);
                int strength = random.Next(-90, -10);
                Radiation radiation = new Radiation { Id = id, Strength = strength };
                await CreateFamilyDocumentIfNotExists("Strength", "Values", radiation);
                Console.WriteLine("Document Created with id: " + id);
                await Task.Delay(2000);
            }

        }
           
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    
    }
}

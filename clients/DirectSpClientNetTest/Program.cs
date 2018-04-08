using DirectSp.DirectSpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectSpClientNetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dspClient = new DirectSpClient
            {
                clientId = "myaccount_a50225f017fb46139b9780e987f0baea",
                authBaseUri = new Uri("https://auth.iranian.cards"),
                resourceApiUri = new Uri("https://loyaltyapi.iranian.cards/api"),
            };


            try
            {
                dspClient.signInByPasswordGrant("Loyalty_Admin", "Password1").Wait();
                var a = dspClient.invoke("System_LoyaltyAccountSearch", new { searchCriteria = default(string)  }).Result;

                Console.WriteLine(a);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex.InnerException.Message);
            }
        }
    }
}

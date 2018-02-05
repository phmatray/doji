namespace Doji.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CryptoCompare;
    using Microsoft.Toolkit.Uwp.Helpers;
    using Models;
    using Pages.UtilityPages.GlossaryUtility;

    public class DataService : IDataService
    {
        private CryptoCompareClient _cryptoCompare;

        public DataService()
        {
            _cryptoCompare = CryptoCompareClient.Instance;
        }

        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données réelles !"));
        }

        public async Task<List<Exchange>> GetExchangesAsync()
        {
            var exchangeListResponse = await _cryptoCompare.Exchanges.ListAsync();
            var exchanges = exchangeListResponse
                .Select(x => new Exchange(x.Key, x.Value.Count))
                .OrderByDescending(x => x.NbrPairs)
                .ToList();

            return exchanges;
        }

        public async Task<List<GlossaryItem>> GetGlossaryAsync()
        {
            var retval = new List<GlossaryItem>();
            using (var jsonStream = await StreamHelper.GetPackagedFileStreamAsync("Pages/UtilityPages/GlossaryUtility/glossary.json"))
            {
                var jsonString = await jsonStream.ReadTextAsync();
                var parsed = GlossaryJson.FromJson(jsonString);

                foreach (var category in parsed.Categories)
                {
                    foreach (var element in category.Elements)
                    {
                        var item = new GlossaryItem
                        {
                            Category = category.Category,
                            Description = element.Description,
                            Title = element.Title
                        };
                        retval.Add(item);
                    }
                }
            }

            return retval;
        }
    }
}
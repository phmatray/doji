namespace Doji.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Toolkit.Uwp.Helpers;
    using Models;
    using Pages.UtilityPages.ExchangeUtility;
    using Pages.UtilityPages.GlossaryUtility;

    public class DataService : IDataService
    {
        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données réelles !"));
        }

        public async Task<List<Exchange>> GetExchangesAsync()
        {
            var retval = new List<Exchange>();
            using (var jsonStream = await StreamHelper.GetPackagedFileStreamAsync("Pages/UtilityPages/ExchangeUtility/exchanges.json"))
            {
                var jsonString = await jsonStream.ReadTextAsync();
                var parsed = ExchangeJson.FromJson(jsonString);

                foreach (var ex in parsed)
                {
                    var item = new Exchange
                    {
                        Name = ex.Name,
                        Description = ex.Description,
                        Country = ex.Country,
                        Change = ex.Change,
                        Logo = ex.Logo
                    };
                    retval.Add(item);
                }
            }

            return retval;
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
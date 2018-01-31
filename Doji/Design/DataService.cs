using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Doji.Models;
using Doji.Services;

namespace Doji.Design
{
    public class DesignDataService : IDataService
    {
        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données de Design !"));
        }

        public Task<List<GlossaryItem>> GetGlossaryAsync()
        {
            var service = new DataService();
            return service.GetGlossaryAsync();
        }
    }
}
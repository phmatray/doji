namespace Doji.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public class DesignDataService : IDataService
    {
        private DataService _dataService;

        public DesignDataService()
        {
            this._dataService = new DataService();
        }

        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données de Design !"));
        }

        public Task<List<GlossaryItem>> GetGlossaryAsync()
            => this._dataService.GetGlossaryAsync();

        public Task<List<Exchange>> GetExchangesAsync()
            => this._dataService.GetExchangesAsync();
    }
}
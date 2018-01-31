namespace Doji.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IDataService
    {
        Task<DataItem> GetData();
        Task<List<GlossaryItem>> GetGlossaryAsync();
    }
}
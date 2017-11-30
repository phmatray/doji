using System.Threading.Tasks;

namespace Doji.Models
{
    public class DataService : IDataService
    {
        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données réelles !"));
        }
    }
}
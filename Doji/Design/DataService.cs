using System.Threading.Tasks;
using Doji.Models;

namespace Doji.Design
{
    public class DesignDataService : IDataService
    {
        public Task<DataItem> GetData()
        {
            return Task.FromResult(new DataItem("Hello World - Données de Design !"));
        }
    }
}
using System.Threading.Tasks;

namespace Doji.Models
{
    public interface IDataService
    {
        Task<DataItem> GetData();
    }
}
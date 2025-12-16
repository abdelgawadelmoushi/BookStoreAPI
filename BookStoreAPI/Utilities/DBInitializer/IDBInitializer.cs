using System.Threading.Tasks;

namespace BookStoreAPI.Utilities.DBInitializer
{
    public interface IDBInitializer
    {
        Task InitializeAsync();
    }
}

using FitStackDBL.Model;
using System.Threading.Tasks;

namespace FitStackDBL.Repository
{
    public interface IUsersRepository
    {
        Task<int> CreateUser(Model.Users user);
        Task<Model.Users?> GetByEmail(string email);
        Task<Model.Users?> GetById(int userId);
        Task UpdateLoginStats(int userId);
       Task<Users?> GetUserByIdAsync(int id);
    }
}

using FitStackDBL.UOW;
using FitStackDBL.Repository;

namespace FitStackDBL
{
    public class Bl
    {
        private readonly string _ConnectionString;
        private readonly UnitOfWork _db;

        public Bl(string connectionString)
        {
            _ConnectionString = connectionString;
            _db = new UnitOfWork(_ConnectionString);

            UsersRepository = new UsersRepository(connectionString);
        }

        public string ConnectionString => _ConnectionString;

        public UsersRepository UsersRepository { get; }
    }
}

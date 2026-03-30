using System;
using System.Collections.Generic;
using System.Text;

namespace FitStackDBL.UOW
{
    public class UnitOfWork
    {
        private string connectionString;

        public UnitOfWork(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }
}

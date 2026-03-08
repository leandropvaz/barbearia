using Barbearia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barbearia.Infrastructure
{
    public class BarbeariaDbContextFactory : IDesignTimeDbContextFactory<BarbeariaDbContext>
    {
        public BarbeariaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BarbeariaDbContext>();

            // Apenas para design time (migrations) — a connection string real vem do appsettings em runtime
            optionsBuilder.UseSqlServer("Server=tcp:barbearia.database.windows.net,1433;Initial Catalog=BarbeariaDb;Persist Security Info=False;User ID=barbearia;Password=@Babi2505;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new BarbeariaDbContext(optionsBuilder.Options);
        }
    }
}

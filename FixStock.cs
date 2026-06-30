using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql("Host=localhost;Database=mypetclinic_db;Username=postgres;Password=postgres");
        using var db = new ApplicationDbContext(builder.Options);
        
        var medicines = db.Medicines.Include(m => m.Batches).ToList();
        foreach (var m in medicines)
        {
            var actualStock = m.Batches.Sum(b => b.CurrentQuantity);
            if (m.StockQuantity != actualStock)
            {
                Console.WriteLine($"Fixing Medicine {m.Id} ({m.Name}): old {m.StockQuantity} -> new {actualStock}");
                m.StockQuantity = actualStock;
            }
        }
        db.SaveChanges();
        Console.WriteLine("Done fixing stock.");
    }
}

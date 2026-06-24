using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AppDbContext : DbContext {
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql("Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.yzbacphijkzkjahlsbdz;Password=cogangviphuonglamhanh;SSL Mode=Require;Trust Server Certificate=true");
    }
}

class Program {
    static void Main() {
        using (var db = new AppDbContext()) {
            db.Database.ExecuteSqlRaw("UPDATE appointments SET appointment_date = appointment_date + interval '1 day' WHERE id IN (113, 114)");
            Console.WriteLine("Fixed DB!");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPetClinic.Infrastructure.Persistence;

namespace WebApi
{
    public static class DbPatcher
    {
        public static void Patch(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT id, user_id, title, content, is_read FROM notifications";
            db.Database.OpenConnection();
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("--- NOTIFICATIONS IN DB ---");
            int count = 0;
            while (reader.Read())
            {
                count++;
                Console.WriteLine($"Id: {reader[0]}, UserId: {reader[1]}, Title: {reader[2]}, IsRead: {reader[4]}");
            }
            Console.WriteLine($"Total: {count}");
        }
    }
}

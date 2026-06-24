using System;
using System.Threading.Tasks;
using Npgsql;

namespace DataFixer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.yzbacphijkzkjahlsbdz;Password=cogangviphuonglamhanh;SSL Mode=Require;Trust Server Certificate=true";

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            Console.WriteLine("Database connected.");

            // 1. Create a Customer for Users who don't have one (by checking user."CustomerId" IS NULL)
            var selectUsersCmd = new NpgsqlCommand("SELECT id, email, full_name, phone FROM users WHERE \"CustomerId\" IS NULL AND role_id = (SELECT id FROM roles WHERE name = 'customer' LIMIT 1);", conn);
            using (var reader = await selectUsersCmd.ExecuteReaderAsync())
            {
                var usersToFix = new System.Collections.Generic.List<dynamic>();
                while (await reader.ReadAsync())
                {
                    usersToFix.Add(new {
                        Id = reader.GetGuid(0),
                        Email = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        FullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Phone = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
                await reader.CloseAsync();

                foreach (var u in usersToFix)
                {
                    Guid newCustomerId = Guid.NewGuid();
                    bool customerCreated = false;

                    if (!string.IsNullOrEmpty(u.Email))
                    {
                        var checkCustomer = new NpgsqlCommand("SELECT id FROM customers WHERE email = @email LIMIT 1", conn);
                        checkCustomer.Parameters.AddWithValue("email", u.Email);
                        var existingId = await checkCustomer.ExecuteScalarAsync();
                        if (existingId != null && existingId != DBNull.Value)
                        {
                            newCustomerId = (Guid)existingId;
                            customerCreated = true;
                        }
                    }

                    if (!customerCreated)
                    {
                        string code = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(100, 999);
                        var insertCustomer = new NpgsqlCommand("INSERT INTO customers (id, customer_code, full_name, phone, email, has_account, created_at) VALUES (@id, @code, @name, @phone, @email, true, @now)", conn);
                        insertCustomer.Parameters.AddWithValue("id", newCustomerId);
                        insertCustomer.Parameters.AddWithValue("code", code);
                        insertCustomer.Parameters.AddWithValue("name", string.IsNullOrEmpty(u.FullName) ? u.Email : u.FullName);
                        insertCustomer.Parameters.AddWithValue("phone", (object)u.Phone ?? DBNull.Value);
                        insertCustomer.Parameters.AddWithValue("email", (object)u.Email ?? DBNull.Value);
                        insertCustomer.Parameters.AddWithValue("now", DateTime.UtcNow);
                        await insertCustomer.ExecuteNonQueryAsync();
                    }

                    var updateUser = new NpgsqlCommand("UPDATE users SET \"CustomerId\" = @customerId WHERE id = @userId", conn);
                    updateUser.Parameters.AddWithValue("customerId", newCustomerId);
                    updateUser.Parameters.AddWithValue("userId", u.Id);
                    await updateUser.ExecuteNonQueryAsync();
                    
                    Console.WriteLine($"Linked Customer {newCustomerId} for User {u.Id}");
                }
            }

            // 2. Map old Pets, Appointments to the new CustomerId
            var updatePets = new NpgsqlCommand(@"
                UPDATE pets 
                SET owner_id = u.""CustomerId""
                FROM users u 
                WHERE pets.owner_id = u.id AND u.""CustomerId"" IS NOT NULL;
            ", conn);
            int petsUpdated = await updatePets.ExecuteNonQueryAsync();
            Console.WriteLine($"Updated {petsUpdated} pets.");

            var updateAppointments = new NpgsqlCommand(@"
                UPDATE appointments 
                SET customer_id = u.""CustomerId""
                FROM users u 
                WHERE appointments.customer_id = u.id AND u.""CustomerId"" IS NOT NULL;
            ", conn);
            int apptsUpdated = await updateAppointments.ExecuteNonQueryAsync();
            Console.WriteLine($"Updated {apptsUpdated} appointments.");

            Console.WriteLine("Data restoration complete.");
        }
    }
}

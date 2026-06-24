using System;
using Npgsql;

var connString = "Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.yzbacphijkzkjahlsbdz;Password=cogangviphuonglamhanh;SSL Mode=Require;Trust Server Certificate=true";

using var conn = new NpgsqlConnection(connString);
conn.Open();

var targetEmail = "tangocnam2210@gmail.com";

// Delete from appointments (if any) using customer_id
using (var cmd = new NpgsqlCommand("DELETE FROM appointments WHERE customer_id IN (SELECT id FROM customers WHERE email = @e)", conn))
{
    cmd.Parameters.AddWithValue("e", targetEmail);
    cmd.ExecuteNonQuery();
}

// Delete from pets (if any) using owner_id
using (var cmd = new NpgsqlCommand("DELETE FROM pets WHERE owner_id IN (SELECT id FROM customers WHERE email = @e)", conn))
{
    cmd.Parameters.AddWithValue("e", targetEmail);
    cmd.ExecuteNonQuery();
}

// Delete from users
using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE email = @e", conn))
{
    cmd.Parameters.AddWithValue("e", targetEmail);
    var rows1 = cmd.ExecuteNonQuery();
    Console.WriteLine($"Deleted {rows1} rows from users.");
}

// Delete from customers
using (var cmd = new NpgsqlCommand("DELETE FROM customers WHERE email = @e", conn))
{
    cmd.Parameters.AddWithValue("e", targetEmail);
    var rows2 = cmd.ExecuteNonQuery();
    Console.WriteLine($"Deleted {rows2} rows from customers.");
}

Console.WriteLine("Done deleting tangocnam2210@gmail.com");

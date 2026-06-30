using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=localhost;Database=mypetclinic_db;Username=postgres;Password=postgres";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd = new NpgsqlCommand("UPDATE \"Medicines\" m SET \"StockQuantity\" = (SELECT COALESCE(SUM(\"CurrentQuantity\"), 0) FROM \"MedicineBatches\" b WHERE b.\"MedicineId\" = m.\"Id\");", conn);
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine($"Updated {rows} medicines.");
    }
}

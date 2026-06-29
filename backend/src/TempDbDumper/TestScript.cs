using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Domain.Entities;

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=MyPetClinic;Username=postgres;Password=postgres"));

var provider = services.BuildServiceProvider();
var db = provider.GetRequiredService<ApplicationDbContext>();

using var transaction = db.Database.BeginTransaction();
try
{
    var customer = new Customer
    {
        Id = Guid.NewGuid(),
        CustomerCode = "CUS-TEST",
        FullName = "Test",
        Phone = "0999999999",
        HasAccount = false,
        Status = "Active",
        CreatedAt = DateTime.UtcNow
    };
    db.Customers.Add(customer);
    db.SaveChanges(); // First save

    var pet = new Pet
    {
        CustomerId = customer.Id,
        Name = "Dog",
        CreatedAt = DateTime.UtcNow
    };
    db.Pets.Add(pet);
    db.SaveChanges(); // Second save
    
    Console.WriteLine("Success!");
    transaction.Rollback();
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
    if (ex.InnerException != null)
    {
        Console.WriteLine("Inner: " + ex.InnerException.Message);
    }
}

namespace App.Api.Persistence.Models;

public sealed record ProductInRange(Guid Id, string Name, decimal Price, string CategoryName);

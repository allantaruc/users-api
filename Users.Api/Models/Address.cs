namespace Users.Api.Models;

public class Address
{
    public int Id { get; set; }    
    public string? Street { get; set; } //MANDATORY        
    public string? City { get; set; } //MANDATORY 
    public int? PostCode { get; set; }
}
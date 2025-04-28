namespace Users.Api.Models;

public class User
{
    public User()
    {
        Employments = [];
    }
    public int Id { get; set; } 
    public string? FirstName { get; set; } //MANDATORY 
    public string? LastName { get; set; } //MANDATORY 
    public string? Email { get; set; } //MANDATORY, UNIQUE
    public string? PasswordHash { get; set; } //Stores the hashed password
    public string? PasswordSalt { get; set; } //Stores the salt used for password hashing
    public Address? Address { get; set; } 
    public List<Employment> Employments { get; set; } //  add, update an existing employment, 
    // delete an existing employment
}
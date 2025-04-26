namespace Users.Api.Models;

public class Employment
{
    public int Id { get; set; }           
    public string? Company { get; set; } //MANDATORY
    public uint? MonthsOfExperience { get; set; } //MANDATORY 
    public uint? Salary { get; set; } //MANDATORY
    public DateTime? StartDate { get; set; } //MANDATORY
    public DateTime? EndDate { get; set; }        
}

namespace JobBoardApi.Models;

public class JobOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}

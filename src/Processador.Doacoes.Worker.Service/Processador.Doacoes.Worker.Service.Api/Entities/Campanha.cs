namespace Processador.Doacoes.Worker.Service.Api.Entities;

public class Campanha
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal FinancialGoal { get; set; }

    public decimal TotalRaised { get; set; }

    public CampanhaStatus Status { get; set; }

    public void AddDonation(decimal amount)
    {
        if (amount <= 0)
            throw new Exception("Donation amount must be greater than zero.");

        TotalRaised += amount;
    }
}
public enum CampanhaStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3
}
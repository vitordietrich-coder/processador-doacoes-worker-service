namespace Processador.Doacoes.WorkerService.Events;

public class DonationReceivedEvent
{
    public Guid DonationId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
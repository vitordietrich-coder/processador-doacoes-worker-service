using Microsoft.EntityFrameworkCore;
using Processador.Doacoes.WorkerService.Workers;
using Campanhas.Microservice.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<CampaignDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHostedService<DonationConsumerWorker>();

var host = builder.Build();

host.Run();
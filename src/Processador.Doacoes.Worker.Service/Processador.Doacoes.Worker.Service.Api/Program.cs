using Microsoft.EntityFrameworkCore;
using Processador.Doacoes.Worker.Service.Api.Data;
using Processador.Doacoes.WorkerService.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<WorkerDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHostedService<DonationConsumerWorker>();

var host = builder.Build();

host.Run();
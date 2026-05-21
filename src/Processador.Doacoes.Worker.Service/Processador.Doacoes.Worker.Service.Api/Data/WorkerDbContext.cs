using Microsoft.EntityFrameworkCore;
using Processador.Doacoes.Worker.Service.Api.Entities;

namespace Processador.Doacoes.Worker.Service.Api.Data;

public class WorkerDbContext : DbContext
{
	public WorkerDbContext(DbContextOptions<WorkerDbContext> options)
		: base(options)
	{
	}

	public DbSet<Campanha> Campanhas { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Campanha>(builder =>
		{
			builder.ToTable("Campanhas");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Title)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(x => x.Description)
				.IsRequired()
				.HasMaxLength(2000);

			builder.Property(x => x.FinancialGoal)
				.HasPrecision(18, 2);

			builder.Property(x => x.TotalRaised)
				.HasPrecision(18, 2);

			builder.Property(x => x.Status)
				.HasConversion<int>();
		});
	}
}
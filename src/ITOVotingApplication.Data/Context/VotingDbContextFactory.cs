using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ITOVotingApplication.Data.Context
{
	public class VotingDbContextFactory : IDesignTimeDbContextFactory<VotingDbContext>
	{
		public VotingDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<VotingDbContext>();

			// Solution root'u bul
			var currentDirectory = Directory.GetCurrentDirectory();
			var directoryInfo = new DirectoryInfo(currentDirectory);

			// Solution root'a git (VotingApplication.sln'in olduğu yer)
			while (directoryInfo != null && !directoryInfo.GetFiles("*.sln").Any())
			{
				directoryInfo = directoryInfo.Parent;
			}

			IConfigurationRoot configuration = null;
			string connectionString = null;

			if (directoryInfo != null)
			{
				// Önce Web projesindeki appsettings.json'ı dene
				var webProjectPath = Path.Combine(directoryInfo.FullName, "VotingApplication.Web");
				if (!Directory.Exists(webProjectPath))
				{
					// src klasörü varsa
					webProjectPath = Path.Combine(directoryInfo.FullName, "src", "VotingApplication.Web");
				}

				if (Directory.Exists(webProjectPath))
				{
					configuration = new ConfigurationBuilder()
						.SetBasePath(webProjectPath)
						.AddJsonFile("appsettings.json", optional: true)
						.AddJsonFile("appsettings.Development.json", optional: true)
						.Build();

					connectionString = configuration.GetConnectionString("DefaultConnection");
				}
			}

			// Web projesinden okuyamazsa, Data projesindeki appsettings.json'ı dene
			if (string.IsNullOrEmpty(connectionString))
			{
				configuration = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: true)
					.Build();

				connectionString = configuration.GetConnectionString("DefaultConnection");
			}

			// Hala boşsa default connection string kullan
			if (string.IsNullOrEmpty(connectionString))
			{
				Console.WriteLine("Warning: Connection string not found in Web or Data project. Using default connection string.");
				connectionString = "Server=(localdb)\\mssqllocaldb;Database=VotingAppDB;Trusted_Connection=True;MultipleActiveResultSets=true";
			}
			else
			{
				Console.WriteLine($"Using connection string from configuration: {connectionString}");
			}

			optionsBuilder.UseSqlServer(connectionString);

			return new VotingDbContext(optionsBuilder.Options);
		}
	}
}
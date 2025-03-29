using Common.ConfigurationSettings;
using Freddie.Helpers.Database;
using Freddie.Helpers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Assign configuration to ConfigurationSettingsHelper
ConfigurationSettingsHelper.Configuration = builder.Configuration;

// Load environment-specific settings
var environment = builder.Environment.EnvironmentName; // Gets Development, Production, or Test
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
      .ReadFrom.Configuration(builder.Configuration)
      .Enrich.FromLogContext()
      .CreateLogger();

// 🔹 Configure logging to capture errors
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(ConfigSettings.ConnectionString.DefaultConnection));

// Register services
builder.Services.AddTransient<GmailApiService>();
builder.Services.AddTransient<GoogleSheetsService>();
builder.Services.AddTransient<RecruitmentProcessor>();
builder.Services.AddTransient<ResumeProcessor>();
builder.Services.AddTransient<OpenAiEvaluationService>();
builder.Services.AddTransient<GoogleAuthService>();
builder.Services.AddTransient<SmtpEmailService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var connection = dbContext.Database.GetDbConnection();

    try
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        // Drop all tables before migration
        await dbContext.Database.ExecuteSqlRawAsync(@"
            PRAGMA foreign_keys = OFF;

            -- Step 1: Drop Foreign Key Constraints
            -- (Not necessary in SQLite, as it drops FKs when dropping tables)

            -- Step 2: Drop Tables
            SELECT 'DROP TABLE IF EXISTS ' || name || ';'
            FROM sqlite_master
            WHERE type='table';

            PRAGMA foreign_keys = ON;
        ");

        try
        {
            await dbContext.Database.MigrateAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation(ex, "An error occurred while applying migrations. The changes were rolled back." + JsonConvert.SerializeObject(ex));
            throw;
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation(ex, "An error occurred while backing up the database." + JsonConvert.SerializeObject(ex));
        throw;
    }
    finally
    {
        await connection.CloseAsync();
    }
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

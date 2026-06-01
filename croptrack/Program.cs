using CropTrack.Data;
using CropTrack.Repositories;
using CropTrack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IFarmerRepository, FarmerRepository>();
builder.Services.AddScoped<IFarmerService, FarmerService>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddScoped<IFieldRepository, FieldRepository>();
builder.Services.AddScoped<IFieldCropRepository, FieldCropRepository>();
builder.Services.AddScoped<IFieldCropService, FieldCropService>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IWeatherLogRepository, WeatherLogRepository>();
builder.Services.AddScoped<IWeatherLogService, WeatherLogService>();
builder.Services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();
builder.Services.AddScoped<IMarketPriceService, MarketPriceService>();
builder.Services.AddHttpClient<IExternalDataService, ExternalDataService>();
builder.Services.AddScoped<IProduceKnowledgeService, ProduceKnowledgeService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });


builder.Services.AddDbContext<FieldDbTrackContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configure JWT authentication with flexible key sourcing
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Resolve signing key: first try config, then environment variable, then a safe default for dev
        var configuredKey = builder.Configuration["Jwt:Key"];
        var jwtKey = string.IsNullOrWhiteSpace(configuredKey)
            ? System.Environment.GetEnvironmentVariable("JWT_KEY")
            : configuredKey;
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            // Do not commit real secrets; this is a dev fallback. Change in production to a secure secret.
            jwtKey = "DevCropTrackSecretKeyChangeThisInProd!";
        }

        var issuer = builder.Configuration["Jwt:Issuer"] ?? "CropTrackAPI";
        var audience = builder.Configuration["Jwt:Audience"] ?? "CropTrackUsers";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            // Use the resolved key for signing validation
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CropTrack API",
        Description = "API for managing farmers, fields, and crops"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5075);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CropTrack API v1");
        c.RoutePrefix = "swagger";
    });
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FieldDbTrackContext>();
    if (db.Database.EnsureCreated())
    {
    }
    else
    {
        try { db.Database.Migrate(); } 
        catch { }
    }

    EnsureFarmerOwnershipColumns(db);
    EnsureProduceKnowledgeBase(db);
}

app.Run();

static void EnsureFarmerOwnershipColumns(FieldDbTrackContext db)
{
    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Crops]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Crops]', N'FarmerId') IS NULL
BEGIN
    ALTER TABLE [Crops] ADD [FarmerId] INT NULL;
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Regions]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Regions]', N'FarmerId') IS NULL
BEGIN
    ALTER TABLE [Regions] ADD [FarmerId] INT NULL;
END;
");

    db.Database.ExecuteSqlRaw(@"
DECLARE @DefaultFarmerId INT = (SELECT TOP(1) [FarmerId] FROM [Farmers] ORDER BY [FarmerId]);

IF @DefaultFarmerId IS NOT NULL
BEGIN
    IF OBJECT_ID(N'[Crops]', N'U') IS NOT NULL AND COL_LENGTH(N'[Crops]', N'FarmerId') IS NOT NULL
    BEGIN
        EXEC sp_executesql N'UPDATE [Crops] SET [FarmerId] = @fid WHERE [FarmerId] IS NULL', N'@fid INT', @fid=@DefaultFarmerId;
    END;

    IF OBJECT_ID(N'[Regions]', N'U') IS NOT NULL AND COL_LENGTH(N'[Regions]', N'FarmerId') IS NOT NULL
    BEGIN
        EXEC sp_executesql N'UPDATE [Regions] SET [FarmerId] = @fid WHERE [FarmerId] IS NULL', N'@fid INT', @fid=@DefaultFarmerId;
    END;
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Crops]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Crops]', N'FarmerId') IS NOT NULL
    AND EXISTS (SELECT 1 FROM [Crops] WHERE [FarmerId] IS NOT NULL)
BEGIN
    BEGIN TRY
        ALTER TABLE [Crops] ALTER COLUMN [FarmerId] INT NOT NULL;
    END TRY
    BEGIN CATCH
    END CATCH;
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Regions]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Regions]', N'FarmerId') IS NOT NULL
    AND EXISTS (SELECT 1 FROM [Regions] WHERE [FarmerId] IS NOT NULL)
BEGIN
    BEGIN TRY
        ALTER TABLE [Regions] ALTER COLUMN [FarmerId] INT NOT NULL;
    END TRY
    BEGIN CATCH
    END CATCH;
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Crops]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Crops]', N'FarmerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Crops_FarmerId' AND object_id = OBJECT_ID(N'[Crops]'))
BEGIN
    CREATE INDEX [IX_Crops_FarmerId] ON [Crops]([FarmerId]);
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Regions]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Regions]', N'FarmerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Regions_FarmerId' AND object_id = OBJECT_ID(N'[Regions]'))
BEGIN
    CREATE INDEX [IX_Regions_FarmerId] ON [Regions]([FarmerId]);
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Crops]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Crops]', N'FarmerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Crops_Farmers_FarmerId')
BEGIN
    ALTER TABLE [Crops] WITH NOCHECK
    ADD CONSTRAINT [FK_Crops_Farmers_FarmerId]
    FOREIGN KEY([FarmerId]) REFERENCES [Farmers]([FarmerId]);
END;
");

    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Regions]', N'U') IS NOT NULL
    AND COL_LENGTH(N'[Regions]', N'FarmerId') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Regions_Farmers_FarmerId')
BEGIN
    ALTER TABLE [Regions] WITH NOCHECK
    ADD CONSTRAINT [FK_Regions_Farmers_FarmerId]
    FOREIGN KEY([FarmerId]) REFERENCES [Farmers]([FarmerId]);
END;
");
}

static void EnsureProduceKnowledgeBase(FieldDbTrackContext db)
{
    db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Produces]', N'U') IS NULL
BEGIN
    CREATE TABLE [Produces] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(MAX) NOT NULL,
        [ScientificName] NVARCHAR(MAX) NOT NULL,
        [Category] NVARCHAR(MAX) NOT NULL,
        [AvgDaysToHarvest] INT NOT NULL,
        [PlantingDepthCm] FLOAT NOT NULL,
        [SpacingCm] FLOAT NOT NULL,
        [MinTempC] FLOAT NOT NULL,
        [MaxTempC] FLOAT NOT NULL,
        [IdealTempC] FLOAT NOT NULL,
        [MinPh] FLOAT NOT NULL,
        [MaxPh] FLOAT NOT NULL,
        [WaterIntensity] NVARCHAR(MAX) NOT NULL
    );
END;
");

    if (!db.Produces.Any())
    {
        db.Produces.AddRange(SeedData.Produces);
        db.SaveChanges();
    }
}

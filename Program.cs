using Dapper;
using Serilog;
using storage.Database;
using storage.Endpoints;
using storage.Repositories;
using storage.Services.Image;
using storage.Validators;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
ILogger logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.AddSerilog();
builder.Services.AddSingleton(logger);

builder.Services.AddSingleton<IImageUploadService, ImageImageUploadService>();
builder.Services.AddSingleton<UploadedImageValidator>();

builder.Services.AddSingleton(new DatabaseConfig { Dsn = builder.Configuration["DatabaseName"] });
builder.Services.AddSingleton<DatabaseBootstrap>();
builder.Services.AddSingleton<ImageRepository>();
builder.Services.AddSingleton<ImageTransformationService>();

builder.Services.AddControllers();

var app = builder.Build();

SqlMapper.AddTypeHandler(new GuidTypeHandler());
SqlMapper.RemoveTypeMap(typeof(Guid));
SqlMapper.RemoveTypeMap(typeof(Guid?));

app.MapImageUploadEndpoints();

app.Services.GetService<DatabaseBootstrap>()!.Setup();

app.Run();
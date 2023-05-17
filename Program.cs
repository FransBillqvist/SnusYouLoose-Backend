using DAL;
using DAL.Interfaces;
using Microsoft.Extensions.Options;
using Repository;
using Services;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoDbSettings>(
builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddScoped<MongoDbSettings>(serviceProvider =>
        serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddScoped(typeof(IGenericMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<ISnuffLogService, SnuffLogService>();
builder.Services.AddScoped<ICurrentSnuffService, CurrentSnuffService>();
builder.Services.AddScoped<ISnuffService, SnuffService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHabitService, HabitService>();

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(
    options =>
    {
        options.Title = "SUL Backend API";
        options.Description = "API for SUL communicating with mongodb";
        options.DocumentName = "v1";
        options.Version = "v1";
    }
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    builder.
    WithOrigins("http://192.168.0.4:5126").
    AllowAnyHeader().
    AllowAnyMethod().
    AllowCredentials());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    builder.
    WithOrigins("https://192.168.0.4:7162").
    AllowAnyHeader().
    AllowAnyMethod().
    AllowCredentials());
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    builder.
    WithOrigins("exp://192.168.0.10:19000").
    AllowAnyHeader().
    AllowAnyMethod().
    AllowCredentials());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi3();
    app.UseReDoc();
    app.UseCors();
}


app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
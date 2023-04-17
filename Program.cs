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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

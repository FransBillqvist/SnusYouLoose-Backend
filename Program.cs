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
        options.Title = "SUL BE API";
        options.Description = "API for SUL";
        options.DocumentName = "v1";
        options.Version = "v1";
    }
);
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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

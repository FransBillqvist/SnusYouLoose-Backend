using DAL.Models;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<SnuffDatabaseSettings>(
builder.Configuration.GetSection("SnuffStorage"));
builder.Services.AddSingleton<CurrentSnuffService>();
builder.Services.AddSingleton<HabitService>();
builder.Services.AddSingleton<SnuffLogService>();
builder.Services.AddSingleton<SnuffService>();
builder.Services.AddSingleton<UserService>();
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

using System.Text;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using DAL;
using DAL.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Repository;
using Services;
using Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

var mongoDbIdentitySettings = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new AspNetCore.Identity.MongoDbCore.Infrastructure.MongoDbSettings
    {
        ConnectionString = "mongodb://localhost:27017",
        DatabaseName = "SnuffStorage"
    },
    IdentityOptionsAction = options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        //lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    }
};

builder.Services.ConfigureMongoDbIdentity<AuthUser, Role, Guid>(mongoDbIdentitySettings)
    .AddUserManager<UserManager<AuthUser>>()
    .AddSignInManager<SignInManager<AuthUser>>()
    .AddRoleManager<RoleManager<Role>>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(x =>
{

    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidIssuer = "http://localhost:5126",
        ValidAudience = "http://localhost:5126",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("B055355up3R53Cr3tK3y@345")),
    };
});

builder.Services.Configure<DAL.MongoDbSettings>(
builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddScoped<DAL.MongoDbSettings>(serviceProvider =>
        serviceProvider.GetRequiredService<IOptions<DAL.MongoDbSettings>>().Value);

builder.Services.AddScoped(typeof(IGenericMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<ISnuffLogService, SnuffLogService>();
builder.Services.AddScoped<ICurrentSnuffService, CurrentSnuffService>();
builder.Services.AddScoped<ISnuffService, SnuffService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<IProgressionService, ProgressionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder.Services.AddControllers();
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                    new HeaderApiVersionReader("x-api-version"),
                                                    new MediaTypeApiVersionReader("x-api-version"));
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

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
    WithOrigins("http://192.168.0.10:5126").
    AllowAnyHeader().
    AllowAnyMethod().
    AllowCredentials());
});

// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(builder =>
//     builder.
//     WithOrigins("https://192.168.0.10:7162").
//     AllowAnyHeader().
//     AllowAnyMethod().
//     AllowCredentials());
// });


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
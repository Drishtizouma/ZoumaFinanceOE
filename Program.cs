


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZoumaFinance;
using ZoumaFinance.Models;
using ZoumaFinance.Repository.Interface;
using ZoumaFinance.Repository;
using ZoumaFinance.Respository;
using ZoumaFinance.Controllers;
using ZoumaFinance.Service;
using ZoumaFinance.Service.Interface;
using ZoumaFinance.Mapper;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;
using System.Text.Json;
using Serilog.Events;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ZoumaAttendanceApplication.Controllers;
using Microsoft.Extensions.Hosting;
using ZoumaFinance.DTO;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DashBoardDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ZoumaFinanceDB")));
const string policyName = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName, builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();

    });
});
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Ignore lower level logs from Microsoft
    .MinimumLevel.Override("System", LogEventLevel.Warning)    // Ignore lower level logs from System
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
//        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
//    });
// Add services to the container.
//builder.Services.AddDbContext<DashBoardDBContext>(options =>
// options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<DashBoardDBContext>();
builder.Services.AddHttpClient();

//builder.Services.AddControllers();

//builder.Services.AddScoped<EmployeeController>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IMarketingRepository, MarketingRepository>();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IPORepository, PORepository>();
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
builder.Services.AddScoped<INCHistoryRepository, NCHistoryRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IQuotationRepo, QuotationRepo>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<ICompanyMasterRepository, CompanyMasterRepository>();
builder.Services.AddScoped<ICompanyBranchRepository, CompanyBranchRepository>();
builder.Services.AddScoped<ICompanyBankDetailsRepository, CompanyBankDetailsRepository>();
builder.Services.AddScoped<ITermConditionRepository, TermConditionRepository>();
builder.Services.AddScoped<IDeskRepository, DeskRepository>();





builder.Services.AddHttpContextAccessor();

// related to background Task
builder.Services.AddScoped<EmployeeController>();
builder.Services.AddScoped<NCHistoryController>();
builder.Services.AddHostedService<PeriodicBackgroundService>();
builder.Services.AddSingleton<BaseUrlProvider>();


//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
//    });

builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IMailtemplateRepository, MailtemplateRepository>();
builder.Services.AddAutoMapper(typeof(EmployeeMappingProfile));
builder.Services.AddScoped<EmployeeRepository>();
//builder.Services.AddHostedService<MyBackgroundService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100_000_000; // 100 MB
});

builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

        });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

//Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value))
    };
});

SD.ZohoAPIBase = builder.Configuration["ServiceUrls:ZohoAPI"];
SD.TokenAPIBase = builder.Configuration["ServiceUrls:TokenAPI"];
SD.DeskAPIBase = builder.Configuration["ServiceUrls:DeskAPI"];

var app = builder.Build();

UserContext.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "EmployeeImages")),
    RequestPath = "/EmployeeImages"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "EmployeeImages","template")),
    RequestPath = "/EmployeeImages/template"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "CustomerImages")),
    RequestPath = "/CustomerImages"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "UploadMakertingData")),
    RequestPath = "/UploadMakertingData"
});
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseCors(policyName);

app.UseMiddleware<BaseUrlMiddleware>();

//app.UseMiddleware<BaseUrlMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<UserActivityMiddleware>();

app.MapControllers();

app.Run();

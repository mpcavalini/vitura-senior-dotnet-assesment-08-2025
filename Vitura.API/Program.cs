using FluentValidation;
using Vitura.API.Services;
using Vitura.API.Mapping;
using Vitura.API.Validation;

var builder = WebApplication.CreateBuilder(args);


// Register the hosted service for data loading
builder.Services.AddHostedService<OrderDataLoader>();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register FluentValidation validators manually
builder.Services.AddScoped<IValidator<OrderQueryParams>, OrderQueryParamsValidator>();

// Register your services
//builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IOrderRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<InMemoryOrderRepository>>();
    return new InMemoryOrderRepository(logger);
});
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderMapper, OrderMapper>();
builder.Services.AddScoped<IOrderValidator, OrderValidator>();



var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for integration tests
public partial class Program { }
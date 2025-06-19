using InstallmentAdvisorApi.Models;
using InstallmentAdvisorApi.Repositories;
using InstallmentAdvisorApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICustomerRepository>(sp => new CustomerRepository("Data"));
builder.Services.AddSingleton<ICustomerService, CustomerService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/customers/{customerId}/usage", (string customerId, ICustomerService service) =>
{
    var result = service.GetUsage(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get customer usage")
    .WithDescription("Get the usage history for the customer per month.");

app.MapGet("/customers/{customerId}/payments", (string customerId, ICustomerService service) =>
{
    var result = service.GetPayments(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get customer payments")
    .WithDescription("Get the payment history for the customer, including dates and amounts.");

app.MapGet("/customers/{customerId}/endofyear-estimate", (string customerId, ICustomerService service) =>
{
    var result = service.GetEndOfYearEstimate(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get end-of-year estimate")
    .WithDescription("Get the end-of-year estimate for the customer, including usage and payments.");

app.MapGet("/customers/{customerId}/pricesheet", (string customerId, ICustomerService service) =>
{
    var result = service.GetPriceSheet(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get price sheet")
    .WithDescription("Get the current and historical price sheet for the customer.");

app.MapGet("/customers/{customerId}/contract", (string customerId, ICustomerService service) =>
{
    var result = service.GetContract(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get customer contract")
    .WithDescription("Get the contract details for the customer, start and end dates and energy types supplied to the customer.");

app.MapGet("/customers/{customerId}/installment", (string customerId, ICustomerService service) =>
{
    var result = service.GetInstallments(customerId);
    return Results.Ok(result);
})
    .WithSummary("Get customer installments")
    .WithDescription("Get the installment history for the customer, including amount, currency, start date, frequency, and status.");

app.MapPost("/customers/{customerId}/installment", (string customerId, InstallmentRequest request, ICustomerService service) =>
{
    var result = service.SaveInstallment(customerId, request);
    return Results.Ok(result);
})
    .WithSummary("Save installment")
    .WithDescription("Save the installment amount for the customer.");

app.Run();

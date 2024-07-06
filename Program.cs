using Basic_Report.Filters;
using Basic_Report.Services;

using Carter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt => opt.OperationFilter<SwaggerFileOperationFilter>());
builder.Services.AddAntiforgery();
builder.Services.AddCarter();

builder.Services.AddScoped<IFileProcessor, FileProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapCarter();

app.UseHttpsRedirection();

app.Run();


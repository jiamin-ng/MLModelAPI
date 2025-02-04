var builder = WebApplication.CreateBuilder(args);

// Read PORT from Environment Variables
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Make the app listen on the specified PORT
app.Urls.Add($"http://*:{port}");

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

using System.Text.Json;
using System.Reflection;
using Microsoft.OpenApi.Models;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Tic Tac Toe API",
		Version = "v1",
		Description = "Local REST API for game state, moves, undo, and the session scoreboard."
	});
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});
builder.Services.AddSingleton<InMemoryGameStore>();
builder.Services.AddSingleton<IComputerMoveSelector, ComputerMoveSelector>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddCors(options => options.AddPolicy("local", policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tic Tac Toe API v1"));
}
app.UseCors("local"); app.MapControllers(); app.Run();
public partial class Program { }

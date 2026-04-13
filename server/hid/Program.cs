using DevSim.Interfaces;
using DevSim.Services;
using System.Threading;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IKeyboardMouseInput,KeyboardMouseInputWin>();
builder.Services.AddSingleton<IGamepadInput,GamepadInput>();
builder.Services.AddSingleton<IClipboardService,ClipboardServiceWin>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

// app.Use(async (context,next) => {
//     var body = context.Request.Body;
//     var route = context.Request.Path;

//     byte[] data = new byte[100];
//     await body.ReadAsync(data,0,100);
//     var datastr = Encoding.Default.GetString(data);
//     Console.WriteLine($"incoming {route} : {datastr}");
//     try
//     {
//         await next(context);
//     }
//     catch (System.Exception err)
//     {
//         Console.WriteLine($"error : {err.Message}");
//         throw;
//     }
// });

app.UseAuthorization();
app.UseWebSockets();


app.MapControllers();

app.Run();

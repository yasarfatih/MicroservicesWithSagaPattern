using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Consumer;
using Stock.API.Model;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueueName, y =>
            {
                y.ConfigureConsumer<OrderCreatedEventConsumer>(context);
            });
    });

});


builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("StockDb"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Stock.Add(new Stock.API.Model.Stock() { Id = 1, ProductId = 1, Count = 100 });
    context.Stock.Add(new Stock.API.Model.Stock() { Id = 2, ProductId = 2, Count = 120 });
    context.SaveChanges();
}


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


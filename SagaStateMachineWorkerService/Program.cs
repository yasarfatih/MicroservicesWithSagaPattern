using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, context) =>
        {
            context.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"), m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            });
        });
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

            cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
            {
                e.ConfigureSaga<OrderStateInstance>(context);
            });

        });
    });
});
var host = builder.Build();
host.Run();

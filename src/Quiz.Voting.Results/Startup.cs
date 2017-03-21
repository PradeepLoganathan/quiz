﻿using System;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quiz.EventSourcing;

namespace Quiz.Voting.Results
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public EventStoreOptions EventStoreOptions { get; }

        public Startup(ILoggerFactory loggerFactory)
        {
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            Configuration = builder.Build();
            loggerFactory.AddConsole();
            EventStoreOptions = EventStoreOptions.Create(Configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventStoreSubscription(EventStoreOptions);
            services.AddWebSocketManager();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
        IServiceProvider serviceProvider,
        IEventStoreConnection eventBus,
        EventTypeResolver typeResolver,
        WebSocketHandler handler)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.MapWebSocketManager("/ws", handler);     
        
            eventBus.StartSubscription(EventStoreOptions, typeResolver, handler.SendMessageToAllAsync)
            .Wait();
        }
    }
}

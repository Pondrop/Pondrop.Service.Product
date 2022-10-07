using AutoMapper;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using System.Text;
using System.Xml.Linq;

namespace Pondrop.Service.Product.Api.Services;

public class ServiceBusListenerService : IServiceBusListenerService
{
    private readonly ILogger<ServiceBusListenerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ServiceBusConfiguration _config;

    private readonly ServiceBusClient _serviceBusClient;

    private ServiceBusProcessor _processor;

    public ServiceBusListenerService(
        IOptions<ServiceBusConfiguration> config,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ServiceBusListenerService> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _serviceProvider = serviceProvider;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        _config = config.Value;

        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
    }


    public async Task StartListener()
    {
        ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        _processor = _serviceBusClient.CreateProcessor(_config.QueueName, _serviceBusProcessorOptions);
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;


        await _processor.StartProcessingAsync().ConfigureAwait(false);
    }

    public async Task StopListener()
    {
        await _processor.CloseAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync().ConfigureAwait(false);
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        try
        {
            if (args.Message.Subject.Contains("Command"))
            {
                var commandType = typeof(UpdateCheckpointByIdCommand);
                var commandTypeName = $"{commandType.FullName!.Replace(nameof(UpdateCheckpointByIdCommand), args.Message.Subject)}, {commandType.Assembly.GetName()}";

                commandType = Type.GetType(commandTypeName);
                var payload = Encoding.UTF8.GetString(args.Message.Body);

                if (commandType is not null && !string.IsNullOrEmpty(payload))
                {
                    var command = JsonConvert.DeserializeObject<JObject>(payload)?.ToObject(commandType);
                    if (command is not null)
                    {
                        try
                        {
                            using var scoped = _serviceProvider.CreateScope();
                            var mediator = scoped.ServiceProvider.GetService<IMediator>();
                            await mediator!.Send(command);

                            switch (command)
                            {
                                case UpdateCategoryCheckpointByIdCommand category:
                                    await mediator!.Send(new UpdateCategoryWithProductsViewCommand() {  CategoryId = category.Id });
                                    await mediator!.Send(new UpdateProductWithCategoriesViewCommand() {  CategoryId = category.Id });
                                    await mediator!.Send(new UpdateCategoryGroupingViewCommand() { CategoryId = category.Id });

                                    break;
                                case UpdateProductCheckpointByIdCommand product:
                                    await mediator!.Send(new UpdateCategoryWithProductsViewCommand() { ProductId = product.Id });
                                    await mediator!.Send(new UpdateProductWithCategoriesViewCommand() { ProductId = product.Id });
                                    break;
                                case UpdateProductCategoryCheckpointByIdCommand productCategory:
                                    await mediator!.Send(new UpdateCategoryWithProductsViewCommand() { ProductCategoryId = productCategory.Id });
                                    await mediator!.Send(new UpdateProductWithCategoriesViewCommand() { ProductCategoryId = productCategory.Id });
                                    break;
                                case UpdateCategoryGroupingCheckpointByIdCommand categoryGrouping:
                                    await mediator!.Send(new UpdateCategoryGroupingViewCommand() { CategoryGroupingId = categoryGrouping.Id });
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to run process event '{args.Message.Subject}'");
                        }
                    }
                }
            }
        }
        finally
        {
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Message handler encountered an exception");
        _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
        _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
        _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

        return Task.CompletedTask;
    }
}
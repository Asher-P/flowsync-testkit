# FlowSync Framework – AI Recreation Guide

This document is a complete, step-by-step blueprint for recreating the **FlowSync** integration-testing framework from scratch. Every detail needed to replicate the exact project structure, code, configuration, and test patterns is captured here. The consumer start and teardown sections are treated as critical – read them carefully.

---

## Table of Contents

1. [What This Project Is](#1-what-this-project-is)
2. [Solution Structure](#2-solution-structure)
3. [Dependency Graph](#3-dependency-graph)
4. [Project Files (.csproj)](#4-project-files-csproj)
5. [FlowSync.Core – Abstractions](#5-FlowSynccore--abstractions)
6. [FlowSync.Orchestration – Orchestration Layer](#6-FlowSyncorchestration--orchestration-layer)
7. [FlowSync.Kafka – Kafka Implementation](#7-FlowSynckafka--kafka-implementation)
8. [CRITICAL: Start Consumer Pattern](#8-critical-start-consumer-pattern)
9. [CRITICAL: Teardown Consumer Pattern](#9-critical-teardown-consumer-pattern)
10. [Test Projects](#10-test-projects)
11. [Configuration – appsettings.json](#11-configuration--appsettingsjson)
12. [CI/CD Pipeline](#12-cicd-pipeline)
13. [NuGet Configuration](#13-nuget-configuration)
14. [End-to-End Flow Summary](#14-end-to-end-flow-summary)

---

## 1. What This Project Is

The FlowSync framework is a **.NET 8 integration-testing library** that lets test code:

1. **Produce** a message to a Kafka topic.
2. **Start a consumer** on a response topic.
3. **Wait** for a matching response message (filtered by Correlation ID or by Message Key).
4. **Tear down** the consumer and delete the consumer group so the next test run starts clean.

The framework is published as a set of NuGet packages (`FlowSync.Core`, `FlowSync.Orchestration`, `FlowSync.Kafka`) consumed by integration test projects (`FlowSync.Tests` with xUnit, `FlowSync.NUnit` with NUnit).

---

## 2. Solution Structure

```
flowsync/
├── FlowSync.sln
├── cicd-main.yml
├── variables.yml
├── versionconfig.yml
├── nuget/
│   └── nuget.config
├── docs/
│   └── (markdown docs)
├── FlowSync.Core/               # abstractions only, no Kafka dependency
├── FlowSync.Kafka/              # KafkaFlow wiring, consumer, producer
├── FlowSync.Orchestration/      # factory, FlowSync steps, configuration
├── FlowSync.Tests/              # xUnit integration tests (not packaged)
└── FlowSync.NUnit/              # NUnit integration tests (not packaged)
```

### Solution file (`FlowSync.sln`)

```
Microsoft Visual Studio Solution File, Format Version 12.00
Project "FlowSync.Orchestration"  -> FlowSync.Orchestration\FlowSync.Orchestration.csproj
Project "FlowSync.Core"           -> FlowSync.Core\FlowSync.Core.csproj
Project "FlowSync.Tests"          -> FlowSync.Tests\FlowSync.Tests.csproj
Project "FlowSync.Kafka"          -> FlowSync.Kafka\FlowSync.Kafka.csproj
Project "FlowSync.NUnit"          -> FlowSync.NUnit\FlowSync.NUnit.csproj
```

---

## 3. Dependency Graph

```
FlowSync.Core
    ↑
FlowSync.Orchestration  (ProjectReference → FlowSync.Core)
    ↑
FlowSync.Kafka          (ProjectReference → FlowSync.Orchestration)
    ↑
FlowSync.Tests          (ProjectReference → FlowSync.Kafka + FlowSync.Orchestration)
FlowSync.NUnit          (ProjectReference → FlowSync.Kafka + FlowSync.Orchestration)
```

Rule: nothing in `FlowSync.Core` or `FlowSync.Orchestration` knows about Kafka. The Kafka package is the only one that references `KafkaFlow`.

---

## 4. Project Files (.csproj)

### FlowSync.Core.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.5" />
  </ItemGroup>
</Project>
```

### FlowSync.Orchestration.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PackageId>FlowSync.Orchestration</PackageId>
    <AssemblyName>FlowSync.Orchestration</AssemblyName>
    <RootNamespace>FlowSync.Orchestration</RootNamespace>
    <Authors>FlowSync Framework Team</Authors>
    <Description>FlowSync orchestration and management framework for integration testing</Description>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\FlowSync.Core\FlowSync.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
  </ItemGroup>
</Project>
```

### FlowSync.Kafka.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="KafkaFlow" Version="4.0.0" />
    <PackageReference Include="KafkaFlow.Extensions.Hosting" Version="4.0.0" />
    <PackageReference Include="KafkaFlow.Serializer.Json" Version="1.5.8" />
    <PackageReference Include="protobuf-net" Version="3.2.30" />
    <PackageReference Include="protobuf-net.Core" Version="3.2.30" />
    <PackageReference Include="Utf8Json" Version="1.3.7" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FlowSync.Orchestration\FlowSync.Orchestration.csproj" />
  </ItemGroup>
</Project>
```

### FlowSync.Tests.csproj (xUnit)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="KafkaFlow.Extensions.Hosting" Version="4.0.0" />
    <PackageReference Include="KafkaFlow.Microsoft.DependencyInjection" Version="4.0.0" />
    <PackageReference Include="MartinCostello.Logging.XUnit" Version="0.4.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.7" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.5" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.5" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.6.0" />
    <PackageReference Include="protobuf-net.Core" Version="3.2.30" />
    <PackageReference Include="Steeltoe.Extensions.Configuration.PlaceholderCore" Version="3.2.8" />
    <PackageReference Include="System.Text.Encoding.Extensions" Version="4.3.0" />
    <PackageReference Include="Utf8Json" Version="1.3.7" />
    <PackageReference Include="xunit" Version="2.4.2" />
    <PackageReference Include="xunit.abstractions" Version="2.0.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FlowSync.Kafka\FlowSync.Kafka.csproj" />
    <ProjectReference Include="..\FlowSync.Orchestration\FlowSync.Orchestration.csproj" />
  </ItemGroup>
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

### FlowSync.NUnit.csproj

Same as `FlowSync.Tests.csproj` but replace:
- `xunit`, `xunit.abstractions`, `xunit.runner.visualstudio` → `NUnit 3.13.3`, `NUnit3TestAdapter 4.2.1`, `NUnit.Analyzers 3.6.1`
- Add `Automation.DataContext Version="3.1.0.152"`
- Change `appsettings.json` copy rule to `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`

---

## 5. FlowSync.Core – Abstractions

All files are in `FlowSync.Core/`.

### Messaging/Consuming/IConsumer.cs

```csharp
namespace FlowSync.Core.Messaging.Consuming;

public interface IConsumer
{
    Task StartConsumeAsync(string consumerName);
}
```

### Messaging/Publishing/IProducer.cs

```csharp
using FlowSync.Core.Messaging.Publishing.Entities;

namespace FlowSync.Core.Messaging.Publishing;

public interface IProducer
{
    Task ProduceAsync(string destination, string key, object message, ProducingExtraData extraData);
}
```

### Messaging/Publishing/Entities/ProducingExtraData.cs

```csharp
namespace FlowSync.Core.Messaging.Publishing.Entities;

public class ProducingExtraData
{
    public Dictionary<string, string> Headers { get; set; } = new();
}
```

### Messaging/FilterService/IFilterService.cs

```csharp
namespace FlowSync.Core.Messaging.FilterService;

public interface IFilterService
{
    void AddFilter(string FlowSyncId);
    string Filter(string obj);
}
```

### Messaging/FilterService/FilterService.cs

```csharp
using Microsoft.Extensions.Logging;

namespace FlowSync.Core.Messaging.FilterService;

public class FilterService : IFilterService
{
    private readonly ILogger<FilterService> _logger;
    private HashSet<string> _whiteListKeys;
    public HashSet<string> MessageList = new HashSet<string>();
    public int ReadCounter = 0;
    private Dictionary<string, Predicate<string>> filterDict = new Dictionary<string, Predicate<string>>();

    public FilterService(ILogger<FilterService> logger)
    {
        _logger = logger;
        _whiteListKeys = new HashSet<string>();
    }

    public void ClearFilters()
    {
        ReadCounter = 0;
        MessageList = new HashSet<string>();
        _whiteListKeys = new();
    }

    private string InvokeFilter(string obj)
    {
        foreach (var (key, predicate) in filterDict)
        {
            if (predicate != null && predicate.GetInvocationList().Any())
            {
                if (predicate.Invoke(obj))
                    return key;
            }
            else
            {
                throw new Exception("Filter Is not exist");
            }
        }
        return null;
    }

    public void AddFilter(string FlowSyncId)
    {
        filterDict[FlowSyncId] = x => x == FlowSyncId;
    }

    public string Filter(string obj) => InvokeFilter(obj);
}
```

### Messaging/Receivers/IMessagePool.cs

```csharp
using FlowSync.Core.Messaging.Models;

namespace FlowSync.Core.Messaging.Receivers;

public interface IMessagePool
{
    List<ResponseContainer> GetMessages(IEnumerable<string> FlowSyncIds);
    void ClearFlowSyncMessages(string FlowSyncId);
    void AddMessage(string FlowSyncId, KeyValuePair<object, object> keyValuePair);
}
```

### Messaging/Receivers/MessagePool.cs

```csharp
using System.Collections.Concurrent;
using FlowSync.Core.Messaging.Models;

namespace FlowSync.Core.Messaging.Receivers;

public class MessagePool : IMessagePool
{
    private ConcurrentDictionary<string, List<KeyValuePair<object, object>>> cachedMessages = new();

    public List<ResponseContainer> GetMessages(IEnumerable<string> FlowSyncIds)
    {
        var allMessages = new List<ResponseContainer>();
        foreach (var FlowSyncId in FlowSyncIds)
        {
            cachedMessages.TryGetValue(FlowSyncId, out var messages);
            if (messages != null && messages.Count > 0)
            {
                allMessages.Add(new ResponseContainer()
                {
                    FlowSyncId = FlowSyncId,
                    Messages = new List<MessageContainer>(messages.ToMessageContainerList())
                });
            }
        }
        return allMessages;
    }

    public void ClearFlowSyncMessages(string FlowSyncId)
    {
        if (cachedMessages.TryGetValue(FlowSyncId, out _))
            cachedMessages[FlowSyncId] = new List<KeyValuePair<object, object>>();
    }

    public void AddMessage(string FlowSyncId, KeyValuePair<object, object> keyValuePair)
    {
        var value = cachedMessages.GetOrAdd(FlowSyncId, s => new List<KeyValuePair<object, object>>());
        value.Add(keyValuePair);
    }
}
```

### Messaging/Models/ResponseContainer.cs

```csharp
namespace FlowSync.Core.Messaging.Models;

public class ResponseContainer
{
    public string FlowSyncId { get; set; }
    public List<MessageContainer> Messages { get; set; }
}

public class MessageContainer
{
    public object Key { get; set; }
    public object Value { get; set; }
}

public static class KeyValuePairExtensions
{
    public static List<MessageContainer> ToMessageContainerList(
        this List<KeyValuePair<object, object>> keyValuePairs)
    {
        return keyValuePairs
            .Select(x => new MessageContainer { Key = x.Key, Value = x.Value })
            .ToList();
    }
}
```

---

## 6. FlowSync.Orchestration – Orchestration Layer

### Configurations/FlowSyncConfiguration.cs

```csharp
namespace FlowSync.Orchestration.Configurations;

public class FlowSyncConfiguration
{
    public IEnumerable<string> ConsumeFrom { get; set; }
    public string ProduceTo { get; set; }
    public ConsumingOptionsConfiguration ConsumingOptions { get; set; }
}
```

### Configurations/ConsumingOptionsConfiguration.cs

```csharp
using FlowSync.Core.Messaging.FilterService;

namespace FlowSync.Orchestration.Configurations;

public class ConsumingOptionsConfiguration
{
    /// <summary>Timeout – default 30 seconds.</summary>
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(30);
    public string ExpectedCorrelationId { get; set; }
    public string ExpectedMessageKey { get; set; }
    public int MsgReceivedCount { get; set; }
}
```

### Entities/Enums/FlowSyncType.cs

```csharp
namespace FlowSync.Orchestration.Entities.Enums;

public enum FlowSyncType
{
    ProduceAndForget,
    ProduceAndWait,
    ConsumeOnly,
}
```

### Entities/Interfaces/IFlowSyncStep.cs

```csharp
using FlowSync.Core.Messaging.Models;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Orchestration.Entities.Enums;

namespace FlowSync.Orchestration.Entities.Interfaces;

public interface IFlowSyncStep : IExecutableStep
{
    FlowSyncType FlowSyncType { get; }
    Task InitializeAsync();
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message);
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message, ProducingExtraData producingExtraData);
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, IEnumerable<T> messages, ProducingExtraData producingExtraData);
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync();
}
```

### Entities/BaseFlowSyncStep.cs

`BaseFlowSyncStep` is abstract. It determines `FlowSyncType` from the configuration, provides all `ExecuteAsync` overloads, and runs the polling loop in `GetTaskResultAsync`.

```csharp
using System.Diagnostics;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Models;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities.Enums;
using FlowSync.Orchestration.Entities.Interfaces;
using FlowSync.Core.Extensions;

namespace FlowSync.Orchestration.Entities;

public abstract class BaseFlowSyncStep : IFlowSyncStep
{
    protected readonly IConsumer _consumer;
    protected readonly IMessagePool _messagePool;
    protected readonly IProducer _producer;
    protected readonly FlowSyncConfiguration _configuration;
    protected readonly IFilterService _filterService;

    public FlowSyncType FlowSyncType
    {
        get
        {
            if (_configuration.ConsumeFrom.IsNullOrEmpty())
                return FlowSyncType.ProduceAndForget;
            else if (!_configuration.ProduceTo.IsNullOrEmpty())
                return FlowSyncType.ProduceAndWait;
            else return FlowSyncType.ConsumeOnly;
        }
    }

    protected BaseFlowSyncStep(
        IConsumer consumer, IProducer producer,
        IFilterService filterService, IMessagePool messagePool,
        FlowSyncConfiguration configuration)
    {
        _producer = producer;
        _configuration = configuration;
        _consumer = consumer;
        _messagePool = messagePool;
        _filterService = filterService;
    }

    public abstract Task InitializeAsync();
    protected abstract string GetFlowSyncIdentifier(string topic);
    protected abstract string GetClearIdentifier();
    protected abstract void AddProducingHeaders(ProducingExtraData producingExtraData);

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message)
        => await ExecuteAsync(key, (IEnumerable<T>)new[] { message }, new ProducingExtraData());

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync()
        => await ExecuteConsumeAsync();

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(
        string key, T message, ProducingExtraData producingExtraData)
        => await ExecuteAsync(key, (IEnumerable<T>)new[] { message }, producingExtraData);

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(
        string key, IEnumerable<T> messages, ProducingExtraData producingExtraData)
    {
        AddProducingHeaders(producingExtraData);

        foreach (var message in messages)
            await _producer.ProduceAsync(_configuration.ProduceTo, key, message, producingExtraData);

        var tcs = new TaskCompletionSource<IEnumerable<ResponseContainer>>();

        switch (FlowSyncType)
        {
            case FlowSyncType.ProduceAndForget:
                tcs.SetResult(new List<ResponseContainer>());
                break;
            case FlowSyncType.ProduceAndWait:
                GetTaskResultAsync(tcs);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return tcs;
    }

    private async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteConsumeAsync()
    {
        var tcs = new TaskCompletionSource<IEnumerable<ResponseContainer>>();
        GetTaskResultAsync(tcs);
        return tcs;
    }

    private async Task GetTaskResultAsync(TaskCompletionSource<IEnumerable<ResponseContainer>> tcs)
    {
        var sw = new Stopwatch();
        sw.Start();

        IEnumerable<string> consumeFlowSyncIds = _configuration.ConsumeFrom.Select(GetFlowSyncIdentifier);

        while (sw.Elapsed <= _configuration.ConsumingOptions.TimeOut)
        {
            var responseContainers = _messagePool.GetMessages(consumeFlowSyncIds);
            if (_configuration.ConsumingOptions.MsgReceivedCount > 0
                && !responseContainers.IsNullOrEmpty()
                && responseContainers.Sum(x => x.Messages.Count) >= _configuration.ConsumingOptions.MsgReceivedCount)
            {
                tcs.SetResult(responseContainers);
                _messagePool.ClearFlowSyncMessages(GetClearIdentifier());
                return;
            }
            await Task.Delay(250);
        }

        sw.Stop();
        tcs.SetException(new TimeoutException("Did not receive enough messages within the timeout scope"));
    }
}
```

### Entities/CorrelationIdFlowSyncStep.cs

Used when `ExpectedMessageKey` is **not** set. A new GUID is generated per FlowSync instance and injected as both `FlowSyncId` and `correlation_id` headers on the produced message.

```csharp
using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;

namespace FlowSync.Orchestration.Entities;

public class CorrelationIdFlowSyncStep : BaseFlowSyncStep
{
    private readonly string _FlowSyncId;

    public CorrelationIdFlowSyncStep(
        IConsumer consumer, IProducer producer,
        IFilterService filterService, IMessagePool messagePool,
        FlowSyncConfiguration configuration)
        : base(consumer, producer, filterService, messagePool, configuration)
    {
        _FlowSyncId = Guid.NewGuid().ToString();
    }

    public override async Task InitializeAsync()
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var consumeFlowSyncId = _FlowSyncId.AddPrefix($"{consumeFrom}_");
                _filterService.AddFilter(consumeFlowSyncId);
            }
            foreach (var consumeFrom in _configuration.ConsumeFrom)
                await _consumer.StartConsumeAsync(consumeFrom);
        }
        else if (FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var expectedCorrelationId = _configuration.ConsumingOptions.ExpectedCorrelationId;
                var consumeFlowSyncId = expectedCorrelationId.AddPrefix($"{consumeFrom}_");
                _filterService.AddFilter(consumeFlowSyncId);
                await _consumer.StartConsumeAsync(consumeFrom);
            }
        }
    }

    protected override string GetFlowSyncIdentifier(string topic)
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait)
            return _FlowSyncId.AddPrefix($"{topic}_");
        if (FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
            return _configuration.ConsumingOptions.ExpectedCorrelationId.AddPrefix($"{topic}_");
        throw new InvalidOperationException($"Unsupported FlowSync type: {FlowSyncType}");
    }

    protected override string GetClearIdentifier()
        => FlowSyncType == Enums.FlowSyncType.ProduceAndWait
            ? _FlowSyncId
            : _configuration.ConsumingOptions.ExpectedCorrelationId;

    protected override void AddProducingHeaders(ProducingExtraData producingExtraData)
    {
        producingExtraData.Headers.Add("FlowSyncId", _FlowSyncId);
        producingExtraData.Headers.Add("correlation_id", _FlowSyncId);
    }
}
```

### Entities/MessageKeyFlowSyncStep.cs

Used when `ConsumingOptions.ExpectedMessageKey` **is** set. Filtering is based on the Kafka message key rather than a header.

```csharp
using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;

namespace FlowSync.Orchestration.Entities;

public class MessageKeyFlowSyncStep : BaseFlowSyncStep
{
    private readonly string _expectedMessageKey;

    public MessageKeyFlowSyncStep(
        IConsumer consumer, IProducer producer,
        IFilterService filterService, IMessagePool messagePool,
        FlowSyncConfiguration configuration)
        : base(consumer, producer, filterService, messagePool, configuration)
    {
        _expectedMessageKey = configuration.ConsumingOptions?.ExpectedMessageKey
            ?? throw new ArgumentException("ExpectedMessageKey is required for MessageKeyFlowSyncStep");
    }

    public override async Task InitializeAsync()
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait || FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var consumeFlowSyncId = _expectedMessageKey.AddPrefix($"{consumeFrom}_key_");
                _filterService.AddFilter(consumeFlowSyncId);
            }
            foreach (var consumeFrom in _configuration.ConsumeFrom)
                await _consumer.StartConsumeAsync(consumeFrom);
        }
    }

    protected override string GetFlowSyncIdentifier(string topic)
        => _expectedMessageKey.AddPrefix($"{topic}_key_");

    protected override string GetClearIdentifier() => _expectedMessageKey;

    protected override void AddProducingHeaders(ProducingExtraData producingExtraData)
    {
        producingExtraData.Headers.Add("FlowSyncId", _configuration.ConsumingOptions.ExpectedMessageKey);
        producingExtraData.Headers.Add("correlation_id", Guid.NewGuid().ToString());
    }
}
```

### Factories/IFlowSyncFactory.cs + FlowSyncFactory.cs

```csharp
// IFlowSyncFactory.cs
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities.Interfaces;

namespace FlowSync.Orchestration.Factories;

public interface IFlowSyncFactory
{
    Task<IFlowSyncStep> CreateFlowSyncStepAsync(FlowSyncConfiguration configuration);
}
```

```csharp
// FlowSyncFactory.cs
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities;
using FlowSync.Orchestration.Entities.Interfaces;

namespace FlowSync.Orchestration.Factories;

public class FlowSyncFactory(
    IConsumer consumer,
    IProducer producer,
    IFilterService filterService,
    IMessagePool messagePool) : IFlowSyncFactory
{
    public async Task<IFlowSyncStep> CreateFlowSyncStepAsync(FlowSyncConfiguration configuration)
    {
        IFlowSyncStep FlowSyncStep = DetermineFlowSyncStepType(configuration);
        await FlowSyncStep.InitializeAsync();
        return FlowSyncStep;
    }

    private IFlowSyncStep DetermineFlowSyncStepType(FlowSyncConfiguration configuration)
    {
        var hasMessageKey = !string.IsNullOrEmpty(configuration.ConsumingOptions?.ExpectedMessageKey);
        var hasCorrelationId = !string.IsNullOrEmpty(configuration.ConsumingOptions?.ExpectedCorrelationId);

        if (hasMessageKey && hasCorrelationId)
            throw new ArgumentException("Cannot specify both ExpectedMessageKey and ExpectedCorrelationId.");

        if (hasMessageKey)
            return new MessageKeyFlowSyncStep(consumer, producer, filterService, messagePool, configuration);

        // Default: correlation ID mode (backward compatible)
        return new CorrelationIdFlowSyncStep(consumer, producer, filterService, messagePool, configuration);
    }
}
```

### Host/HostExtensions.cs

```csharp
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FlowSync.Orchestration.Host;

public static class HostExtensions
{
    public static IServiceCollection AddFlowSyncService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IFlowSyncFactory, FlowSyncFactory>();
        serviceCollection.AddSingleton<IFilterService, FilterService>();
        serviceCollection.AddSingleton<IMessagePool, MessagePool>();
        return serviceCollection;
    }
}
```

---

## 7. FlowSync.Kafka – Kafka Implementation

### Configurations

#### KafkaBrokerConfiguration.cs

```csharp
namespace FlowSync.Kafka.Configurations;

public class KafkaBrokerConfiguration
{
    public IEnumerable<string> BootstrapServers { get; set; } = new List<string>();
    public KafkaCredentialsConfiguration Credentials { get; set; } = new KafkaCredentialsConfiguration();
}
```

#### KafkaCredentialsConfiguration.cs

```csharp
namespace FlowSync.Kafka.Configurations;

public class KafkaCredentialsConfiguration
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Mechanism { get; set; }
    public bool TlsEnabled { get; set; }
    public string? SecurityProtocol { get; set; }

    public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public static KafkaCredentialsConfiguration FromEnvironment(
        string usernameVar, string passwordVar, string mechanismVar,
        string tlsEnabledVar, string securityProtocolVar)
    {
        return new KafkaCredentialsConfiguration
        {
            Username = Environment.GetEnvironmentVariable(usernameVar),
            Password = Environment.GetEnvironmentVariable(passwordVar),
            Mechanism = Environment.GetEnvironmentVariable(mechanismVar),
            TlsEnabled = bool.TryParse(Environment.GetEnvironmentVariable(tlsEnabledVar), out var tls) && tls,
            SecurityProtocol = Environment.GetEnvironmentVariable(securityProtocolVar)
        };
    }
}
```

#### KafkaConsumerConfiguration.cs

```csharp
using KafkaFlow;

namespace FlowSync.Kafka.Configurations;

public class KafkaConsumerConfiguration
{
    public string ConsumerGroup { get; set; }
    public string Topic { get; set; }
    public IDeserializer ConsumerDeserialization { get; set; }
    public Type ConsumingType { get; set; }
    public int? WorkersCount { get; set; } = null;
    public int? BufferSize { get; set; } = null;
}
```

#### KafkaProducerConfiguration.cs

```csharp
using KafkaFlow;

namespace FlowSync.Kafka.Configurations;

public class KafkaProducerConfiguration
{
    public string ProducerTopic { get; set; }
    public ISerializer ProducerSerializer { get; set; }
}
```

### Middlewares/FilterMiddleware.cs

`FilterMiddleware` runs on every consumed message. It first tries to match by Correlation ID header, then falls back to matching by message key. Only messages that match a registered filter pass on to `PushMessageMiddleware`.

```csharp
using System.Text;
using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using KafkaFlow;
using Microsoft.Extensions.Logging;

namespace FlowSync.Kafka.Middlewares;

public class FilterMiddleware : IMessageMiddleware
{
    private static readonly string[] CorrelationIdHeaderOptions = { "correlation_id", "CorrelationId" };

    private readonly IFilterService _filterService;
    private readonly ILogger<FilterMiddleware> _logger;

    public FilterMiddleware(IFilterService filterService, ILogger<FilterMiddleware> logger)
    {
        _filterService = filterService;
        _logger = logger;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        // Try correlation ID first (header-based)
        var correlationId = GetCorrelationId(context);
        if (correlationId != null)
        {
            var FlowSyncIdentifier = correlationId.AddPrefix($"{context.ConsumerContext.Topic}_");
            var FlowSyncId = _filterService.Filter(FlowSyncIdentifier);
            if (FlowSyncId != null)
            {
                context.Items["FlowSyncId"] = FlowSyncId;
                await next(context).ConfigureAwait(false);
                return;
            }
        }

        // Fall back to message key
        var messageKey = Encoding.UTF8.GetString(context.Message.Key as byte[]);
        if (messageKey != null)
        {
            var FlowSyncIdentifier = messageKey.AddPrefix($"{context.ConsumerContext.Topic}_key_");
            var FlowSyncId = _filterService.Filter(FlowSyncIdentifier);
            if (FlowSyncId != null)
            {
                context.Items["FlowSyncId"] = FlowSyncId;
                await next(context).ConfigureAwait(false);
                return;
            }
        }
        // Message is silently dropped – no matching FlowSync
    }

    private string? GetCorrelationId(IMessageContext context)
    {
        foreach (var option in CorrelationIdHeaderOptions)
        {
            var correlationId = context.Headers.GetString(option);
            if (correlationId != null) return correlationId;
        }
        return null;
    }
}
```

### Middlewares/PushMessageMiddleware.cs

```csharp
using FlowSync.Core.Messaging.Receivers;
using KafkaFlow;

namespace FlowSync.Kafka.Middlewares;

public class PushMessageMiddleware : IMessageMiddleware
{
    private readonly IMessagePool _messagePool;

    public PushMessageMiddleware(IMessagePool messagePool)
    {
        _messagePool = messagePool;
    }

    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        _messagePool.AddMessage(
            context.Items["FlowSyncId"].ToString(),
            new KeyValuePair<object, object>(context.Message.Key, context.Message.Value));
        return next(context);
    }
}
```

### Middlewares/ErrorHandlingMiddleware.cs

```csharp
using KafkaFlow;
using Microsoft.Extensions.Logging;

namespace FlowSync.Kafka.Middlewares;

public class ErrorHandlingMiddleware : IMessageMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message on topic {Topic}", context.ConsumerContext.Topic);
        }
    }
}
```

### Builders/KafkaConfigurationBuilder.cs

The builder constructs the full KafkaFlow topology. Important points:
- Consumer name (`WithName`) is set to the **topic name** – this is how `StartConsumeAsync(consumerName)` can look it up later.
- `AutoOffsetReset.Latest` – consumer only sees new messages.
- Default workers count: 50, buffer size: 30.
- Middleware pipeline: `ErrorHandlingMiddleware` → `FilterMiddleware` (Singleton) → deserializer → `PushMessageMiddleware`.
- `IKafkaBus` is registered as a singleton via `resolve.CreateKafkaBus()` – **do not call `StartAsync` on it here**.

```csharp
using FlowSync.Kafka.Configurations;
using FlowSync.Kafka.Middlewares;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using AutoOffsetReset = KafkaFlow.AutoOffsetReset;
using SaslMechanism = KafkaFlow.Configuration.SaslMechanism;
using SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol;

namespace FlowSync.Kafka.Builders;

public class KafkaConfigurationBuilder : IKafkaConfigurationBuilder
{
    public KafkaBrokerConfiguration BrokerConfiguration { get; private set; } = new();
    public IEnumerable<KafkaConsumerConfiguration> KafkaConsumers { get; set; } = new List<KafkaConsumerConfiguration>();
    public IEnumerable<KafkaProducerConfiguration> KafkaProducers { get; set; } = new List<KafkaProducerConfiguration>();

    public IKafkaConfigurationBuilder ConfigureBroker(Action<IKafkaBrokerConfigurationBuilder> brokerConfigurationBuilder)
    {
        IKafkaBrokerConfigurationBuilder builder = new KafkaBrokerConfigurationBuilder();
        brokerConfigurationBuilder.Invoke(builder);
        BrokerConfiguration = builder.Build();
        return this;
    }

    public IKafkaConfigurationBuilder AddConsumer(Action<IKafkaConsumerConfigurationBuilder> consumerBuilder)
    {
        IKafkaConsumerConfigurationBuilder builder = new KafkaConsumerConfigurationBuilder();
        consumerBuilder.Invoke(builder);
        KafkaConsumers = KafkaConsumers.Append(builder.Build());
        return this;
    }

    public IKafkaConfigurationBuilder AddProducer(Action<IKafkaProducerConfigurationBuilder> producerBuilder)
    {
        IKafkaProducerConfigurationBuilder builder = new KafkaProducerConfigurationBuilder();
        producerBuilder.Invoke(builder);
        KafkaProducers = KafkaProducers.Append(builder.Build());
        return this;
    }

    public IServiceCollection BuildKafkaFlow(IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddKafkaFlowHostedService(builder =>
            {
                builder.AddCluster(cluster =>
                {
                    cluster.WithBrokers(BrokerConfiguration.BootstrapServers);

                    if (BrokerConfiguration.Credentials.HasCredentials)
                    {
                        cluster.WithSecurityInformation(info =>
                        {
                            info.SecurityProtocol = ParseSecurityProtocol(BrokerConfiguration.Credentials.SecurityProtocol);
                            info.SaslUsername = BrokerConfiguration.Credentials.Username;
                            info.SaslPassword = BrokerConfiguration.Credentials.Password;
                            info.SaslMechanism = ParseSaslMechanism(BrokerConfiguration.Credentials.Mechanism);
                        });
                    }
                    else
                    {
                        cluster.WithSecurityInformation(info =>
                            info.SecurityProtocol = SecurityProtocol.Plaintext);
                    }

                    foreach (var cfg in KafkaConsumers)
                    {
                        cluster.AddConsumer(consumer =>
                        {
                            consumer.WithName(cfg.Topic);                     // ← name == topic name
                            consumer.Topic(cfg.Topic);
                            consumer.WithAutoOffsetReset(AutoOffsetReset.Latest);
                            consumer.WithGroupId(cfg.ConsumerGroup);
                            consumer.WithWorkersCount(cfg.WorkersCount ?? 50);
                            consumer.WithBufferSize(cfg.BufferSize ?? 30);
                            consumer.AddMiddlewares(middlewares =>
                            {
                                middlewares
                                    .Add<ErrorHandlingMiddleware>()
                                    .Add<FilterMiddleware>(MiddlewareLifetime.Singleton)
                                    .AddSingleTypeDeserializer(
                                        _ => cfg.ConsumerDeserialization,
                                        cfg.ConsumingType)
                                    .Add<PushMessageMiddleware>();
                            });
                        });
                    }

                    foreach (var cfg in KafkaProducers)
                    {
                        cluster.AddProducer(cfg.ProducerTopic, producer =>
                        {
                            producer.DefaultTopic(cfg.ProducerTopic)
                                .AddMiddlewares(m => m.AddSerializer(_ => cfg.ProducerSerializer));
                        });
                    }
                });
            })
            .AddSingleton<IKafkaBus>(resolve => resolve.CreateKafkaBus());
    }

    private SecurityProtocol ParseSecurityProtocol(string? v) => v?.ToLower() switch
    {
        "saslssl" or "sasl_ssl" => SecurityProtocol.SaslSsl,
        "ssl" => SecurityProtocol.Ssl,
        "saslplaintext" or "sasl_plaintext" => SecurityProtocol.SaslPlaintext,
        _ => SecurityProtocol.Plaintext
    };

    private SaslMechanism ParseSaslMechanism(string? v) => v?.ToLower() switch
    {
        "plain" => SaslMechanism.Plain,
        "scram-sha-256" or "scram_sha_256" => SaslMechanism.ScramSha256,
        "scram-sha-512" or "scram_sha_512" => SaslMechanism.ScramSha512,
        "gssapi" => SaslMechanism.Gssapi,
        _ => SaslMechanism.Plain
    };
}
```

### Extensions/HostExtensions.cs

The single entry point for test DI setup. `AddKafkaFlowSync` is the method tests call.

```csharp
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Kafka.Builders;
using FlowSync.Kafka.Consumers;
using FlowSync.Kafka.Producers;
using FlowSync.Orchestration.Host;
using Microsoft.Extensions.DependencyInjection;

namespace FlowSync.Kafka.Extensions;

public static class HostExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddSingleton<IConsumer, KafkaConsumer>();
        return services;
    }

    public static IServiceCollection AddKafkaFlowSync(
        this IServiceCollection serviceCollection,
        Action<IKafkaConfigurationBuilder> kafkaConfigurationBuilder)
    {
        serviceCollection
            .AddFlowSyncService()
            .AddSingleton<IConsumer, KafkaConsumer>()
            .AddSingleton<IProducer, KafkaProducer>();

        var builder = new KafkaConfigurationBuilder();
        kafkaConfigurationBuilder.Invoke(builder);
        builder.BuildKafkaFlow(serviceCollection);

        return serviceCollection;
    }
}
```

---

## 8. CRITICAL: Start Consumer Pattern

> This section describes the exact start-up sequence every time a FlowSync step is initialized. **Do not simplify or skip any step.**

### Location

`FlowSync.Kafka/Consumers/KafkaConsumer.cs` implements `IConsumer.StartConsumeAsync`.

### Full Implementation

```csharp
using KafkaFlow;
using KafkaFlow.Consumers;
using IConsumer = FlowSync.Core.Messaging.Consuming.IConsumer;

namespace FlowSync.Kafka.Consumers;

public class KafkaConsumer : IConsumer
{
    private readonly IKafkaBus _kafkaBus;

    public KafkaConsumer(IKafkaBus kafkaBus)
    {
        _kafkaBus = kafkaBus;
    }

    public async Task StartConsumeAsync(string consumerName)
    {
        // Step 1 – cold-start workaround:
        // KafkaFlow does not populate Consumers.All until the bus has been started at least once.
        // If no consumers exist yet, do a quick start+stop to initialise the internal collection.
        if (!_kafkaBus.Consumers.All.Any())
        {
            await _kafkaBus.StartAsync();
            await _kafkaBus.StopAsync();
        }

        // Step 2 – look up the consumer by name.
        // Consumer name == topic name (set via WithName(topic) in KafkaConfigurationBuilder).
        var relevantConsumer = _kafkaBus.Consumers.GetConsumer(consumerName);
        if (relevantConsumer != null)
            await relevantConsumer.StartAsync();
        else
            throw new NullReferenceException($"The Consumer with name {consumerName} not exist");

        // Step 3 – poll until the consumer reaches Running status.
        // This prevents the test from producing before the consumer is ready.
        while (!_kafkaBus.Consumers.All
                   .Where(x => x.ConsumerName == consumerName)
                   .Any(y => y.Status == ConsumerStatus.Running))
        {
            await Task.Delay(TimeSpan.FromSeconds(0.2));
        }

        // Step 4 – extra stabilisation delay.
        // Even after status == Running there is a brief window before offsets are assigned.
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}
```

### Why Each Step Matters

| Step | Reason |
|---|---|
| Start + Stop if empty | KafkaFlow does not initialise its consumer registry until the hosted service starts. Without this the subsequent `GetConsumer` call returns null. |
| `GetConsumer(consumerName)` | Looks up by the name given to `WithName()` in the builder, which is always the topic name. |
| Poll for `ConsumerStatus.Running` | Without this guard the test can produce a message before the consumer joins the group and misses it entirely. |
| `Task.Delay(5s)` | Waits for partition assignment to complete after the status is `Running`. Skipping this can still cause messages to be missed at test start. |

### Flow in Context

`FlowSyncFactory.CreateFlowSyncStepAsync` calls `FlowSyncStep.InitializeAsync()` which calls `_consumer.StartConsumeAsync(topic)` per consumed topic. The call is awaited, so the test does not execute until the consumer is fully ready.

---

## 9. CRITICAL: Teardown Consumer Pattern

> This is the most complex part of the framework. It must be placed in a `finally` block so it always runs, even when a test assertion fails. **Skipping teardown leaves a zombie consumer group that prevents the next test run from working.**

### Location

`FlowSync.Tests/GeneralTests.cs` (xUnit) and `FlowSync.NUnit/MessageKeyFlowSyncTest.cs` (NUnit) both implement `DeleteConsumerGroupAsync`. The implementations are identical in logic.

### Full Implementation

```csharp
public async Task DeleteConsumerGroupAsync()
{
    var kafkaBus = _provider.GetRequiredService<IKafkaBus>();

    // Step 1 – stop every individual consumer managed by this bus.
    // This causes each consumer to commit its final offsets and leave the group.
    foreach (var messageConsumer in kafkaBus.Consumers.All)
    {
        await messageConsumer.StopAsync();
    }

    // Step 2 – wait for all consumers to fully stop and for Kafka to rebalance.
    // 10 seconds is required; shorter waits leave the group in a non-Empty state
    // and DeleteGroupsAsync will fail.
    Thread.Sleep(TimeSpan.FromSeconds(10));

    // Step 3 – build an AdminClient using the same broker credentials as the test.
    var kafkaBrokerConfig = Configuration
        .GetSection("KafkaBrokerConfiguration")
        .Get<KafkaBrokerConfiguration>();

    var adminClientConfig = new AdminClientConfig
    {
        BootstrapServers = string.Join(",", kafkaBrokerConfig.BootstrapServers),
    };

    // Apply security protocol if configured
    if (Enum.TryParse<SecurityProtocol>(
            kafkaBrokerConfig.Credentials.SecurityProtocol, true, out var securityProtocol))
    {
        adminClientConfig.SecurityProtocol = securityProtocol;
    }

    // Apply SASL credentials if present
    if (kafkaBrokerConfig.Credentials.HasCredentials)
    {
        adminClientConfig.SaslUsername = kafkaBrokerConfig.Credentials.Username;
        adminClientConfig.SaslPassword = kafkaBrokerConfig.Credentials.Password;

        if (Enum.TryParse<SaslMechanism>(
                kafkaBrokerConfig.Credentials.Mechanism, true, out var saslMechanism))
        {
            adminClientConfig.SaslMechanism = saslMechanism;
        }
    }

    var adminClient = new AdminClientBuilder(adminClientConfig).Build();

    // Step 4 – stop the KafkaBus (the hosted service level, not just individual consumers).
    await kafkaBus.StopAsync();

    // Step 5 – retry loop: keep trying to delete the group until it succeeds.
    // DeleteGroupsAsync throws DeleteGroupsException if the group is still active.
    // Calling kafkaBus.StopAsync() again inside the loop ensures the bus is truly stopped
    // before each attempt.
    while (true)
    {
        try
        {
            await kafkaBus.StopAsync();

            await adminClient.DeleteGroupsAsync(new List<string>
            {
                OutliersConsts.CONSUMER_GROUP  // the group name from your Consts class
            });
            break; // success – exit loop
        }
        catch (DeleteGroupsException e)
        {
            // NUnit variant adds this early exit for "group does not exist" error:
            if (e.Message.Contains("The group id does not exist"))
                return;

            Console.WriteLine(e); // log and retry
        }
    }
}
```

### Where to Call It (try/finally)

**xUnit (called synchronously from finally):**

```csharp
[Fact]
public async Task FlowSyncTest()
{
    try
    {
        // ... test body ...
    }
    finally
    {
        Console.WriteLine("Clean Up");
        DeleteConsumerGroupAsync().GetAwaiter().GetResult();
    }
}
```

**NUnit (called with await from finally):**

```csharp
[Test]
public async Task MyTest()
{
    try
    {
        // ... test body ...
    }
    finally
    {
        Console.WriteLine("Clean Up");
        await DeleteConsumerGroupAsync();
    }
}
```

### Why Each Step Matters

| Step | Reason |
|---|---|
| Stop all `kafkaBus.Consumers.All` | Gracefully stops each KafkaFlow consumer worker, commits offsets, sends `LeaveGroup` to the broker. |
| `Thread.Sleep(10s)` | Kafka broker needs time to mark the group as `Empty` after all members leave. The delete call will be rejected if any member is still listed. |
| Build `AdminClientConfig` from same credentials | The admin client must authenticate with the same broker. Without matching SASL config the `DeleteGroupsAsync` call will be rejected. |
| `kafkaBus.StopAsync()` before each delete attempt | Ensures the KafkaFlow hosted service is stopped so no internal reconnect races can re-activate the group between attempts. |
| Retry loop | Even with the 10-second sleep, the first delete attempt can still fail if the broker hasn't fully processed the group departure. The loop retries until success. |
| "group does not exist" early exit | If a previous run already deleted the group, the retry loop exits immediately instead of looping forever. |

---

## 10. Test Projects

### DI Setup Pattern (xUnit – constructor injection)

The entire DI container is built in the test class constructor. There is no shared fixture or `IClassFixture` – each test class creates its own `IServiceProvider`.

```csharp
public class GeneralTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    public IConfigurationRoot Configuration { get; set; }
    private IServiceProvider _provider;

    public GeneralTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        IServiceCollection services = new ServiceCollection();

        // 1. Build configuration with Steeltoe placeholder resolution
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddPlaceholderResolver()   // resolves ${VAR} from env/SSM
            .Build();
        // 2. Register services
        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton<FixtureMarketFactory>();
        services.AddLogging(lb => lb.AddConsole());

        // 3. Wire up the FlowSync framework with Kafka
        services.AddKafkaFlowSync(builder =>
        {
            var kafkaBrokerConfig = Configuration
                .GetSection("KafkaBrokerConfiguration")
                .Get<KafkaBrokerConfiguration>();

            builder
                .ConfigureBroker(brokerBuilder =>
                {
                    brokerBuilder
                        .WithBootstrapServers(kafkaBrokerConfig.BootstrapServers)
                        .WithCredentials(kafkaBrokerConfig.Credentials);
                })
                .AddConsumer(consumerBuilder =>
                {
                    consumerBuilder
                        .AddTopic(OutliersConsts.CONSUMER_TOPIC)
                        .AddConsumerGroup(OutliersConsts.CONSUMER_GROUP)
                        .AddConsumingSerializer(new KafkaUTF8Serializer())
                        .AddConsumingType(typeof(OutlierMarket));
                })
                .AddProducer(producerBuilder =>
                {
                    producerBuilder
                        .AddProducerTopic(OutliersConsts.PRODUCER_OUTLIER)
                        .AddProducerSerializer(new KafkaProtobufSerializer());
                });
        });

        _provider = services.BuildServiceProvider();
    }
}
```

### DI Setup Pattern (NUnit – `[SetUp]`)

```csharp
[SetUp]
public void SetUp()
{
    IServiceCollection services = new ServiceCollection();

    Configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddPlaceholderResolver()
        .Build();
    services.AddFlowSyncService();       // ← also valid instead of AddKafkaFlowSync when you need manual wiring
    services.AddSingleton<FixtureMarketFactory>();
    services.AddLogging(lb => lb.AddConsole());
    services.AddSingleton<IConfiguration>(_ => Configuration);

    services.AddKafkaFlowSync(builder =>
    {
        var kafkaBrokerConfig = Configuration
            .GetSection("KafkaBrokerConfiguration")
            .Get<KafkaBrokerConfiguration>();

        builder
            .ConfigureBroker(b =>
            {
                b.WithBootstrapServers(kafkaBrokerConfig.BootstrapServers)
                 .WithCredentials(kafkaBrokerConfig.Credentials);
            })
            .AddConsumer(c =>
            {
                c.AddTopic(OutliersConsts.CONSUMER_TOPIC)
                 .AddConsumerGroup(OutliersConsts.CONSUMER_GROUP)
                 .AddConsumingSerializer(new KafkaUTF8Serializer())
                 .AddConsumingType(typeof(OutlierMarket))
                 .SetWorkersCount(55)
                 .SetBufferSize(50);
            })
            .AddProducer(p =>
            {
                p.AddProducerTopic(OutliersConsts.PRODUCER_OUTLIER)
                 .AddProducerSerializer(new KafkaProtobufSerializer());
            });
    });

    _provider = services.BuildServiceProvider();
}
```

### Complete Test Method (CorrelationId mode)

```csharp
[Fact]
public async Task FlowSyncTest()
{
    try
    {
        // 1. Create the FlowSync step – this starts the consumer internally
        var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
        var FlowSyncService = await FlowSyncFactory.CreateFlowSyncStepAsync(new FlowSyncConfiguration
        {
            ProduceTo = OutliersConsts.PRODUCER_OUTLIER,
            ConsumeFrom = new[] { OutliersConsts.CONSUMER_TOPIC },
            ConsumingOptions = new ConsumingOptionsConfiguration
            {
                TimeOut = TimeSpan.FromSeconds(30),
                MsgReceivedCount = 1,
            }
        });

        // 2. Build the message
        var fixture = await FixtureFactory.CreateInPlayFixture();
        var fixtureMarket = fixtureMarketFactory.GetBaseFixtureMarket(fixture.Id);
        fixture.LastUpdate = DateTime.UtcNow;
        var message = new InPlayMarketsDistribution
        {
            Fixture = fixture,
            ProviderId = fixtureMarket.ProviderId,
            MarketsToDistribute = new List<FixtureMarket> { fixtureMarket }
        };

        // 3. Produce and get a task that will complete when the response arrives
        var waitForMessagesTask = await FlowSyncService.ExecuteAsync(
            $"{fixtureMarket.FixtureId}_{fixtureMarket.MarketId}", message);

        await Task.Delay(1000);

        // 4. Await the response
        var messages = await waitForMessagesTask.Task;
        Console.WriteLine(messages.JsonSerialize());
    }
    finally
    {
        Console.WriteLine("Clean Up");
        DeleteConsumerGroupAsync().GetAwaiter().GetResult();
    }
}
```

### Complete Test Method (MessageKey mode)

```csharp
[Test]
public async Task MessageKeyFlowSyncTest_ProduceWithKeyAndConsumeByKey()
{
    try
    {
        var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
        var FlowSyncService = await FlowSyncFactory.CreateFlowSyncStepAsync(new FlowSyncConfiguration
        {
            ProduceTo = OutliersConsts.PRODUCER_OUTLIER,
            ConsumeFrom = new[] { OutliersConsts.CONSUMER_TOPIC },
            ConsumingOptions = new ConsumingOptionsConfiguration
            {
                TimeOut = TimeSpan.FromSeconds(30),
                MsgReceivedCount = 1,
                ExpectedMessageKey = $"Outliers:InPlay:{fixture.Id}:{fixtureMarket.MarketId}"
            }
        });

        var waitForMessagesTask = await FlowSyncService.ExecuteAsync(
            $"{fixtureMarket.FixtureId}_{fixtureMarket.MarketId}", message);

        await Task.Delay(1000);

        var messages = await waitForMessagesTask.Task;
        Console.WriteLine(messages.JsonSerialize());
    }
    finally
    {
        Console.WriteLine("Clean Up");
        await DeleteConsumerGroupAsync();
    }
}
```

### Consts Class Pattern

Create one consts class per test project with all Kafka topic and group names:

```csharp
namespace FlowSync.Tests.Consts;

public class OutliersConsts
{
    // Kafka connection env var names
    public const string KAFKA_BOOTSTRAP         = "MESSAGE_BROKER_KAFKA_HERMES_BOOTSTRAP_BROKER_TLS";
    public const string KAFKA_SASL_USERNAME     = "MESSAGE_BROKER_KAFKA_SASL_USERNAME";
    public const string KAFKA_SASL_PASSWORD     = "MESSAGE_BROKER_KAFKA_SASL_PASSWORD";
    public const string KAFKA_SASL_MECHANISM    = "MESSAGE_BROKER_KAFKA_SASL_MECHANISM";
    public const string KAFKA_TLS_ENABLED       = "MESSAGE_BROKER_KAFKA_TLS_ENABLED";
    public const string KAFKA_SECURITY_PROTOCOL = "MESSAGE_BROKER_KAFKA_SECURITY_PROTOCOL";

    // Topics
    public const string PRODUCER_OUTLIER                      = "DI.InPlay.Markets.Raw";
    public const string PRODUCER_INPLAY_OUTLIERED_MARKET      = "DI.InPlayMarket.Outliered";
    public const string PRODUCER_INPLAY_FIXTURE_STATUS_SIGNAL = "DI.InPlay.SuspensionManager.FixtureSignal";

    // Consumer
    public const string CONSUMER_GROUP = "Automation-DI-InPlayOutlier-Tests";
    public const string CONSUMER_TOPIC = "DI.InPlayMarket.Outliered";
}
```

---

## 11. Configuration – appsettings.json

Both test projects carry an identical `appsettings.json`. Values use Steeltoe `${PLACEHOLDER}` syntax, which is resolved from environment variables or AWS SSM Parameter Store at runtime.

```json
{
  "Environment": "ResultingServiceTest",
  "TestEnvironment": "QA",

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Enrichers.Thread", "Serilog.Enrichers.Span" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "System.Net.Http.HttpClient": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "Enrich": [ "WithThreadId", "WithSpan" ],
    "Properties": {
      "system": "DI",
      "domain": "OS"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": {
            "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
            "template": "{ {timestamp: @t, level: ..., message: @m, exception: @x} }\n"
          }
        }
      }
    ]
  },

  "AllowedHosts": "*",

  "Kestrel": {
    "EndPoints": {
      "Http": { "Url": "http://*:5000" }
    }
  },

  "KafkaBrokerConfiguration": {
    "BootstrapServers": [ "${MESSAGE_BROKER_KAFKA_HERMES_BOOTSTRAP_BROKER_TLS}" ],
    "Credentials": {
      "Username":         "${MESSAGE_BROKER_KAFKA_SASL_USERNAME}",
      "Password":         "${MESSAGE_BROKER_KAFKA_SASL_PASSWORD}",
      "Mechanism":        "${MESSAGE_BROKER_KAFKA_SASL_MECHANISM}",
      "TlsEnabled":       "${MESSAGE_BROKER_KAFKA_TLS_ENABLED}",
      "SecurityProtocol": "${MESSAGE_BROKER_KAFKA_SECURITY_PROTOCOL}"
    }
  },

  "SSM": [
    "/message-broker/kafka/main-message-broker",
    "/message-broker/kafka/hermes",
    "/db/mysql/main",
    "/message-broker/rmq/os",
    "/message-broker/rmq/stm",
    "/message-broker/rmq/stm-customers-inplay",
    "/message-broker/rmq/stm-customers-prematch",
    "/db/redis/os",
    "/db/redis/stm"
  ],

  "ConnectionStrings": {
    "Stm":      "server=${DB_MYSQL_MAIN_HOST};database=stm;UID=${DB_MYSQL_MAIN_USERNAME};password=${DB_MYSQL_MAIN_PASSWORD}",
    "Data":     "server=${DB_MYSQL_MAIN_HOST};database=data;UID=${DB_MYSQL_MAIN_USERNAME};password=${DB_MYSQL_MAIN_PASSWORD}",
    "Crm":      "server=${DB_MYSQL_MAIN_HOST};database=data;UID=${DB_MYSQL_MAIN_USERNAME};password=${DB_MYSQL_MAIN_PASSWORD}",
    "Redis":    "${db:redis:stm-clustered:endpoint}:${db:redis:stm-clustered:port}",
    "RabbitMQ": "amqp://${message-broker:rmq:stm:runner-username}:${message-broker:rmq:stm:runner-password}@${message-broker:rmq:stm:endpoint}:${message-broker:rmq:stm:port}/Metadata"
  },

  "Redis": {
    "HostName": "${DB:REDIS:DATA-INTEGRITY-CLUSTERED:ENDPOINT}",
    "DatabaseNumber": 0
  }
}
```

**Key points:**
- `KafkaBrokerConfiguration` section maps exactly to `KafkaBrokerConfiguration` C# class.
- `AddPlaceholderResolver()` must be called on the `ConfigurationBuilder` for `${...}` substitution to work.
- `AddPlaceholderResolver()` must be called on the `ConfigurationBuilder` for `${...}` substitution to work.

---

## 12. CI/CD Pipeline

### `cicd-main.yml`

```yaml
trigger:
  - main

pool: k8s-agents-ci

variables:
  - template: variables.yml

steps:
  - task: gitversion/setup@0
    displayName: GitVersion Setup
    inputs:
      versionSpec: '5.x'

  - task: gitversion/execute@0
    displayName: "Calculate version"
    inputs:
      useConfigFile: true
      configFilePath: '$(versionconfig)'

  - script: echo current version is $(GitVersion.SemVer).$(GitVersion.CommitsSinceVersionSource)
    displayName: 'Display calculated version'

  - task: DotNetCoreCLI@2
    displayName: "Restore Packages"
    inputs:
      command: 'restore'
      projects: '$(solution)'
      feedsToUse: 'config'
      nugetConfigPath: '$(nugetconfig)'

  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: 'build'
      projects: '$(solution)'
      arguments: '--no-restore --configuration "$(releaseconfig)" /p:Version=$(GitVersion.SemVer).$(GitVersion.CommitsSinceVersionSource)'
      versioningScheme: 'byBuildNumber'

  - task: ChangedFiles@1
    name: CheckChanges
    inputs:
      rules: Simulator/*.*
      variable: HasChanged

  - task: DotNetCoreCLI@2
    displayName: Nuget Push
    inputs:
      command: 'push'
      packagesToPush: '$(packlocation)/*.nupkg'
      nuGetFeedType: 'internal'
      publishVstsFeed: '$(feedname)'
      versioningScheme: byBuildNumber
```

### `variables.yml`

```yaml
variables:
  nugetconfig:   ./nuget/nuget.config
  solution:      ./FlowSync.sln
  releaseconfig: Release
  feedname:      DataIntegrityInfra/DIGeneralFeed
  packlocation:  ./**
  versionconfig: ./versionconfig.yml
```

### `versionconfig.yml`

```yaml
next-version: 5.1.0
```

---

## 13. NuGet Configuration

### `nuget/nuget.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <activePackageSource>
    <add key="All" value="(Aggregate source)" />
  </activePackageSource>
  <disabledPackageSources>
    <add key="Microsoft and .NET" value="true" />
  </disabledPackageSources>
</configuration>
```

---

## 14. End-to-End Flow Summary

```
Test Constructor / [SetUp]
  │
  ├─ Build IConfiguration (appsettings.json + env vars + Steeltoe placeholders)
  ├─ services.AddKafkaFlowSync(...)
  │     ├─ AddFlowSyncService() → registers IFlowSyncFactory, IFilterService, IMessagePool
  │     ├─ Singleton<IConsumer, KafkaConsumer>
  │     ├─ Singleton<IProducer, KafkaProducer>
  │     └─ KafkaConfigurationBuilder.BuildKafkaFlow()
  │           ├─ AddKafkaFlowHostedService (registers KafkaFlow topology)
  │           └─ Singleton<IKafkaBus> = resolve.CreateKafkaBus()
  └─ _provider = services.BuildServiceProvider()

Test body
  │
  ├─ FlowSyncFactory.CreateFlowSyncStepAsync(config)
  │     ├─ Determines step type (CorrelationId vs MessageKey)
  │     └─ FlowSyncStep.InitializeAsync()
  │           ├─ filterService.AddFilter(FlowSyncId)   ← registers expected identifier
  │           └─ consumer.StartConsumeAsync(topic)    ← SEE SECTION 8
  │
  ├─ FlowSyncService.ExecuteAsync(key, message)
  │     ├─ AddProducingHeaders (FlowSyncId / correlation_id)
  │     ├─ producer.ProduceAsync(topic, key, message, headers)
  │     └─ Returns TaskCompletionSource (polls MessagePool every 250ms)
  │
  ├─ await Task.Delay(1000)   ← allow the message to travel through the system
  │
  └─ var messages = await waitForMessagesTask.Task
        └─ BaseFlowSyncStep.GetTaskResultAsync polls _messagePool until
           MsgReceivedCount satisfied or TimeOut

On each consumed message
  │
  ├─ ErrorHandlingMiddleware  (wraps in try/catch)
  ├─ FilterMiddleware         (correlation ID header → then message key)
  │     └─ if match: sets context.Items["FlowSyncId"]
  ├─ Deserializer
  └─ PushMessageMiddleware    (adds to MessagePool under FlowSyncId)

finally block
  └─ DeleteConsumerGroupAsync()   ← SEE SECTION 9
```

---

*End of guide. Sections 8 and 9 are the most critical to get right – the consumer will be missed or left dangling if either pattern is implemented incorrectly.*

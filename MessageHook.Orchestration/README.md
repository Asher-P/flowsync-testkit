# MessageHook.Orchestration

Orchestration layer for **MessageHook** — provides the step factory, configuration, and execution logic for produce-then-consume integration test flows.

> Most users should install **MessageHook.Kafka** instead, which pulls this package automatically and adds the Kafka transport layer.

## Installation

```bash
dotnet add package MessageHook.Orchestration
```

## Key Types

| Type | Description |
|---|---|
| `IMessageHookFactory` | Resolves the correct step type based on configuration |
| `MessageHookConfiguration` | Defines what topic to produce to, what to consume from, and consuming options |
| `ConsumingOptionsConfiguration` | Timeout, expected message count, optional `ExpectedMessageKey` |
| `IMessageHookStep` | The executable step returned by the factory |

## Configuration

```csharp
var config = new MessageHookConfiguration
{
    ProduceTo   = "request-topic",           // topic to produce to (optional — omit for consume-only)
    ConsumeFrom = new[] { "response-topic" },
    ConsumingOptions = new ConsumingOptionsConfiguration
    {
        TimeOut            = TimeSpan.FromSeconds(30),
        MsgReceivedCount   = 1,
        ExpectedMessageKey = "optional-key"  // set to filter by Kafka message key
    }
};
```

## Consuming Modes

- **Correlation ID** — default mode. MessageHook injects a correlation ID when producing and matches it on the consumed response.
- **Message Key** — set `ExpectedMessageKey` to filter consumed messages by Kafka message key instead.

## Source & Issues

[github.com/Asher-P/messagehook-testkit](https://github.com/Asher-P/messagehook-testkit)

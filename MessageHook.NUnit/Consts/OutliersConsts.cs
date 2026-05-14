namespace MessageHook.NUnit.Consts;

public class OutliersConsts
{
    // Headers
    public const string CORRELATION_ID_HEADER_NAME = "MESSAGE_BROKER_KAFKA_CORRELATION_ID_HEADER_NAME";
    
    // Kafka connection
    public const string KAFKA_BOOTSTRAP = "MESSAGE_BROKER_KAFKA_HERMES_BOOTSTRAP_BROKER_TLS";
    public const string KAFKA_SASL_USERNAME = "MESSAGE_BROKER_KAFKA_SASL_USERNAME";
    public const string KAFKA_SASL_PASSWORD = "MESSAGE_BROKER_KAFKA_SASL_PASSWORD";
    public const string KAFKA_SASL_MECHANISM = "MESSAGE_BROKER_KAFKA_SASL_MECHANISM";
    public const string KAFKA_TLS_ENABLED = "MESSAGE_BROKER_KAFKA_TLS_ENABLED";
    public const string KAFKA_SECURITY_PROTOCOL = "MESSAGE_BROKER_KAFKA_SECURITY_PROTOCOL";
    
    // Producers
    public const string PRODUCER_OUTLIER = "A";
    public const string PRODUCER_INPLAY_OUTLIERED_MARKET = "B";
    public const string PRODUCER_INPLAY_FIXTURE_STATUS_SIGNAL = "DI.InPlay.SuspensionManager.FixtureSignal";

    // Consumer
    public const string CONSUMER_GROUP = "Automation-DI-InPlayOutlier-Tests";
    public const string CONSUMER_TOPIC = "B";
}
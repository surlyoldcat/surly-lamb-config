using System;
using Amazon;
using Amazon.Kinesis;
using Eti.LambdaPlumbing.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Amazon.Kinesis.Stream;
using Serilog.Sinks.Amazon.Kinesis.Stream.Sinks;

namespace Eti.LambdaPlumbing.Logging
{
    /// <summary>
    /// Basic log wrapper around a Serilog implementation. This
    /// wrapper will write to both Console (and therefore CloudWatch)
    /// and Kinesis. Implemented as IDisposable so that we can
    /// force the underlying logger to flush prior to the Lambda
    /// context getting shut down.
    /// </summary>
    public class ELogWrapper : IDisposable
    {
        private const int NUM_SHARDS = 2;

        private readonly LogEventLevel consoleLevel;
        private readonly LogEventLevel kinesisLevel;
        
        public ELogWrapper(LogEventLevel minCloudWatchLevel = LogEventLevel.Debug,
            LogEventLevel minSplunkLevel = LogEventLevel.Information)
        {
            consoleLevel = minCloudWatchLevel;
            kinesisLevel = minSplunkLevel;
            SetupLogging(
                ConfigManager.Setting(ConfigKeys.LogStream),
                NUM_SHARDS,
                ConfigManager.Setting(ConfigKeys.AwsRegion));
        }

        public void Debug(string payload)
        {
            Log.Debug(payload);   
        }

        public void Debug(LogPayloadBuilder payloadBuilder)
        {
            Debug(payloadBuilder.BuildJson());
        }
        
        public void Information(string payload)
        {
            Log.Information(payload);
        }
        
        public void Information(LogPayloadBuilder payloadBuilder)
        {
            Information(payloadBuilder.BuildJson());
        }
        
        public void Warning(string payload)
        {
            Log.Warning(payload);    
        }

        public void Warning(LogPayloadBuilder payloadBuilder)
        {
            Warning(payloadBuilder.BuildJson());
        }
        
        public void Error(string payload)
        {
            Log.Error(payload);
        }

        public void Error(LogPayloadBuilder payloadBuilder)
        {
            Error(payloadBuilder.BuildJson());
        }

        public void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }
        
        public void Dispose()
        {
            try
            {
                Log.CloseAndFlush();
            }
            catch
            {
                // ignored
            }
        }

        private void SetupLogging(string kinesisName, int numKinesisShards,  string awsRegion)
        {   
            //log rules:
            //log everything to Console- this will go to CloudWatch.
            //if we can init the Kinesis/Splunk stream, log to that at a specified min level.
            //if we are unable to init Kinesis, log a message about that.
            AmazonKinesisClient kinesis = SetupKinesis(awsRegion, kinesisName, numKinesisShards);
            bool kinesisSuccess = null != kinesis;
                        
            if (kinesisSuccess)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(restrictedToMinimumLevel: consoleLevel)
                    .WriteTo.AmazonKinesis(
                        kinesisClient: kinesis,
                        streamName: kinesisName,
                        period: TimeSpan.FromSeconds(1),
                        bufferBaseFilename: "./logs/kinesis-buffer",
                        minimumLogEventLevel: kinesisLevel
                    )
                    .CreateLogger();

            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                    .CreateLogger();
                Log.Error($"ELogWrapper failed to initialize Kinesis (Splunk) stream: {kinesisName}");
            }

        }
     
        private static AmazonKinesisClient SetupKinesis(string awsRegion, string streamName, int numShards = 1)
        {
            //note, this is copied from the documentation for the Kinesis Serilog sink. i'm assuming
            //that the CreateAndWaitForStream... call is necessary.
            var client = new AmazonKinesisClient(RegionEndpoint.GetBySystemName(awsRegion));
            bool streamSuccess = KinesisApi.CreateAndWaitForStreamToBecomeAvailable(client, streamName, numShards);
            return streamSuccess ? client : null;
        }

    }
}
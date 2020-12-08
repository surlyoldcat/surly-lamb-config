# surly-lamb-config
a **rough** sketch of alternative configuration and logging for Lambdas. there are a few primary features:
- using Serilog to provide unified, structured logging to multiple destinations (Cloudwatch and Kinesis (Splunk))
- unified configuration- environment vars, custom settings, and items from the 2 DynamoDB "config" tables
- explicit in-memory caching, only of config values that are required by the individual Lambda



## Projects
### Eti.LambdaPlumbing.Lib
Where the actual code of interest lives. There's a namespace for Configuration, and one for Logging. The logging implementation depends on the Config implementation (for obtaining environment variables.)

#### Eti.LambdaPlumbing.Configuration.ConfigManager
- Provides static methods for reading config data. 
- Also has code in it for doing the ETL work to get data from NetworkConfig into fancy new key-value collection.

#### Eti.LambdaPlumbing.Logging.ELogWrapper
-  Provides a pretty standard logger interface.
- Setup is handled in a static constructor 
- Nothing terribly magical here, just note that the use of the IDisposable pattern is mandatory for ensuring that log events at the end of a Lambda execution will actually get flushed to the sorta-kinda async Kinesis logger
- Min log levels can be set differently for Console vs Kinesis. And plenty of other interesting configuration options. We could even have separate logging contexts for "trace info" vs "info for Splunk." Or log directly to a Splunk HEC.

## HelloWorld
A simple Lambda created with SAM. It includes a simple example of using the logging and configuration features in a Lambda. Essentially, this would become boilerplate code.

## Configuration
The Config piece does the following:
1.  For "NetworkConfig" it will read key-value pairs from a new table, as they are requested.
2. It will then cache those pairs using a regular .Net MemoryCache, with an absolute expiration.
3. For Service Registry, it does pretty much the same thing, except there is no new table.

### What? A new table?
Yes. A new table. A flat key-value store. With a range key named "scope" which corresponds to the Service Name of our project. The current table name (in NA-DEV) is `rkt-eti-resource-mappings` . The "key" is a logicalId, the value is the physicalId. Here's a sample item:
```
{
  "key": "EtiProductBundle",
  "scope": "eti",
  "value": "eti-develop-data-1572978028-EtiProductBundle-D2BGJ6BSPVXC"
}

```
To get data into this table, we'd need a Lambda listening to DynamoDB events. When the config changes, rewrite our 'cache.' The code for this already exists in the ConfigExtractor namespace.

### For the love of Humperdink, why?!?!?
The NetworkConfig table contains *all* config data for an entire project. ETI, Cyber, etc.  Currently, each Lambda has to read and parse a large JSON blob, just to extract the 4 or 5 values it needs. This approach eliminates the need to parse/query that data at runtime, and turns the config into a per-app hashtable. 

Note, the ServiceRegistry table is 'slim' enough that it doesn't require this treatment.

### Why use a memory cache?
Because it is more intentional and predictable than just stuffing the config data into a static variable. Static variables do not have an expiration. And the end result will differ greatly depending on how the Lambda is provisioned. For an "always on" Lambda, the static config will never change until the instance is restarted (or the Lambda context is otherwise recycled.) For a "regular" Lambda, the state of the static variables will vary depending on how frequently the Lambda is invoked, and the details of that are left up to the AWS runtime. 

Using an explicit in-memory cache provides the same cost savings as using statics, but it does so in a way which is:
- Clearly specified in the code
- Subject to a configurable expiration. So, even when provisioned, the config will expire every N minutes. This eliminates the need to recycle provisioned Lambdas when there is a config change. And you can turn it off.
- Consistent regardless of Lambda provisioning.


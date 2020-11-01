# surly-lamb-config
possible way to handle configuration for the lambdas, which will allow a little more flexibility (and enable local development.)
the simplest and probably most foolproof way to differentiate local vs hosted is to just use the DEBUG conditional compilation constant. that *should* eliminate any chance of local config ending up enabled in a deployed Lambda. As opposed to config rules or overrides, where you have to have the order just right. anyway. maybe worthless...

```
#if DEBUG
            
            var foo = new ConfigBuilder()
                .WithAwsProfile("NALAB")
                .UseLocalFile("bigoldconfigfile.json")
                .Build();

            
#else
            var foo = new ConfigBuilder()
                            .WithDefaultAwsProfile()
                            .EnableCaching()
                            .UseDynamoDB()
                            .Build();
#endif
```

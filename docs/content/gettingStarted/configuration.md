---
title: Configuration
weight: 50
---

The library configuration object can be accessed through the API class.

{{< highlight csharp "linenos=table" >}}
IChangeHandlerApi api = ChangeHandlerApiFactory.NewApiInstance();
IConfiguration conf = api.Configuration;
{{< /highlight >}}

From there, there are a couple of configuration options that can be set.

{{< highlight csharp "linenos=table" >}}
/// <summary>
/// Global flag to enable/disable change detection.
/// If this field is set to false, no data is saved to memory in the BeforeChange method and no handlers are called in the AfterChange method.
/// </summary>
bool ChangeHandlingEnabled { get; set; }

/// <summary>
/// This represents the maximum number of cells that will be stored in memory.
/// If the number of cells affected by a change exceeds this limit, the source/target data will not be loaded.
/// This means that the entire range will be flagged as changed.
/// Setting this to a high value may cause out-of-memory errors.
/// </summary>
long MaxMemorySize { get; set; }
{{< /highlight >}}

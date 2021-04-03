---
title: Documentation
---

<!-- markdownlint-capture -->
<!-- markdownlint-disable MD033 -->

<span class="badge-placeholder">[![Project](https://img.shields.io/badge/project-gitlab-brightgreen?style=flat&logo=gitlab)](https://gitlab.com/hectorjsmith/excel-change-handler/)</span>
<span class="badge-placeholder">[![Build Status](https://gitlab.com/hectorjsmith/excel-change-handler/badges/main/pipeline.svg)](https://gitlab.com/hectorjsmith/excel-change-handler/commits/main)</span>
<span class="badge-placeholder">[![Nuget package](https://badgen.net/nuget/v/CSharpExcelChangeHandler/latest)](https://www.nuget.org/packages/CSharpExcelChangeHandler/)</span>
<span class="badge-placeholder">[![License: MIT](https://img.shields.io/badge/license-MIT-brightgreen)](https://gitlab.com/hectorjsmith/excel-change-handler/-/blob/main/LICENSE)</span>

<!-- markdownlint-restore -->
# Excel Change Handler
*C# library to enable detecting and processing changes made on an Excel worksheet*

## How to Use

*See the included example project to see it in action*

### API

The library is driven by a main API class. The class has methods that should get triggered before and after a change.

A new instance of the API can be created using the static factory method:

```csharp
IChangeHandlerApi api = ChangeHandlerApiFactory.NewApiInstance();
```

### Detect Changes

The API class needs to be informed of changes that happen in the Excel workbook. The library intentionally does not hook directly into Excel events for a few reasons:
- This approach makes the code more modular and easier to test
- No dependency on Excel interop

There are two methods that should get called to handle a change. One is to be called before the change, and one after.

```csharp
api.BeforeChange(wrappedSheet, wrappedRange);
api.AfterChange(wrappedSheet, wrappedRange);
```

To avoid having any dependencies on Excel interop libraries, the sheet and range the API class takes are wrappers.
The client code must create instances of the `IWorksheet` and `IRange` interfaces that expose the information the library needs (sheet name, range address, etc).
These instances can then be passed into the before/after methods on the API.

**Note**: The provided sheet and range data objects are internally wrapped in a caching class to improve performance. That means properties on these objects are only read once.

The provided example application shows an implementation of these wrapper interfaces based on excel ranges and worksheets.
The example application also shows how before and after methods can be hooked into the `SheetSelectionChange` and `SheetChange` events respectively.

Note that in order to correctly detect changes, the same API instance must be used for both `BeforeChange` and `AfterChange`.
This is because the state memory from before the change is tied to that particular API instance. If a different instance is used, there will be no memory of the data provided in the `BeforeChange` method.

### Handle Changes

The library can be setup with a number of change handlers. Each change handler must implement the `IChangeHandler` interface.

To add a new change handler:

```csharp
api.AddCustomHandler(...);
```

The library contains a couple of default implementations to get started. A standard change highlighter and a standard change logger.
These can be created by using the change handler factory on the API.

```csharp
api.ChangeHandlerFactory.NewSimpleChangeHighlighter(...);
api.ChangeHandlerFactory.NewSimpleChangeLogger();
```

Custom handlers can be crated by extending the `IChangeHandler` interface and implementing the `HandleChange` method.

One of the parameters to the `HandleChange` method is the memory comparison object - `IMemoryComparison`.
This object contains is the result of comparing the data provided to the library in the `BeforeChange` and `AfterChange` methods and holds a lot of useful data about the comparison.

### Generic Types

It is possible to create two types of API instances. A standard one or a generic one.

The API allows creating instances that are generic on the sub-type of `IWorksheet` or `IRange` that is used.

A generic API instance can be created using the same static factory:

```csharp
IGenericChangeHandlerApi<MySheet, MyRange> api = ChangeHandlerApiFactory.NewGenericApiInstance<MySheet, MyRange>();
```

The `IChangeHandler` interface is also generic on the type of `IWorksheet` and `IRange` used. When adding a new handler to the API instance, the exact same type of worksheet and range must be used.
This is an example from one of the unit tests:

```csharp
IGenericChangeHandlerApi<SimpleMockSheet, SimpleMockRange> api = ChangeHandlerApiFactory.NewGenericApiInstance<SimpleMockSheet, SimpleMockRange>();
GenericMockChangeHandler<SimpleMockSheet, SimpleMockRange> handler = new GenericMockChangeHandler<SimpleMockSheet, SimpleMockRange>();
```

This also means that when the handler is called, it has access to the exact type of object used to call the `BeforeChange` and `AfterChange` methods.
This makes it easy to propagate custom classes down to the change handlers.

### Configuration

The library configuration object can be accessed through the API class:

```csharp
api.Configuration
```

### Logging

The library supports injecting a custom logger to log system messages and errors. To inject a logger the client code must create a class that implements the `ILogger` interface and inject it into the main API.

```
class MyLogger : ILogger {
    // ...
}
...
api.SetApplicationLogger(new MyLogger());
```

Note that the logger being used is specific to each API instance.

---

## Design

The Excel event for `SheetChange` is triggered after the change has occured and does not contain information about what the state was **before** the change.
That is why the library has a `BeforeChange` and a `AfterChange` method. It allows the library to keep track of what the data was before the change happened.

With access to the data before and after the change, the library can detect false-changes - the `SheetChange` event can get triggered when the cell didn't actually change (e.g. paste formatting).
It also allows for logging changes made to a worksheet with the before and after values (e.g. for peer review).

### How Changes are Detected

The library detects changes by comparing the data provided in the `BeforeChange` and the `AfterChange` methods.

Once the `BeforeChange` method is called, the library will store the following information in memory:
- Sheet name
- Sheet row number
- Sheet column number
- Range address
- Range data (if range size is below memory limit)

When the `AfterChange` method is called, the data in memory is compared to the data provided:

- If memory is blank, consider a valid change
- Check range locations (sheet name + range address)
  - If location does not match, valid change
- Check range dimensions
  - If range dimensions do not match, valid change
- Compare data
  - If data in memory is blank, valid change
  - Read data from input range and compare to data in memory
    - If data is different, valid change
- If everything matches, no change - handles do not get called

**NOTE:** The code in `BeforeChange` and `AfterChange` does not fire if no handlers have been set, or if `api.Configuration.ChangeHandlingEnabled` is set to `false`.

The `IMemoryComparison` object provided to the change handlers will include the results of this comparison as well as information about what data was in memory before the change:

```csharp
public bool HasSheetSizeChanged { get; }
public bool IsNewRow { get; }
public bool IsRowDelete { get; }
public bool IsNewColumn { get; }
public bool IsColumnDelete { get; }
public bool LocationMatches { get; }
public bool LocationMatchesAndDataMatches { get; }
IChangeProperties? PropertiesBeforeChange { get; }
IChangeProperties PropertiesAfterChange { get; }
```

The `PropertiesBeforeChange` property will be null if no memory was set before the change was recorded (i.e. the `BeforeChange` method was not called). The `PropertiesAfterChange` will never be null and include data known about the new state of the range.

All properties on the `IChangeProperties` interface are nullable and will only be populated if known.

**NOTE:** The range formula data before and after change may be null, this data is only read when necessary to improve performance.

### Max Memory Size

To avoid memory issues when tracking changes on very large sheets/ranges, this library has a configured memory limit.
If the number of cells provided in the `BeforeChange` method is over the max memory value, the range data will not be stored.
In this case the library will not be able to accurately detect exactly which cells were changed in the changed range.

This limit can be changed through the `api.Configuration` property. By default, it is set to 250,000.


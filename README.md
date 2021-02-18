<h1 align="center">
  <br>
   <img src="https://i.imgur.com/B09Hpmf.png" alt="Logo OpenCollections" title="Logo OpenCollections by DekuDesu ( (https://i.imgur.com/B09Hpmf.png )" />
  <br>
</h1>
<p align="center">  
<a href="https://www.codacy.com?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=DekuDesu/OpenCollections&amp;utm_campaign=Badge_Grade)"><img src="https://app.codacy.com/project/badge/Grade/55eb3a074c52477ead4d70b6ae157718"></a>
<a href="https://www.codacy.com?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=DekuDesu/OpenCollections&amp;utm_campaign=Badge_Grade">
<a href="https://travis-ci.com/DekuDesu/OpenCollections"><img src="https://travis-ci.com/DekuDesu/OpenCollections.svg?branch=master"></a>
 <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/license-MIT-blue.svg"></a>
</p>

<p align="center">
  OpenCollections is a pure CSharp implementation of basic data pipeline modules. These basic Consumers, Producers, Readers, and Writers are powerful at creating simple pipelines for manipulating large sets of data synchronously and/or asynchronously. Great for developing small applications quickly.
</p>

## Table of Contents

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/bac44be919bc4def9966c542c02c7a7c)](https://app.codacy.com/gh/DekuDesu/OpenCollections?utm_source=github.com&utm_medium=referral&utm_content=DekuDesu/OpenCollections&utm_campaign=Badge_Grade)

* [Installation](#installation)
* [Documentation](#usage)
	* [Setup](#setup)
	* [Available Module Types](#available-module-types)
		* [Enumerable Readers](#enumerable-readers)
			* [`EnumerableStreamReader`](#enumerablestreamreader)
				* [Factory Overloads](#enumerablestreamreader-factory-overloads)
				* [Public Members](#enumerablestreamreader-public-members)
			* [`EnumerableMultiReader`](#enumerablemultireader)
				* [Factory Overloads](#enumerablemultireader-factory-overloads)
				* [Public Members](#enumerablemultireader-public-members)
		* [Producers](#producers)
			* [`ConcurrentProducer<out T>`](#producers)
				* [Factory Overloads](#concurrentproducer-factory-overloads)
				* [Public Members](#concurrentproducer-public-members)
		* [Consumers / Writers](#consumers)
			* [`ConcurrentConsumer<in T, out TResult>`](#consumers)
				* [Factory Overloads](#concurrentconsumer-factory-overloads)
				* [Public Members](#concurrentconsumer-public-members)
			* [`MultiConcurrentConsumer<in T, out TResult>`](#multiconcurrentconsumer)
				* [Factory Overloads](#multiconcurrentconsumer-factory-overloads)
				* [Public Members](#concurrentconsumer-public-members)
			* [`ConcurrentWriter<in T>`](#concurrentwriter)
				* [Factory Overloads](#concurrentwriter-factory-overloads)
				* [Public Members](#concurrentwriter-public-members)
## Installation
Type | Instructions
------------ | -------------
Visual Studio | Manage NuGet Packages and search OpenCollections
NuGet Command Line | ```nuget install OpenCollections -%OutputDirectory% packages```
Manual Installation | Download current release, add OpenCollections.dll to your project and add a reference to the .dll.

## Usage
### Setup
Before you can use OpenCollections add a ```usings``` statement to access the OpenCollections objects.

#### Example:
```csharp
...
using System.Threading.Tasks;

using OpenCollections;

namespace YourNamespace
{
...
```

### Available Module Types
As of v1.0.0 there are 4 types of modules
Module | General Purpose
---------|---------
Enumerable Readers | Reads from flat files, but can be iterated
Writers | Writes to files
Consumers | Takes items from producers, and manipulates the items
Producers | Produces items from Enumerable objects

## Enumerable Readers

#### EnumerableStreamReader
The `EnumerableStreamReader` is a OpenCollections object that enumerates over a file instead of reading to a collection or a list. This is most commonly used with OpenCollections producers.

#### Example:
If you have a flat(.txt) file at path `C:\TestData.txt`that contains the following data:
```
1
2
3
4
```
```csharp
var reader = Factory.CreateReader("C:\TestData.txt");
foreach(var line in reader.ReadLines())
{
	Console.WriteLine(line);
}
```
Output:
```
1
2
3
4
```

#### EnumerableMultiReader
The `EnumerableMultiReader` is a module that enumerates over many files instead of a single path you can pass any number of `string` or a `string[]` with the paths.

##### Example:
Say you have 3 flat files
Path | Data
-|-
C:\TestData.txt | 1 2 3
D:\BackupData.txt | 4 5 6
G:\Data.txt | 7 8 9

```csharp
string[] paths = {"C:TestData.txt", "D:\BackupData.txt", "G:\Data.txt"};

var reader = Factory.CreateMultiReader(paths);

foreach(string line in reader.ReadLines())
{
	Console.WriteLine(line);
}
```
Output:
```
1
2
3
4
5
6
7
8
9
```

#### Reader Detailed Overview
##### EnumerableStreamReader Factory Overloads
```csharp
IEnumerableReader<string> CreateReader(string FilePath)
```
##### EnumerableStreamReader Public Members
```csharp
string Path { get; } // the path the reader reads from
IEnumerable<string> ReadLine();
IEnumerator<string> GetEnumerator();
IEnumerator IEnumerable.GetEnumerator()
void Dispose();
```
##### EnumerableMultiReader Factory Overloads
```csharp
IEnumerableReader<string> CreateReader(params string[] FilePaths)
```
##### EnumerableMultiReader Public Members
```csharp
string[] Paths { get; } // the paths the reader reads from
bool Reading { get; } // true when reader has a open file handle
int CurrentPath { get; } // the index of the current open file handle
IEnumerable<string> ReadLine();
IEnumerator<string> GetEnumerator();
IEnumerator IEnumerable.GetEnumerator()
void Dispose();
```

## Producers

####  `ConcurrentProducer<out T>`
The `ConcurrentProducer<T>` produces items from an enumerable object either synchronously or asynchronously. 

The items it produces is immediately added to a thread safe collection called an [`IProducerConsumerCollection<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.iproducerconsumercollection-1?view=net-5.0) such as a [`ConcurrentQueue<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=net-5.0),[`ConcurrentBag<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentbag-1?view=net-5.0), or [`ConcurrentStack<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentstack-1?view=net-5.0). For more information about thread safe collections see: [`System.Collections.Concurrent`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent?view=net-5.0). 

By default the object instantiates a new `ConcurrentQueue<T>` as it's output collection. The producer, like all OpenCollections `IConcurrentOutput<T>` has a publically accessible `IProducerConsumerCollection<T> ResultCollection { get; set;}`. This can be changed at any time to reference a new/different collection. 

#### Example:
```csharp
List<int> numbers = {0, 1, 2, 3, 4, 5};

var producer = Factory.CreateProducer(numbers);

producer.Produce();

Console.WriteLine(producer.ResultCollection);
```
Output:
```csharp
ConcurrentQueue<int>(6) { 0, 1, 2, 3, 4, 5 }
```
#### `ConcurrentProducer<out T>` Detailed Overview
##### ConcurrentProducer Factory Overloads
```csharp
IConcurrentProducer<T> CreateProducer(IEnumerable<T> EnumerableObject)
```
##### ConcurrentProducer Public Members
```csharp
IEnumerable<T> Enumerable { get; } // the object the producer should produce items from

IProducerConsumerCollection<T> ResultCollection { get; set; } // the thread safe collection this producer outputs items to

bool Producing { get; } // true when the producer is enumerating the object

event Action Started; // Invoked when the producer first begins enumerating

event Action Finished; // Invoked when the producer just finishes enumerating

event Action CollectionChanged; // Invoked every time the producer outputs an item

void Produce(); // Produce all items from the enumerable synchronously

async Task ProduceAsync(); // produces items from the enumerable asynchronously, use Cancel() to cancel producing items.

async Task ProduceAsync(CancellationToken token); // produces items from the enumerable asynchronously using the provided CancellationToken. To cancel producing items you must call cancel on the original token and not using Cancel();

void Cancel(); // cancels the asynchronous task producing items, if there it one

void Dispose(); // disposes of the currently managed enumerable, if there is one
```

## Consumers
####  `ConcurrentConsumer<in T, out TResult>`
The `ConcurrentConsumer<in T, out TResult>` consumes items from a [thread safe collection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.iproducerconsumercollection-1?view=net-5.0), runs an operation on them to produce a result, and immediately adds it to a thread safe collection. 

The `ConcurrentConsumer<in T, out TResult>`, like all `IConcurrentOutput<TResult>` objects, outputs its items to it's publically accessibly thread safe collection `IProducerConsumerCollection<TResult> ResultCollection { get; set; }` which can be assigned to a new/different collection at any time.

By default the `ConcurrentConsumer<in T, out TResult>` instantiates and outputs to a `ConcurrentQueue<TResult>`.

The `ConcurrentConsumer<in T, out TResult>` is also an `IConcurrentInput<in T>` object. This means that it has a publically accessible collection `IProducerConsumerCollection<T> Collection { get; set; }` that the consumer 'consumes' from. This, by default, has no reference and has a value of `null` and must be assigned to before the consumer can 'consume' items and run operations on them.

The only thing different when using a `ConcurrentConsumer`, is an operation must be assigned. This is in the form of `Func<T,TResult> Operation { get; set; }`. This operation is run on all items that are consumed and outputted to the `ResultCollection`.

#### Example
Say you have a `int` matrix:
```csharp
int[][] matrix = 
{
	new int[]{ 1, 1 },
    new int[]{ 2, 2 },
    new int[]{ 3, 3 },
};
```
further more, say we want to get the `int[]` sums of every matrix
```csharp
var producer = Factory.CreateProducer(matrix);

var consumer = Factory.CreateConsumer<int[],int>(producer);

consumer.Operation = (x) => x[0] + x[1];

producer.Produce();

consumer.Consume();

Console.WriteLine(consumer.ResultCollection);
```
Output:
```csharp
ConcurrentQueue<int>(3) { 2, 4, 6 }
```

#### MultiConcurrentConsumer
The `MultiConcurrentConsumer<in T, out TResult>` is just like the `ConcurrentConsumer<in T, out TResult>`, the only difference is that it consumes from a `ICollection<IProducerConsumerCollection<T>> Collections { get; set; }` instead of a single  `IProducerConsumerCollection<T>`.

#### Example:
```csharp
int[] numbers = { 1, 2, 3 };

var firstProducer = Factory.CreateProducer(numbers);

var secondProducer = Factory.CreateProducer(numbers);

var multiConsumer = Factory.CreateMultiConsumer<int,string>(firstProducer, secondProducer);

multiConsumer.Operation = (x) => $"{x} + {x} = {x << 1}";

await Task.WhenAll(firstProducer.ProduceAsync,secondProducer.ProduceAsync);

Console.WriteLine(multiConsumer.ResultCollection);
```
Output:
```
ConcurrentQueue<string>(6) { "1 + 1 = 2", "2 + 2 = 4", "1 + 1 = 2", "3 + 3 = 6", "2 + 2 = 4", "3 + 3 = 6" }
```
####  ConcurrentWriter
The  `ConcurrentWriter<in T>` is a consumer similar to `ConcurrentConsumer<in T, out TResult>` in that it consumes items from a provided `IProducerConsumerCollection<T> Collection { get; set; }`. 

However, instead of 'consuming' the item and running a operation on the item, the writer writes the item to a file.

#### Example:
```csharp
IEnumerable<object> RandomObject()
{
	yield return null;
    yield return 13;
    yield return "data";
}

var producer = Factory.CreateProducer( RandomObject() );

var writer = Factory.CreateWriter( "C:\Test.txt", producer );

producer.Produce();
```
Output:
<br> C:\Test.txt
```

13
data
```

#### `ConcurrentConsumer<in T,out TResult>` Detailed Overview
##### ConcurrentConsumer Factory Overloads
Creates a default consumer
```csharp
IConcurrentConsumer<T, TResult> CreateConsumer<T, TResult>();
```
Creates a consumer who's `Collection<in T>` is assigned on construction with the provided thread safe collection
```csharp
CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection)
```

Creates a consumer who's `OutputCollection<out TResult>` is assigned on construction with the provided thread safe collection
```csharp
CreateConsumer<T, TResult>(IProducerConsumerCollection<TResult> OutputCollection)
```

Creates a consumer who's `Collection<in T>` and `OutputCollection<out TResult>` is assigned on construction with the provided thread safe collection
```csharp
CreateConsumer<T, TResult>(IProducerConsumerCollection<T> InputCollection, IProducerConsumerCollection<TResult> OutputCollection)
```

Creates a consumer who's `Collection<in T>`  is assigned on construction to the `ResultCollection<out T>` of the `IConcurrentOutput<out T>` object provided (normally a `ConcurrentProducer<>` or `ConcurrentConsumer<>`).
```csharp
CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom)
```

Creates a consumer who's `Collection<in T>`  is assigned on construction to the `ResultCollection<out T>` of the `IConcurrentOutput<out T>` object provided AND `ResultCollection<out TResult>`  is assigned to the `Collection<in TResult>` of the `IConcurrentInput<in T>` object provided (normally a `ConcurrentProducer<>` or `ConcurrentConsumer<>`).
```csharp
CreateConsumer<T, TResult>(IConcurrentOutput<T> ObjectToConsumeFrom, IConcurrentInput<TResult> ObjectToOutputTo)
```

##### ConcurrentConsumer Public Members
```csharp
bool Consuming { get; } // true when consuming items

IProducerConsumerCollection<T> Collection { get; set; } // the collection that the consumer consumes items from

IProducerConsumerCollection<TResult> ResultCollection { get; set; } // the collection that the consumer outputs items to after its done running an operation on the item

public Func<T, TResult> Operation { get; set; } // the operation that should be run on all the items the consumer consumes.

event Action Started; // Invoked when the consumer first begins consuming

event Action Finished; // Invoked when the consumer just finishes consuming

event Action CollectionChanged; // Invoked every time the consumer outputs an item

void Consume(); // consumes items from Collection<T> synchronously

void ConsumeAsync(); // consumes items from Collection<T> Asynchronously, use Cancel(); to cancel consumption

void ConsumeAsync(CancellationToken token); // consumes items from Collection<T> Asynchronously, to cancel consumption you must cancel the original token provided to this object.

void Dispose();
```

#### `MultiConcurrentConsumer<in T, out TResult>` Detailed Overview
##### MultiConcurrentConsumer Factory Overloads

Creates a new default `MultiConcurrentConsumer<in T, out TResult>`
```csharp
 IConcurrentMultiConsumer<T, TResult> CreateMultiConsumer<T, TResult>()
```

Creates a new `MultiConcurrentConsumer<in T, out TResult>` with any number of objects to consume from. The consumer will subscribe to the events of all the  `ObjectsToConsumeFrom` objects and will automatically consume items as they are produced, whether synchronously or asynchronously.

```csharp
 CreateMultiConsumer<T, TResult>(params IConcurrentOutput<T>[] ObjectsToConsumeFrom
```

See `ConcurrentConsumer<in T,out TResult>` Detailed Overview for more information about available methods and public members.

#### `ConcurrentWriter<in T>` Detailed Overview
##### ConcurrentWriter Factory Overloads
Creates a default writer that writes to a path
```csharp
IConcurrentWriter<T> CreateWriter<T>(string Path)
```

Creates a writer that writes to path, and consumes items from `Collection`
```csharp
CreateWriter<T>(string Path, IProducerConsumerCollection<T> Collection)
```

Creates a writer that writes to path, and consumes items from `IConcurrentOutput<T> Producer`. It will consume and write whenever the `Producers` `CollectionChanged` event is invoked.
```csharp
CreateWriter<T>(string Path, IConcurrentOutput<T> Producer)
```

##### ConcurrentWriter Public Members
```csharp
string Path { get; } // the path the writer will write to

Task WriteLinesAsync(); // writes lines asynchronously, use Cancel() to stop writing;

Task WriteLinesAsync(bool append);  // writes lines asynchronously, use Cancel() to stop writing, append is if the writer appends new lines to the end of the file or not

Task WriteLinesAsync(CancellationToken token);// writes lines asynchronously you must call cancel on the original token provided to this object, you can not use Cancel();

Task WriteLinesAsync(bool append, CancellationToken token); // writes lines asynchronously you must call cancel on the original token provided to this object, you can not use Cancel(); append is if the writer appends new lines to the end of the file or not

Task WriteAsync(); // writes asynchronously use Cancel(); to stop writing

Task WriteAsync(bool append); // writes asynchronously use Cancel(); to stop writing append is if the writer appends to the end of the file or not

Task WriteAsync(CancellationToken token); // writes asynchronously you must call cancel on the original token provided to this object, you can not use Cancel();

Task WriteAsync(bool append, CancellationToken token); // writes asynchronously you must call cancel on the original token provided to this object, you can not use Cancel(); append is if the writer appends new lines to the end of the file or not

void WriteLines(); // writes lines synchronously

void WriteLines(bool append); // writes lines synchronously append is if the writer appends new lines to the end of the file or not

void Write(); //writes synchronously

void Write(bool append);  //writes synchronously, append is if the writer appends to the end of the file or not

void Cancel(); // cancels asynchronous writing

void Dispose();
```

# .Net Channels Performance - Benchmarks
## Introduction
In the [this article](https://github.com/paola-homsi/.NetChannels) here we talked about channels, what are they and how to create a channel write to it and read from it. 
In this article let’s talk about the more interesting stuff, their Performance!

## So, What about Performance?
This is the interesting part for me, all of the previous introduction and how to use Channels is interesting and everything but the real question is what about performance?

How much load can we put on these channels? If we’re going to use a bounded channel what is the limit?

As much as .NET channels sound simple, but behind the scenes there is a complicated and pretty efficient written code. 
Let’s check that together and try some benchmarks to see how efficient channels are. I’ve taken the code samples from [this amazing blog](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/) written by Stephen Toub and wanted to try them myself.

In the first benchmark, we’re trying to write then read an integer value 10 millions time, but in here, the write is happening before the read meaning that the read will always have an element to be read in the channel, thus the read will not have to wait and will always complete synchronously.

```
private readonly Channel<int> s_channel = Channel.CreateUnbounded<int>();

[Benchmark]
public async Task WriteThenRead()
{
   ChannelWriter<int> writer = s_channel.Writer;
   ChannelReader<int> reader = s_channel.Reader;
   for (int i = 0; i < 10_000_000; i++)
   {
      writer.TryWrite(i);
      await reader.ReadAsync();
   }
}
```

In the second benchmark, we will try to read then write to our channel, the same 10 millions integer elements. In this benchmark the read is completed asynchronously because by the time we’re reading, there’s still no element written in the channel, so we have to wait till the write is done in order to be able to read an element

```
private readonly Channel<int> s_channel = Channel.CreateUnbounded<int>();

[Benchmark]
public async Task ReadThenWrite()
{
   ChannelWriter<int> writer = s_channel.Writer;
   ChannelReader<int> reader = s_channel.Reader;
   for (int i = 0; i < 10_000_000; i++)
   {
      ValueTask<int> vt = reader.ReadAsync();
      writer.TryWrite(i);
      await vt;
   }
}
```

As shown below in the results table, when reading asynchronously the performance is a bit slower which makes sense because the read takes time till it’s notified that there are new elements written to the channel.
The memory allocation in both cases was 72 Bytes, although this number was varying in different runs but it was close most of the times to this value. Based on these findings I believe that the memory allocations is actually minimal in the .NET channel implementations.

![Integers benchmark results](https://github.com/paola-homsi/.NetChannelsBenchmark/blob/master/NetChannelsBenchmark/assets/firstbenckmark.png)

However, I wanted to go a step further in this benchmark and try to write/read an object instead of just an integer, so I went ahead and created an object with a size of exactly 40 Bytes, then tried the same above benchmarks on it.

```
private readonly Channel<object> s_channel = Channel.CreateUnbounded<object>();
[Benchmark]
public async Task WriteThenRead()
{
   ChannelWriter<object> writer = s_channel.Writer;
   ChannelReader<object> reader = s_channel.Reader;
   for (int i = 0; i < 10_000_000; i++)
   {
      var obj = new
      {
         item1 = "123",
         item2 = "456",
         item3 = "789"
      };
      writer.TryWrite(obj);
      await reader.ReadAsync();
   }
}
```

Then I tried reading the object from the channel before writing to it, to test reading asynchronously

```
private readonly Channel<object> s_channel = Channel.CreateUnbounded<object>();
[Benchmark]
public async Task ReadThenWrite()
{
   ChannelWriter<object> writer = s_channel.Writer;
   ChannelReader<object> reader = s_channel.Reader;
   for (int i = 0; i < 10_000_000; i++)
   {
      var obj = new
      { 
         item1 = "123",
         item2 = "456",
         item3 = "789"
      };
      ValueTask<object> vt = reader.ReadAsync();
      writer.TryWrite(obj);
      await vt;
   }
}
````

The results I found were very interesting! and these numbers were consistent in every time I run the benchmark.

![Objects benchmark results](https://github.com/paola-homsi/.NetChannelsBenchmark/blob/master/NetChannelsBenchmark/assets/secondbenchmark.png)

So performance wise it’s still the same outcome reading asynchronously is a bit slower than reading synchronously.

Now about the memory allocation, as shown in the results table, both benchmarks have allocated exactly 381 MB for the 10M objects, if we did some quick calculations we see that 40 Bytes * 10M is exactly 381.47 MB of memory. cool!

Trying to replicate this results, I tried another object of size 72 Bytes, and I did the calculations before running the benchmarks, as I calculated the memory allocation should be exactly 686.65 MB.

Here’s the benchmark results!

![Objects benchmark results](https://github.com/paola-homsi/.NetChannelsBenchmark/blob/master/NetChannelsBenchmark/assets/thridbenchmark.png)

## Summary
I find these results very interesting because this means that the channels code itself is almost allocation free! and most of the allocation that will happen in our code is very predictable and completely depends on the size of our object.

I hope these findings make it much easier to decide the channel capacity based on the object size, the load of writes rate and the reads performance.





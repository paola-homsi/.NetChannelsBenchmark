﻿using System.Threading.Tasks;
using System.Threading.Channels;
using BenchmarkDotNet.Attributes;

namespace NetChannelsBenchmark
{
    [MemoryDiagnoser]
    public class OjectChannelBenchmark
    {
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

    }
}

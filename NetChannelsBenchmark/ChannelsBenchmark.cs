using System.Threading.Tasks;
using System.Threading.Channels;
using BenchmarkDotNet.Attributes;

namespace NetChannelsBenchmark
{
    [MemoryDiagnoser]
    public class ChannelsBenchmark
    {
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

    }
}

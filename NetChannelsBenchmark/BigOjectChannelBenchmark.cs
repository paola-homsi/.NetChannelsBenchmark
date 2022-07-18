using System.Threading.Tasks;
using System.Threading.Channels;
using BenchmarkDotNet.Attributes;

namespace NetChannelsBenchmark
{
    [MemoryDiagnoser]
    public class BigOjectChannelBenchmark
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
                    item1 = "123123123123123123123123",
                    item2 = "456456456456456456456456",
                    item3 = "789789789789789789789789",
                    item4 = "abcabcabcabcabcabcabcabc",
                    item5 = "defdefdefdefdefdefdefdef",
                    item6 = "zzzzzzzzzzzzzzzzzzzzzzzz",
                    item7 = "uyytuyytuyytuyytuyytuyyt"
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
                    item1 = "123123123123123123123123",
                    item2 = "456456456456456456456456",
                    item3 = "789789789789789789789789",
                    item4 = "abcabcabcabcabcabcabcabc",
                    item5 = "defdefdefdefdefdefdefdef",
                    item6 = "zzzzzzzzzzzzzzzzzzzzzzzz",
                    item7 = "uyytuyytuyytuyytuyytuyyt"
                };
                ValueTask<object> vt = reader.ReadAsync();
                writer.TryWrite(obj);
                await vt;
            }
        }

    }
}

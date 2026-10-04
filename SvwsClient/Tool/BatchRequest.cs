namespace SvwsClient.Tool;

public class BatchRequest
{
    public static async Task<IEnumerable<T>> SendRequestAsync<T>(IEnumerable<Func<Task<T>>> requests, int batchSize = 50)
    {
        var result = new List<T>();
        var numberOfBatches = (int)Math.Ceiling((double)requests.Count() / batchSize);

        for (int i = 0; i < numberOfBatches; i++)
        {
            var requestsForCurrentBatch = requests.Skip(i * batchSize).Take(batchSize);
            var tasks = requestsForCurrentBatch.Select(request => request());
            result.AddRange(await Task.WhenAll(tasks));
        }

        return result;
    }
}
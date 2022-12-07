// See https://aka.ms/new-console-template for more information

using Thready.Lib;

var count = 0;

Console.WriteLine("Hello, Thready!");
var worker = new ThreadyWorker();

await worker.StartAsync(IncreaseCount, TimeSpan.FromSeconds(1));

Console.ReadKey();
await worker.StopAsync();

async Task IncreaseCount()
{
    count++;
    Console.WriteLine(count);
}
using System.Timers;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (object sender, ElapsedEventArgs e) => Console.WriteLine("Elapsed");
        timer.AutoReset = false;
        timer.Start();

        Console.WriteLine("Start sleep");
        Thread.Sleep(2000);
        Console.WriteLine("End sleep");
        timer.Start();

        Console.ReadLine();
    }
}
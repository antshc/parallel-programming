namespace Core.Logger;

public class ConsoleOutputLogger : IOutputLogger
{
    public void Log(string message) => Console.WriteLine(message);
}

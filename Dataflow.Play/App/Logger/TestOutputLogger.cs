using Xunit.Abstractions;

namespace Dataflow.Play.App.Logger;

public interface IOutputLogger
{
    void Log(string message);
}

internal class TestOutputLogger : IOutputLogger
{
    private readonly ITestOutputHelper _output;

    public TestOutputLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Log(string message) => _output.WriteLine(message);
}

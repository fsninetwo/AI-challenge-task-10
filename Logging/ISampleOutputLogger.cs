using AIConsoleApp.Models;
using System.Collections.Generic;

namespace AIConsoleApp.Logging;

public interface ISampleOutputLogger
{
    void Record(string request, IEnumerable<Product> products);
    void RecordNoMatch(string request);
    void Flush();
} 
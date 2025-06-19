namespace AIConsoleApp.Services;
using System.Threading.Tasks;

public interface ITextProcessor
{
    /// <summary>
    /// Sends the user's input to an AI model and returns its response.
    /// </summary>
    /// <param name="input">User prompt.</param>
    /// <returns>AI-generated response.</returns>
    Task<string> ProcessAsync(string input);
} 
using AIConsoleApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIConsoleApp.Services;

public interface IProductSearch
{
    Task<IEnumerable<Product>> SearchAsync(string userQuery);
} 
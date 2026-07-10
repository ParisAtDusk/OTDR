using System.Threading.Tasks;
using Avalonia.Controls;

namespace OTDR.Core.Interfaces;
public interface IFileDialogService
{
    Task<string?> ShowSaveCsvDialogAsync(Window owner);
    Task<string?> ShowOpenCsvDialogAsync(Window owner);
}
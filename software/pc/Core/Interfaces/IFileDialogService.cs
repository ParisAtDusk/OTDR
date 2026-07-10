using System.Threading.Tasks;
using Avalonia.Controls;

public interface IFileDialogService
{
    Task<string?> ShowSaveCsvDialogAsync(Window owner);
    Task<string?> ShowOpenCsvDialogAsync(Window owner);
}
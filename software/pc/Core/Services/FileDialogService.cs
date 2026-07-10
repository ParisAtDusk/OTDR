using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

public class FileDialogService : IFileDialogService
{
    public async Task<string?> ShowSaveCsvDialogAsync(Window owner)
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Export CSV",
                SuggestedFileName = "trace.csv",
                DefaultExtension = "csv",
                FileTypeChoices =
                [
                    new FilePickerFileType("CSV files")
                    {
                        Patterns = ["*.csv"]
                    }
                ]
            });

        return file?.Path.LocalPath;
    }

    public async Task<string?> ShowOpenCsvDialogAsync(Window owner)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open CSV",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("CSV files")
                    {
                        Patterns = ["*.csv"]
                    }
                ]
            });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }
}
public interface ISettingsService
{
    AppSettings Settings { get; }

    Task LoadAsync();
    Task SaveAsync();
}
namespace Classicle
{
    public interface ISettings
    {
        Settings LoadSettings();
        void SaveSettings(Settings settings);
    }
}

namespace ConfigServices.Service
{
    public interface IAppPaths
    {
        string GetPreferredConfigPath();
        string GetProgramDataPath();
    }
}
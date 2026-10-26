namespace DiscoveryDeviceAndroid.Core;

public static class LocalizadorAdb
{
    public static string Localizar(string? caminhoExplicito = null)
    {
        if (!string.IsNullOrWhiteSpace(caminhoExplicito))
        {
            var caminho = Path.GetFullPath(caminhoExplicito.Trim().Trim('"'));
            if (Directory.Exists(caminho)) caminho = Path.Combine(caminho, "adb.exe");
            if (File.Exists(caminho)) return caminho;
            throw new FileNotFoundException("O adb.exe informado não foi encontrado.", caminho);
        }

        foreach (var raiz in new[]
                 {
                     Environment.GetEnvironmentVariable("AndroidSdkDirectory"),
                     Environment.GetEnvironmentVariable("ANDROID_HOME"),
                     Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "android-sdk"),
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "android-sdk")
                 })
        {
            if (string.IsNullOrWhiteSpace(raiz)) continue;
            var candidato = Path.Combine(raiz.Trim('"'), "platform-tools", "adb.exe");
            if (File.Exists(candidato)) return Path.GetFullPath(candidato);
        }

        foreach (var diretorio in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(diretorio)) continue;
            try
            {
                var candidato = Path.Combine(diretorio.Trim('"'), OperatingSystem.IsWindows() ? "adb.exe" : "adb");
                if (File.Exists(candidato)) return Path.GetFullPath(candidato);
            }
            catch (ArgumentException) { }
        }
        throw new FileNotFoundException("ADB não encontrado. Informe o caminho de platform-tools/adb.exe do Android SDK usado pelo Visual Studio.");
    }
}

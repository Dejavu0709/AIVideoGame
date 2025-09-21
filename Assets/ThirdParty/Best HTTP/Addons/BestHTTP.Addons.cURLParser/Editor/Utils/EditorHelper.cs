using System;
using System.IO;
using System.Linq;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public static class EditorHelper
    {
        public static string Folder_Plugin = "BestHTTP.Addons.cURLParser";

        public static string GetPluginFolder()
        {
            // Maybe use UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath, or something from the CompilationPipeline class?
            var current = Directory.GetCurrentDirectory();
            var matchedDirectories = Directory.GetDirectories(current, Folder_Plugin, SearchOption.AllDirectories);
            if (matchedDirectories == null || matchedDirectories.Length == 0)
                throw new Exception("Couldn't find plugin directory!");
            return matchedDirectories.FirstOrDefault();
        }

        public static string GetRelativePluginFolder()
        {
            string absolutePath = GetPluginFolder();
            int idx = absolutePath.IndexOf("Assets");
            return absolutePath.Substring(idx);
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using GinjaGaming.Core.Extensions;

namespace GinjaGaming.Editor
{
    public class CustomiseNewScriptFiles : AssetModificationProcessor
    {

        // Set these to control aspects of the file customisation
        private const bool SkipNamespaceParentsInMenuPath = true;
        private const bool UseFirstParentOnlyInMenuPath = true;

        public static void OnWillCreateAsset(string path)
        {
            // Check to see that the file exists first
            if (!path.EndsWith(".cs.meta"))
            {
                return;
            }
            CustomiseFile(path);
        }

        private static void CustomiseFile(string fullMetaFilePath)
        {
            // Get some basics about the file
            string sourceFilePath = AssetDatabase.GetAssetPathFromTextMetaFilePath(fullMetaFilePath);
            string sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            string fileContents = File.ReadAllText(sourceFilePath);

            // Derive custom properties
            string scriptName = sourceFileName;
            string fileNamespace = GetNamespace(sourceFilePath);
            string scriptFriendlyName = GetScriptFriendlyName(scriptName);
            string editorClass = GetEditorClass(sourceFileName);
            string menuName = GetMenuName(sourceFileName);
            string menuPath = GetMenuPath(fileNamespace, UseFirstParentOnlyInMenuPath, SkipNamespaceParentsInMenuPath);

            // Replace placeholders with custom properties
            fileContents = UpdateFile(fileContents, fileNamespace, scriptName, scriptFriendlyName, menuPath, menuName, editorClass);

            // Write the contents back to the file
            File.WriteAllText(sourceFilePath, fileContents);
            AssetDatabase.Refresh();
        }

        private static string GetNamespace(string fullFilePath)
        {
            // Get the folder only part of the path and clean it up
            string fileNamespace = Path.GetDirectoryName(fullFilePath).Replace('\\', '.');

            // Remove the 'Assets.' prefix and reference to "Scripts."
            fileNamespace = fileNamespace.RemoveString("Assets.").RemoveString("Scripts.");

            // Clean up any white space
            fileNamespace = fileNamespace.RemoveWhiteSpace();

            return fileNamespace;
        }

        private static string GetParentNamespace(string fileNamespace, bool getFirstParentPartOnly, bool skipFirstParent)
        {
            var findIndex = fileNamespace.LastIndexOf(".", StringComparison.Ordinal);
            var newNamespace = fileNamespace.Substring(0, findIndex);

            if (skipFirstParent)
            {
                findIndex = newNamespace.IndexOf(".", StringComparison.Ordinal);
                if (findIndex > 0)
                {
                    newNamespace = newNamespace.Substring(findIndex + 1);
                }
            }

            if (getFirstParentPartOnly)
            {
                findIndex = newNamespace.IndexOf(".", StringComparison.Ordinal);
                if (findIndex > 0)
                {
                    newNamespace = newNamespace.Substring(0, findIndex);
                }
            }

            return newNamespace;
        }

        private static string GetScriptFriendlyName(string scriptName)
        {
            return scriptName.AddSpacesCapitalised();
        }

        private static string GetMenuPath(string fullNamespace, bool getFirstParentOnly, bool skipFirstParent)
        {
            return GetParentNamespace(fullNamespace, getFirstParentOnly, skipFirstParent).AddSpacesCapitalised();
        }

        private static string GetMenuName(string scriptName)
        {
            return scriptName.AddSpacesCapitalised();
        }

        private static string GetEditorClass(string fileName)
        {
            return fileName.RemoveString("Editor");
        }

        private static string UpdateFile(string file, string fullNamespace, string scriptName,
            string scriptFriendlyName, string menuPath, string menuName, string editorClass)
        {
            // Derive and replace the correct Namespace for the file and path
            string newFile = file;
            newFile = newFile.Replace("#NAMESPACE#", fullNamespace);
            newFile = newFile.Replace("#SCRIPTNAME#", scriptName);
            newFile = newFile.Replace("#SCRIPTFRIENDLYNAME#", scriptFriendlyName);
            newFile = newFile.Replace("#MENUNAME#", menuName);
            newFile = newFile.Replace("#MENUPATH#", menuPath);
            newFile = newFile.Replace("#EDITORCLASS#", editorClass);
            newFile = newFile.Replace("#EDITORCLASSINSTANCE#", editorClass.FirstCharToLowerCase());
            return newFile;
        }
    }
}
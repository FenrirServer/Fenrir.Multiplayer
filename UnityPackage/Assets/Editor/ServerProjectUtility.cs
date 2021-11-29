using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

/// <summary>
/// Fenrir server solution utility
/// </summary>
public static class ServerProjectUtility
{
    /// <summary>
    /// Path to the server directory, e.g. ProjectRoot/Server
    /// Should located next to the Library folder
    /// </summary>
    static string _serverDirectoryPath => Path.Combine(Application.dataPath, "../Server");

    static string _serverSolutionFilePath = Path.Combine(_serverDirectoryPath, "ServerApplication.sln");

    const string _editorAsmdefFileName = "Fenrir.Multiplayer.Editor";
    const string _projectTemplateRelativePath = "Templates/ServerSolutionTemplate.zip";

    [MenuItem("Window/Fenrir/Open Server Project")]
    public static void OpenServerProject()
    {
        if(!ServerProjectExists())
        {
            // Show confirmation window to generate a server project
            if(!DisplayCreateServerProjectDialogue())
            {
                return;
            }

            // Generate server project
            GenerateServerProject();
        }

        OpenServerProjectFile();
    }


    static bool ServerProjectExists()
    {
        // Check if server solution directory exists and is not empty
        return Directory.Exists(_serverDirectoryPath) && Directory.GetFiles(_serverDirectoryPath).Length > 0;
    }

    static bool DisplayCreateServerProjectDialogue()
    {
        return EditorUtility.DisplayDialog(
            "Generate Server Project?", 
            "Server project was not found. Would you like to generate one?", 
            "Generate", 
            "Cancel"
            );
    }

    static void GenerateServerProject()
    {
        // Find path to the template archive
        string projectTemplatePath = Path.Combine(Application.dataPath, "../", GetFenrirEditorRelativePath(), _projectTemplateRelativePath);

        if(!File.Exists(projectTemplatePath))
        {
            throw new FileNotFoundException("Failed to find Fenrir server template: " + projectTemplatePath);
        }

        // Unzip into target directory
        ZipFile.ExtractToDirectory(projectTemplatePath, _serverDirectoryPath);

        // Setup template variables
        Dictionary<string, string> templateVariables = new Dictionary<string, string>();
        templateVariables.Add("{APPLICATION_NAME}", GetSanitizedProjectName());
        templateVariables.Add("{SOLUTION_GUID}", Guid.NewGuid().ToString());
        templateVariables.Add("{SERVER_PROJECT_GUID}", Guid.NewGuid().ToString());
        templateVariables.Add("{SHARED_PROJECT_GUID}", Guid.NewGuid().ToString());

        // Replace template variables
        ProcessTemplateDirectory(_serverDirectoryPath, templateVariables);
    }

    static void ProcessTemplateDirectory(string directoryPath, Dictionary<string, string> templateVariables)
    {

        // Process this directory name
        if (directoryPath.Contains("{") && directoryPath.Contains("}"))
        {
            string newDirectoryPath = ProcessTemplateText(directoryPath, templateVariables);
            Directory.Move(directoryPath, newDirectoryPath);
            directoryPath = newDirectoryPath;
        }

        // Process files
        foreach (string fileName in Directory.GetFiles(directoryPath))
        {
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                ProcessTemplateFile(filePath, templateVariables);
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat("Failed to process template file {0}: {1}", filePath, e.ToString());
            }
        }

        // Process nested directories
        foreach (string directoryName in Directory.GetDirectories(directoryPath))
        {
            try
            {
                ProcessTemplateDirectory(directoryName, templateVariables);
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat("Failed to process template directory {0}: {1}", directoryName, e.ToString());
            }
        }
    }

    static void ProcessTemplateFile(string filePath, Dictionary<string, string> templateVariables)
    {
        // Process file name
        if(filePath.Contains("{") && filePath.Contains("}"))
        {
            string newFilePath = ProcessTemplateText(filePath, templateVariables);
            File.Move(filePath, newFilePath);
            filePath = newFilePath;
        }

        // Process file contents
        string text = File.ReadAllText(filePath);
        string newText = ProcessTemplateText(text, templateVariables);
        File.WriteAllText(filePath, newText);
    }

    static string ProcessTemplateText(string text, Dictionary<string, string> templateVariables)
    {
        StringBuilder sb = new StringBuilder(text);

        foreach(KeyValuePair<string, string> kvp in templateVariables)
        {
            sb.Replace(kvp.Key, kvp.Value);
        }

        return sb.ToString();
    }

    static string GetFenrirEditorRelativePath()
    {
        var assetGuids = AssetDatabase.FindAssets($"{_editorAsmdefFileName} t:{nameof(AssemblyDefinitionAsset)}");
        if (assetGuids.Length == 0)
        {
            throw new FileNotFoundException("Failed to find Fenrir editor script root: " + _editorAsmdefFileName);
        }

        return Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(assetGuids[0]));
    }

    static void OpenServerProjectFile()
    {
        InternalEditorUtility.OpenFileAtLineExternal(_serverSolutionFilePath, 1);
    }

    static string GetSanitizedProjectName()
    {
        // Get folder name
        string[] dataPathFolders = Application.dataPath.Split('/');
        string projectName = dataPathFolders[dataPathFolders.Length - 2];

        // Sanitize
        Regex re = new Regex("[^a-zA-Z0-9.]");
        string projectNameSanitized = re.Replace(projectName, "");

        return projectNameSanitized;
    }
}

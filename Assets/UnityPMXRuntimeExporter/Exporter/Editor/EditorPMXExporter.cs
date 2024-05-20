using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityPMXExporter;
using System.Diagnostics;
using Codice.CM.Common.Serialization;


public class EditorPMXExporter : ScriptableWizard
{
    public string ExportPath = "model.pmx";

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(ExportPath)|| ExportPath == "model.pmx")
        {
            ExportPath = $"{Application.dataPath}/Model/model.pmx";
        }
    }

    void OnWizardCreate()
    {
        if (Selection.activeGameObject)
        {
            ModelExporter.ExportModel(Selection.activeGameObject, ExportPath);
            Process.Start(new ProcessStartInfo()
            {
                FileName = ExportPath,
                UseShellExecute = true
            });
        }
    }

    private void OnWizardUpdate()
    {
        isValid = !(string.IsNullOrEmpty(ExportPath) || string.IsNullOrEmpty(Path.GetFileName(ExportPath)));
    }

    [MenuItem("GameObject/Exprot PMX Model", true)]
    private static bool CheckObjectType()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject != null && selectedObject.GetType() == typeof(GameObject))
        {
            return true;
        }
        return false;
    }

    [MenuItem("GameObject/Exprot PMX Model")]
    static void CreateWindow()
    {
        DisplayWizard("Exprot PMX Model", typeof(EditorPMXExporter), "Export");
    }
}
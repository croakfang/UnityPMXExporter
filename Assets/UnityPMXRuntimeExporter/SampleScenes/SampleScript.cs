using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityPMXExporter;

public class SampleScript : MonoBehaviour
{
    public GameObject Target;
    public void ExportModel()
    {
        if (Target)
        {
            var dir = Application.persistentDataPath;
            var path = $"{dir}/model.pmx";
            ModelExporter.ExportModel(Target, path);
            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }
}

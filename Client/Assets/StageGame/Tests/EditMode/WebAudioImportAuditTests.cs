using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace StageGame.Tests
{
    public sealed class WebAudioImportAuditTests
    {
        [Serializable] sealed class ClipReport
        {
            public string source, activeTarget, defaultFormat, webFormat, loadType;
            public int samples, frequency;
            public float peak;
            public string[] artifacts;
        }
        [Serializable] sealed class Report { public List<ClipReport> clips=new List<ClipReport>(); }
        [Test] public void ExportActualImportedAudioArtifactsAndSettings()
        {
            const string directory="Evidence/web-audio-fsb-20261016/unity-import";
            Directory.CreateDirectory(directory);var report=new Report();
            foreach(string name in new[]{"TypingKey.wav","TypingKeyAacSafe.wav","ButtonClick.wav","ButtonClickAacSafe.wav","GoStonePlaceHeavy.wav","EasyLemon.mp3"})
            {
                string source=Editor.ProjectBuilder.Root+"Audio/"+name;
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(source);Assert.NotNull(clip);
                var importer=(AudioImporter)AssetImporter.GetAtPath(source);
                var hash=AssetDatabaseExperimental.LookupArtifact(new ArtifactKey(new GUID(AssetDatabase.AssetPathToGUID(source))));
                Assert.IsTrue(AssetDatabaseExperimental.GetArtifactPaths(hash,out var paths));
                float peak=0;
                if(clip.length<1)
                {
                    var data=new float[clip.samples*clip.channels];Assert.IsTrue(clip.GetData(data,0));peak=data.Max(value=>Mathf.Abs(value));
                }
                int index=0;
                foreach(string path in paths)
                {
                    if(new FileInfo(path).Length<1024*1024)
                        File.Copy(path,Path.Combine(directory,Path.GetFileNameWithoutExtension(name)+"-"+index+Path.GetExtension(path)),true);
                    index++;
                }
                report.clips.Add(new ClipReport{source=source,activeTarget=EditorUserBuildSettings.activeBuildTarget.ToString(),defaultFormat=importer.defaultSampleSettings.compressionFormat.ToString(),webFormat=importer.GetOverrideSampleSettings("WebGL").compressionFormat.ToString(),loadType=importer.GetOverrideSampleSettings("WebGL").loadType.ToString(),samples=clip.samples,frequency=clip.frequency,peak=peak,artifacts=paths});
            }
            File.WriteAllText(Path.Combine(directory,"import-settings.json"),JsonUtility.ToJson(report,true));
        }
    }
}

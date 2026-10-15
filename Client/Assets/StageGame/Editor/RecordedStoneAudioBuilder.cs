using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StageGame.Editor
{
    // Decode the licensed recording with Unity's importer for reproducible PCM trimming.
    public static class RecordedStoneAudioBuilder
    {
        public static void DecodeSource()
        {
            const string source="Archive/Audio/RecordedStone/GoStoneSounds-preview.mp3";
            const string temporary="Assets/StageGame/Audio/__GoStoneSource.mp3";
            const string output="Archive/Audio/RecordedStone/GoStoneSounds-decoded.wav";
            if(File.Exists(temporary)||File.Exists(temporary+".meta"))throw new IOException("Temporary import path already exists.");
            File.Copy(source,temporary);
            try
            {
                AssetDatabase.ImportAsset(temporary,ImportAssetOptions.ForceSynchronousImport);
                var importer=(AudioImporter)AssetImporter.GetAtPath(temporary);
                var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=AudioCompressionFormat.PCM;
                importer.defaultSampleSettings=settings;importer.SaveAndReimport();
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(temporary);
                var samples=new float[clip.samples*clip.channels];
                if(!clip.GetData(samples,0))throw new InvalidOperationException("Could not decode source recording.");
                using(var writer=new BinaryWriter(File.Create(output)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);
                    writer.Write((short)1);writer.Write((short)clip.channels);writer.Write(clip.frequency);
                    writer.Write(clip.frequency*clip.channels*2);writer.Write((short)(clip.channels*2));writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);
                    foreach(float sample in samples)writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample,-1,1)*32767));
                }
                Debug.Log($"[Recorded stone] Decoded {clip.length:F3}s, {clip.frequency}Hz, {clip.channels} channel(s).");
            }
            finally { AssetDatabase.DeleteAsset(temporary); }
        }
    }
}

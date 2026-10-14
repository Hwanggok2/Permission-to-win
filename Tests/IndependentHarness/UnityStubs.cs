// Test doubles only: not Unity serialization, persistence, rendering, or coroutine proof.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace UnityEngine
{
    public sealed class HeaderAttribute : Attribute { public HeaderAttribute(string text) { } }
    public class GameObject
    {
        public bool activeSelf;
        public void SetActive(bool value) { activeSelf = value; }
    }
    public class Component
    {
        public GameObject gameObject = new GameObject();
        public T GetComponentInChildren<T>() where T : new() => new T();
    }
    public sealed class Coroutine
    {
        public IEnumerator Iterator;
        public bool Stopped;
    }
    public class MonoBehaviour : Component
    {
        public readonly List<Coroutine> Coroutines = new List<Coroutine>();
        protected Coroutine StartCoroutine(IEnumerator iterator)
        {
            var coroutine = new Coroutine { Iterator = iterator };
            Coroutines.Add(coroutine);
            if (!iterator.MoveNext()) coroutine.Stopped = true;
            return coroutine;
        }
        protected void StopCoroutine(Coroutine coroutine) { coroutine.Stopped = true; }
        public void AdvanceCoroutines()
        {
            foreach (var coroutine in Coroutines.ToArray())
                if (!coroutine.Stopped && !coroutine.Iterator.MoveNext()) coroutine.Stopped = true;
        }
    }
    public struct Color
    {
        public Color(float r, float g, float b, float a) { }
        public static implicit operator Color(Color32 c) => new Color();
    }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) { } }
    public static class Mathf { public static int Clamp(int n, int low, int high) => Math.Clamp(n, low, high); }
    public enum KeyCode { Escape }
    public static class Input { public static bool GetKeyDown(KeyCode code) => false; }
    public sealed class WaitForSecondsRealtime { public WaitForSecondsRealtime(float time) { } }
    public static class Time { public static float realtimeSinceStartup; }
    public sealed class AudioClip { }
    public sealed class AudioSource { public void PlayOneShot(AudioClip clip, float volume) { } }
    public static class Debug { public static void LogError(object message) { } public static void Log(object message) { } }
    public sealed class TextAsset { public string text; }
    public static class Resources
    {
        public static readonly Dictionary<string, TextAsset> Assets = new Dictionary<string, TextAsset>();
        public static T Load<T>(string name) where T : class => Assets.TryGetValue(name, out var asset) ? asset as T : null;
    }
    public static class JsonUtility
    {
        static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
        public static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
        public static string ToJson(object value) => JsonSerializer.Serialize(value, Options);
    }
    public static class PlayerPrefs
    {
        public static readonly Dictionary<string, string> Values = new Dictionary<string, string>();
        public static bool ThrowOnSave;
        public static bool HasKey(string key) => Values.ContainsKey(key);
        public static string GetString(string key) => Values[key];
        public static void SetString(string key, string value) { Values[key] = value; }
        public static void Save() { if (ThrowOnSave) throw new InvalidOperationException("injected PlayerPrefs.Save failure"); }
        public static void Reset() { Values.Clear(); ThrowOnSave = false; }
    }
}
namespace UnityEngine.UI
{
    public sealed class Text : UnityEngine.Component { public string text; }
    public sealed class Image : UnityEngine.Component { public UnityEngine.Color color; }
    public sealed class Button : UnityEngine.Component
    {
        public bool interactable = true;
        public readonly ButtonEvent onClick = new ButtonEvent();
    }
    public sealed class ButtonEvent
    {
        readonly List<Action> handlers = new List<Action>();
        public void AddListener(Action handler) { handlers.Add(handler); }
        public void Invoke() { foreach (var handler in handlers) handler(); }
    }
}

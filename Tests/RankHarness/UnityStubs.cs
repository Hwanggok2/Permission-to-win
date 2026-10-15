// Rank harness test doubles: not Unity serialization, persistence, rendering, pointer, or native coroutine proof.
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
        public Transform transform = new RectTransform();
        public T GetComponentInChildren<T>() where T : new() => new T();
        public T GetComponent<T>() where T : class, new() => this is T own ? own : transform is T ownTransform ? ownTransform : this is UI.Button button && button.targetGraphic is T graphic ? graphic : new T();
    }
    public class Transform
    {
        public GameObject gameObject = new GameObject();
        public Vector3 localEulerAngles;
        Transform parent;
        readonly List<Transform> children = new List<Transform>();
        public Vector3 localScale = Vector3.one;
        public void SetParent(Transform value) { parent?.children.Remove(this); parent=value; parent?.children.Add(this); }
        public int GetSiblingIndex() => parent == null ? 0 : parent.children.IndexOf(this);
        public void SetSiblingIndex(int index) { if(parent==null)return; parent.children.Remove(this); parent.children.Insert(Math.Clamp(index,0,parent.children.Count),this); }
        public void SetAsLastSibling() { if(parent!=null) SetSiblingIndex(parent.children.Count-1); }
    }
    public class RectTransform : Transform
    {
        public enum Axis { Horizontal, Vertical }
        public Vector2 anchoredPosition, sizeDelta;
        public void SetSizeWithCurrentAnchors(Axis axis,float size) { if(axis==Axis.Horizontal)sizeDelta.x=size;else sizeDelta.y=size; }
    }
    public class CanvasGroup : Component { public float alpha=1; public bool interactable=true, blocksRaycasts=true; }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x=x; this.y=y; }
        public static Vector2 up => new Vector2(0,1);
        public static Vector2 zero => new Vector2(0,0);
        public float sqrMagnitude => x*x+y*y;
        public static float Distance(Vector2 a,Vector2 b) => MathF.Sqrt((a-b).sqrMagnitude);
        public static Vector2 operator -(Vector2 a,Vector2 b) => new Vector2(a.x-b.x,a.y-b.y);
        public static Vector2 operator /(Vector2 a,float b) => new Vector2(a.x/b,a.y/b);
        public static Vector2 operator +(Vector2 a,Vector2 b) => new Vector2(a.x+b.x,a.y+b.y);
        public static Vector2 operator *(Vector2 a,float b) => new Vector2(a.x*b,a.y*b);
    }
    public struct Vector3
    {
        public float x,y,z;
        public Vector3(float x,float y,float z) {this.x=x;this.y=y;this.z=z;}
        public static Vector3 one => new Vector3(1,1,1);
        public static Vector3 operator *(Vector3 a,float b) => new Vector3(a.x*b,a.y*b,a.z*b);
    }
    public sealed class Coroutine
    {
        public IEnumerator Iterator;
        public bool Stopped;
        public readonly Stack<IEnumerator> Stack = new Stack<IEnumerator>();
        public float WaitUntil;
        public void Step()
        {
            if(Stopped || Time.realtimeSinceStartup<WaitUntil)return;
            while(Stack.Count>0)
            {
                var current=Stack.Peek();
                if(!current.MoveNext()){Stack.Pop();continue;}
                if(current.Current is IEnumerator nested){Stack.Push(nested);continue;}
                if(current.Current is WaitForSecondsRealtime wait) WaitUntil=Time.realtimeSinceStartup+wait.Duration;
                return;
            }
            Stopped=true;
        }
    }
    public class MonoBehaviour : Component
    {
        public readonly List<Coroutine> Coroutines = new List<Coroutine>();
        protected Coroutine StartCoroutine(IEnumerator iterator)
        {
            var coroutine = new Coroutine { Iterator = iterator };
            Coroutines.Add(coroutine);
            coroutine.Stack.Push(iterator); coroutine.Step();
            return coroutine;
        }
        protected void StopCoroutine(Coroutine coroutine) { coroutine.Stopped = true; }
        public void AdvanceCoroutines()
        {
            foreach (var coroutine in Coroutines.ToArray())
                coroutine.Step();
        }
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a=1) { this.r=r; this.g=g; this.b=b; this.a=a; }
        public static Color white => new Color(1,1,1,1);
        public static implicit operator Color(Color32 c) => new Color(c.r/255f,c.g/255f,c.b/255f,c.a/255f);
    }
    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r=r; this.g=g; this.b=b; this.a=a; }
    }
    public static class Mathf
    {
        public static int Clamp(int n,int low,int high)=>Math.Clamp(n,low,high);
        public static float Clamp01(float n)=>Math.Clamp(n,0,1);
        public static float Pow(float n,float p)=>MathF.Pow(n,p);
        public static float Lerp(float a,float b,float t)=>a+(b-a)*Clamp01(t);
        public static float SmoothStep(float a,float b,float t) { t=Clamp01(t); return a+(b-a)*t*t*(3-2*t); }
        public static float Atan2(float y,float x)=>MathF.Atan2(y,x);
        public static float Min(float a,float b)=>MathF.Min(a,b);
        public const float Rad2Deg=180f/MathF.PI;
    }
    public enum KeyCode { Escape }
    public static class Input { public static bool EscapePressed; public static bool GetKeyDown(KeyCode code) { bool pressed=EscapePressed;EscapePressed=false;return pressed; } }
    public sealed class WaitForSecondsRealtime { public readonly float Duration; public WaitForSecondsRealtime(float time) { Duration=time; } }
    public static class Time { public static float realtimeSinceStartup; public static float unscaledDeltaTime=.05f; }
    public class ScriptableObject { }
    public sealed class Sprite { public string name; }
    public sealed class AudioClip { }
    public sealed class AudioSource { public void PlayOneShot(AudioClip clip, float volume) { } }
    public static class Debug
    {
        public static readonly List<string> Errors = new List<string>();
        public static void LogError(object message) { Errors.Add(message.ToString()); }
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
    }
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
    public class Graphic : UnityEngine.Component { public UnityEngine.Color color; public UnityEngine.RectTransform rectTransform => (UnityEngine.RectTransform)transform; }
    public sealed class Text : Graphic { public string text; public float preferredWidth=120; } // Fixed test metric; not rendered font geometry.
    public sealed class Image : Graphic { public bool enabled=true; public UnityEngine.Sprite sprite; }
    public sealed class ScrollRect : UnityEngine.Component { public float verticalNormalizedPosition=1; public UnityEngine.RectTransform content=new UnityEngine.RectTransform(); }
    public sealed class Button : UnityEngine.Component
    {
        public bool interactable = true;
        public Graphic targetGraphic = new Image();
        public readonly ButtonEvent onClick = new ButtonEvent();
    }
    public sealed class ButtonEvent
    {
        readonly List<Action> handlers = new List<Action>();
        public void AddListener(Action handler) { handlers.Add(handler); }
        public void Invoke() { foreach (var handler in handlers) handler(); }
    }
}

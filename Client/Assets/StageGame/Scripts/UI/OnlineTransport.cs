using System;
using System.Collections.Concurrent;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#else
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace StageGame
{
    public sealed class OnlineTransport : MonoBehaviour
    {
        public Action<string> Received;
        public Action Opened;
        public Action<int> Closed;
        readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void PTWConnect(string endpoint, string target);
        [DllImport("__Internal")] static extern void PTWSend(string text);
        [DllImport("__Internal")] static extern void PTWClose();
#else
        ClientWebSocket socket;
        CancellationTokenSource cancellation;
        readonly SemaphoreSlim sendLock = new SemaphoreSlim(1);
#endif
        public void Connect(string endpoint)
        {
            Close();
#if UNITY_WEBGL && !UNITY_EDITOR
            PTWConnect(endpoint, gameObject.name);
#else
            socket = new ClientWebSocket(); cancellation = new CancellationTokenSource();
            _ = Receive(socket, endpoint, cancellation.Token);
#endif
        }
        public void Send(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PTWSend(text);
#else
            if (socket != null) _ = SendAsync(socket, text, cancellation.Token);
#endif
        }
        public void Close()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PTWClose();
#else
            cancellation?.Cancel(); socket?.Abort(); socket?.Dispose(); socket = null;
            cancellation?.Dispose(); cancellation = null;
#endif
            while(messages.TryDequeue(out _)) {}
        }
        public void OnNetworkData(string text) { if(text != "pong") messages.Enqueue(text); }
        public void OnNetworkOpen(string unused) => messages.Enqueue("@open");
        public void OnNetworkClose(string code) => messages.Enqueue("@close:" + code);
        void Update()
        {
            while(messages.TryDequeue(out string value))
            {
                if(value == "@open") Opened?.Invoke();
                else if(value.StartsWith("@close:")) Closed?.Invoke(int.TryParse(value.Substring(7),out int code)?code:1006);
                else Received?.Invoke(value);
            }
        }
        void OnDestroy() => Close();
#if !UNITY_WEBGL || UNITY_EDITOR
        async Task Receive(ClientWebSocket current, string endpoint, CancellationToken token)
        {
            try
            {
                await current.ConnectAsync(new Uri(endpoint), token); if(token.IsCancellationRequested)return; messages.Enqueue("@open");
                byte[] buffer = new byte[16384]; var builder = new StringBuilder();
                while(current.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await current.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if(result.MessageType == WebSocketMessageType.Close) { if(!token.IsCancellationRequested)messages.Enqueue("@close:"+(int)(result.CloseStatus??WebSocketCloseStatus.Empty));return; }
                    builder.Append(Encoding.UTF8.GetString(buffer,0,result.Count));
                    if(builder.Length>32768)throw new InvalidOperationException("Oversized message");
                    if(result.EndOfMessage) { if(!token.IsCancellationRequested && builder.ToString()!="pong")messages.Enqueue(builder.ToString());builder.Clear(); }
                }
            }
            catch(Exception) { if(!token.IsCancellationRequested)messages.Enqueue("@close:1006"); }
        }
        async Task SendAsync(ClientWebSocket current, string text, CancellationToken token)
        {
            bool held=false;
            try { await sendLock.WaitAsync(token);held=true;if(current.State==WebSocketState.Open)await current.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)),WebSocketMessageType.Text,true,token); }
            catch(Exception) { if(!token.IsCancellationRequested)messages.Enqueue("@close:1006"); }
            finally { if(held)sendLock.Release(); }
        }
#endif
    }
}

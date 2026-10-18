mergeInto(LibraryManager.library, {
  $PTW: { socket: null, timer: null },
  PTWConnect__deps: ['$PTW'],
  PTWConnect: function (endpointPtr, targetPtr) {
    var target = UTF8ToString(targetPtr), socket = new WebSocket(UTF8ToString(endpointPtr));
    PTW.socket = socket;
    socket.onopen = function () { if(PTW.socket === socket) SendMessage(target, 'OnNetworkOpen', ''); };
    socket.onmessage = function (event) { if(PTW.socket === socket && typeof event.data === 'string' && event.data !== 'pong') SendMessage(target, 'OnNetworkData', event.data); };
    socket.onclose = function (event) { if(PTW.socket === socket) SendMessage(target, 'OnNetworkClose', String(event.code)); };
    PTW.timer = setInterval(function () { if(socket.readyState === 1)socket.send('ping'); }, 20000);
  },
  PTWSend__deps: ['$PTW'],
  PTWSend: function (ptr) { if(PTW.socket && PTW.socket.readyState === 1) PTW.socket.send(UTF8ToString(ptr)); },
  PTWClose__deps: ['$PTW'],
  PTWClose: function () { clearInterval(PTW.timer); var socket=PTW.socket;PTW.socket=null;if(socket)socket.close(1000); },
  PTWShare: function (urlPtr, titlePtr, targetPtr, copyOnly) {
    var url=UTF8ToString(urlPtr), title=UTF8ToString(titlePtr), target=UTF8ToString(targetPtr);
    var copy=function(){ if(!navigator.clipboard) {SendMessage(target,'OnShareResult','failed');return;} navigator.clipboard.writeText(url).then(function(){SendMessage(target,'OnShareResult','copied');},function(){SendMessage(target,'OnShareResult','failed');}); };
    if(!copyOnly && navigator.share) navigator.share({title:title,url:url}).then(function(){SendMessage(target,'OnShareResult','shared');},function(error){if(error.name!=='AbortError')copy();}); else copy();
  }
});

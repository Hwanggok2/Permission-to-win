import base64,json
from pathlib import Path
folder=Path(__file__).resolve().parent
client=folder.parents[1]
clips=[(p.name,p) for p in sorted((folder/'codec').glob('*.m4a'))]
clips += [(name+'-source.wav',client/'Assets/StageGame/Audio'/f'{name}.wav') for name in ('TypingKey','ButtonClick','GoStonePlaceHeavy')]
data=[dict(name=name,data=base64.b64encode(p.read_bytes()).decode()) for name,p in clips]
page='''<!doctype html><meta charset="utf-8"><title>StageGame AAC decode regression</title>
<h1>StageGame AAC decode regression</h1><p>Local codec probe, not a Unity player build. Decodes without playing audio.</p>
<button id="run">Decode all clips</button><pre id="results">Ready</pre><script>
const clips=__DATA__;
document.querySelector('#run').onclick=async()=>{
 const result={userAgent:navigator.userAgent,sampleRate:44100,clips:[]};
 const context=new OfflineAudioContext(1,44100,44100);
 for(const clip of clips){
  try{
   const encoded=Uint8Array.from(atob(clip.data),c=>c.charCodeAt(0));
   const decoded=await context.decodeAudioData(encoded.buffer);
   let peak=0,sum=0,first=-1,last=-1;
   for(let c=0;c<decoded.numberOfChannels;c++){
    const pcm=decoded.getChannelData(c);
    for(let i=0;i<pcm.length;i++){
     const v=Math.abs(pcm[i]);peak=Math.max(peak,v);sum+=v*v;
     if(v>.01){if(first<0||i<first)first=i;last=Math.max(last,i);}
    }
   }
   result.clips.push({name:clip.name,ok:true,frames:decoded.length,seconds:decoded.duration,peak,rms:Math.sqrt(sum/(decoded.length*decoded.numberOfChannels)),firstAudibleMs:first<0?null:first/44.1,lastAudibleMs:last<0?null:last/44.1});
  }catch(error){result.clips.push({name:clip.name,ok:false,error:String(error)});}
 }
 document.querySelector('#results').textContent=JSON.stringify(result,null,2);
};</script>'''
(folder/'codec-probe.html').write_text(page.replace('__DATA__',json.dumps(data)),encoding='utf-8')
print('Wrote browser probe with',len(data),'clips.')

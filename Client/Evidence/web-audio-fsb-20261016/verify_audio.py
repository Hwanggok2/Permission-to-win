"""Verify source preservation and decode actual Unity AAC imports, without a player build."""
import array, hashlib, json, math, subprocess, wave
from pathlib import Path

folder=Path(__file__).resolve().parent
client=folder.parents[1]
ffmpeg=next((client/'Library/AudioCodecVerifier').rglob('ffmpeg*.exe'))
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def pcm(path):
    with wave.open(str(path),'rb') as clip:
        assert clip.getsampwidth()==2
        rate=clip.getframerate();channels=clip.getnchannels();data=array.array('h',clip.readframes(clip.getnframes()))
    return data,rate,channels
def measure(path):
    data,rate,channels=pcm(path)
    signal=[sample/32768 for sample in data]
    return dict(path=path.relative_to(client).as_posix(),sha256=sha(path),seconds=len(data)/rate/channels,peak=max(map(abs,signal)),rms=math.sqrt(sum(x*x for x in signal)/len(signal)),energy=sum(x*x for x in signal),firstAboveOnePercentMs=next((i/rate/channels*1000 for i,x in enumerate(signal) if abs(x)>.01),None))
report={'method':'Unity 6000.3.15f1 Windows actual WebGL AudioImporter AAC .resource decoded by FFmpeg; desktop, not iOS','clips':[]}
before=json.loads((folder/'before.json').read_text())
for entry in before['assets']:
    if entry['path'].startswith('Assets/StageGame/Audio/'):
        assert sha(client/entry['path'])==entry['sha256'],entry['path']
for name in ('TypingKey','ButtonClick','GoStonePlaceHeavy'):
    source=client/'Assets/StageGame/Audio'/f'{name}.wav'
    row={'name':name,'originalSource':measure(source)}
    for suffix in (('', 'AacSafe') if name!='GoStonePlaceHeavy' else ('',)):
        actual=folder/'unity-import'/f'{name}{suffix}-1.resource'
        assert actual.read_bytes()[4:8]==b'ftyp','Expected actual imported AAC/MP4 stream'
        decoded=actual.with_name(f'{name}{suffix}-decoded.wav')
        subprocess.run([str(ffmpeg),'-hide_banner','-loglevel','error','-y','-i',str(actual),'-c:a','pcm_s16le',str(decoded)],check=True)
        row['guardedUnityAAC' if suffix else 'originalUnityAAC']=measure(decoded)
    if name!='GoStonePlaceHeavy':
        guarded=source.with_name(f'{name}AacSafe.wav')
        original,rate,_=pcm(source);data,_,_=pcm(guarded)
        assert data[2048:2048+len(original)]==original
        assert not any(data[:2048]) and not any(data[2048+len(original):])
        assert guarded.read_bytes()==(folder/'codec'/f'{name}-guarded.wav').read_bytes()
        row['guardedSource']=measure(guarded)
        row['guardedPeakRetention']=row['guardedUnityAAC']['peak']/row['originalSource']['peak']
        row['guardedEnergyRetention']=row['guardedUnityAAC']['energy']/row['originalSource']['energy']
        assert row['guardedPeakRetention']>.9
        assert row['guardedEnergyRetention']>.8
    report['clips'].append(row)
report['originalAudioAndMetasUnchanged']=True
report['deviceSafariKakaoValidation']='PENDING new WebGL build and real device retest'
(folder/'codec-report.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report,indent=2))

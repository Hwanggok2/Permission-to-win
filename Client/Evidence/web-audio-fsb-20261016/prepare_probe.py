"""Prepare local codec probes only; no project assets or player builds are changed."""
import base64,hashlib,json,wave
from pathlib import Path
folder=Path(__file__).resolve().parent
client=folder.parents[1]
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def snapshot(paths):return [dict(path=p.relative_to(client).as_posix(),bytes=p.stat().st_size,sha256=sha(p)) for p in paths if p.is_file()]
if not (folder/'before.json').exists():
    (folder/'before.json').write_text(json.dumps(dict(assets=snapshot(sorted((client/'Assets').rglob('*'))),builds=snapshot(sorted((client/'Builds').rglob('*')))),indent=2)+'\n',encoding='utf-8')
for name in ('TypingKey','ButtonClick','GoStonePlaceHeavy'):
    source=client/'Assets/StageGame/Audio'/f'{name}.wav'
    with wave.open(str(source),'rb') as w:
        params=w.getparams();frames=w.readframes(w.getnframes())
    for mode in ('tail','guarded'):
        lead=2048 if mode=='guarded' else 0
        count=max(8820,lead+params.nframes+2048)
        with wave.open(str(folder/'codec'/f'{name}-{mode}.wav'),'wb') as w:
            w.setparams(params)
            w.writeframes(bytes(lead*params.nchannels*params.sampwidth)+frames+bytes((count-lead-params.nframes)*params.nchannels*params.sampwidth))
print('Prepared original-preserving probes with trailing-only or 2048-sample leading guard and trailing silence.')

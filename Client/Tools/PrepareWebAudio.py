"""Preserve originals; make AAC-safe copies of the two short synthesized UI sounds.

Windows Media Foundation's AAC priming can discard the start of short sounds.
2048 leading zero samples protect the attack; a silent tail gives Safari enough
AAC packets to decode. The original audible PCM samples are copied verbatim.
"""
import io
import json
import wave
from pathlib import Path

CLIENT = Path(__file__).resolve().parents[1]
LEADING_SAMPLES = 2048
MINIMUM_SECONDS = .2


def padded(source):
    with wave.open(str(source), 'rb') as clip:
        params = clip.getparams()
        assert params.framerate == 44100 and params.sampwidth == 2 and params.nchannels == 1
        original = clip.readframes(params.nframes)
    total = max(round(params.framerate * MINIMUM_SECONDS), LEADING_SAMPLES + params.nframes + 2048)
    stream = io.BytesIO()
    with wave.open(stream, 'wb') as clip:
        clip.setparams(params)
        clip.writeframes(bytes(LEADING_SAMPLES * 2) + original + bytes((total - LEADING_SAMPLES - params.nframes) * 2))
    return stream.getvalue()


if __name__ == '__main__':
    for name in ('TypingKey', 'ButtonClick'):
        source = CLIENT / 'Assets/StageGame/Audio' / f'{name}.wav'
        target = source.with_name(f'{name}AacSafe.wav')
        data = padded(source)
        if target.exists() and target.read_bytes() != data:
            raise FileExistsError(f'Preserving existing audio: {target}')
        if not target.exists():
            target.write_bytes(data)
        print(json.dumps(dict(path=str(target),bytes=len(data),leadingSamples=LEADING_SAMPLES,seconds=MINIMUM_SECONDS)))

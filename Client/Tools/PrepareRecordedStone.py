"""Trim a single real Go-stone placement; no synthesis, pitch shift, EQ or reverb.

Source: https://freesound.org/people/billhails2/sounds/588317/ (CC0 1.0).
Decode the archived public HQ preview with RecordedStoneAudioBuilder.DecodeSource first.
"""
import hashlib
import io
import json
import math
import struct
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ARCHIVE = ROOT / "Archive/Audio/RecordedStone"
SOURCE_HASH = "b79a29c861d62f14a63551df9c615b075eeb9b51d2f2cf3f9cbe086b14ff2015"


def prepare():
    source = ARCHIVE / "GoStoneSounds-preview.mp3"
    assert hashlib.sha256(source.read_bytes()).hexdigest() == SOURCE_HASH
    decoded = ARCHIVE / "GoStoneSounds-decoded.wav"
    with wave.open(str(decoded), "rb") as audio:
        assert audio.getnchannels() == 1 and audio.getsampwidth() == 2
        rate = audio.getframerate()
        assert rate == 44100
        # Fifth placement has more natural low/mid board resonance than the third.
        start = round(6.221 * rate)
        length = round(.300 * rate)
        audio.setpos(start)
        samples = list(struct.unpack(f"<{length}h", audio.readframes(length)))
    gain = .65 * 32767 / max(abs(value) for value in samples)
    fade = round(.010 * rate)
    values = [round(value * gain * min(1, (length - 1 - i) / fade))
              for i, value in enumerate(samples)]
    stream = io.BytesIO()
    with wave.open(stream, "wb") as audio:
        audio.setnchannels(1)
        audio.setsampwidth(2)
        audio.setframerate(rate)
        audio.writeframes(struct.pack(f"<{length}h", *values))
    output = ROOT / "Assets/StageGame/Audio/GoStonePlaceHeavy.wav"
    data = stream.getvalue()
    if output.exists() and output.read_bytes() != data:
        raise FileExistsError(f"Preserving existing different audio: {output}")
    output.write_bytes(data)
    provenance = {
        "title": "GoStoneSounds.ogg", "author": "billhails2",
        "sourcePage": "https://freesound.org/people/billhails2/sounds/588317/",
        "downloadUrl": "https://cdn.freesound.org/previews/588/588317_12616419-hq.mp3",
        "downloadKind": "Public HQ MP3 preview, not the login-required original OGG",
        "license": "CC0 1.0 Universal",
        "licenseUrl": "https://creativecommons.org/publicdomain/zero/1.0/",
        "verifiedDate": "2026-10-15",
        "recording": "Author describes five real Yunzi stone placements, recorded with a Marantz Professional Umpire microphone. Placements 1-3 use a wooden tabletop board with legs; 4-5 use a thin shin-kaya board.",
        "selection": "Fifth placement, shin-kaya board; stronger natural low/mid resonance",
        "sourceSha256": SOURCE_HASH,
        "decodedSha256": hashlib.sha256(decoded.read_bytes()).hexdigest(),
        "processing": {"startFrame": start, "frames": length, "sampleRate": rate,
                       "gain": gain, "tailFadeFrames": fade, "syntheticLayers": False},
        "asset": output.relative_to(ROOT).as_posix(),
        "assetSha256": hashlib.sha256(data).hexdigest(),
        "durationSeconds": length / rate,
        "peak": max(abs(value) for value in values) / 32768,
        "rms": math.sqrt(sum(value * value for value in values) / length) / 32768,
    }
    (ARCHIVE / "heavy-provenance.json").write_text(json.dumps(provenance, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(provenance, indent=2))


if __name__ == "__main__":
    prepare()

"""Reproducible UI/typing clicks. Stone placement uses a licensed real recording."""
import io
import math
import random
import struct
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RATE = 44100


def impact(seconds, modes, noise_decay, peak, seed):
    rng = random.Random(seed)
    values = []
    smooth = 0.0
    for index in range(round(seconds * RATE)):
        t = index / RATE
        noise = rng.uniform(-1, 1)
        smooth += .35 * (noise - smooth)
        attack = 1 - math.exp(-t / .00025)
        strike = .6 * (noise - smooth) * math.exp(-t / noise_decay)
        body = sum(level * math.sin(2 * math.pi * hz * t) * math.exp(-t / decay)
                   for hz, level, decay in modes)
        tail = min(1, (seconds - t) / .008)
        values.append((strike + body) * attack * tail)
    gain = peak / max(abs(value) for value in values)
    pcm = b"".join(struct.pack("<h", round(value * gain * 32767)) for value in values)
    stream = io.BytesIO()
    with wave.open(stream, "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm)
    return stream.getvalue()


def save(name, data):
    path = ROOT / "Assets/StageGame/Audio" / name
    if path.exists() and path.read_bytes() != data:
        raise FileExistsError(f"Preserving existing audio: {path}")
    if not path.exists():
        path.write_bytes(data)
    print(f"{name}: {len(data)} bytes")


if __name__ == "__main__":
    # A lighter, shorter tick for UI feedback, intentionally below the placement sound.
    save("ButtonClick.wav", impact(.055, [(1250, .7, .006), (2460, .3, .003)],
                                   .0012, .28, 14016))
    # Short key press with a stronger noisy attack; ends before the next 40ms letter tick.
    save("TypingKey.wav", impact(.035, [(880, .25, .008), (1780, .45, .007), (3100, .2, .003)],
                                .005, .55, 14018))

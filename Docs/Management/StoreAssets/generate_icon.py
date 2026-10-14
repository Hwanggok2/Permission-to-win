"""Create original geometric store icon drafts; no external image inputs."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
SIZE = 1024
BG, PANEL, EMPTY, MINT, CORAL = '#0F1B2B', '#1D3048', '#30455F', '#64E6C0', '#FF9586'
circles = []
for row in range(4):
    for col in range(4):
        color = MINT if col == 3 - row else CORAL if row >= 2 and col == 3 else EMPTY
        circles.append((272 + col * 160, 272 + row * 160, color))

svg = [f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {SIZE} {SIZE}">',
       '<title>StageGame original geometric icon draft</title>',
       f'<rect width="1024" height="1024" fill="{BG}"/>',
       f'<rect x="136" y="136" width="752" height="752" rx="120" fill="{PANEL}"/>']
for x, y, color in circles:
    svg.append(f'<circle cx="{x}" cy="{y}" r="57" fill="{color}"/>')
svg.append('</svg>')
(ROOT / 'icon-draft.svg').write_text('\n'.join(svg) + '\n', encoding='utf-8')

# Render the same primitives at 2x for clean antialiased PNG exports.
scale = 2
im = Image.new('RGB', (SIZE * scale, SIZE * scale), BG)
draw = ImageDraw.Draw(im)
draw.rounded_rectangle(tuple(v * scale for v in (136, 136, 888, 888)), radius=120 * scale, fill=PANEL)
for x, y, color in circles:
    draw.ellipse(tuple(v * scale for v in (x-57, y-57, x+57, y+57)), fill=color)
for size in (1024, 512):
    im.resize((size, size), Image.Resampling.LANCZOS).save(ROOT / f'icon-draft-{size}.png')
print('Generated SVG and 1024/512 PNG icon drafts.')

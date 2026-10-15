"""Download only the four reviewed CC BY 4.0 candidates from the artist's site."""
import hashlib
import json
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from urllib.parse import quote
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parent
BASE = 'https://incompetech.com/music/royalty-free/'
CANDIDATES = [
    ('USUAN1200076', 'easy-lemon', '평화로운'),
    ('USUAN1400037', 'carefree', '평화로운 / 밝은'),
    ('USUAN1500073', 'bit-quest', '신나는 / 8비트'),
    ('USUAN1500027', 'dreams-become-real', '잔잔한 / 몽환적인'),
]

def get(url):
    with urlopen(Request(url, headers={'User-Agent': 'Mozilla/5.0'}), timeout=45) as response:
        return response.read(), response.headers.get('Content-Type', '')

catalog = json.loads(get(BASE + 'pieces.json')[0])
selected = []
for isrc, slug, mood in CANDIDATES:
    track = next(item for item in catalog if item['isrc'] == isrc)
    selected.append({**track, 'slug': slug, 'mood_ko': mood,
                     'page': BASE + 'index.html?isrc=' + isrc,
                     'download': BASE + 'mp3-royaltyfree/' + quote(track['filename']),
                     'local_file': 'originals/' + slug + '.mp3'})

(ROOT / 'originals').mkdir(exist_ok=True)
(ROOT / 'evidence').mkdir(exist_ok=True)
with ThreadPoolExecutor(max_workers=4) as pool:
    downloaded = list(pool.map(lambda t: get(t['download']), selected))
for track, (data, content_type) in zip(selected, downloaded):
    if len(data) < 1024 or not ('audio/' in content_type or data[:3] == b'ID3'):
        raise ValueError('Response is not an audio file: ' + track['title'])
    target = ROOT / track['local_file']
    if target.exists() and target.read_bytes() != data:
        raise FileExistsError('Existing candidate differs; preserve it: ' + str(target))
    target.write_bytes(data)
    track.update(bytes=len(data), sha256=hashlib.sha256(data).hexdigest(), content_type=content_type,
                 author='Kevin MacLeod', license='CC BY 4.0',
                 license_url='https://creativecommons.org/licenses/by/4.0/', modifications='None')

sources = [
    ('artist-license-page.html', BASE + 'licenses/'),
    ('artist-faq.html', BASE + 'faq.html'),
    ('track-page-template.html', selected[0]['page']),
    ('cc-by-4.0-legalcode.html', 'https://creativecommons.org/licenses/by/4.0/legalcode.en'),
]
evidence = []
for filename, url in sources:
    data, _ = get(url)
    (ROOT / 'evidence' / filename).write_bytes(data)
    evidence.append({'file': 'evidence/' + filename, 'url': url, 'sha256': hashlib.sha256(data).hexdigest()})

record = {'checked_date': '2026-10-15', 'status': 'Candidates only; not integrated into game',
          'catalog_source': BASE + 'pieces.json', 'tracks': selected, 'evidence': evidence,
          'validation': 'Downloaded and hashed. Human listening, seamless looping and Unity playback not verified.'}
(ROOT / 'candidates.json').write_text(json.dumps(record, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'tracks': len(selected), 'total_bytes': sum(t['bytes'] for t in selected)}, ensure_ascii=True))

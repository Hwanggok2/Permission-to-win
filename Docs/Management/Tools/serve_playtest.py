"""Serve an isolated Unity WebGL snapshot for a temporary external playtest."""
import argparse
import gzip
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import mimetypes
from pathlib import Path
import shutil
from urllib.parse import unquote, urlsplit


def prepare(root, cache):
    allowed = {'.html', '.js', '.css', '.data', '.wasm', '.unityweb', '.json',
               '.png', '.jpg', '.jpeg', '.svg', '.ico', '.txt', '.mp3', '.ogg', '.wav'}
    compressible = {'.html', '.js', '.css', '.data', '.wasm', '.json', '.svg', '.txt'}
    files = {}
    for path in root.rglob('*'):
        if not path.is_file() or path.is_symlink() or not path.resolve().is_relative_to(root):
            continue
        relative = path.relative_to(root)
        if path.suffix.lower() not in allowed or any(part.startswith('.') for part in relative.parts):
            continue
        content_type = {'.wasm': 'application/wasm', '.data': 'application/octet-stream',
                        '.unityweb': 'application/octet-stream', '.js': 'application/javascript'}.get(path.suffix.lower())
        content_type = content_type or mimetypes.guess_type(str(path))[0] or 'application/octet-stream'
        compressed = None
        if path.suffix.lower() in compressible and path.stat().st_size > 1024:
            compressed = cache / (relative.as_posix() + '.gz')
            compressed.parent.mkdir(parents=True, exist_ok=True)
            with path.open('rb') as source, compressed.open('wb') as target:
                with gzip.GzipFile(filename='', mode='wb', fileobj=target, compresslevel=6, mtime=0) as zipped:
                    shutil.copyfileobj(source, zipped)
        files['/' + relative.as_posix()] = (path, compressed, content_type)
    if '/index.html' not in files:
        raise RuntimeError('Snapshot must contain index.html')
    return files


def handler_for(files):
    class Handler(BaseHTTPRequestHandler):
        server_version = 'StageGamePlaytest'

        def do_GET(self):
            self.serve(False)

        def do_HEAD(self):
            self.serve(True)

        def serve(self, head):
            request = unquote(urlsplit(self.path).path)
            record = files.get('/index.html' if request == '/' else request)
            if record is None:
                self.send_error(404)
                return
            original, compressed, content_type = record
            use_gzip = compressed is not None and 'gzip' in self.headers.get('Accept-Encoding', '').lower()
            source = compressed if use_gzip else original
            self.send_response(200)
            self.send_header('Content-Type', content_type)
            self.send_header('Content-Length', str(source.stat().st_size))
            self.send_header('Cache-Control', 'no-store')
            self.send_header('Vary', 'Accept-Encoding')
            self.send_header('X-Content-Type-Options', 'nosniff')
            self.send_header('X-Robots-Tag', 'noindex, nofollow, noarchive')
            self.send_header('Referrer-Policy', 'no-referrer')
            if use_gzip:
                self.send_header('Content-Encoding', 'gzip')
            self.end_headers()
            if not head:
                try:
                    with source.open('rb') as stream:
                        shutil.copyfileobj(stream, self.wfile)
                except (BrokenPipeError, ConnectionResetError):
                    pass

        def log_message(self, format, *args):
            # Keep visitor addresses and URLs out of persistent test logs.
            pass
    return Handler


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, required=True)
    parser.add_argument('--cache', type=Path, required=True)
    parser.add_argument('--port', type=int, default=8772)
    args = parser.parse_args()
    root, cache = args.root.resolve(strict=True), args.cache.resolve()
    if cache.is_relative_to(root):
        raise ValueError('Compression cache must be outside the served snapshot')
    files = prepare(root, cache)
    server = ThreadingHTTPServer(('127.0.0.1', args.port), handler_for(files))
    print(f'Playtest ready on 127.0.0.1:{args.port}; serving {len(files)} snapshot files', flush=True)
    server.serve_forever()


if __name__ == '__main__':
    main()

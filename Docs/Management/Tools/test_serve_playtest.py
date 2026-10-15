import gzip
import http.client
from pathlib import Path
import tempfile
import threading
import unittest
from http.server import ThreadingHTTPServer

from serve_playtest import handler_for, prepare


class PublicSnapshotTests(unittest.TestCase):
    def test_only_snapshot_files_are_served_and_gzip_preserves_bytes(self):
        temporary_root = Path(__file__).resolve().parents[3] / '.work'
        temporary_root.mkdir(exist_ok=True)
        with tempfile.TemporaryDirectory(prefix='stagegame-playtest-', dir=temporary_root) as temporary:
            base = Path(temporary)
            assert base.resolve().is_relative_to(temporary_root.resolve())
            root = base / 'public'
            root.mkdir()
            (root / 'index.html').write_text('<title>4목</title>', encoding='utf-8')
            payload = b'\0asm' + b'game-only' * 1000
            (root / 'game.wasm').write_bytes(payload)
            (root / '.env').write_text('not-public', encoding='utf-8')
            (root / 'source.cs').write_text('not-public', encoding='utf-8')
            (base / 'private.txt').write_text('not-public', encoding='utf-8')
            server = ThreadingHTTPServer(('127.0.0.1', 0), handler_for(prepare(root, base / 'cache')))
            worker = threading.Thread(target=server.serve_forever, daemon=True)
            worker.start()
            connection = http.client.HTTPConnection('127.0.0.1', server.server_port)
            try:
                connection.request('GET', '/game.wasm', headers={'Accept-Encoding': 'gzip'})
                response = connection.getresponse()
                self.assertEqual(200, response.status)
                self.assertEqual('application/wasm', response.getheader('Content-Type'))
                self.assertEqual('gzip', response.getheader('Content-Encoding'))
                self.assertEqual(payload, gzip.decompress(response.read()))
                for request in ('/.env', '/source.cs', '/../private.txt', '/%2e%2e/private.txt', '/cache/'):
                    connection.request('GET', request)
                    response = connection.getresponse()
                    self.assertEqual(404, response.status, request)
                    response.read()
                connection.request('GET', '/')
                response = connection.getresponse()
                self.assertEqual(200, response.status)
                self.assertIn('noindex', response.getheader('X-Robots-Tag'))
                response.read()
            finally:
                connection.close()
                server.shutdown()
                server.server_close()
                worker.join()


if __name__ == '__main__':
    unittest.main()

"""Local shell check: current template/adapter with the EXISTING WebGL binary; no rebuild or deploy."""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import re

client = Path(__file__).resolve().parents[2]
template = client / "Assets/WebGLTemplates/StageGamePortrait"
build = client / "Builds/WebGL"

class Preview(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(build), **kwargs)

    def do_GET(self):
        route = self.path.split("?", 1)[0]
        if route not in ("/", "/index.html", "/host-layout.js", "/ui-text.js"):
            return super().do_GET()
        name = "index.html" if route == "/" else route[1:]
        body = (template / name).read_text(encoding="utf-8")
        if name == "index.html":
            old = (build / "index.html").read_text(encoding="utf-8")
            replacements = {"PRODUCT_NAME": "4목", "LOADER_FILENAME": "WebGL.loader.js", "DATA_FILENAME": "WebGL.data", "FRAMEWORK_FILENAME": "WebGL.framework.js", "CODE_FILENAME": "WebGL.wasm"}
            for source, field in (("COMPANY_NAME", "companyName"), ("PRODUCT_NAME", "productName"), ("PRODUCT_VERSION", "productVersion")):
                replacements[f"JSON.stringify({source})"] = re.search(rf"{field}:\s*(\"[^\"]*\")", old).group(1)
            body = re.sub(r"\{\{\{\s*(.*?)\s*\}\}\}", lambda m: replacements[m.group(1)], body)
        data = body.encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8" if name.endswith("html") else "application/javascript; charset=utf-8")
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(data)

ThreadingHTTPServer(("127.0.0.1", 8767), Preview).serve_forever()

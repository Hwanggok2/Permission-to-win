"""Package local license notices and the official downloaded UCL page without modifying source packages."""
from html.parser import HTMLParser
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
NOTICES = ROOT / "Assets/StreamingAssets/Notices"


class VisibleText(HTMLParser):
    def __init__(self):
        super().__init__()
        self.hidden = 0
        self.parts = []

    def handle_starttag(self, tag, attrs):
        if tag in ("script", "style"):
            self.hidden += 1

    def handle_endtag(self, tag):
        if tag in ("script", "style"):
            self.hidden -= 1
        if tag in ("p", "div", "li", "h1", "h2", "h3", "br"):
            self.parts.append("\n")

    def handle_data(self, data):
        if not self.hidden:
            self.parts.append(data)


parser = VisibleText()
parser.feed((ROOT / "Logs/unity-companion-license.html").read_text(encoding="utf-8"))
text = "".join(parser.parts)
begin = text.rindex('Unity Companion License ("License")')
end = text.index("(10.29.2024)", begin) + len("(10.29.2024)")
license_text = text[begin:end].strip()
assert "v1.4" in license_text and "10. Governing Law and Venue" in license_text
(NOTICES / "Unity-Companion-License-Full.txt").write_text(
    "Source: https://unity.com/legal/licenses/unity-companion-license\nRetrieved: 2026-10-14\n\n" + license_text + "\n", encoding="utf-8")

parts = ["StageGame prototype - Third-party notices\nMade with Unity. Unity is a trademark of Unity Technologies.\n\n"
         "Noto Sans CJK KR: Adobe copyright and SIL OFL 1.1.\nUnity uGUI: Unity Companion License.\n"
         "Test tooling notices are retained for development distribution; they do not imply test assemblies ship in the player.\n"]
font_terms = None
for path in sorted(NOTICES.glob("*-OFL.txt")):
    if path.name == "NotoSansKR-OFL.txt":
        continue
    notice = path.read_text(encoding="utf-8")
    preamble = notice.index("PREAMBLE")
    terms = notice[preamble:]
    if font_terms is not None and "".join(font_terms.split()) != "".join(terms.split()):
        raise ValueError(f"Font license terms differ; do not consolidate: {path}")
    font_terms = terms
    parts.append("\n--- " + path.name + " ---\n" + notice[:preamble])
if font_terms is not None:
    parts.append("\n--- Shared SIL Open Font License 1.1 terms for the fonts above ---\n" + font_terms)
for path in sorted(NOTICES.glob("*.txt")):
    # Font copyright/RFN headers and their identical shared terms are already included above.
    if path.name != "SOURCES.txt" and not path.name.endswith("-OFL.txt"):
        parts.append("\n--- " + path.name + " ---\n" + path.read_text(encoding="utf-8"))
(ROOT / "Assets/StageGame/Resources/LegalNotices.txt").write_text("\n".join(parts), encoding="utf-8")
print("Prepared notice text:", len("\n".join(parts)), "characters")

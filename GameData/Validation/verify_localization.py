"""Read-only source/consumer agreement checks, with a preservation snapshot option."""
import argparse
import hashlib
import json
from pathlib import Path
import sys
from xml.etree import ElementTree as ET

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
import export_data as data


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def cell_signature(cell):
    return (cell.value, cell.data_type, cell.number_format,
            *(ET.tostring(part.to_tree()) for part in
              (cell.font, cell.fill, cell.border, cell.alignment, cell.protection)))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--before', type=Path, help='optional untouched pre-edit workbook')
    parser.add_argument('--report', type=Path, default=ROOT / 'Validation/localization-results.json',
                        help='preservation evidence destination; used only with --before')
    args = parser.parse_args()
    request_path = ROOT.parent / 'Client/LOCALIZATION-REQUEST.json'
    request = json.loads(request_path.read_text(encoding='utf-8'))
    revision_path = ROOT / 'Validation/approved-string-revisions.json'
    revision_rows = json.loads(revision_path.read_text(encoding='utf-8'))['revisions']
    revisions = {r['key']: r for r in revision_rows}
    assert len(revisions) == len(revision_rows)
    expected = data.serialize(data.validate(data.read_source(data.SOURCE)))
    for name, content in expected.items():
        assert (data.GENERATED / name).read_bytes() == content, f'stale {name}'
    entries = json.loads(expected['strings.json'])['strings']
    by_key = {r['key']: r for r in entries}
    assert len(by_key) == len(entries)
    for key, revision in revisions.items():
        assert by_key[key] == {'key': key, **revision['after']}, f'approved copy mismatch: {key}'
    assert len({r['key'] for r in request['strings']}) == len(request['strings'])
    for row in request['strings']:
        actual = by_key[row['key']]
        for locale in ('ko', 'en'):
            revision = revisions.get(row['key'])
            if revision:
                assert row[locale] in (revision['before'][locale], revision['after'][locale]), row['key']
            else:
                assert actual[locale] == row[locale], f"copy mismatch: {row['key']}/{locale}"
            assert len(data.format_indices(actual[locale], row['key'])) == row['formatArgCount']
    # These unchanged runtime bytes also cover source stage/ad value preservation.
    stable = {
        'stages.json': '2d04b124a42f0f3798ad942e9aacab97ee34a25f05b380144cd70f75c3904992',
        'ads.json': 'd19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a',
    }
    for name, digest in stable.items():
        assert sha(data.GENERATED / name) == digest, f'unrelated content changed: {name}'
    preservation = None
    if args.before:
        old, new = load_workbook(args.before), load_workbook(data.SOURCE)
        try:
            assert old.sheetnames == new.sheetnames
            changed_cells = []
            for name in old.sheetnames:
                a, b = old[name], new[name]
                if name != 'Strings':
                    assert a.max_row == b.max_row and a.max_column == b.max_column
                assert a.freeze_panes == b.freeze_panes
                assert a.sheet_view.showGridLines == b.sheet_view.showGridLines
                assert ET.tostring(a.auto_filter.to_tree()) == ET.tostring(b.auto_filter.to_tree())
                assert list(a.merged_cells.ranges) == list(b.merged_cells.ranges)
                for row in a:
                    for cell in row:
                        target = b[cell.coordinate]
                        revision = revisions.get(a.cell(cell.row, 2).value) if name == 'Strings' else None
                        locale = {3: 'ko', 5: 'en'}.get(cell.column)
                        if revision and locale and cell.value != target.value:
                            assert cell.value == revision['before'][locale], (name, cell.coordinate, 'before copy')
                            assert target.value == revision['after'][locale], (name, cell.coordinate, 'after copy')
                            assert cell_signature(cell)[1:] == cell_signature(target)[1:], (name, cell.coordinate, 'style')
                            changed_cells.append(f'{name}!{cell.coordinate}')
                        else:
                            assert cell_signature(cell) == cell_signature(target), (name, cell.coordinate)
                for key, dim in a.row_dimensions.items():
                    assert dim.height == b.row_dimensions[key].height, (name, key, 'row height')
                for key, dim in a.column_dimensions.items():
                    assert dim.width == b.column_dimensions[key].width, (name, key, 'column width')
                for validation in a.data_validations.dataValidation:
                    # Export may group ranges; test effective rules for every original cell.
                    for row in a:
                        for cell in row:
                            if cell.coordinate in validation:
                                assert any(cell.coordinate in v and v.type == validation.type
                                           and v.formula1 == validation.formula1 and v.formula2 == validation.formula2
                                           for v in b.data_validations.dataValidation), (name, cell.coordinate, 'validation')
            preservation = {'beforeSha256': sha(args.before), 'unchangedSheets': ['Stages', 'AdConfig'],
                            'comparedStringsRange': old['Strings'].calculate_dimension(),
                            'approvedChangedCells': changed_cells,
                            'otherValuesStylesDimensionsPanesValidation': 'passed'}
        finally:
            old.close()
            new.close()
    report = {'sourceSha256': sha(data.SOURCE), 'clientRequestSha256': sha(request_path),
              'approvedRevisionsSha256': sha(revision_path), 'approvedRevisedKeys': sorted(revisions),
              'strings': len(entries), 'clientRequestedKeys': len(request['strings']),
              'locales': ['ko', 'en'], 'clientCopyAndFormatArgs': 'passed',
              'stableRuntimeHashes': stable, 'stringsSha256': sha(data.GENERATED / 'strings.json'),
              'preservation': preservation}
    # Only overwrite the evidence file when the full preservation comparison ran.
    if args.before:
        args.report.write_text(
            json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()

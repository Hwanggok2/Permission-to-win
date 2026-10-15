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
    rank_path = ROOT / 'Validation/rank-ui-changes.json'
    rank_change = json.loads(rank_path.read_text(encoding='utf-8'))
    revision_rows += rank_change['revisions']
    v9_path = ROOT / 'Validation/v9-ui-changes.json'
    v9_change = json.loads(v9_path.read_text(encoding='utf-8'))
    revision_rows += v9_change['revisions']
    story_path = ROOT / 'Validation/story-changes.json'
    story_change = json.loads(story_path.read_text(encoding='utf-8'))
    revision_rows += story_change['revisions']
    title_path = ROOT / 'Validation/title-v10-changes.json'
    title_change = json.loads(title_path.read_text(encoding='utf-8'))
    revision_rows += title_change['revisions']
    stage_revisions = {r['id']: r for r in story_change['stageRevisions']}
    revisions, histories = {}, {}
    for revision in revision_rows:
        key = revision['key']
        if key in revisions:
            assert revisions[key]['after'] == revision['before'], f'broken revision chain: {key}'
            histories[key].append(revision['after'])
        else:
            histories[key] = [revision['before'], revision['after']]
        revisions[key] = revision
    expected = data.serialize(data.validate(data.read_source(data.SOURCE)))
    for name, content in expected.items():
        assert (data.GENERATED / name).read_bytes() == content, f'stale {name}'
    entries = json.loads(expected['strings.json'])['strings']
    by_key = {r['key']: r for r in entries}
    assert len(by_key) == len(entries)
    for key, revision in revisions.items():
        assert by_key[key] == {'key': key, **revision['after']}, f'approved copy mismatch: {key}'
    for addition in rank_change['additions'] + v9_change['additions'] + story_change['additions']:
        revision = revisions.get(addition['key'])
        approved = {'key': addition['key'], **revision['after']} if revision else addition
        if revision:
            assert {locale: addition[locale] for locale in ('ko', 'en')} in histories[addition['key']]
        assert by_key[addition['key']] == approved, f"new copy mismatch: {addition['key']}"
    assert len({r['key'] for r in request['strings']}) == len(request['strings'])
    for row in request['strings']:
        actual = by_key[row['key']]
        for locale in ('ko', 'en'):
            revision = revisions.get(row['key'])
            if revision:
                assert row[locale] in [v[locale] for v in histories[row['key']]], row['key']
            else:
                assert actual[locale] == row[locale], f"copy mismatch: {row['key']}/{locale}"
            assert len(data.format_indices(actual[locale], row['key'])) == row['formatArgCount']
    # New presentation fields are additive; all eight previous stage fields are unchanged.
    stages = json.loads(expected['stages.json'])['stages']
    assert len(stages) == len(rank_change['previousStageFields']) == 30
    for old_stage, new_stage in zip(rank_change['previousStageFields'], stages):
        assert old_stage == {key: new_stage[key] for key in old_stage}, old_stage['id']
    assert len(story_change['previousStages']) == len(stages)
    for old_stage, new_stage in zip(story_change['previousStages'], stages):
        approved_stage = dict(old_stage)
        revision = stage_revisions.get(old_stage['id'])
        if revision:
            assert {key: old_stage[key] for key in revision['before']} == revision['before']
            approved_stage.update(revision['after'])
        assert approved_stage == new_stage, old_stage['id']
    stable = {
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
                if name == 'Stages':
                    assert a.max_row == b.max_row and a.max_column in (11, 17) and b.max_column == 17
                elif name != 'Strings':
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
                        stage_revision = stage_revisions.get(a.cell(cell.row, 2).value) if name == 'Stages' else None
                        field = {15: 'aiIntroKey', 16: 'aiWinKey', 17: 'aiLoseKey'}.get(cell.column)
                        if stage_revision and field and cell.value != target.value:
                            assert cell.value == stage_revision['before'][field], (name, cell.coordinate, 'before reference')
                            assert target.value == stage_revision['after'][field], (name, cell.coordinate, 'after reference')
                            assert cell_signature(cell)[1:] == cell_signature(target)[1:], (name, cell.coordinate, 'style')
                            changed_cells.append(f'{name}!{cell.coordinate}')
                        elif revision and locale and cell.value != target.value:
                            assert cell.value in [v[locale] for v in histories[revision['key']]], (name, cell.coordinate, 'before copy')
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
            preservation = {'beforeSha256': sha(args.before), 'unchangedSheets': ['AdConfig'],
                            'comparedStagesRange': old['Stages'].calculate_dimension(),
                            'comparedStringsRange': old['Strings'].calculate_dimension(),
                            'approvedChangedCells': changed_cells,
                            'otherValuesStylesDimensionsPanesValidation': 'passed'}
        finally:
            old.close()
            new.close()
    report = {'sourceSha256': sha(data.SOURCE), 'clientRequestSha256': sha(request_path),
              'approvedRevisionsSha256': sha(revision_path), 'approvedRevisedKeys': sorted(revisions),
              'rankChangeSha256': sha(rank_path), 'legacyStageFieldsUnchanged': True,
              'stagesSha256': sha(data.GENERATED / 'stages.json'),
              'rankUiAddedStrings': len(rank_change['additions']),
              'rankUiRevisedStrings': len(rank_change['revisions']),
              'v9ChangeSha256': sha(v9_path), 'v9AddedStrings': len(v9_change['additions']),
              'v9RevisedStrings': len(v9_change['revisions']),
              'storyChangeSha256': sha(story_path), 'storyAddedStrings': len(story_change['additions']),
              'storyRevisedStrings': len(story_change['revisions']),
              'titleChangeSha256': sha(title_path), 'titleRevisedStrings': len(title_change['revisions']),
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

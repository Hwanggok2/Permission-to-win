"""Read-only check of the artifact-tool-authored opening/role revision."""
from pathlib import Path
import hashlib, json, openpyxl

root = Path(__file__).resolve().parents[2]
evidence = root/'Client/Evidence/opening-ranks-20261015'
before = openpyxl.load_workbook(evidence/'GameData-before.xlsx')
after = openpyxl.load_workbook(evidence/'GameData-candidate.xlsx')
assert before.sheetnames == after.sheetnames
changes = []
for old, new in zip(before, after):
    assert old.max_row == new.max_row and old.max_column == new.max_column
    assert str(old.merged_cells) == str(new.merged_cells)
    assert str(old.data_validations) == str(new.data_validations)
    for row in old:
        for cell in row:
            other = new[cell.coordinate]
            assert cell._style == other._style, (old.title,cell.coordinate,'style')
            if cell.value != other.value:
                allowed = (old.title == 'Stages' and cell.column in (8,9,10) and 5 <= cell.row <= 34) or (old.title == 'Strings' and cell.column in (3,5) and cell.row in (291,292,406))
                assert allowed, (old.title,cell.coordinate,cell.value,other.value)
                changes.append({'sheet':old.title,'cell':cell.coordinate,'before':cell.value,'after':other.value})
assert after['Strings']['B291'].value == 'story_opponent_005'
assert after['Strings']['B406'].value == 'story_opponent_028'
assert sum(not after['Stages'].cell(row,9).value for row in range(5,35)) == 24
assert sum(after['Stages'].cell(row,8).value == -1 for row in range(5,35)) == 8
(evidence/'workbook-changes.json').write_text(json.dumps({'changes':changes,'styleAndValidationPreserved':True},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'changedCells':len(changes),'emptyBoards':24,'aiStarts':8,'stylesPreserved':True}))

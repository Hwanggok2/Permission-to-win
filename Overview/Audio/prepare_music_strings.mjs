import fs from 'node:fs/promises';
import { FileBlob, SpreadsheetFile } from '@oai/artifact-tool';

const root = 'C:/Github/StageGame';
const evidence = root + '/Overview/Audio/evidence/integration-20261015';
await fs.mkdir(evidence, { recursive: true });
const source = root + '/GameData/GameData_MVP.xlsx';
const apply = process.argv.includes('--apply');
const verify = process.argv.includes('--verify');
const workbook = await SpreadsheetFile.importXlsx(await FileBlob.load(source));
const strings = workbook.worksheets.getItem('Strings');
if (apply) {
  if (strings.getRange('B423').values[0][0]) throw new Error('Music rows already exist; use --verify.');
  await fs.copyFile(source, evidence + '/workbook-before.xlsx');
  strings.getRange('A423:E423').copyFrom(strings.getRange('A422:E422'), 'all');
  strings.getRange('A424:E424').copyFrom(strings.getRange('A422:E422'), 'all');
  strings.getRange('A423:E424').values = [
    ['USE', 'settings_music_on', '배경음악 켜짐', '설정의 독립 배경음악 상태', 'Music on'],
    ['USE', 'settings_music_off', '배경음악 꺼짐', '설정의 독립 배경음악 상태', 'Music off'],
  ];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'table', range:'Strings!A423:E424', include:'values,formulas', tableMaxRows:2, tableMaxCols:5, maxChars:1500})).ndjson);
  console.log((await workbook.inspect({kind:'match', searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!', options:{useRegex:true,maxResults:10}, maxChars:1000})).ndjson);
  await (await SpreadsheetFile.exportXlsx(workbook)).save(evidence + '/workbook-candidate.xlsx');
}
const preview = await workbook.render({sheetName:'Strings', range:apply || verify ? 'A421:E424' : 'A420:E422', scale:1, format:'png'});
await fs.writeFile(evidence + `/strings-${verify ? 'final' : apply ? 'after' : 'before'}.png`, new Uint8Array(await preview.arrayBuffer()));

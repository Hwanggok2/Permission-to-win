import fs from 'node:fs/promises';
import { FileBlob, SpreadsheetFile } from '@oai/artifact-tool';
const work = 'C:/Github/StageGame/GameData/Validation/multiplayer-feedback-20261019';
const wb = await SpreadsheetFile.importXlsx(await FileBlob.load(work + '/before.xlsx'));
const sheet = wb.worksheets.getItem('Strings');
const values = sheet.getRange('A1:E800').values;
const end = values.findLastIndex(row => row.some(x => x !== null && x !== '')) + 1;
const render = async (name,range) => {
  const image = await wb.render({sheetName:'Strings',range,scale:1});
  await fs.writeFile(`${work}/${name}.png`,new Uint8Array(await image.arrayBuffer()));
};
if (process.argv.includes('--preview')) {
  console.log((await wb.inspect({kind:'workbook,sheet,table',maxChars:3000,tableMaxRows:4,tableMaxCols:5})).ndjson);
  console.log(JSON.stringify({end,lastRows:values.slice(end-5,end)}));
  await render('before',`B${end-4}:E${end}`);
  process.exit(0);
}
const changes = [
  ['mp_outcome_win','승리!','You win!'],
  ['mp_outcome_loss','패배','You lose'],
  ['mp_outcome_draw','무승부','Draw'],
  ['mp_outcome_cancelled','경기 무효','Match cancelled'],
  ['mp_rating_result','Rating {0}  ({1})','Rating {0}  ({1})'],
  ['mp_rating_unchanged','레이팅 변동 없음 · {0}','Rating unchanged · {0}'],
  ['mp_move_pending','착수 확인 중…','Confirming move…'],
];
for (const [i,[key,ko,en]] of changes.entries()) {
  if (values.some(row=>row[1]===key)) throw new Error('Key already exists: '+key);
  const row = end+i+1;
  sheet.getRange(`A${row}:E${row}`).copyFrom(sheet.getRange(`A${end}:E${end}`),'all');
  sheet.getRange(`A${row}:E${row}`).values = [['USE',key,ko,'멀티 대국 결과·레이팅·착수 상태',en]];
  sheet.getRange(`A${row}:E${row}`).format.rowHeight = 22;
}
wb.recalculate();
console.log((await wb.inspect({kind:'table',range:`Strings!A${end+1}:E${end+changes.length}`,include:'values,formulas',tableMaxRows:7,tableMaxCols:5,maxChars:3000})).ndjson);
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:50},summary:'final formula error scan',maxChars:1500})).ndjson);
await render('after',`B${end+1}:E${end+changes.length}`);
await (await SpreadsheetFile.exportXlsx(wb)).save(work + '/after.xlsx');
await fs.writeFile(work + '/changes.json',JSON.stringify({startRow:end+1,changes},null,2));
console.log(JSON.stringify({added:changes.length,startRow:end+1}));

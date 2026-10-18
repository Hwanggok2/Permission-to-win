import fs from 'node:fs/promises';
import { FileBlob, SpreadsheetFile } from '@oai/artifact-tool';
const work = 'C:/Github/StageGame/GameData/Validation/ad-offer-20261018';
const wb = await SpreadsheetFile.importXlsx(await FileBlob.load(work + '/before.xlsx'));
const sheet = wb.worksheets.getItem('Strings');
const ads = wb.worksheets.getItem('AdConfig');
const values = sheet.getRange('A1:E800').values;
const end = values.findLastIndex(row => row.some(x => x !== null && x !== '')) + 1;
const render = async (name,sheetName,range) => {
  const image = await wb.render({sheetName,range,scale:1});
  await fs.writeFile(`${work}/${name}.png`,new Uint8Array(await image.arrayBuffer()));
};
if (process.argv.includes('--preview')) {
  console.log((await wb.inspect({kind:'workbook,sheet,table',maxChars:3000,tableMaxRows:4,tableMaxCols:5})).ndjson);
  console.log(JSON.stringify({end,ads:ads.getRange('A5:G5').values,lastRows:values.slice(end-5,end)}));
  await render('before-strings','Strings',`B${end-4}:E${end}`);
  await render('before-ads','AdConfig','A1:G5');
  process.exit(0);
}
const changes = [
  ['ad_offer_title','광고 제거','Remove ads'],
  ['ad_offer_subtitle','대국에만 집중하세요.','Keep your focus on the board.'],
  ['ad_offer_ads','전면·배너 광고 제거','Remove interstitial & banner ads'],
  ['ad_offer_hint','싱글 힌트 판당 1회','1 solo hint per game'],
  ['ad_offer_permanent','구매하면 전면·배너 광고가 영구적으로 사라집니다','Remove interstitial and banner ads permanently.'],
  ['ad_offer_buy','{0} · 광고 제거','{0} · Remove ads'],
  ['ad_offer_later','지금은 괜찮아요','Not now'],
  ['ad_offer_restore','구매 복원','Restore'],
  ['ad_offer_pending','구매 내역을 확인하고 있어요.','Checking your purchase.'],
  ['ad_offer_failed','처리하지 못했어요. 잠시 후 다시 시도해 주세요.','Something went wrong. Please try again later.'],
  ['ad_offer_restored','광고 제거가 적용됐어요.','Ad removal is now active.'],
  ['ad_offer_none','복원할 구매 내역이 없어요.','No purchase to restore.'],
];
for (const [i,[key,ko,en]] of changes.entries()) {
  if (values.some(row=>row[1]===key)) throw new Error('Key already exists: '+key);
  const row = end+i+1;
  sheet.getRange(`A${row}:E${row}`).copyFrom(sheet.getRange('A493:E493'),'all');
  sheet.getRange(`A${row}:E${row}`).values = [['USE',key,ko,'광고 제거 구매 안내',en]];
  sheet.getRange(`A${row}:E${row}`).format.rowHeight = 22;
}
ads.getRange('B5:D5').values = [[0,1,0]];
wb.recalculate();
console.log((await wb.inspect({kind:'table',range:`Strings!A${end+1}:E${end+changes.length}`,include:'values,formulas',tableMaxRows:12,tableMaxCols:5,maxChars:4000})).ndjson);
console.log((await wb.inspect({kind:'table',range:'AdConfig!A5:G5',include:'values,formulas',tableMaxRows:1,tableMaxCols:7,maxChars:1500})).ndjson);
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:50},summary:'final formula error scan',maxChars:1500})).ndjson);
await render('after-strings','Strings',`B${end+1}:E${end+changes.length}`);
await render('after-ads','AdConfig','A1:G5');
await (await SpreadsheetFile.exportXlsx(wb)).save(work + '/after.xlsx');
await fs.writeFile(work + '/changes.json',JSON.stringify({startRow:end+1,changes,ads:{range:'AdConfig!B5:D5',before:[3,2,90],after:[0,1,0]}},null,2));
console.log(JSON.stringify({added:changes.length,startRow:end+1}));

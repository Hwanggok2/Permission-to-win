import fs from 'node:fs/promises';
import {createRequire} from 'node:module';
import {fileURLToPath,pathToFileURL} from 'node:url';
const require=createRequire('C:/Github/StageGame/Client/Evidence/turn-timer-rating-20261015/prepare-workbook.mjs');
const {FileBlob,SpreadsheetFile}=await import(pathToFileURL(require.resolve('@oai/artifact-tool')));
const folder=fileURLToPath(new URL('.',import.meta.url));
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(folder+'GameData-before.xlsx'));
const apply=process.argv.includes('--apply');
if(apply){
  const profiles=JSON.parse((await fs.readFile(folder+'final-profiles.json','utf8')).replace(/^\uFEFF/,''));
  const rows=profiles.flatMap(p=>Array.from({length:2},()=>[p.aiDepth,p.aiNodeBudget,p.aiRandomPermille]));
  workbook.worksheets.getItem('Stages').getRange('E5:G34').values=rows;
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(folder+'GameData-candidate.xlsx');
}
for(const [name,range] of [['early','E4:G18'],['late','E19:G34']]){
  console.log((await workbook.inspect({kind:'table',range:'Stages!'+range,include:'values,formulas',tableMaxRows:16,tableMaxCols:3,maxChars:3000})).ndjson);
  const image=await workbook.render({sheetName:'Stages',range,scale:1.5,format:'png'});
  await fs.writeFile(folder+`${name}-${apply?'after':'before'}.png`,new Uint8Array(await image.arrayBuffer()));
}

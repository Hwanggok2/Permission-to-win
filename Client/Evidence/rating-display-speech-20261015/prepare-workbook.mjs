import fs from 'node:fs/promises';
import {FileBlob,SpreadsheetFile} from '@oai/artifact-tool';
const directory=new URL('.',import.meta.url).pathname.replace(/^\/(\w:)/,'$1');
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(directory+'GameData-before.xlsx'));
const strings=workbook.worksheets.getItem('Strings');
const editing=process.argv.includes('--apply');
if(editing){
  strings.getRange('A427:E427').copyFrom(strings.getRange('A425:E425'),'all');
  strings.getRange('A427:E427').values=[['USE','ai_rating','AI 레이팅 {0}','스테이지 선택/대국 상대의 내부 실력 등급','AI Rating {0}']];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(directory+'GameData-candidate.xlsx');
}
console.log((await workbook.inspect({kind:'table',range:'Strings!A425:E427',include:'values,formulas',tableMaxRows:3,tableMaxCols:5,maxChars:1500})).ndjson);
const preview=await workbook.render({sheetName:'Strings',range:'B425:E427',scale:1,format:'png'});
await fs.writeFile(directory+`workbook-${editing?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));

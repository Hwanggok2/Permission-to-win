import fs from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import {FileBlob, SpreadsheetFile} from '@oai/artifact-tool';

const directory=fileURLToPath(new URL('.',import.meta.url));
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load(directory+'GameData-before.xlsx'));
const editing=process.argv.includes('--apply');
if(editing){
  const strings=workbook.worksheets.getItem('Strings');
  strings.getRange('C427').values=[['Rating {0}']];
  strings.getRange('E427').values=[['Rating {0}']];
  workbook.recalculate();
  console.log((await workbook.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!',options:{useRegex:true,maxResults:20},maxChars:1000})).ndjson);
  await(await SpreadsheetFile.exportXlsx(workbook)).save(directory+'GameData-candidate.xlsx');
}
console.log((await workbook.inspect({kind:'table',range:'Strings!B427:E427',include:'values,formulas',tableMaxRows:1,tableMaxCols:4,maxChars:1000})).ndjson);
const preview=await workbook.render({sheetName:'Strings',range:'B425:E427',scale:1,format:'png'});
await fs.writeFile(directory+`workbook-${editing?'after':'before'}.png`,new Uint8Array(await preview.arrayBuffer()));

import assert from 'node:assert/strict';
import {writeFile} from 'node:fs/promises';

const base = new URL(process.argv[2]);
assert.ok(base.protocol === 'https:' || (base.protocol === 'http:' && ['localhost', '127.0.0.1'].includes(base.hostname)));
const checks = [
  {path:'/healthz', status:200, body:{apiVersion:1,status:'ok'}},
  {path:'/v1/status', status:200, body:{apiVersion:1,status:'ready',purchases:{enabled:false,reason:'PAYMENTS_NOT_CONFIGURED'}}},
  {path:'/v1/status', method:'POST', status:405, body:{apiVersion:1,code:'METHOD_NOT_ALLOWED'}},
  {path:'/v1/purchases/verify', method:'POST', status:404, body:{apiVersion:1,code:'NOT_FOUND'}},
  {path:'/v1/status', origin:'https://unapproved.example', status:403, body:{apiVersion:1,code:'ORIGIN_NOT_ALLOWED'}},
];
const results=[];
for(const check of checks) {
  const response=await fetch(new URL(check.path,base),{
    method:check.method??'GET',
    headers:check.origin?{Origin:check.origin}:{},
    signal:AbortSignal.timeout(10000),
    redirect:'error',
  });
  const body=await response.json();
  assert.equal(response.status,check.status,check.path);
  assert.deepEqual(body,check.body,check.path);
  assert.equal(response.headers.get('cache-control'),'no-store');
  if(check.origin)assert.equal(response.headers.get('access-control-allow-origin'),null);
  results.push({path:check.path,method:check.method??'GET',origin:check.origin??null,status:response.status,body});
}
const report={checkedAt:new Date().toISOString(),baseUrl:base.origin,checksPassed:results.length,results};
if(process.argv[3])await writeFile(process.argv[3],JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify(report,null,2));

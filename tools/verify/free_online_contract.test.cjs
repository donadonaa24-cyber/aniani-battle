const assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm'),{stripTypeScriptTypes}=require('node:module');
const path=require('node:path'),root=path.resolve(__dirname,'../..');
const script=fs.readFileSync(path.join(root,'supabase/functions/anibato-free-rooms/index.ts'),'utf8');
let handler,calls=[];
vm.runInNewContext(stripTypeScriptTypes(script),{
 Deno:{serve:fn=>handler=fn,env:{get:k=>k==='SUPABASE_URL'?'https://example.supabase.co':'test-only-server-key'}},
 Response,Request,Uint8Array,TextDecoder,AbortSignal,Set,JSON,Object,String,
 fetch:async(url,options)=>{calls.push({url,options});return Response.json({id:'test-room'});}
});
async function main(){
 const endpoint='https://example.supabase.co/functions/v1/anibato-free-rooms';
 const request=(body,origin='https://donadonaa24-cyber.github.io')=>new Request(endpoint,{method:'POST',headers:{origin,'Content-Type':'application/json'},body:JSON.stringify(body)});
 let r=await handler(request({action:'create',args:{p_host:'a'.repeat(32),p_join:'b'.repeat(32),p_offer:{type:'offer',sdp:'test'}}}));
 assert.equal(r.status,200);assert.equal(calls.length,1);assert(calls[0].url.endsWith('/rpc/anibato_room_create'));assert.equal(r.headers.get('Access-Control-Allow-Origin'),'https://donadonaa24-cyber.github.io');assert(!(await r.text()).includes('test-only-server-key'));
 for(const action of ['aniani_draw_gacha','aniani_wallet','__proto__','constructor']){r=await handler(request({action,args:{}}));assert.equal(r.status,400);}
 r=await handler(request({action:'exchange',args:{p_room:'id',p_key:'b'.repeat(32),evil:'forbidden'}}));assert.equal(r.status,400);
 r=await handler(request({action:'close',args:{}},'https://unrelated.invalid'));assert.equal(r.status,403);
 r=await handler(request({action:'create',args:{p_host:'x'.repeat(74000)}}));assert.equal(r.status,400);
 assert.equal(calls.length,1,'only the allowlisted bounded request reaches the database');
 const api=require(path.join(root,'AniBato/Assets/WebGLTemplates/AniBato/online.js'));
 assert(api.team([0,1,8]));assert(!api.team([0,0,8]));assert(!api.team([0,1,9]));assert(!api.team([0,1]));
 assert(api.code('12345678-1234-1234-1234-123456789abc.'+'a'.repeat(32)));assert(!api.code('123456'));
 const config=JSON.parse(fs.readFileSync(path.join(root,'AniBato/Assets/WebGLTemplates/AniBato/online-config.json')));assert.equal(config.plan,'free');assert.deepEqual(Object.keys(config).sort(),['plan','url']);
 assert.equal(api.LIMITS.polls,38);assert.equal(api.LIMITS.roomSeconds,120);assert.equal(api.LIMITS.sessionMs,1200000);
 console.log('PASS: 22 free-online contract/security checks, no real API calls or credentials.');
}
main().catch(e=>{console.error(e);process.exit(1);});

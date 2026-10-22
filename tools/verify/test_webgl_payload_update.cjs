const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const html=fs.readFileSync(process.argv[2]||'tools/webgl/index.html','utf8');
const source=html.match(/async function preparePayload\(\)\{[\s\S]*?\n\}(?=\ndocument\.querySelector\('#boot'\))/)?.[0];
assert(source,'canonical bootstrap found');
const base='https://example.test/games/anibato/',version='new',expected=base+'payload-worker.js?v='+version;
function harness(controller){
 const events=new Map(),timers=new Map();let timerId=0;
 const worker={controller,ready:Promise.resolve(),register:async(url,options)=>{assert.equal(url,expected);assert.deepEqual({...options},{scope:'./'});},addEventListener:(name,fn)=>events.set(name,fn),removeEventListener:(name,fn)=>{if(events.get(name)===fn)events.delete(name);}};
 const prepare=vm.runInNewContext('('+source+')',{URL,location:{href:base},navigator:{serviceWorker:worker},document:{querySelector:()=>({textContent:JSON.stringify({version})})},setTimeout:fn=>{timers.set(++timerId,fn);return timerId;},clearTimeout:id=>timers.delete(id)});
 return {worker,events,timers,prepare,change(url){worker.controller={scriptURL:url};events.get('controllerchange')?.();}};
}
(async()=>{
 // A previously installed controller and an already fulfilled ready must not start the new payload early.
 const updating=harness({scriptURL:base+'payload-worker.js?v=old'});let complete=false;
 const pending=updating.prepare().then(()=>complete=true);await new Promise(setImmediate);assert.equal(complete,false);
 updating.change(base+'payload-worker.js?v=another-old');await new Promise(setImmediate);assert.equal(complete,false);
 updating.change(expected);await pending;assert.equal(complete,true);assert.equal(updating.events.size,0);assert.equal(updating.timers.size,0);
 const existing=harness({scriptURL:expected});await existing.prepare();assert.equal(existing.events.size,0);
 const initial=harness(null);const first=initial.prepare();await new Promise(setImmediate);initial.change(expected);await first;
 const failed=harness(null);const timeout=failed.prepare();await new Promise(setImmediate);for(const fire of failed.timers.values())fire();await assert.rejects(timeout,/ゲーム配信の準備に失敗/);assert.equal(failed.events.size,0);
 console.log('PASS: old ready/controller waits, wrong version rejected, current controller immediate, first install, timeout cleanup');
})().catch(error=>{console.error(error);process.exitCode=1;});

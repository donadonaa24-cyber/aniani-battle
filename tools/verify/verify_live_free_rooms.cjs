// Exercise only this game's short-lived signalling data. Never print capability keys or SDP.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'../..'),config=JSON.parse(fs.readFileSync(path.join(root,'AniBato/Assets/WebGLTemplates/AniBato/online-config.json')));
const headers={Origin:'https://donadonaa24-cyber.github.io','Content-Type':'application/json'};
let room;const host=crypto.randomBytes(16).toString('hex'),join=crypto.randomBytes(16).toString('hex');let checks=0;
async function call(action,args){const response=await fetch(config.url,{method:'POST',headers,body:JSON.stringify({action,args}),signal:AbortSignal.timeout(15000)});return {status:response.status,value:await response.json()};}
async function main(){
 try{
  const options=await fetch(config.url,{method:'OPTIONS',headers:{Origin:headers.Origin,'Access-Control-Request-Method':'POST'}});assert.equal(options.status,204);checks++;
  const invalid=await call('aniani_wallet',{});assert.equal(invalid.status,400);checks++;
  const created=await call('create',{p_host:host,p_join:join,p_offer:{type:'offer',sdp:'anibato-disposable-connectivity-check'}});assert.equal(created.status,200,JSON.stringify(created.value));room=created.value.id;assert(room);checks++;
  const wrong=await call('exchange',{p_room:room,p_key:crypto.randomBytes(16).toString('hex')});assert.equal(wrong.status,400);checks++;
  const offer=await call('exchange',{p_room:room,p_key:join});assert.equal(offer.status,200);assert.equal(offer.value.offer.type,'offer');checks++;
  const answer=await call('exchange',{p_room:room,p_key:join,p_answer:{type:'answer',sdp:'anibato-disposable-answer'}});assert.equal(answer.status,200);checks++;
  const reply=await call('exchange',{p_room:room,p_key:host});assert.equal(reply.status,200);assert.equal(reply.value.answer.sdp,'anibato-disposable-answer');checks++;
  const duplicate=await call('exchange',{p_room:room,p_key:join,p_answer:{type:'answer',sdp:'duplicate'}});assert.equal(duplicate.status,400);checks++;
 }finally{if(room){const closed=await call('close',{p_room:room,p_key:host});assert.equal(closed.status,200);assert.equal(closed.value,true);checks++;}}
 const proof={at:new Date().toISOString(),project:'bjrewemniqfchggrrkdd',checks,pass:true,publicClientContainsNoApiKey:true,commonTablesNotWritten:true,temporaryRoomClosed:true};
 fs.writeFileSync(path.join(root,'TestResults/live_free_rooms.json'),JSON.stringify(proof,null,2));console.log(JSON.stringify(proof));
}
main().catch(e=>{console.error(e.message);process.exit(1);});

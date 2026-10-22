/* Supabase is only a bounded signalling mailbox. Battle packets never use its Realtime/DB. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.AniBatoOnline=api;})(typeof globalThis!=='undefined'?globalThis:this, function(){
 'use strict';
 const LIMITS=Object.freeze({roomSeconds:120, polls:38, pollMs:3000, packetBytes:8192, packetRate:120, bufferedBytes:65536, sessionMs:20*60*1000});
 const ROSTER=['透真','蓮','千鶴','湊','シロ','結衣','黒獅子王','暁','龍太'];
 const WORKS=['星の終わり','推し駒battle','Battle à la carte','星の終わり','星の終わり','推し駒battle','推し駒battle','Battle à la carte','Battle à la carte'];
 let pc=null,channel=null,host=false,active=false,busy=false,config=null,room=null,hostKey='',round=0;
 let picked=[],peerTeam=null,selfReady=false,otherReady=false,localNext=false,remoteNext=false,queue=[],flushTimer=null,deadline=null;
 let received=0,windowStart=0,sessionGeneration=0,startedAt=0,build='',lastStatus=null,peerRound=0,pending=[];
 const $=id=>document.getElementById(id);
 function tell(s){$('online-message').textContent=s;}
 function unity(method,arg=''){window.unityInstance?.SendMessage('OnlineBridge',method,arg);}
 function team(v){return Array.isArray(v)&&v.length===3&&new Set(v).size===3&&v.every(n=>Number.isInteger(n)&&n>=0&&n<9);}
 function code(v){return typeof v==='string'&&/^[0-9a-f-]{36}\.[0-9a-f]{32}$/.test(v);}
 function randomKey(){return Array.from(crypto.getRandomValues(new Uint8Array(16)),v=>v.toString(16).padStart(2,'0')).join('');}
 async function rpc(name,args){
  if(!config||config.plan!=='free'||!/^https:\/\/[a-z]{20}\.supabase\.co\/functions\/v1\/anibato-free-rooms$/.test(config.url))throw Error('接続設定の準備中です。CPU・同じPCでの対戦は利用できます。');
  const response=await fetch(config.url,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({action:name,args}),signal:AbortSignal.timeout(10000)});
  const value=await response.json();if(!response.ok){if(value.error==='FREE_ROOM_LIMIT')throw Error('無料枠保護のため、現在の接続枠がいっぱいです。少し待つか明日お試しください。');throw Error('部屋が見つからないか、期限切れです。新しい招待コードで接続してください。');}return value;
 }
 async function settings(){if(config)return;const r=await fetch('online-config.json',{cache:'no-store',signal:AbortSignal.timeout(5000)});if(!r.ok)throw Error('オンライン設定の準備中です。');config=await r.json();}
 function emit(value){
  if(!channel||channel.readyState!=='open')throw Error('相手との接続が切れました。');
  const text=JSON.stringify(value);if(text.length>LIMITS.packetBytes||channel.bufferedAmount>LIMITS.bufferedBytes)throw Error('通信が混み合ったため安全に停止しました。');channel.send(text);
 }
 function flush(){if(!queue.length)return;const frames=queue;queue=[];try{emit({type:'frames',values:frames});}catch(e){stop(e.message);}}
 function onStatus(status){lastStatus=status;if(!active)return;if(status.screen==='Result')$('online-active').textContent='オンライン：両者が決定で次へ';else $('online-active').textContent='オンライン P'+(host?1:2)+'　通信待ちでは試合が停止します';maybeNext();}
 function send(value){
  if(!active)return;
  if(value.startsWith('round|')){round=Number(value.split('|')[1]);localNext=remoteNext=false;emit({type:'round',round});deliver();return;}
  queue.push(value);if(queue.length>=4)flush();
 }
 function deliver(){if(!round||peerRound!==round)return;const messages=pending;pending=[];for(const text of messages)unity('Packet',text);}
 function receive(event){
  try{
   if(typeof event.data!=='string'||event.data.length>LIMITS.packetBytes)throw Error('通信データが不正です。');
   const now=performance.now();if(now-windowStart>1000){received=0;windowStart=now;}if(++received>LIMITS.packetRate)throw Error('通信量の上限を超えました。');
   const m=JSON.parse(event.data);
   if(m.type==='hello'){
    if(m.protocol!==1||m.build!==build||!team(m.team))throw Error('ゲームの版が違います。両方のページを再読み込みしてください。');peerTeam=m.team;tell('接続できました。両者の「対戦の準備完了」で開始します。');$('online-ready').disabled=false;
   }else if(m.type==='ready'){otherReady=true;maybeStart();}
   else if(m.type==='start'){
    if(host||active||!selfReady||!otherReady||!Number.isInteger(m.stage)||m.stage<0||m.stage>2||!Number.isInteger(m.seed)||m.seed<0||m.seed>0xffffffff)throw Error('開始データが不正です。');start(m.stage,m.seed);
   }else if(m.type==='frames'){
    if(!active||!Array.isArray(m.values)||m.values.length>6||m.values.some(v=>typeof v!=='string'||v.length>750||!(/^[ih]\|[1-9][0-9]{0,2}\|/.test(v))))throw Error('入力データが不正です。');
    for(const text of m.values){if(pending.length>=180)throw Error('入力待ちの上限を超えました。');pending.push(text);}deliver();
   }else if(m.type==='round'){if(!Number.isInteger(m.round)||m.round<1||m.round>999)throw Error('ラウンドが不正です。');peerRound=m.round;deliver();}
   else if(m.type==='next'){remoteNext=true;maybeNext();}
   else if(m.type==='leave')stop('相手が対戦を終了しました。');
   else throw Error('未知の通信データです。');
  }catch(e){stop(e.message);}
 }
 function attach(ch){
  channel=ch;ch.onmessage=receive;
  ch.onopen=()=>{clearTimeout(deadline);busy=false;flushTimer=setInterval(flush,35);startedAt=performance.now();emit({type:'hello',protocol:1,build,team:picked});tell('相手のチーム情報を確認しています…');if(host&&room)rpc('close',{p_room:room,p_key:hostKey}).catch(()=>{});};
  ch.onclose=()=>{if(channel===ch)stop('接続が終了しました。');};ch.onerror=()=>stop('直接通信に接続できませんでした。有料中継は使用しません。');
 }
 function peer(isHost){
  stop('',false);host=isHost;busy=true;windowStart=performance.now();received=0;selfReady=otherReady=false;peerTeam=null;localNext=remoteNext=false;round=peerRound=0;pending=[];
  // Free STUN only. No TURN credentials, relay service, microphone or camera.
  pc=new RTCPeerConnection({iceServers:[{urls:'stun:stun.l.google.com:19302'}],iceTransportPolicy:'all'});
  if(host)attach(pc.createDataChannel('anibato',{ordered:true}));else pc.ondatachannel=e=>attach(e.channel);
  pc.onconnectionstatechange=()=>{if(pc&&['failed','closed'].includes(pc.connectionState))stop('直接通信に接続できませんでした。別の回線でお試しください。');};
  deadline=setTimeout(()=>stop('接続待ちが終了しました。招待コードは2分で失効します。'),LIMITS.roomSeconds*1000);
  renderTeam();return sessionGeneration;
 }
 async function description(localPeer){
  await new Promise((resolve,reject)=>{
   if(localPeer.iceGatheringState==='complete')return resolve();
   const timeout=setTimeout(()=>{localPeer.removeEventListener('icegatheringstatechange',change);reject(Error('回線情報を取得できませんでした。再度お試しください。'));},8000);
   function change(){if(localPeer.iceGatheringState==='complete'){clearTimeout(timeout);localPeer.removeEventListener('icegatheringstatechange',change);resolve();}}localPeer.addEventListener('icegatheringstatechange',change);
  });return {type:localPeer.localDescription.type,sdp:localPeer.localDescription.sdp};
 }
 async function create(){
  if(picked.length!==3||busy||active)return;
  let gen=sessionGeneration;try{
   await settings();if(gen!==sessionGeneration)return;gen=peer(true);const current=pc;hostKey=randomKey();const joinKey=randomKey();tell('無料の接続用ルームを準備しています…');
   await current.setLocalDescription(await current.createOffer());const offer=await description(current);if(gen!==sessionGeneration)return;
   const value=await rpc('create',{p_host:hostKey,p_join:joinKey,p_offer:offer});if(gen!==sessionGeneration)return;room=value.id;
   $('online-code').value=room+'.'+joinKey;$('online-invite').hidden=false;tell('この招待コードを相手に渡してください。2分以内に参加できます。');
   for(let i=0;i<LIMITS.polls&&gen===sessionGeneration;i++){
    await new Promise(resolve=>setTimeout(resolve,LIMITS.pollMs));if(gen!==sessionGeneration||!pc)return;
    const reply=await rpc('exchange',{p_room:room,p_key:hostKey});if(gen!==sessionGeneration)return;
    if(reply.answer){await current.setRemoteDescription(reply.answer);tell('直接通信を接続しています…');return;}
   }if(gen===sessionGeneration)stop('招待の有効期限が切れました。');
  }catch(e){if(gen===sessionGeneration)stop(e.message);}
 }
 async function join(){
  const invite=$('online-join-code').value.trim();if(!code(invite)){tell('招待コードを省略せず貼り付けてください。');return;}if(picked.length!==3||busy||active)return;
  let gen=sessionGeneration;try{
   await settings();if(gen!==sessionGeneration)return;gen=peer(false);const current=pc;const [id,key]=invite.split('.');tell('招待ルームに接続しています…');
   const reply=await rpc('exchange',{p_room:id,p_key:key});if(gen!==sessionGeneration)return;
   await current.setRemoteDescription(reply.offer);await current.setLocalDescription(await current.createAnswer());const answer=await description(current);if(gen!==sessionGeneration)return;
   await rpc('exchange',{p_room:id,p_key:key,p_answer:answer});if(gen===sessionGeneration)tell('直接通信を接続しています…');
  }catch(e){if(gen===sessionGeneration)stop(e.message);}
 }
 function maybeStart(){if(host&&selfReady&&otherReady&&peerTeam&&!active){const stage=Number($('online-stage').value),seed=crypto.getRandomValues(new Uint32Array(1))[0];emit({type:'start',stage,seed});start(stage,seed);}}
 function start(stage,seed){
  active=true;busy=false;round=0;peerRound=0;pending=[];
  unity('Begin',JSON.stringify({player:host?0:1,first:host?picked:peerTeam,second:host?peerTeam:picked,stage,seed}));
  $('online-dialog').hidden=true;$('online-exit').hidden=false;$('pause').disabled=true;$('unity-canvas').focus();$('online-active').textContent='オンライン P'+(host?1:2)+'　P1操作を使用';
 }
 function maybeNext(){if(active&&localNext&&remoteNext&&lastStatus?.screen==='Result'){localNext=remoteNext=false;unity('ContinueRound');}}
 function stop(reason='',notifyUnity=true){
  const was=active;active=busy=false;sessionGeneration++;clearTimeout(deadline);clearInterval(flushTimer);flushTimer=null;queue=[];pending=[];
  const oldChannel=channel;channel=null;if(oldChannel){oldChannel.onclose=oldChannel.onerror=null;oldChannel.close();}const oldPeer=pc;pc=null;if(oldPeer){oldPeer.onconnectionstatechange=null;oldPeer.close();}
  if(host&&room&&config)rpc('close',{p_room:room,p_key:hostKey}).catch(()=>{});room=null;
  if(typeof document==='undefined')return;
  $('online-ready').disabled=true;$('online-invite').hidden=true;$('online-exit').hidden=true;$('online-active').textContent='';$('pause').disabled=!window.unityInstance;
  if(reason){$('online-dialog').hidden=false;tell(reason);}if(was&&notifyUnity)unity('Disconnected');renderTeam();
 }
 function renderTeam(){
  if(typeof document==='undefined')return;
  for(const b of $('online-team').children){const i=Number(b.dataset.character);b.setAttribute('aria-pressed',String(picked.includes(i)));b.disabled=busy||active;b.querySelector('strong').textContent=(picked.includes(i)?(picked.indexOf(i)+1)+'. ':'')+ROSTER[i];}
  $('online-create').disabled=$('online-join').disabled=picked.length!==3||busy||active;$('online-selection').textContent=picked.length?picked.map(n=>ROSTER[n]).join(' → '):'3人を選択（同キャラ不可・選んだ順に出場）';
 }
 function ui(value){
  if(value==='open'){if(!window.unityInstance){tell('先にゲームを読み込んで「開演」を押してください。');}$('online-dialog').hidden=false;renderTeam();}
  else if(value==='next'){if(active&&!localNext){localNext=true;emit({type:'next'});maybeNext();}}
  else if(value.startsWith('stop|'))stop(value.slice(5),false);
 }
 function init(){
  build=window.AniBatoBuild;
  ROSTER.forEach((name,i)=>{const b=document.createElement('button');b.type='button';b.dataset.character=i;b.setAttribute('aria-pressed','false');const strong=document.createElement('strong');strong.textContent=name;const small=document.createElement('small');small.textContent=WORKS[i];b.append(strong,small);b.onclick=()=>{if(busy||active)return;if(picked.includes(i))picked=picked.filter(n=>n!==i);else if(picked.length<3)picked.push(i);renderTeam();};$('online-team').append(b);});
  $('online-open').onclick=()=>{if(!window.unityInstance){$('online-dialog').hidden=false;tell('先にゲームを読み込んで「開演」を押してください。');return;}ui('open');};
  $('online-close').onclick=()=>{if(!active)stop('',false);$('online-dialog').hidden=true;$('unity-canvas').focus();};
  $('online-create').onclick=()=>create();$('online-join').onclick=()=>join();
  $('online-copy').onclick=()=>navigator.clipboard.writeText($('online-code').value).then(()=>tell('招待コードをコピーしました。')).catch(()=>{$('online-code').select();tell('コードを選択しました。コピーして相手に渡してください。');});
  $('online-ready').onclick=()=>{if(!peerTeam||selfReady)return;selfReady=true;emit({type:'ready'});$('online-ready').disabled=true;tell('相手の準備完了を待っています…');maybeStart();};
  $('online-exit').onclick=()=>{try{emit({type:'leave'});}catch{}stop('対戦を終了しました。');};
  window.addEventListener('pagehide',()=>{try{emit({type:'leave'});}catch{}stop('',false);});
  setInterval(()=>{if(active&&performance.now()-startedAt>LIMITS.sessionMs)stop('無料試験版の連続接続は20分までです。新しく部屋を作ってください。');},1000);
  renderTeam();
 }
 if(typeof document!=='undefined'){if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',init);else init();}
 return {send,ui,onStatus,LIMITS,team,code};
});

// Read Unity-generated assets only. Never rewrite scene/asset YAML.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const tag=process.argv[2],assert=(ok,n)=>{if(!ok)throw Error(n);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Verification tag required');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function copy(from,to){fs.mkdirSync(path.dirname(to),{recursive:true});if(!fs.existsSync(to)||sha(from)!==sha(to))fs.copyFileSync(from,to);}
function result(platform){const xml=fs.readFileSync(`TestResults/${tag}_${platform}.xml`,'utf8'),head=xml.match(/<test-run\b[^>]*>/)[0];
 const a=Object.fromEntries([...head.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]])),r=Object.fromEntries(['total','passed','failed','skipped'].map(k=>[k,Number(a[k])]));
 assert(a.result==='Passed'&&r.total>0&&r.total===r.passed&&r.failed===0&&r.skipped===0,platform+' must fully pass');return r;}
const edit=result('EditMode'),play=result('PlayMode');
const buildAll=fs.readFileSync(`TestResults/${tag}_buildall.log`,'utf8'),build=fs.readFileSync(`TestResults/${tag}_build.log`,'utf8');
assert(buildAll.includes('生成完了:')&&buildAll.includes('return code 0'),'BuildAll success required');
assert(build.includes('Build Finished, Result: Success.')&&fs.existsSync(`Build/${tag}/AniBato.exe`),'Windows success required');
const chosenEnsemble=read('art_source/title_paper/selected.json');
assert(chosenEnsemble.version===5&&sha(chosenEnsemble.source)===chosenEnsemble.sha256&&
 sha('AniBato/Assets/_Project/Art/PaperEnsemble/ensemble.png')===chosenEnsemble.sha256,'Runtime ensemble must match corrected v5');
const chosenHand=read('art_source/paper_roster_action/chizuru/normal3_hand_fix/selected.json');
assert(sha(chosenHand.prepared)===chosenHand.preparedSha256&&
 sha('AniBato/Assets/_Project/Art/Paper/pose_attack3_impact_paper_chizuru.png')===chosenHand.preparedSha256,'Runtime pan grip must match selected native pack');
const guidMap=new Map();
function scan(folder){for(const e of fs.readdirSync(folder,{withFileTypes:true})){const p=folder+'/'+e.name;if(e.isDirectory())scan(p);else if(e.name.endsWith('.meta')){const m=fs.readFileSync(p,'utf8').match(/^guid: ([a-f0-9]+)/m);if(m)guidMap.set(m[1],p.slice(0,-5));}}}
scan('AniBato/Assets/_Project/Art');scan('AniBato/Assets/_Project/Data');
const roster=[['toma','Touma','透真'],['ren','Ren','蓮'],['chizuru','Chizuru','千鶴'],['minato','Minato','湊'],['shiro','Shiro','シロ'],['yui','Yui','結衣'],['king','King','黒獅子王'],['akatsuki','Akatsuki','暁'],['ryuta','Ryuta','龍太']];
const actions=[['normal1','通常1・2段共通',6],['normal3','通常3段',8],['up','上攻撃',30],['down','下攻撃',25],['air','空中攻撃',27],['special','特殊技',10],['super','超必殺技',14]];
const base='イラスト確認/現在ゲームで採用中',entries=[];
for(const [key,asset,name] of roster){
 const text=fs.readFileSync(`AniBato/Assets/_Project/Data/${asset}.asset`,'utf8'),g=text.match(/PaperMotion: \{[^}]*guid: ([a-f0-9]+)/)[1];
 const motionPath=guidMap.get(g),motion=fs.readFileSync(motionPath,'utf8'),poses=[];
 for(const match of motion.matchAll(/^  - Key: (.+)\r?\n([\s\S]*?)(?=^  - Key:|^  HeavyDamage:)/gm)){
  const states=match[2].match(/    States: ([a-f0-9]+)/)[1],bytes=Buffer.from(states,'hex'),ids=[];
  for(let i=0;i<bytes.length;i+=4)ids.push(bytes.readUInt32LE(i));
  const list=match[2].match(/    Frames:\r?\n([\s\S]*?)    HoldTicks:/)[1];
  poses.push({key:match[1].trim(),states:ids,frames:[...list.matchAll(/guid: ([a-f0-9]+)/g)].map(m=>guidMap.get(m[1]))});
 }
 const packedFile=`art_source/paper_roster_action/${key}/packed-manifest.json`;
 const packed=fs.existsSync(packedFile)?read(packedFile):null;
 const complete=['toma','ren','chizuru'].includes(key)||Boolean(packed?.complete&&/ActionArtworkSchemaVersion: 1\b/.test(text)),folder=base+'/'+name,selected=[];
 const idlePath=guidMap.get(motion.match(/^  Sprite: \{[^}]*guid: ([a-f0-9]+)/m)[1]);copy(idlePath,folder+'/00_待機基準.png');
 for(const [id,label,state] of actions){
  const pose=poses.find(p=>p.states.includes(state));assert(pose&&pose.frames.every(Boolean),name+' '+id+' missing');
  if(id==='down'||id==='air')assert(pose.frames.length===1,name+' single-key policy');
  if(complete)assert(pose.frames.length===(['down','air'].includes(id)?1:2),name+' adopted count');
  const files=complete?pose.frames:[];
  for(let i=0;i<files.length;i++){
   const runtime=files[i],title=label+(files.length===1?'_1枚採用':i===0?'_予備':'_当てる'),reviewFile=folder+'/'+title+'.png';copy(runtime,reviewFile);
   if(complete&&packed?.complete&&key!=='toma'){
    const phase=files.length===1?'impact':i===0?'prepare':'impact',frame=packed.frames.find(f=>f.id===id+'_'+phase);
    assert(frame&&sha(runtime)===frame.sha256,name+' selected/runtime mismatch '+id+'_'+phase);
   }
   selected.push({id:id+'_'+(files.length===1?'single':i===0?'prepare':'impact'),title,runtime,sha256:sha(runtime),reviewFile});
  }
 }
 const report={key,name,unityVerifiedTag:tag,motionAsset:motionPath,completeAttackArtwork:complete,bodyFrames:selected.length,
  uniqueBodyImages:new Set(selected.map(e=>e.sha256)).size,entries:selected,extraVariants:'追加指示後',artApproved:false};
 write(folder+'/採用一覧.json',report);write(`art_source/paper_roster_action/${key}/current-adoption.json`,report);entries.push(report);
 fs.writeFileSync(folder+'/必ず読んでください.txt',complete?`現在のゲーム用攻撃12枠・${report.uniqueBodyImages}ユニーク原画です。通常1・2段共通2枚／3段2枚／上2枚／下1枚／空中1枚／特殊2枚／超必殺2枚。適切な原画の共有は採用一覧のSHAと原画manifestのreuseOfに記録します。${tag}で実Unity4工程合格。絵の最終承認は別。原画の候補や修正前はこのフォルダに混ぜません。\n`:'専用の攻撃原画は未完成です。下・空中は1枚表示へ設定済みですが、現在は本人の既存紙イラストを仮表示しています。待機や基本5姿勢の完成を全攻撃の完成とは扱いません。\n');
}
const shots=[];
const completed=entries.filter(e=>e.completeAttackArtwork).map(e=>e.key),remaining=entries.filter(e=>!e.completeAttackArtwork).map(e=>e.key);
for(const sub of ['rework_r4_chizuru','rework_r4_ren','rework_fourkeys','rework_r5',...['minato','shiro','yui','king','akatsuki'].filter(key=>completed.includes(key)).map(key=>'rework_r4_'+key)]){
 const from=`TestResults/${tag}_screenshots/${sub}`;assert(fs.existsSync(from),'Missing real render captures '+sub);
 for(const name of fs.readdirSync(from).filter(n=>n.endsWith('.png'))){const to=`docs/screenshots/${tag}/${sub}/${name}`;copy(from+'/'+name,to);shots.push(to);}
}
const ensembleSelection=read('art_source/title_paper/selected.json');
assert(ensembleSelection.version===5,'Corrected five-digit ensemble required');
copy(ensembleSelection.source,base+'/集合絵_採用中.png');
fs.writeFileSync(base+'/ここから確認してください.txt',`このフォルダは${tag}で確認した現在の採用画像です。人物名ごとの日本語動作名で指摘してください。専用攻撃完成：${entries.filter(e=>e.completeAttackArtwork).map(e=>e.name).join('・')}。未完成：${entries.filter(e=>!e.completeAttackArtwork).map(e=>e.name).join('・')}。各12枠、ユニーク原画数は各採用一覧に記載。古い別フォルダには旧確認用の画像が残ります。こちらの採用一覧.jsonが現在版です。空中・下は1枚、通常は予備／当てる2枚。他動作の追加差分は追加指示後。\n`);
fs.writeFileSync('イラスト確認/最初に読んでください.txt','現在ゲームで使用している画像は「現在ゲームで採用中」フォルダを見てください。そこから人物名→動作名で選べます。ほかの確認フォルダは以前の採用・候補の履歴を含みます。最新の採用一覧.jsonと区別してください。カバン版は指定により保護しています。\n');
const selected=read('art_source/title_paper/selected.json');Object.assign(selected,{gameConnected:true,unityVerifiedTag:tag});write('art_source/title_paper/selected.json',selected);
const hand='art_source/paper_roster_action/chizuru/normal3_hand_fix/selected.json',h=read(hand);Object.assign(h,{gameConnected:true,unityVerifiedTag:tag});write(hand,h);
const summary={tag,buildAll:'success',edit,play,windowsBuild:'success',singleActions:['down','air'],normalUniqueFrames:4,completedDedicatedCharacters:completed,remainingDedicatedCharacters:remaining,reviewFolder:base,ensembleVersion:ensembleSelection.version,screenshots:`docs/screenshots/${tag}/`,screenshotCount:shots.length};
write('art_source/paper_roster_action/single-key-adoption.json',summary);console.log(JSON.stringify(summary));

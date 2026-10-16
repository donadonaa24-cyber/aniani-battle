// Confirm one 14-key roster batch only after the real Unity four-stage run.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const [key,tag]=process.argv.slice(2),workspace=fs.realpathSync(process.cwd());
const assert=(ok,n)=>{if(!ok)throw Error(n);},read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function copy(a,b){assert(path.resolve(a).startsWith(workspace+path.sep)&&path.resolve(b).startsWith(workspace+path.sep),'Workspace required');fs.mkdirSync(path.dirname(b),{recursive:true});if(!fs.existsSync(b)||sha(a)!==sha(b))fs.copyFileSync(a,b);}
function result(platform){const xml=fs.readFileSync(`TestResults/${tag}_${platform}.xml`,'utf8'),root=xml.match(/<test-run\b[^>]*>/)[0];
 const a=Object.fromEntries([...root.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));const c=Object.fromEntries(['total','passed','failed','skipped'].map(k=>[k,Number(a[k])]));
 assert(a.result==='Passed'&&c.total>0&&c.total===c.passed&&c.failed===0&&c.skipped===0,platform+' must fully pass');return c;}
try{
 assert(key==='ren','Reviewed character configuration required');assert(/^[a-z0-9_]+$/.test(tag),'Invalid verification tag');
 const edit=result('EditMode'),play=result('PlayMode'),all=fs.readFileSync(`TestResults/${tag}_buildall.log`,'utf8'),build=fs.readFileSync(`TestResults/${tag}_build.log`,'utf8');
 assert(all.includes('生成完了:')&&all.includes('return code 0'),'BuildAll must pass');
 assert(build.includes('Build Finished, Result: Success.')&&fs.existsSync(`Build/${tag}/AniBato.exe`),'Windows build must pass');
 const root=`art_source/paper_roster_action/${key}/`,m=read(root+'manifest.json'),p=read(root+'packed-manifest.json'),audit=read(root+'pixel-audit.json');
 assert(m.jobs.length===14&&p.complete&&p.frames.length===14&&audit.complete&&audit.bodyFrames.length===14,'Fourteen audited keys required');
 const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));assert(selected.every(Boolean),'Missing selected raw');
 const folder=`イラスト確認/${m.name}/攻撃原画_ゲーム採用済み`,entries=[];
 for(const e of selected){const f=p.frames.find(f=>f.id===e.id),runtime=`AniBato/Assets/_Project/Art/PaperAction/${key}_${e.id}.png`;
  assert(sha(e.source)===e.sha256&&sha(e.prepared)===f.sha256&&sha(runtime)===f.sha256,e.id+' source/runtime mismatch');
  const title=e.label+'_'+(e.phase==='prepare'?'予備':'当てる'),file=folder+'/'+title+'.png';copy(e.prepared,file);
  entries.push({id:e.id,title,source:e.source,prepared:e.prepared,runtime,reviewFile:file,status:'ゲーム採用／美術最終承認は別'});
  e.status='game_adopted_unapproved';e.gameConnected=true;
 }
 copy(`art_source/paper_roster_idle/prepared/${key}_idle.png`,folder+'/00_待機基準.png');
 const capture=`TestResults/${tag}_screenshots/rework_r4_${key}`,shots=fs.readdirSync(capture).filter(f=>f.endsWith('.png'));
 assert(shots.length>=48,'Both-facing prepare/impact/recovery/release captures required');
 for(const file of shots)copy(capture+'/'+file,`docs/screenshots/rework_r4_${key}/`+file);
 m.integrated=true;m.verifiedTag=tag;Object.assign(p,{reviewed:true,gameConnected:true,verifiedTag:tag});write(root+'manifest.json',m);write(root+'packed-manifest.json',p);
 const review=read(root+'part-review.json');review.gameConnected=true;review.unityVerifiedTag=tag;write(root+'part-review.json',review);
 write(root+'review-index.json',{schemaVersion:1,character:m.name,artApproved:false,unityVerifiedTag:tag,folder,entries});
 write(root+'review-pending.json',{requiredSlots:14,savedSlots:14,gameConnectedSlots:14,unpackedSlots:0,artApproved:false,notes:['全14枠を実Unityへ接続。絵の最終承認は別。','移動・溜め・被弾等の本人原画は後続。']});
 fs.writeFileSync(folder+'/必ず読んでください.txt',`現在ゲームで使う${m.name}の攻撃14原画と待機基準。通常1・2段は共通2枚、3段のみ大振り2枚。上／下／空中N／特殊／超必殺は各2枚。${tag}: BuildAll成功／Edit${edit.passed}/${edit.total}／Play${play.passed}/${play.total}／Windows成功。美術最終承認は別、原画の耳・靴の投影の微差はpart-review.jsonへ。移動・溜め・被弾等は未完成。\n`);
 fs.writeFileSync(`イラスト確認/${m.name}/攻撃原画_確認中/必ず読んでください.txt`,'このフォルダは旧確認用です。現在使用の14原画は隣の「攻撃原画_ゲーム採用済み」を見てください。原画候補は履歴として保持し、現在使用とは区別します。\n');
 if(fs.existsSync(root+'pending-review-index.json')){const pending=read(root+'pending-review-index.json');pending.supersededBy=folder;pending.note='旧未梱包候補の一覧。現在の採用版はreview-index.json。';write(root+'pending-review-index.json',pending);}
 const adoption={schemaVersion:1,key,tag,buildAll:'success',edit,play,windowsBuild:'success',bodyFrames:14,complete:true,artApproved:false,screenshots:`docs/screenshots/rework_r4_${key}/`,screenshotCount:shots.length,remaining:'移動・溜め・被弾等の未移行姿勢。全員の攻撃後に後続。'};
 write(root+'adoption.json',adoption);console.log(JSON.stringify(adoption));
}catch(e){console.error(e.stack);process.exitCode=1;}

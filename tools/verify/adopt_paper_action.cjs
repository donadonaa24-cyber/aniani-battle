// Adopt only a whole character whose real Unity four-stage verification is complete.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const [key,tag]=process.argv.slice(2),workspace=fs.realpathSync(process.cwd());
const assert=(ok,text)=>{if(!ok)throw Error(text);};
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function inside(p){const absolute=path.resolve(p);assert(absolute.startsWith(workspace+path.sep),'Workspace path required');return absolute;}
function copy(source,destination){inside(source);inside(destination);if(!fs.existsSync(destination)||sha(source)!==sha(destination))fs.copyFileSync(source,destination);}
function results(platform){
 const file=`TestResults/${tag}_${platform}.xml`,xml=fs.readFileSync(file,'utf8'),root=xml.match(/<test-run\b[^>]*>/)[0];
 const attributes=Object.fromEntries([...root.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));
 const counts=Object.fromEntries(['total','passed','failed','skipped'].map(k=>[k,Number(attributes[k])]));
 assert(attributes.result==='Passed'&&counts.total>0&&counts.passed===counts.total&&counts.failed===0&&counts.skipped===0,platform+' must fully pass');
 return counts;
}
try{
 assert(key==='toma','本人の指示と接続を追加してから次のキャラを採用する');assert(/^[a-z0-9_]+$/.test(tag),'Invalid tag');
 const edit=results('EditMode'),play=results('PlayMode');
 const buildAll=fs.readFileSync(`TestResults/${tag}_buildall.log`,'utf8'),build=fs.readFileSync(`TestResults/${tag}_build.log`,'utf8');
 assert(buildAll.includes('生成完了:')&&buildAll.includes('return code 0'),'BuildAll must pass');
 assert(build.includes('Build Finished, Result: Success.')&&fs.existsSync(`Build/${tag}/AniBato.exe`),'Windows build must pass');
 const root=`art_source/paper_roster_action/${key}/`,m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),effects=read(root+'effects-manifest.json');
 assert(m.name==='透真'&&m.jobs.length===16&&packed.complete&&packed.frames.length===16,'16 selected body images required');
 const selected=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));assert(selected.every(Boolean),'Selected source missing');
 for(const e of selected){
  const frame=packed.frames.find(f=>f.id===e.id),runtime=`AniBato/Assets/_Project/Art/PaperAction/toma_${e.id}.png`;
  assert(sha(inside(e.source))===e.sha256&&sha(inside(e.prepared))===frame.sha256&&sha(runtime)===frame.sha256,e.id+' hash mismatch');
 }
 assert(effects.entries.length===3,'Three selected effect images required');
 for(const e of effects.entries)assert(sha(inside(e.source))===e.sha256&&sha(`AniBato/Assets/_Project/Art/PaperAction/toma_effect_${e.id}.png`)===e.sha256,e.id+' effect mismatch');
 const from=inside('イラスト確認/透真/攻撃原画_確認中'),to=inside('イラスト確認/透真/攻撃原画_ゲーム採用済み');
 assert(fs.existsSync(from)||fs.existsSync(to),'Review folder missing');
 if(fs.existsSync(from)){assert(!fs.existsSync(to),'Do not overwrite an older adopted folder');assert(fs.realpathSync(from).startsWith(workspace+path.sep),'Review folder escapes workspace');fs.renameSync(from,to);}
 const labels={drone:'飛翔物_ドローン',rocket:'飛翔物_ロケット',bullet:'飛翔物_銃弾'};
 for(const e of effects.entries)copy(e.source,path.join(to,labels[e.id]+'.png'));
 for(const e of selected){e.status='game_adopted_unapproved';e.gameConnected=true;}
 for(const e of effects.entries){e.status='game_adopted_unapproved';e.gameConnected=true;}
 m.integrated=true;m.verifiedTag=tag;packed.reviewed=true;packed.gameConnected=true;packed.verifiedTag=tag;effects.gameConnected=true;effects.verifiedTag=tag;
 write(root+'manifest.json',m);write(root+'packed-manifest.json',packed);write(root+'effects-manifest.json',effects);
 const review=read(root+'part-review.json');review.gameConnected=true;review.unityVerifiedTag=tag;write(root+'part-review.json',review);
 const index=read(root+'review-index.json');index.unityVerifiedTag=tag;index.folder='イラスト確認/透真/攻撃原画_ゲーム採用済み';
 for(const entry of index.entries)entry.status='ゲーム採用／美術の最終承認は別';write(root+'review-index.json',index);
 fs.writeFileSync(path.join(to,'必ず読んでください.txt'),`このフォルダは現在ゲームで使う透真の攻撃16枚と飛翔物3枚、待機基準です。${tag}でBuildAll成功・Edit${edit.passed}/${edit.total}・Play${play.passed}/${play.total}・Windowsビルド成功。絵の最終美術承認は別で、微差はpart-review.jsonへ記録。移動・回避・被弾などの未移行も残っています。動作名と予備／当てるを指定して指摘してください。一覧は縮小確認用で、ゲーム人物画像は1304×1450・再サンプルなしです。\n`);
 const record={schemaVersion:1,key,tag,buildAll:'success',edit,play,windowsBuild:'success',bodyFrames:16,effectFrames:3,artApproved:false,screenshots:'docs/screenshots/rework_r4_toma/'};
 write(root+'adoption.json',record);console.log(JSON.stringify(record));
}catch(error){console.error(error.message);process.exitCode=1;}

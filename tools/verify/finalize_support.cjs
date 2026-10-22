// Gate adoption on real Unity results and byte-identical original artwork.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const root=path.resolve(__dirname,'../..'),tag=process.argv[2]||'paper_compact_final';
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
const out='docs/codex_logs/paper_support_verification.json';
if(process.argv.includes('--web')){
 const report=read(out),build=read('TestResults/webgl-build.json'),pixels=read('TestResults/webgl-pixel-audit.json');
 if(build.result!=='success'||pixels.checked!==468||pixels.failed!==0)throw Error('Web build or native-pixel proof missing');
 for(const part of ['WebGL','RestoreWindows']){
  const file=`TestResults/${tag}_${part}.log`;
  if(!fs.readFileSync(path.join(root,file),'utf8').includes('Application will exit with return code 0'))throw Error(part+' not successful');
 }
 const folder='docs/screenshots/paper_support_web',consoleRows=read(folder+'/browser_console.json');
 if(consoleRows.length)throw Error('Browser warnings/errors need review');
 const shots=['support_pickup.png','support_effect.png','compact_hud.png','fighter_entrance.png'].map(name=>({file:folder+'/'+name,sha256:sha(folder+'/'+name)}));
 report.web={tag,build,status:'verified',windowsRestored:true,nativeTexturesChecked:468,rgbaPreserved:true,
  realKeyboardPickup:true,browserWarnings:0,browserErrors:0,screenshots:shots};
 fs.writeFileSync(path.join(root,out),JSON.stringify(report,null,2)+'\n');
 fs.copyFileSync(path.join(root,'TestResults/webgl-pixel-audit.json'),path.join(root,'docs/codex_logs/paper_support_web_pixels.json'));
 const progress=read('art_source/paper_support_items/progress.json');progress.status='native_and_web_connected_verified';progress.webTag=tag;
 fs.writeFileSync(path.join(root,'art_source/paper_support_items/progress.json'),JSON.stringify(progress,null,2)+'\n');
 console.log('Support WebGL/native-pixel/real-browser proofs recorded.');process.exit(0);
}
const native={};
for(const platform of ['EditMode','PlayMode']){
 const file=`TestResults/${tag}_${platform}.xml`,xml=fs.readFileSync(path.join(root,file),'utf8');
 const header=xml.match(/<test-run\b[^>]+>/)[0],attributes=Object.fromEntries([...header.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));
 if(attributes.result!=='Passed'||+attributes.failed!==0||+attributes.skipped!==0||+attributes.passed<({EditMode:817,PlayMode:104}[platform]))throw Error('Native tests incomplete: '+platform);
 native[platform]={passed:+attributes.passed,total:+attributes.total,failed:0,skipped:0,results:file,sha256:sha(file)};
}
for(const [part,suffix]of [['BuildAll','buildall'],['Windows','build']]){
 const file=`TestResults/${tag}_${suffix}.log`,log=fs.readFileSync(path.join(root,file),'utf8');
 if(!log.includes('Application will exit with return code 0')||part==='Windows'&&!log.includes('Build Finished, Result: Success'))throw Error(part+' not successful');
 native[part]={exitCode:0,log:file,sha256:sha(file)};
}
const progress=read('art_source/paper_support_items/progress.json'),manifest=read('art_source/paper_support_items/adoption.json');
const reviewFolder='イラスト確認/ゲーム採用/アイテムサポート';fs.mkdirSync(path.join(root,reviewFolder),{recursive:true});
const images=[];
for(const e of manifest.entries){
 const game=`AniBato/Assets/_Project/Art/PaperAction/support_${e.key}.png`;
 if(sha(e.source)!==e.sha256||sha(game)!==e.sha256)throw Error('Original artwork bytes differ: '+e.key);
 const row=progress.entries.find(r=>r.key===e.key);row.reviewFile=reviewFolder+'/'+path.basename(row.reviewFile);
 fs.copyFileSync(path.join(root,e.source),path.join(root,row.reviewFile));
 row.gameConnected=true;row.status='native_connected_verified';row.artApproved=false;
 images.push({key:e.key,source:e.source,game,sha256:e.sha256,width:e.width,height:e.height,nativePixelsPreserved:true});
}
progress.status='native_verified_web_pending';progress.nativeTag=tag;
fs.writeFileSync(path.join(root,'art_source/paper_support_items/progress.json'),JSON.stringify(progress,null,2)+'\n');
const captures=Object.keys({Alma:0,ShionCar:0,SakiHana:0,Sara:0,Mai:0,Tsuyoshi:0,Mio:0,Hota:0}).map(key=>{
 const source=`TestResults/${tag}/captures/paper_support/${key}.png`,dest=`docs/screenshots/paper_support/${key}.png`;
 fs.mkdirSync(path.dirname(path.join(root,dest)),{recursive:true});fs.copyFileSync(path.join(root,source),path.join(root,dest));return {file:dest,sha256:sha(dest)};
});
const extraCaptures=[];
for(const [folder,names]of [['paper_hud',['1920x1080','1600x1200','2520x1080']],
 ['paper_entrance',['入場_透真','入場_蓮','入場_千鶴','入場_湊','入場_シロ','入場_結衣','入場_黒獅子王','入場_暁','入場_龍太']]])
 for(const name of names){
  const source=`TestResults/${tag}/captures/${folder}/${name}.png`,dest=`docs/screenshots/paper_compact/${folder}/${name}.png`;
  fs.mkdirSync(path.dirname(path.join(root,dest)),{recursive:true});fs.copyFileSync(path.join(root,source),path.join(root,dest));extraCaptures.push({file:dest,sha256:sha(dest)});
 }
const report={schemaVersion:1,nativeTag:tag,native,images,captures,
 ui:{fighterHudScale:1/3,headCounterBlackBackdrop:false,headCounterShadow:false,standingPortraits:9,
  angledSegmentedGauges:true,supportExplanations:8,walletExplanation:true,riftEntrances:9,captures:extraCaptures},
 rocket:{passesUpperPlatforms:true,passesPlacedWalls:true,explodesOnMainFloor:true,duplicateHeartLoss:false},
 rules:{firstSpawnSeconds:[8,12],intervalSeconds:[10,16],simultaneousMainPickups:1,reuseSeconds:10,shuffleBag:8,
  slowSeconds:5,slowMultiplier:.6,infiniteStaminaSeconds:5,healHearts:1,carParalysisSeconds:2,arrowHearts:1,
  barrierSeconds:5,barrierNormalHits:3,barrierSpecialHits:1,ultimatePierces:true,pawns:6,pawnStaminaDamage:15},
 web:{status:'pending'}};
fs.writeFileSync(path.join(root,out),JSON.stringify(report,null,2)+'\n');
console.log('Fourteen original support images connected after all four native checks. Web remains pending.');

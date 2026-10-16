// Adopt the display/input revision only after the real Unity four-stage evidence exists.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const tag=process.argv[2];if(!/^[a-z0-9_]+$/.test(tag||''))throw Error('Verified tag required');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const assert=(ok,msg)=>{if(!ok)throw Error(msg);};
function atomic(p,bytes){fs.mkdirSync(path.dirname(p),{recursive:true});const temp=p+'.pending';fs.writeFileSync(temp,bytes);fs.renameSync(temp,p);}
function write(p,v){atomic(p,JSON.stringify(v,null,2)+'\n');}
function copy(from,to){if(!fs.existsSync(to)||sha(from)!==sha(to))atomic(to,fs.readFileSync(from));}
function counts(platform){const xml=fs.readFileSync(`TestResults/${tag}_${platform}.xml`,'utf8').match(/<test-run\b[^>]*>/)[0];const a=Object.fromEntries([...xml.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));assert(a.result==='Passed'&&a.total===a.passed&&a.failed==='0'&&a.skipped==='0','Real Unity '+platform+' must fully pass');return Object.fromEntries(['total','passed','failed','skipped'].map(k=>[k,+a[k]]));}
async function sheet(files,destination,columns=5){const rows=Math.ceil(files.length/columns);const inputs=await Promise.all(files.map(async(file,i)=>({input:await sharp(file).resize(260,290,{fit:'contain',background:'#efebe3'}).png().toBuffer(),left:i%columns*260,top:Math.floor(i/columns)*290})));atomic(destination,await sharp({create:{width:columns*260,height:rows*290,channels:4,background:'#efebe3'}}).composite(inputs).png().toBuffer());}
async function main(){
 const edit=counts('EditMode'),play=counts('PlayMode');
 const buildAll=fs.readFileSync(`TestResults/${tag}_buildall.log`,'utf8');
 assert(buildAll.includes('return code 0')&&buildAll.includes('生成完了:'),'BuildAll failed');
 assert(fs.readFileSync(`TestResults/${tag}_build.log`,'utf8').includes('Build Finished, Result: Success.')&&fs.existsSync(`Build/${tag}/AniBato.exe`),'Windows failed');
 const root='art_source/paper_roster_action/toma/',m=read(root+'manifest.json'),packed=read(root+'packed-manifest.json'),dual=read(root+'dual_gun/manifest.json');
 assert(dual.entries.length===2&&dual.entries.find(e=>e.id==='normal3_impact').source.endsWith('_v3.png'),'Latest palm-side v3 required');
 for(const e of dual.entries){const f=packed.frames.find(f=>f.id===e.id);assert(sha(e.source)===e.sha256&&sha(f.prepared)===f.sha256&&sha(`AniBato/Assets/_Project/Art/PaperAction/toma_${e.id}.png`)===f.sha256,'Latest source must be game source');const selected=m.entries.find(v=>v.id===e.id&&v.source===e.source);selected.gameConnected=true;selected.status='game_adopted_unapproved';e.gameConnected=true;e.status=selected.status;}
 dual.verification=tag;dual.gameConnected=true;write(root+'dual_gun/manifest.json',dual);m.verifiedTag=tag;write(root+'manifest.json',m);
 const index=read(root+'review-index.json');index.unityVerifiedTag=tag;
 for(const e of dual.entries){const row=index.entries.find(r=>r.id===e.id);Object.assign(row,{source:e.source,prepared:e.prepared,version:e.version,runtime:`AniBato/Assets/_Project/Art/PaperAction/toma_${e.id}.png`,status:'ゲーム採用・上側左手は掌側・美術最終承認は別'});}
 write(root+'review-index.json',index);
 const adoption=read(root+'adoption.json');adoption.normalRevision={tag,normalUniqueFrames:4,bodyUniqueFrames:14,comboSecondUsesFirst:true,third:'dual-pistol; upper left palm side; impact raw v3'};write(root+'adoption.json',adoption);
 const review=read(root+'part-review.json');review.dualGunRevision={tag,rawImpactVersion:3,userRequirement:'上側左手は掌と握った指の内側、下側右手は従来の甲側。v2を却下。',artApproved:false,canvas:'1304x1450',pixelsPerUnit:532.5893,resampledCharacterPixels:0,croppedAlphaOnePixels:dual.entries.map(e=>({id:e.id,count:e.croppedOutsideAlphaOnePixels}))};write(root+'part-review.json',review);
 const folder='イラスト確認/透真/攻撃原画_ゲーム採用済み/';
 for(const e of dual.entries)copy(e.prepared,folder+(e.phase==='prepare'?'通常3段_予備.png':'通常3段_当てる.png'));
 const current=packed.frames.filter(f=>!f.id.startsWith('normal2')).map(f=>f.prepared);assert(current.length===14,'14 unique body images');
 await sheet(['art_source/paper_roster_idle/prepared/toma_idle.png',...current],folder+'一覧_比較用.png');
 atomic(folder+'必ず読んでください.txt',`現在のゲームは通常4原画（1・2段共通ピストル、3段二丁拳銃）を含む14姿勢と飛翔物3枚。3段の当てる絵はv3で上側左手の掌が見える版、上側の甲が見えるv2は使用しません。${tag}でEdit${edit.passed}/${edit.total}・Play${play.passed}/${play.total}・BuildAll／Windows成功。最終美術承認は別です。一覧は縮小比較専用。\n`);
 const four='イラスト確認/全9人_通常4原画/透真/';
 copy(dual.entries.find(e=>e.phase==='prepare').prepared,four+'03_通常3段_大きな予備.png');copy(dual.entries.find(e=>e.phase==='impact').prepared,four+'04_通常3段_大きく当てる.png');
 await sheet([four+'00_待機基準.png',four+'01_通常1・2段共通_予備.png',four+'02_通常1・2段共通_当てる.png',four+'03_通常3段_大きな予備.png',four+'04_通常3段_大きく当てる.png'],four+'一覧_縮小比較専用.png');
 atomic(four+'必ず読んでください.txt',`現在は1・2段ピストル、3段二丁拳銃。上側左手は掌側のv3。${tag}全4工程合格。美術最終承認は別。原画を再拡縮していません。\n`);
 const chi='イラスト確認/千鶴/01_ゲームで採用済み/';
 for(const phase of ['anticipation','impact'])copy(`AniBato/Assets/_Project/Art/Paper/pose_special_neutral_${phase}_paper_chizuru.png`,chi+(phase==='anticipation'?'特殊技・おにぎり_構え_採用済み.png':'特殊技・おにぎり_投げ終わり_採用済み.png'));
 copy('AniBato/Assets/_Project/Art/Paper/pose_special_neutral_anticipation_paper_chizuru.png',chi+'特殊技・おにぎり_採用済み.png');
 const cr=read('art_source/paper_cutout/chizuru-release-review.json');cr.gameConnected=true;cr.verifiedTag=tag;write('art_source/paper_cutout/chizuru-release-review.json',cr);
 const dest='docs/screenshots/paper_release/';fs.mkdirSync(dest,{recursive:true});let screenshots=0;
 for(const file of fs.readdirSync(`TestResults/${tag}_screenshots/paper_release`).filter(f=>f.endsWith('.png'))){copy(`TestResults/${tag}_screenshots/paper_release/`+file,dest+file);screenshots++;}
 for(const file of fs.readdirSync(`TestResults/${tag}_screenshots/rework_r4_toma`).filter(f=>/^normal3_.*\.png$/.test(f)||/^(Drone|Rocket)_.*\.png$/.test(f))){copy(`TestResults/${tag}_screenshots/rework_r4_toma/`+file,dest+file);screenshots++;}
 let captureVerification=null;
 const captureTag=process.argv[3];
 if(captureTag){
  assert(/^[a-z0-9_]+$/.test(captureTag),'Invalid capture tag');
  const xml=fs.readFileSync(`TestResults/${captureTag}_PlayMode.xml`,'utf8').match(/<test-run\b[^>]*>/)[0];assert(/result="Passed"/.test(xml)&&/failed="0"/.test(xml),'Supplemental real capture test must pass');
  const files=fs.readdirSync(`TestResults/${captureTag}_screenshots/rework_r4_toma`).filter(f=>/^projectile_(Drone|Rocket)_.*\.png$/.test(f));assert(files.length===4,'Four complete projectile views required');
  for(const file of files){copy(`TestResults/${captureTag}_screenshots/rework_r4_toma/`+file,dest+file);screenshots++;}
  captureVerification={tag:captureTag,scope:'enlarged drone and rocket capture bounds, both facings',playPassed:1};
 }
 const record={tag,buildAll:'success',edit,play,windowsBuild:'success',screenshots:dest,screenshotCount:screenshots,captureVerification,webGL:'browser verification recorded separately',artApproved:false};write('art_source/paper_roster_action/toma/dual_gun/adoption.json',record);write('art_source/paper_cutout/release-adoption.json',record);console.log(JSON.stringify(record));
}
main().catch(e=>{console.error(e.stack);process.exitCode=1;});

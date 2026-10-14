// Mark the exact integrated sources after actual Unity verification, preserving art issues.
const fs=require('node:fs'),crypto=require('node:crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const tag=process.argv[2];if(!tag||!/^[a-z0-9_]+$/.test(tag))throw Error('Verification tag required');
function result(platform){
 const xml=fs.readFileSync('TestResults/'+tag+'_'+platform+'.xml','utf8');const opening=xml.match(/<test-run\b([^>]*)>/);if(!opening)throw Error('Missing test result');
 const value=k=>{const m=opening[1].match(new RegExp('\\b'+k+'="(\\d+)"'));if(!m)throw Error('Missing '+k);return Number(m[1]);};
 const passed=value('passed');if(value('failed')!==0||value('skipped')!==0||passed!==value('total'))throw Error(platform+' is not all passed');return passed;
}
const editPassed=result('EditMode'),playPassed=result('PlayMode');
if(!fs.readFileSync('TestResults/'+tag+'_build.log','utf8').includes('Build Finished, Result: Success.')||!fs.existsSync('Build/'+tag+'/AniBato.exe'))throw Error('Windows build missing');
if(!fs.readFileSync('TestResults/'+tag+'_buildall.log','utf8').includes('Exiting batchmode successfully now!'))throw Error('BuildAll unsuccessful');
const catPath='art_source/paper_cutout/review-catalog.json',cat=read(catPath),manifest=read('art_source/paper_cutout/runtime-manifest.json');
const active=new Map([...manifest.poses.flatMap(p=>p.frames||[p]),...manifest.effects].map(p=>[p.source,p]));
const previous=new Map(cat.entries.map(r=>[r.key,r.category]));
for(const row of cat.entries){
 const source=active.get(row.source);if(!source)throw Error('Not integrated: '+row.source);
 if(crypto.createHash('sha256').update(fs.readFileSync(row.source)).digest('hex')!==source.sha256)throw Error('Source changed');
 row.artReview=row.artReview||source.artReview||'not_applicable';
 row.category='adopted';row.gameConnected=true;
 row.artNote=source.artNote||row.artNote||'';
 row.note=(row.key==='idle'||row.key==='walk')?'ゲーム接続済み。待機は小さく揺れ、歩きは進行方向へ15度固定。':
  'ユーザー指定でゲームへ先行接続済み。'+(row.artReview==='known_art_issue'?'絵の修正残：'+row.artNote.replace(/ゲーム未採用/g,'絵の最終審査は未完了'):'部位サイズと手の最終確認は残ります。')+' 原画の拡縮はしていません。';
}
cat.runtimeIntegration={verified:true,tag,editPassed,playPassed,buildAll:true,windowsBuild:true,artApproval:false};
fs.writeFileSync(catPath,JSON.stringify(cat,null,2)+'\n');
const handPath='art_source/paper_cutout/hand-review.json',hand=read(handPath);
hand.note='全ポーズをユーザー指定でゲームへ先行接続済み。approvedは絵としての最終審査であり、gameAdoptedと区別する。';
for(const row of hand.entries){row.gameAdopted=true;row.status='integrated_art_review_pending';row.note=cat.entries.find(r=>r.key===row.key).note;}
fs.writeFileSync(handPath,JSON.stringify(hand,null,2)+'\n');
const planPath='art_source/paper_cutout/all-actions-plan.json',plan=read(planPath);
for(const action of plan.actions){const row=cat.entries.find(r=>r.key===action.key);action.selected=row.source;action.version=Number((row.source.match(/_v(\d+)\.png$/)||[])[1]||row.version||1);action.frames=manifest.poses.find(p=>p.key===action.key)?.frames;action.gameConnected=true;}
for(const effect of plan.effects){const row=cat.entries.find(r=>r.key==='effect_'+effect.key);if(row)effect.selected=row.source;effect.gameConnected=!!row;}
plan.run.selected=cat.entries.find(r=>r.key==='run').source;plan.run.normalizedCanvasPending=false;plan.run.gameConnected=true;
plan.runtimeManifest='runtime-manifest.json';plan.integration=cat.runtimeIntegration;
fs.writeFileSync(planPath,JSON.stringify(plan,null,2)+'\n');
if(manifest.normalWeapon==='frying_pan'){
 const comparePath='art_source/paper_cutout/variants/fryingpan/comparison-manifest.json',compare=read(comparePath);
 compare.status='eight_sources_integrated_four_comparison_only';compare.verification=cat.runtimeIntegration;
 for(const row of compare.entries)row.gameConnected=active.has(row.prepared);
 fs.writeFileSync(comparePath,JSON.stringify(compare,null,2)+'\n');
 fs.writeFileSync('イラスト確認/千鶴/予備_カバン版_削除禁止/現在の扱い.txt','カバン版は予備として保護。現在ゲーム内の全通常攻撃はフライパンです。以前の採用表示も削除せず保管しています。\n');
 fs.writeFileSync('art_source/paper_cutout/variants/fryingpan/現在の採用.txt','横1段・横2段・上下3段の組合せをゲームで採用。上下2段も方向別攻撃に使用。使用原画8枚、残り4枚は比較用。正確な対応はruntime-manifest.json。現在の名前付き画像はイラスト確認/千鶴/01_ゲームで採用済み/。\n');
}
process.stdout.write(JSON.stringify({adoptedImages:cat.entries.length,editPassed,playPassed,previousCategories:Object.fromEntries(previous)})+'\n');

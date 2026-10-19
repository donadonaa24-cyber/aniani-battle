// Collect genuine per-stage results; never rename an earlier run into a later tag.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const check=(v,message)=>{if(!v)throw Error(message);};
const buildTag='special_refinement_3',playTag='special_refinement_5',bundle='special_refinement_3_5';
function tests(tag,platform){
 const file=`TestResults/${tag}_${platform}.xml`,xml=fs.readFileSync(file,'utf8'),root=xml.match(/<test-run\s[^>]*>/)[0];
 const attr=k=>root.match(new RegExp('\\b'+k+'="([^"]*)"'))?.[1];
 const row={tag,platform,source:file,sha256:sha(file),total:Number(attr('total')),passed:Number(attr('passed')),failed:Number(attr('failed')),skipped:Number(attr('skipped')),seconds:Number(attr('duration'))};
 check(attr('result')==='Passed'&&row.total>0&&row.passed===row.total&&row.failed===0&&row.skipped===0,`Tests did not all pass: ${file}`);return row;
}
const edit=tests(buildTag,'EditMode'),play=tests(playTag,'PlayMode');
const build=`TestResults/${buildTag}_buildall.log`,windows=`TestResults/${buildTag}_build.log`;
check(fs.readFileSync(build,'utf8').includes('Exiting batchmode successfully now!'),'BuildAll did not complete');
check(fs.readFileSync(windows,'utf8').includes('Build Finished, Result: Success.'),'Windows build did not succeed');
check(fs.existsSync(`Build/${buildTag}/AniBato.exe`),'Windows executable missing');
function files(dir){return fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?files(path.join(dir,e.name)):[path.join(dir,e.name)]);}
const production=files('AniBato/Assets/_Project/Scripts').filter(p=>p.endsWith('.cs')).sort();
const buildTime=fs.statSync(windows).mtimeMs;
check(production.every(p=>fs.statSync(p).mtimeMs<=buildTime),'Production code changed after the validated Windows build');
const productionSha=crypto.createHash('sha256').update(production.map(p=>p.replaceAll('\\','/')+':'+sha(p)).join('\n')).digest('hex');
const base='art_source/paper_roster_action/akatsuki/',manifest=read(base+'manifest.json'),packed=read(base+'packed-manifest.json');
const adoption=read(base+'current-adoption.json');
for(const id of ['normal1_prepare','normal1_impact']){
 const job=manifest.jobs.find(j=>j.id===id),entry=manifest.entries.find(e=>e.id===id&&e.version===job.version);
 const frame=packed.frames.find(f=>f.id===id),current=adoption.entries.find(e=>e.id===id);
 check(entry&&frame&&current,'Slash adoption entry missing');
 check(sha(entry.source)===entry.sha256&&sha(frame.prepared)===frame.sha256&&sha(current.runtime)===frame.sha256,'Slash source/runtime mismatch');
 Object.assign(entry,{gameConnected:true,status:'native_verified_art_approval_pending',unityVerifiedTag:bundle});
 current.sha256=frame.sha256;fs.copyFileSync(current.runtime,current.reviewFile);
}
// The old upper preparation shared the former normal texture. It now has its own
// unchanged PNG, so resolve that former alias without adopting the new slash there.
for(const current of adoption.entries){
 if(sha(current.runtime)===current.sha256)continue;
 const canonical=`AniBato/Assets/_Project/Art/PaperAction/akatsuki_${current.id}.png`;
 check(fs.existsSync(canonical)&&sha(canonical)===current.sha256,'Unresolved unchanged artwork alias: '+current.id);
 current.runtime=canonical;fs.copyFileSync(canonical,current.reviewFile);
}
Object.assign(manifest,{integrated:true,verifiedTag:bundle});Object.assign(packed,{reviewed:true,gameConnected:true,verifiedTag:bundle});
Object.assign(adoption,{unityVerifiedTag:bundle,uniqueBodyImages:10});
write(base+'manifest.json',manifest);write(base+'packed-manifest.json',packed);write(base+'current-adoption.json',adoption);
write('イラスト確認/現在ゲームで採用中/暁/採用一覧.json',adoption);
const slash=read('art_source/directional_specials/akatsuki-slash-adoption.json');
Object.assign(slash,{gameConnected:true,nativeVerifiedTag:bundle,artApproved:false});
for(const entry of slash.entries)Object.assign(entry,{gameConnected:true,status:'native_verified_art_approval_pending',unityVerifiedTag:bundle});
write('art_source/directional_specials/akatsuki-slash-adoption.json',slash);
const curry='art_source/directional_specials/chizuru_curry_residue_v1.png',curryRuntime='AniBato/Assets/_Project/Art/PaperAction/directional_chizuru_curry_residue_v1.png';
check(sha(curry)===sha(curryRuntime),'Curry source/runtime mismatch');
const curryReview='イラスト確認/現在ゲームで採用中/千鶴/超必殺技_残留カレー.png';fs.copyFileSync(curryRuntime,curryReview);
write('art_source/directional_specials/curry-residue-adoption.json',{source:curry,runtime:curryRuntime,reviewFile:curryReview,sha256:sha(curry),nativeWidth:2172,nativeHeight:724,noResampling:true,gameConnected:true,nativeVerifiedTag:bundle,artApproved:false});
const status=read('art_source/directional_specials/artwork-status.json');
for(const entry of status.entries)if([path.basename(curry),'akatsuki_slash_prepare_v2.png','akatsuki_slash_impact_v1.png'].includes(entry.file))entry.state='native_connected_verified_'+bundle;
write('art_source/directional_specials/artwork-status.json',status);
const progressPath='docs/codex_tasks/directional_specials_progress.json',progress=read(progressPath);
for(const entry of progress.requirements)if(['touma_guns_drone_rocket','king_three_supports_special_and_ultimate','ryuta_reading_buffs','akatsuki_pyramid_card_slash_and_trap','chizuru_curry_and_meal','all_nine_stamina','artwork_and_visuals','native_all_four'].includes(entry.id))entry.status='native_verified_'+bundle;
progress.nextTask='DIRECTIONAL_SPECIALS_FINAL_WEB_AUDIT.md';write(progressPath,progress);
const captures=`TestResults/${playTag}/captures`,destination=`docs/screenshots/${bundle}`;fs.mkdirSync(destination,{recursive:true});
const selected=['special_refinement/pyramid_landed_wall_1.png','special_refinement/curry_settled_1.png','special_refinement/curry_settled_-1.png','special_refinement/akatsuki_card_consumed_1.png','special_refinement/ryuta_spell_speed.png'];
for(const file of selected){check(fs.existsSync(captures+'/'+file),'Capture missing: '+file);fs.copyFileSync(captures+'/'+file,destination+'/'+path.basename(file));}
const report={schemaVersion:1,date:'2026-10-20',bundle,goalComplete:false,nativeComplete:true,tests:[edit,play],
 buildAll:{tag:buildTag,source:build,sha256:sha(build),success:true},windows:{tag:buildTag,source:windows,sha256:sha(windows),executable:`Build/${buildTag}/AniBato.exe`,success:true},
 productionScriptsSha256:productionSha,productionScripts:production.length,
 splitRunReason:'Only the PlayMode test changed after run 3: consumed drones are verified by their exact hit event and heart loss; independent summon members are verified through their parent group. Production scripts and assets were not changed.',
 cardRules:{opponentOnly:true,consumeOnHit:true,maximumPerOwner:2,unhitSeconds:20,guardConsumes:false},
 captures,durableCaptures:destination,captureCount:files(captures).filter(p=>p.endsWith('.png')).length,
 captureNote:'Camera stage captures omit the HUD overlay. Simulation and HUD state are asserted by real Unity tests. Art human approval is separate.',
 previousFindings:['run 1: vector subtraction compile error fixed, Edit 690/693; fixed wall push separation and old sprite/shot assumptions',
 'focus 2: wall overlap asserted during hitstop; assert after thaw while retaining exact 600-tick lifetime',
 'focus 3: Edit 48/48, Play 4/6; preserve formation identity and test an unobstructed falling path',
 'focus 4: Play 6/6','run 2: cancelled during Edit to align pyramid bottom and collision height',
 'run 3: Edit 697/697, Play 90/91, BuildAll and Windows pass; consumed drone was incorrectly expected to remain visible',
 'run 4: cancelled to fix that test; consumption focus passes all 9 characters and 18 skills'],
 preserved:['45 halved normal hitboxes','14/10 gauge','four normal keys, one down and one aerial key','Ryuta approved hit v5','protected bag artwork','512/4096 Web memory'],
 remaining:['actual final WebGL build','RGBA audit','browser input/audio/display/console verification','whole-requirement audit']};
write('docs/codex_logs/directional_refinement_verification.json',report);
console.log(JSON.stringify({bundle,edit:edit.passed,play:play.passed,captures:report.captureCount,nativeComplete:true,goalComplete:false}));

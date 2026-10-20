// Finalize only after all four real Unity stages pass, and retain original artwork versions.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const tag=process.argv[2],read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const assert=(v,m)=>{if(!v)throw Error(m);};
assert(/^[a-z0-9_]+$/.test(tag||''),'Verification tag required');
const result=platform=>{
 const file=`TestResults/${tag}_${platform}.xml`,text=fs.readFileSync(file,'utf8'),run=text.match(/<test-run\b[^>]*>/)[0];
 const attrs=Object.fromEntries([...run.matchAll(/([\w-]+)="([^"]*)"/g)].map(m=>[m[1],m[2]]));
 assert(attrs.result==='Passed'&&+attrs.failed===0&&+attrs.total===+attrs.passed&&+attrs.skipped===0,platform+' incomplete');
 return {total:+attrs.total,passed:+attrs.passed,failed:0,sha256:sha(file)};
};
const edit=result('EditMode'),play=result('PlayMode');
const all=`TestResults/${tag}_buildall.log`,build=`TestResults/${tag}_build.log`;
assert(fs.readFileSync(all,'utf8').includes('生成完了:')&&fs.readFileSync(all,'utf8').includes('return code 0'),'BuildAll incomplete');
assert(fs.readFileSync(build,'utf8').includes('Build Finished, Result: Success.')&&fs.existsSync(`Build/${tag}/AniBato.exe`),'Windows incomplete');
const base='art_source/paper_roster_action/ryuta/',manifest=read(base+'manifest.json'),packed=read(base+'packed-manifest.json'),current=read(base+'current-adoption.json');
const kick=read('art_source/ryuta_kick/adoption.json');
for(const phase of ['prepare','impact']){
 const id='normal3_'+phase,job=manifest.jobs.find(j=>j.id===id),entry=manifest.entries.find(e=>e.id===id&&e.version===job.version);
 const live=current.entries.find(e=>e.id===id),frame=packed.frames.find(f=>f.id===id);
 assert(sha(entry.source)===entry.sha256&&sha(entry.prepared)===frame.sha256&&sha(live.runtime)===frame.sha256,'Native kick/runtime mismatch');
 live.sha256=sha(live.runtime);fs.copyFileSync(live.runtime,live.reviewFile);
 entry.label=job.label=phase==='prepare'?'通常3段の前蹴り予備':'通常3段の前蹴り';
 entry.references=phase==='prepare'?['art_source/paper_roster_action/ryuta/generated/normal1_impact_v3.png']:
  ['art_source/ryuta_kick/prepare_v1.png','art_source/paper_roster_action/ryuta/generated/normal1_impact_v3.png'];
 delete entry.referenceRoles;
 Object.assign(entry,{gameConnected:true,artApproved:false,status:'game_adopted_unapproved',unityVerifiedTag:tag});
 const row=kick.entries.find(e=>e.id===id);Object.assign(row,entry);
}
for(const phase of ['prepare','impact']){
 const live=current.entries.find(e=>e.id==='up_'+phase),runtime='AniBato/Assets/_Project/Art/PaperAction/ryuta_up_'+phase+'.png';
 assert(sha(runtime)===live.sha256,'Original uppercut changed');
 live.runtime=runtime;fs.copyFileSync(runtime,live.reviewFile);
}
if(current.unityVerifiedTag!==tag)current.previousUnityVerifiedTag=current.unityVerifiedTag;
Object.assign(current,{unityVerifiedTag:tag,uniqueBodyImages:12});
Object.assign(kick,{gameConnected:true,unityVerifiedTag:tag,artApproved:false});
Object.assign(packed,{verifiedTag:tag,gameConnected:true});Object.assign(manifest,{verifiedTag:tag});
write(base+'manifest.json',manifest);write(base+'packed-manifest.json',packed);write(base+'current-adoption.json',current);write('art_source/ryuta_kick/adoption.json',kick);
for(const name of ['adoption.json','review-index.json']){
 const file=base+name;if(!fs.existsSync(file))continue;const value=read(file);
 Object.assign(value,{tag,body:current.entries,uniqueBodyImages:12,latestNote:'通常3段は蹴り2原画、通常1/2・上攻撃は維持。龍太通常の追加オーバーレイのみ非表示。',edit,play});write(file,value);
}
const rift=read('art_source/king_march/adoption.json');assert(sha(rift.source)===sha(rift.runtime),'Rift RGBA source differs');
Object.assign(rift,{gameConnected:true,unityVerifiedTag:tag,artApproved:false});write('art_source/king_march/adoption.json',rift);
const report={tag,buildAll:{success:true,sha256:sha(all)},edit,play,windows:{success:true,sha256:sha(build)},
 artwork:{kick:'art_source/ryuta_kick/adoption.json',rift:'art_source/king_march/adoption.json',artApproved:false},
 baseline:'docs/codex_logs/stamina_baseline.json',tuning:'docs/codex_logs/stamina_final.json',
 notes:'All stages actually executed on this tag. No personal absolute paths. Existing illustration versions retained.'};
write('docs/codex_logs/readable_stamina_verification.json',report);console.log(JSON.stringify(report));

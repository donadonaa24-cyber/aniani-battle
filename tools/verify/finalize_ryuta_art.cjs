const fs=require('fs'),sharp=require('sharp'),root='art_source/paper_roster_action/ryuta/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,'')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
async function main(){
 const m=read(root+'effects-manifest.json'),punch=m.entries.find(e=>e.id==='punch');if(!punch)throw Error('Save effects first');
 for(const [id,state] of [['punch_second','Attack2'],['punch_third','Attack3'],['punch_back','BackAttack']]){
  if(m.jobs.some(j=>j.id===id))continue;
  const e={...punch,id,state,reuseOf:{id:'punch',version:punch.version,sha256:punch.sha256},newlyGenerated:false,reason:'通常の衝撃を共有。3段は上方へ大きく表示。'};
  if(state==='Attack3')Object.assign(e,{worldWidth:1.6,worldHeight:1.6,offsetX:-.2,offsetY:1});
  const file=root+'prompts/effect_'+id+'_v1.json';write(file,e);m.jobs.push({id,version:1,promptFile:file});m.entries.push(e);
 }
 for(const id of ['spell_prepare','spell_impact']){
  const job=m.jobs.find(j=>j.id===id),e=m.entries.find(e=>e.id===id&&e.version===job.version);if(!e)throw Error('Missing spell');
  e.unmirroredArtwork=true;
  if(id==='spell_prepare')Object.assign(e,{separateAnticipationOffset:true,offsetX:.4,offsetY:.5});
  const j=read(job.promptFile);Object.assign(j,{unmirroredArtwork:e.unmirroredArtwork,separateAnticipationOffset:e.separateAnticipationOffset||false,offsetX:e.offsetX,offsetY:e.offsetY});write(job.promptFile,j);
 }
 write(root+'effects-manifest.json',m);
 const body=read(root+'manifest.json'),selected=body.jobs.map(j=>body.entries.find(e=>e.id===j.id&&e.version===j.version));
 const effects=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
 if(selected.some(e=>!e)||effects.some(e=>!e))throw Error('Incomplete');
 for(const e of effects.filter((e,i,a)=>a.findIndex(v=>v.sha256===e.sha256)===i))await sharp(e.source).flatten({background:'#ffffff'}).png().toFile(root+'review/effect_'+e.id+'_on_white.png');
 write(root+'part-review.json',{schemaVersion:1,key:'ryuta',bodySlotsCompleted:12,uniqueBodyImages:10,artApproved:false,
  selected:selected.map(({id,version,source,sha256,native})=>({id,version,source,sha256,native})),effects:effects.map(({id,source,sha256,native})=>({id,source,sha256,native})),
  findings:[
   '通常1・2段は左ジャブの予備v1と決めv3。甲側左拳の親指は上、旧v1/v2の下側親指を修正。下決めv2も同じ基準へ修正し、握りで隠れる指を表へ増やしていない。',
   '通常3予備v1で画像左の腰へ引いた右腕を、決めv3では同じ手前右肩から画像左上へ突き上げる。旧決めv1/v2は奥の左肩が上がっていたため不採用。上2枠はこの2原画を原寸共有。',
   'バットは左手がノブ側、右手が先端側の順を維持。英語本は同じ紺と金の開いた本。決めは左手で本、右手は人差し指と上の親指と握る3指。これは本作独自の演出。',
   '採用待機の頭髪上部335px、靴158/252pxを参照。姿勢の投影と生成の微差が残り、全ての部位が完全同寸とは扱わない。低姿勢を梱包時に拡大せず原画画素を保持。',
   'アッパーの上余白7px、バット予備9px／決め右9px、下右14pxなど要求40px未満がある。可視輪郭は切れていない。FXも余白80pxに満たない箇所がある。',
   '原画の透明RGBと可視alphaを区別し、白背景診断を別保存。透明原本を編集しない。絵の最終承認はユーザー確認と区別する。',
   '呪文の予備効果は本の近く、決めは前方の別位置に表示。文字効果は左右反転せず位置だけ向きに合わせる。人物全体の左右反転とは別。'
  ]});
 console.log(JSON.stringify({slots:selected.length,uniqueBodies:new Set(selected.map(e=>e.sha256)).size,effectBindings:effects.length,uniqueEffects:new Set(effects.map(e=>e.sha256)).size}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

const fs=require('fs'),sharp=require('sharp'),root='art_source/paper_roster_action/akatsuki/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
async function main(){
 const m=read(root+'effects-manifest.json'),slash=m.entries.find(e=>e.id==='slash'&&e.version===1);
 if(!slash)throw Error('Selected slash required');
 for(const [id,state] of [['slash_second','Attack2'],['slash_back','BackAttack']]){
  if(m.jobs.some(j=>j.id===id))continue;
  const e={...slash,id,state,reuseOf:{id:'slash',version:1,sha256:slash.sha256},newlyGenerated:false,reason:'通常1・2段と背後に同じ切り絵斬撃を原寸共有。'};
  const file=root+'prompts/effect_'+id+'_v1.json';write(file,e);m.jobs.push({id,version:1,promptFile:file});m.entries.push(e);
 }
 write(root+'effects-manifest.json',m);
 const body=read(root+'manifest.json'),selected=body.jobs.map(j=>body.entries.find(e=>e.id===j.id&&e.version===j.version));
 const effects=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version));
 for(const e of effects.filter((e,i,a)=>a.findIndex(v=>v.sha256===e.sha256)===i))
  await sharp(e.source).flatten({background:'#ffffff'}).png().toFile(root+'review/effect_'+e.id+'_on_white.png');
 await sharp(root+'generated/normal3_impact_v2.png').extract({left:935,top:297,width:220,height:190}).resize(660,570,{kernel:'nearest'}).flatten({background:'#ffffff'}).png().toFile(root+'review/normal3_impact_v2_hand_3x.png');
 write(root+'part-review.json',{
  schemaVersion:1,key:'akatsuki',bodySlotsCompleted:12,uniqueBodyImages:9,artApproved:false,
  selected:selected.map(({id,version,source,sha256,native})=>({id,version,source,sha256,native})),
  effects:effects.map(({id,version,source,sha256,native})=>({id,version,source,sha256,native})),
  findings:[
   '本人の左肩から画面右の肘・手首へつながる左腕でカードを保持。単純な全身反転で手を修正していない。',
   '通常予備v1の頭身過大は不採用、v2を選定。通常3投げ終わりv1は小指不足で不採用、v2は親指と4指の掌側。特殊の投げ終わりも同じv2を再利用。',
   '左の甲が見える保持と掌側の保持が姿勢で変わる。握りで隠れる指を5本すべて見えるとは主張しない。上決めは左の甲、左の親指は画像左側。',
   '待機の髪約335pxと靴を原寸で比較。投影・姿勢で見かけの幅は変わるため全姿勢で全く同寸とは扱わない。原画は一切再サンプルせず1304×1450へ透明梱包。',
   '上予備は通常予備、特殊2枠は通常3段を共有し12表示枠／9原画。下・空中Nは決め1枚のみ。',
   '超必殺は両手で砂をこねる予備と、空手の左人差し指で前方上へ指令する決め。指令の向きを下向きと誤記しない。ピラミッドは別描画で相手頭上から落下。',
   '正式背面を参照したカードv2を選定。身体の小さい印刷は省略や文字差が残り、正式背面と全画素同一と扱わない。',
   '原画プレビューの透明画素にあるRGBは不透明な灰色背景ではない。白背景診断とalphaを区別し、透明原本を保持。',
   'ピラミッドは左右26px、斬撃は右33pxなど要求80pxより狭い透明余白があるが可視輪郭は切れていない。美術最終承認と実装上の選定を区別。',
   '通常3段の実投射は明示された技指定による移行。元の発生／硬直・前進・1通常被弾と半減後の最遠端を維持し別途テストする。'
  ]
 });
 console.log(JSON.stringify({bindings:effects.length,uniqueEffects:new Set(effects.map(e=>e.sha256)).size,slots:selected.length}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

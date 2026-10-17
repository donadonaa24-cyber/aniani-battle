const fs=require('fs'),root='art_source/paper_roster_action/king/',read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const m=read(root+'effects-manifest.json'),claw=m.entries.find(e=>e.id==='claw'&&e.version===1);
if(!claw)throw Error('Selected claw required');
for(const [id,state] of [['claw_second','Attack2'],['claw_back','BackAttack']]){
 if(m.jobs.some(j=>j.id===id))continue;
 const j={...claw,id,state,reuseOf:{id:'claw',version:1,sha256:claw.sha256},newlyGenerated:false,reason:'通常1・2段と背後の爪に同じ原画を共有。'};
 const file=root+'prompts/effect_'+id+'_v1.json';write(file,j);m.jobs.push({id,version:1,promptFile:file});m.entries.push(j);
}
write(root+'effects-manifest.json',m);
const review=read(root+'part-review.json'),body=read(root+'manifest.json');
review.bodySlotsCompleted=12;review.uniqueBodyImages=9;
review.selected=body.jobs.map(j=>body.entries.find(e=>e.id===j.id&&e.version===j.version)).map(e=>({id:e.id,version:e.version,source:e.source,sha256:e.sha256,native:e.native}));
review.findings.push('上は余分な腕を除いたv2、下は爪ブーツ3本ずつへ直したv4、空中は両足が浮いたv1。上と特殊の予備に共通通常予備、超必殺の決めに王を原寸再利用。',
 '王の字は正面から読める原画のまま。表示の左右反転で文字を逆にせず、部下を落とす性能は既存のまま。',
 'ユーザー指定の部下は豹妬レイラ・銀狼シオン・鹿角ノア。既存ポータルの原作容姿を確認し、汎用の歩は生成せず中止。全身人物の落下攻撃として本作独自のポーズを生成。',
 'レイラv1は豹フード・爪・髪と尾、シオンv3は狼フード・杖と両膝／ブーツ2個、ノアv3は鹿角・紫結晶の杖と左手の下向き指示。握りで隠れる指を5本見えると主張しない。',
 'ノアv1/v2は左手の6指を原寸3倍の診断画像で確認し不採用。v3は人差し指のみ伸ばし中指・薬指・小指を曲げ親指を添える。シオンv1の3脚状とv2の片足しか見えない形は不採用。',
 '召喚人物の角やマントの余白は指定40pxより狭い箇所が残る。ノア上9px、レイラ右3px、シオン右6px。可視alpha32の輪郭は原画端から離れている。原寸を保存し、最終美術承認と区別。',
 '咆哮v1/v2は光彩が残り不採用、v3は平たい括弧状の紙圧力線。爪と咆哮以外の人物に効果を焼き込まず別描画。');
review.effects=m.jobs.map(j=>m.entries.find(e=>e.id===j.id&&e.version===j.version)).map(e=>({id:e.id,version:e.version,source:e.source,sha256:e.sha256,native:e.native}));
review.rejectedEffects=m.entries.filter(e=>e.status==='rejected').map(e=>({id:e.id,version:e.version,reason:e.rejectionReason}));write(root+'part-review.json',review);
console.log(JSON.stringify({bindings:m.jobs.length,uniqueSources:new Set(review.effects.map(e=>e.sha256)).size}));

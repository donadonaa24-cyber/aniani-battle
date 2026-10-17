const fs=require('fs'),r='art_source/paper_roster_action/king/',read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const m=read(r+'effects-manifest.json'),job=m.jobs.find(j=>j.id==='roar'),old=m.entries.find(e=>e.id==='roar'&&e.version===2);if(job.version!==2||!old)throw Error('Expected v2');
old.status='rejected';old.rejectionReason='背景の光彩が除去されていない。';
const j={...read(job.promptFile),version:3,source:r+'generated/effect_roar_v3.png',references:[],referenceRoles:[],
prompt:'Use case: stylized-concept. 真の透明PNGのゲーム用図形一枚。三つの大きさの異なる角ばった括弧「）」を横に並べた赤・黒・金の平たい切り紙。左から小、中、大の順。各括弧は赤の太い帯と細い黒・金のふち。帯の輪郭はギザギザで硬く、細いクリーム白の切り抜き縁。右側に小さな金の三角形を三つ。背景も括弧の間も完全透明、透明部へ一切着色しない。全体を1024×1024の中央に一つ、透明余白80px以上。無地の平らなベタ色だけ。光・発光・グラデーション・グロー・ぼかし・影・背景色・背景模様・紙の質感・文字・人物なし。咆哮の紙の圧力線として使う一つの素材。'};
job.version=3;job.promptFile=r+'prompts/effect_roar_v3.json';write(job.promptFile,j);write(r+'effects-manifest.json',m);

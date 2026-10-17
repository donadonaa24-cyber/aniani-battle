const fs=require('fs'),root='art_source/paper_roster_action/king/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const m=read(root+'effects-manifest.json'),job=m.jobs.find(j=>j.id==='roar'),old=m.entries.find(e=>e.id==='roar'&&e.version===job.version);
if(job.version!==1||!old)throw Error('Expected saved roar v1');
old.status='rejected';old.rejectionReason='紙片以外に広い光彩と背景状の不透明な色がある。';
const j=read(job.promptFile);j.version=2;j.source=root+'generated/effect_roar_v2.png';j.references=[old.source];j.referenceRoles=['編集対象。光彩だけを除去'];
j.prompt='Use case: background-extraction. 画像1の赤・黒・金の切り絵の衝撃波を編集。鋭い輪郭の紙片だけを完全に不透明で残し、その紙片の外側と紙片同士の広い隙間は全て完全透明にする。赤色・金色・茶色・白のぼんやりした光彩やグラデーションを全て除去する。黒い背景色も全て除去。硬い赤・黒・金の弧と三角の紙片、薄い硬い白縁だけが残る。色を透過の市松模様に置き換えない。背景が本当に透明なPNG。全周の透明余白80px以上、紙片を切らない。人物や文字を追加しない。';
job.version=2;job.promptFile=root+'prompts/effect_roar_v2.json';write(job.promptFile,j);write(root+'effects-manifest.json',m);

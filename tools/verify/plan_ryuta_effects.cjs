const fs=require('fs'),root='art_source/paper_roster_action/ryuta/';
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Already planned');
const common='Use case: stylized-concept. オリジナル2Dペープサート格闘ゲーム用の切り絵エフェクト一個。太く硬いアニメ線、平らなセル塗り、細いクリーム白の切り抜き縁。真の透明PNG1024×1024、全周80px透明余白、切れなし。図形の隙間は完全透明。人物・手・武器・本体の本・背景・床・影・グロー・ぼかし・グラデーション・写真・3D・複数案なし。';
const effects=[
 {id:'punch',kind:'ability',look:'Default',state:'Attack1',duringStartup:false,duringRecovery:true,worldWidth:1.2,worldHeight:1.1,offsetX:1,offsetY:.15,prompt:common+'右へ当たった拳の衝撃を示す白と金の角ばった星形の破裂、中心は小さく周りに大きい透明な隙間、尖った帯3つと小さい三角の紙吹雪4つ。赤い縁を少量。文字なし。'},
 {id:'bat',kind:'ability',look:'Bat',state:'SpecialNeutral',duringStartup:false,duringRecovery:true,worldWidth:2.7,worldHeight:1.8,offsetX:1.3,offsetY:.2,prompt:common+'バットの水平スイングを示す横長の三日月の切り紙の帯を2本。白と黄土色と金、左下から右上へ振り抜く弧、帯同士の間と中央は広い完全透明。小さな星2個と三角紙片4個。バットそのものや手は描かない。文字なし。'},
 {id:'spell_prepare',kind:'ability',look:'Spell',state:'Ultimate',phase:'prepare',duringStartup:true,duringRecovery:false,worldWidth:1.2,worldHeight:.8,offsetX:.2,offsetY:.1,prompt:common+'英語の本から浮く小さい切り紙の文字A B Cの3個だけ。濃紺のA、金のB、赤のC、白い縁。それぞれ別々の角度で低い半円に並べ、間は完全透明。小さい四角紙片を3個。大きい爆発は描かない。'},
 {id:'spell_impact',kind:'ability',look:'Spell',state:'Ultimate',phase:'impact',duringStartup:false,duringRecovery:true,worldWidth:7,worldHeight:4.5,offsetX:2.5,offsetY:0,prompt:common+'謎の英語の呪文が右前へ爆発する、横長の大きい切り紙の波。大きいA B C X Y Zの文字6個と、白い角ばったページ状の紙片、金と赤と紺の鋭い弧。左から右へ広がる動き。文字は互いに離し、中央と図形間に広い透明な隙間を残す。文字や紙片が黒い不透明雲に合体しない。'}
];
const jobs=[];for(const e of effects){const j={...e,key:'ryuta',version:1,source:root+'generated/effect_'+e.id+'_v1.png',references:[],referenceRoles:[],tool:'built-in image_gen',status:'planned'};const file=root+'prompts/effect_'+e.id+'_v1.json';fs.writeFileSync(file,JSON.stringify(j,null,2)+'\n');jobs.push({id:e.id,version:1,promptFile:file});}
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'ryuta',jobs,entries:[],gameConnected:false},null,2)+'\n');
console.log('Four effect sources planned. Normal aliases are added after saving.');

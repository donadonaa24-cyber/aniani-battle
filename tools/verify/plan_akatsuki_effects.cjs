const fs=require('fs'),root='art_source/paper_roster_action/akatsuki/';
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Already planned');
const effects=[
 {id:'card',kind:'projectile',look:'Card',fitAttackHitbox:true,sortingOrder:7,references:['art_source/title_paper/references/battle_a_la_carte_card_back_user_20261017.png'],referenceRoles:['正式なカード背面の意匠'],
 prompt:'Use case: background-extraction. 参照画像はユーザー本人のゲーム『Battle à la carte』の正式なカード背面。この一枚をゲーム中で投げるカードの透過スプライトへ描き直す。正面向き、上向きにまっすぐ立てた一枚の縦長の長方形、角だけ小さく丸い。濃紺の市松、クリーム金の装飾枠、料理帽、赤黒のハート・スペード・クラブ・ダイヤ、ナイフとフォーク、中央の金色の正確な「Battle à la Carte」を参照どおり保つ。余分な赤い外枠や別の模様を加えない。細い硬いクリーム白の切り抜き縁を外周へ。カードの印刷面は不透明、外だけ完全透明。カードの紙そのものは光らせず普通のアニメのセル塗り。1024×1536の真の透明PNG一枚、上下左右80px以上の透明余白を設けカードを切らない。手・指・人物・影・背景・斜めの厚み・速度線・グロー・ぼかし・3D・複数案なし。'},
 {id:'pyramid',kind:'projectile',look:'Pyramid',fitAttackHitbox:true,sortingOrder:7,references:[],referenceRoles:[],
 prompt:'Use case: stylized-concept. オリジナル2Dペープサート格闘ゲームで頭上から落とす砂のピラミッド一個。幅の広い正三角形に近い明快な全体シルエット、上端は一つの頂点、下辺は水平。明るい黄土色と砂色の大きな石積みを、太いアニメ線と少数の平らなセル塗りで描く。中央の少し右に一本の稜線、左面は明るい砂色、右面は濃い黄土色。平面のアニメ小道具として、シンプルなピラミッドの絵一つ。細い硬いクリーム白の切り抜き縁を外周へ。1024×1024の真の透明PNG、中央に一個、全周80px以上の透明余白で底と頂点を切らない。背景・床・影・人物・顔・手足・文字・文字状の壁画・宝石・光彩・グロー・ぼかし・写実・3Dモデル・ピクセル・複数案はなし。'},
 {id:'slash',kind:'ability',look:'Default',state:'Attack1',duringStartup:false,duringRecovery:true,worldWidth:1.45,worldHeight:1.2,offsetX:.85,offsetY:.15,references:[],referenceRoles:[],
 prompt:'Use case: stylized-concept. 真の透明PNGのゲーム用図形一枚。右へ振り抜くカードの切り絵の斬撃。濃紺・金・クリーム白の短い尖った三日月の帯を2本、互いの間に広い完全透明の隙間を残し斜めに平行に並べる。周囲へ小さな赤と金の三角の紙片を4つ。太い硬いアニメ線と平らなベタ色、細い硬いクリーム白の切り抜き縁だけ。1024×1024、中央に効果一個、全周80px以上透明余白。紙片同士の隙間も完全透明。不透明な背景・光彩・グロー・ぼかし・グラデーション・人物・手・カード本体・文字・床・影・写実・3Dなし。'},
 {id:'sand',kind:'ability',look:'Pyramid',state:'Ultimate',duringStartup:true,duringRecovery:false,worldWidth:1.1,worldHeight:.6,offsetX:.25,offsetY:-.05,references:[],referenceRoles:[],
 prompt:'Use case: stylized-concept. 真の透明PNGのゲーム用図形一枚。砂をこねる手の間から立ち上がる小さい切り絵の砂粒の輪。黄土色と淡い砂色の角ばった小さな紙片を8個ほど、低い横長の半円にまばらに並べる。中央と紙片の間は広く完全透明、顔や手や器は描かない。アニメ線と平らなベタ色、薄い硬い白縁。1024×1024中央、80px以上透明余白。砂煙・不透明な雲・背景・発光・グロー・グラデーション・ぼかし・影・文字・人物・ピラミッド本体なし。'}
];
const jobs=[];for(const e of effects){const j={...e,key:'akatsuki',version:1,source:root+'generated/effect_'+e.id+'_v1.png',tool:'built-in image_gen',status:'planned'};
const file=root+'prompts/effect_'+e.id+'_v1.json';fs.writeFileSync(file,JSON.stringify(j,null,2)+'\n');jobs.push({id:e.id,version:1,promptFile:file});}
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'akatsuki',jobs,entries:[],gameConnected:false},null,2)+'\n');
const m=JSON.parse(fs.readFileSync(root+'manifest.json','utf8'));for(const job of m.jobs){const file=job.promptFile,j=JSON.parse(fs.readFileSync(file,'utf8'));j.prompt=j.prompt.replace('幅約100px／長辺約150px','幅約140px／長辺約210px');fs.writeFileSync(file,JSON.stringify(j,null,2)+'\n');}
console.log('Card first, then nine body drawings and remaining effects.');

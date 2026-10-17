const fs=require('fs');const root='art_source/paper_roster_action/yui/';
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Effect jobs already planned');
const common={kind:'ability',look:'Default',duringStartup:false,duringRecovery:true,offsetX:1.05,offsetY:.15};
const effects=[{...common,id:'tap',state:'Attack1',worldWidth:1.3,worldHeight:1.3,
 prompt:'オリジナル2Dペープサート対戦ゲームの、可愛いステッキ打撃の切り絵の衝撃エフェクト一個。小さな金色の五角の星一個を右寄りに置き、その周りへ紫とピンクの小さいハート三個、白と淡い黄色の短いギザギザの紙片六個ほどを扇状に散らす。紙片と紙片の隙間は広く完全透明にして、中央を一枚の光の雲で塗りつぶさない。クリーム白・紫・ピンク・金色の平面的な普通のアニメ塗り、画用紙を切った硬い輪郭と細いクリーム白縁。1024×1024の真の透明PNG、全体を中央に収め、上下左右に80px以上透明余白。人物・手・杖・猫・顔・文字・数字・背景・床・影・光彩・グロー・ぼかし・写真・3D・ピクセル・複数案はなし。1個のまとまった衝撃素材だけ。'},
 {...common,id:'magic',state:'Attack3',worldWidth:1.8,worldHeight:1.4,
 prompt:'オリジナル2Dペープサート対戦ゲームの、ステッキから右へ放つ可愛い魔法の切り絵エフェクト一個。右端寄りに大きい金色の五角星、その左後方から右へ押し出す紫・ラベンダー・白の角ばった紙の衝撃波と短い三角の紙片。小さいピンクのハート二個と金の星三個が一緒に右へ飛ぶ。明快な右向きの魔法の一撃として一つの扇形のシルエット。紙片の隙間と全周は完全透明で、大きい不透明な丸い背景を敷かない。クリーム白・金・紫・ラベンダー・ピンクの平らな普通のアニメ塗り、硬い輪郭と細いクリーム白縁。1536×1024の真の透明PNG、全体に80px以上の透明余白で切らずに収める。人物・手・杖・猫・顔・文字・数字・背景・床・影・光彩・グロー・ぼかし・写真・3D・ピクセル・複数案なし。魔法の紙片の一撃だけ。'}];
const jobs=[];for(const e of effects){const id=e.id,j={...e,key:'yui',version:1,source:root+'generated/effect_'+id+'_v1.png',references:[],referenceRoles:[],tool:'built-in image_gen',status:'planned'};
 const p=root+'prompts/effect_'+id+'_v1.json';fs.writeFileSync(p,JSON.stringify(j,null,2)+'\n');jobs.push({id,version:1,promptFile:p});}
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'yui',jobs,entries:[],gameConnected:false},null,2)+'\n');
console.log('Two unique Yui effect jobs planned; tap will be bound to both shared normals and the back attack.');

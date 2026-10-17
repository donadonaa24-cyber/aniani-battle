const fs=require('fs');const root='art_source/paper_roster_action/king/';
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Effect jobs already planned');
const effects=[
 {id:'claw',kind:'ability',look:'Default',state:'Attack1',duringStartup:false,duringRecovery:true,offsetX:.85,offsetY:.15,worldWidth:1.45,worldHeight:1.2,
 prompt:'Use case: stylized-concept. オリジナル2Dペープサート格闘ゲームの、邪悪な爪のひっかきの切り絵エフェクト一個。右へ振り抜く3本の短い平行した斜めの爪痕。濃い紫・黒・深紅の平らな紙の太い曲線が右上から右下へ弧を描き、縁に薄い金の線とクリーム白縁。小さい紫の角ばった紙片を4つほど周囲へ散らす。3本の爪痕の間は広く完全透明、中央を大きい不透明な丸い雲で埋めない。普通のアニメの線とセル塗り、硬い切り絵の輪郭。1024×1024の真の透明PNG、全体を中央に収め全周80px以上余白。人物・手・指・手甲・顔・文字・数字・背景・床・影・光彩・グロー・ぼかし・3D・写実・ピクセル・複数案なし。爪の紙の効果一つだけ。'},
 {id:'roar',kind:'ability',look:'Default',state:'Attack3',duringStartup:false,duringRecovery:true,offsetX:.8,offsetY:.55,worldWidth:1.7,worldHeight:1.2,
 prompt:'Use case: stylized-concept. オリジナル2Dペープサート格闘ゲームの、王の咆哮が右前方へ広がる切り絵の衝撃波一個。左を発生点に右へ広がる扇形で、深紅・黒・金色の角ばった三重の弧状の紙片。右側ほど大きい弧を置き、短い金の三角の紙片を右へ放射する。弧の間は完全透明、ひとつながりの不透明な丸い背景を敷かない。大きい蹴りと同時に発生する雄叫びの圧力として、太く明快な右向きのシルエット。普通のアニメの線とセル塗り、硬い紙の輪郭と薄いクリーム白縁。1536×1024の真の透明PNG、中央に一つ、全周80px以上余白で切らない。人物・獅子の顔・口・手足・駒・文字・数字・背景・床・影・光彩・グロー・ぼかし・3D・写実・ピクセル・複数案なし。'},
 {id:'pawn',kind:'projectile',look:'King',fitAttackHitbox:true,fitEachProjectileHitbox:true,sortingOrder:7,
 prompt:'Use case: stylized-concept. オリジナル2Dペープサート格闘ゲームで、黒獅子王が頭上から召喚して落とす部下の将棋駒一個。真っ直ぐ上向き、正面から見た平たい五角形の蜂蜜色の木の駒。太い黒と金の縁、下部に細い深紅の帯、小さい金の丸い鋲を左右に一つずつ。中央に太く大きい正確な墨文字「歩」一字だけを読みやすく置く。主役の王と区別して王冠・獅子の顔・宝石は付けない。普通のアニメの線とセル塗り、奥行きの斜め厚みのない正面の平たい小道具、薄い硬いクリーム白の切り抜き縁。1024×1024の真の透明PNGに駒一体、全周80px以上透明余白。人物・顔・手・足・尾・腕・王冠・ほかの駒・「歩」以外の文字・背景・床・影・速度線・光彩・グロー・ぼかし・3D・写実・ピクセル・複数案なし。'}
];
const jobs=[];
for(const e of effects){const j={...e,key:'king',version:1,source:root+'generated/effect_'+e.id+'_v1.png',references:[],referenceRoles:[],tool:'built-in image_gen',status:'planned'};
 const promptFile=root+'prompts/effect_'+e.id+'_v1.json';fs.writeFileSync(promptFile,JSON.stringify(j,null,2)+'\n');jobs.push({id:e.id,version:1,promptFile});}
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'king',jobs,entries:[],gameConnected:false},null,2)+'\n');
console.log('King: claw, roar and one pawn source planned. Normal effects will be shared by action; pawn scales to each live hitbox.');

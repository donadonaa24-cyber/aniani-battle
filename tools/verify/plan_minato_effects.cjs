// Full prompts for the three presentation-only Minato effects. No battle values are changed.
const fs=require('fs');
const root='art_source/paper_roster_action/minato/';
const definitions=[
 {id:'teleport',look:'Teleport',kind:'ability',state:'SpecialNeutral',worldHeight:2.7,duringStartup:true,duringRecovery:false,
  prompt:'オリジナル2D対戦ゲームの瞬間移動の接続エフェクトを1個だけ生成。人物の背後に置く、縦長の切り絵風の空間の裂け目。シアン、水色、白、濃紺の平たい画用紙を数枚重ねたような鋭い輪郭。縦向きの細い楕円の輪を左右のギザギザした三日月状の紙片が囲む。中央は完全に透明で、内側に人物を後で置いても顔と体を隠さない。青い短い紙片を少量だけ外周へ。輪郭は硬く、薄いクリーム白の切り抜き縁。全体は縦横比およそ幅0.6:高さ1、1024×1536の透明キャンバスの中央に30px以上の余白で収める。平面的なアニメセル塗り。人物、手、顔、文字、数字、機械、武器、背景、床、影、煙の写真、3D、ぼかし、グロー、ピクセル、シート、複数案なし。縁の外と中央は真の透明。'},
 {id:'mist',look:'BlackHole',kind:'ability',state:'Ultimate',worldHeight:2.8,duringStartup:false,duringRecovery:true,
  prompt:'オリジナル2D対戦ゲームで人物の背後に表示する意識状態の霧のオーラ1個。白、水色、淡い青紫、紺の平たい切り絵の雲を重ねる。人物そのものは描かず、縦長の大きな空洞の周りを細い三日月状の霧の紙片6〜9枚が囲む。上と左右にふわりと立つ形、下側も細く巻くが床は描かない。全体は幅0.75:高さ1程度の縦長、中心60パーセントは完全に透明で、後で重ねる人物の顔や四肢を隠さない。霧は写真ではなく明快な輪郭と2〜3段階のセル塗りで、画用紙の雲のようにカタカタ動かせる形。薄いクリーム白の切り抜き縁。1024×1536の透明キャンバス、全体を40px以上余白で収める。人物、顔、手、眼、文字、背景、床、影、黒い球、光線、写実的煙、ぼかし、グロー、3D、ピクセル、複数案なし。外周と空洞は真の透明。'},
 {id:'blackhole',look:'BlackHole',kind:'projectile',fitAttackHitbox:true,sortingOrder:3,
  prompt:'オリジナル2Dペープサート対戦ゲームの、触れると危険なブラックホール状の空間エフェクト1個。横長の楕円、幅:高さが5:4ほど。濃紺と黒紫の深い中心、青紫とシアンと白の鋭い切り絵の渦の輪を3層ほど重ねる。紙を切って重ねた平面的な楕円の渦、左右に短いギザギザの紙片を少し。中心は不透明な深い濃紺、外側は真の透明、薄いクリーム白の切り抜き縁で輪郭が読みやすい。全体を1536×1024の透明キャンバスに十分な余白で収め、紙片を切らない。暗い色でも青い縁で舞台の上で見分けられる。普通のアニメセル塗り、2D舞台用の危険地帯。人物、手、顔、星空、銀河、背景、床、文字、数字、ロゴ、光線、写実、グロー、ぼかし、3D、ピクセル、複数案なし。'}
];
fs.mkdirSync(root+'prompts',{recursive:true});
const jobs=definitions.map(d=>{
 const p=root+'prompts/effect_'+d.id+'_v1.json';
 if(fs.existsSync(p))throw Error('Effect already planned: '+d.id);
 fs.writeFileSync(p,JSON.stringify({...d,key:'minato',version:1,source:root+'generated/effect_'+d.id+'_v1.png',references:[],referenceRoles:[],tool:'built-in image_gen',status:'planned'},null,2)+'\n');
 return {id:d.id,version:1,promptFile:p};
});
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Manifest already exists');
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'minato',jobs,entries:[],gameConnected:false},null,2)+'\n');
console.log('Planned 3 Minato effects, no generation performed');

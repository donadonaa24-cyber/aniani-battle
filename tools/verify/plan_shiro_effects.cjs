const fs=require('fs');const root='art_source/paper_roster_action/shiro/';
const definitions=[
 {id:'emp',look:'Emp',kind:'ability',state:'SpecialNeutral',worldHeight:4,worldWidth:6,duringStartup:false,duringRecovery:false,
  prompt:'2Dペープサート対戦ゲームの電磁パルスの切り絵エフェクト1個。横:縦が約3:2の横長の輪。シアン、水色、白、濃紺の平らな紙の輪2〜3層と、外周の短いギザギザの稲妻状紙片8個ほど。中心60パーセントは完全透明、外側も完全透明。人物の背後に置くので中央を塗りつぶさない。普通のアニメセル塗りで、厚紙を切って重ねた硬い輪郭、薄いクリーム白縁を一周。1536×1024の透明PNG、全体を60px以上の余白で中央に収める。平たい正面の横長のパルス輪で、床に寝かせた輪にしない。人物・犬・顔・目・手・背景・床・影・数字・文字・グロー・ぼかし・写実・3D・ピクセル・複数案なし。固い紙片の形のみ、半透明の光雲を付けない。'},
 {id:'alma_prepare',look:'Alma',kind:'ability',state:'Ultimate',phase:'prepare',worldHeight:5,worldWidth:9,duringStartup:true,duringRecovery:true,
  anticipationText:'01010101 10101010 01010101\n10101010 01010101 10101010\n01010101 10101010 01010101\n10101010 01010101 10101010',
  alternateAnticipationText:'10101010 01010101 10101010\n01010101 10101010 01010101\n10101010 01010101 10101010\n01010101 10101010 01010101',anticipationTextSize:.08,
  prompt:'オリジナル2D対戦ゲームの管理AIに接続する空間の切り絵エフェクト1個。横長のデジタルな額縁のような形。横:縦が約1.8:1。青とシアンと白の細い直線と角ばった紙パネルで上辺、下辺、四隅を囲む。枠内に小さい矩形の紙片を数個だけ、中央の広い80パーセントは完全透明の空洞にし、後で文字とキャラを重ねられる。数字や記号や文字はこの絵に描かない。3枚の薄い透明な画面の額縁を少しずらして貼り重ねたような平面的な工作舞台の効果。普通のアニメセル塗り、硬い輪郭と細いクリーム白縁。1536×1024の透明キャンバス、全体に60px以上余白。人物・犬・顔・眼・人型AI・球・ロボット・背景・床・影・光彩・ぼかし・写真・3D・ピクセル・複数案なし。透明な部分はalpha0で、枠の紙片だけを不透明に描く。'},
 {id:'alma_impact',look:'Alma',kind:'ability',state:'Ultimate',phase:'impact',worldHeight:5,worldWidth:9,duringStartup:true,duringRecovery:true,
  prompt:'オリジナル2Dペープサート対戦ゲームのALMAの攻撃エフェクト1個。本作独自の抽象的なAI攻撃の姿。画面中央の小さな白銀と濃紺の幾何学的な機械の核から、左右へシアン・青・白の大きい角ばった紙の衝撃波が広がる。核は顔や目のない六角形、周囲に分割された青い紙の輪3層、横へ短い矩形パネルや鋭い紙片が飛び出す。全体の横:縦は約1.8:1。前後の人物を隠さないよう輪の間と中央の周りは透明な抜きを大きく取り、真っ黒な全面の塗りつぶしを避ける。大きい攻撃を正面から見た平たい切り絵の波。1536×1024の真の透明PNG、全体を60px以上余白で切らずに収める。普通のアニメセル塗りと硬いクリーム白縁、不透明な紙片の形のみ。人物・犬・動物・顔・人型AI・目・背景・床・影・数字・文字・粒・光彩・ぼかし・写実・3D・ピクセル・複数案なし。'}
];
if(fs.existsSync(root+'effects-manifest.json'))throw Error('Effects already planned');
const jobs=definitions.map(d=>{const promptFile=root+'prompts/effect_'+d.id+'_v1.json';if(fs.existsSync(promptFile))throw Error('Existing prompt');fs.writeFileSync(promptFile,JSON.stringify({...d,key:'shiro',version:1,source:root+'generated/effect_'+d.id+'_v1.png',references:[],referenceRoles:[],tool:'built-in image_gen',status:'planned'},null,2)+'\n');return{id:d.id,version:1,promptFile};});
fs.writeFileSync(root+'effects-manifest.json',JSON.stringify({schemaVersion:1,key:'shiro',jobs,entries:[],gameConnected:false},null,2)+'\n');
console.log('Three effects planned; exact binary text will be drawn by the game');

// The user's three named executives replace the ungenerated generic pawn.
const fs=require('fs'),crypto=require('crypto');
const root='art_source/paper_roster_action/king/',read=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const m=read(root+'effects-manifest.json');
if(m.jobs.some(j=>j.id==='leila'))throw Error('Support jobs already planned');
if(m.entries.some(e=>e.id==='pawn'))throw Error('Do not discard a generated asset');
m.cancelledJobs=[...(m.cancelledJobs||[]),...m.jobs.filter(j=>j.id==='pawn').map(j=>({...j,status:'cancelled_before_generation',reason:'2026-10-18 ユーザー指定：汎用歩ではなくレイラ・シオン・ノア。'}))];
m.jobs=m.jobs.filter(j=>j.id!=='pawn');
const people=[
 {id:'leila',name:'豹妬レイラ',original:'shade1.webp',description:'豹の女性幹部／援型',identity:'黒から赤紫へ流れる長いウェーブの髪、赤い瞳、額の金の菱形、豹耳のある豹柄の毛皮フード、裏が深紅の豹柄マント、黒と金の装飾鎧と爪手袋、豹の顔のベルト、金の鎖、豹の尾。参照の成人女性らしい顔・衣服を維持。',action:'斜め下の画面右へ爪で飛びかかる全身ポーズ。右肩から右肘・手首へ続く右の爪手袋を下前へ差し出し、左手は胸の横へ引く。両膝を曲げて両足は浮く。髪と豹柄マントは上後ろへ広がり、落下攻撃と読める。5本の指が同じ方向の刃5本にならず親指の対向を描く。'},
 {id:'shion',name:'銀狼シオン',original:'shade2.webp',description:'狼の男性幹部／銀型',identity:'短い黒髪と白銀の前髪、青い瞳、額の金の菱形、狼の顔と尖った耳のある灰色フード、黒い毛皮の肩、黒と銀の鱗状鎧と金の細い縁、青い宝石、黒いマント、黒銀のブーツ、銀色の狼の頭を先端に持つ黒い長杖。参照の少年らしい顔と頭身を保つ。',action:'両手で同じ一本の狼頭の杖を握り、両肘を曲げて斜め下の画面右へ杖の頭を打ち下ろす瞬間の全身。左右の肩から両腕がそれぞれ手袋へつながり、二つの手で一本の杖を握る。両膝は曲げて足が浮く。狼のフードと顔を杖で隠さず、マントが上後ろへ流れて落下攻撃と読める。杖は短縮せず対角線に傾けて全体を収める。'},
 {id:'noa',name:'鹿角ノア',original:'shade3.webp',description:'鹿の女性幹部／角型',identity:'茶色の長い髪、紫の瞳、額の金の菱形、左右の鹿耳と枝分かれする鹿の角が付いた暗いフード、黒と深緑の金縁の衣装とマント、紫の結晶の留め具、黒いブーツ、枝に囲まれた大きな紫の結晶が先端に付く一本の杖。参照の少女らしい顔と頭身を保つ。',action:'右手で一本の枝の杖を胸の横へ持ち、左腕を斜め下の画面右へ伸ばし掌を開いて攻撃を指示する全身。左右の肩・肘・手首と手の所属が一続き、開いた左手は親指1本と4本指の計5本。両膝を軽く曲げ、両足は浮く。衣装とマントが上後ろへ流れ、落下して攻撃する姿勢。別の浮く結晶・魔法・光はこの人物原画へ描かない。'}
];
const references=[];
for(let i=0;i<people.length;i++){
 const p=people[i],ref='docs/art_reference/king_supports/'+p.id+'.webp';
 const j={id:p.id,key:'king',name:p.name,kind:'projectile',look:'King',variantIndex:i,version:1,fitAttackHitbox:true,fitEachProjectileHitbox:true,sortingOrder:7,
 source:root+'generated/effect_'+p.id+'_v1.png',references:[ref,'art_source/paper_roster_idle/generated/king_idle_v1.png'],
 referenceRoles:['本人の原作容姿資料（ユーザー作品の既存ポータル掲載画像）','絵柄のみ。黒獅子王をこの人物へ混ぜない'],tool:'built-in image_gen',status:'planned',
 prompt:`Use case: identity-preserve. Asset type: オリジナル2Dペープサート格闘ゲームの召喚キャラクターの全身透過原画。画像1は『推し駒Battle』の${p.name}本人の容姿基準。画像2は同じゲームの黒獅子王で、太いアニメ線・明快なセル塗り・白縁の絵柄だけの参照。画像1の本人一人だけを画像2と統一感ある普通のアニメ絵に描き直す。\n容姿：${p.identity}\n動作：${p.action}\nComposition: 1189×1323の真の透明PNG、全身一人だけ。角や頭巾の先・両手・両足・杖・尾まで全体を切らず、全周40px以上の透明余白を残す。少し斜め横で胸側が見える。衣装や動物の飾りを余分な手足へ変えず、左右の腕2本・脚2本、人間の顔1つ。指は各5本、握る指を杖に巻き、手首をひねって左右の手を取り違えない。素材全体の外周へ薄く硬いクリーム白の切り抜き縁。既存キャラの髪と装備の色・模様を変えない。\nAvoid: 王冠・黒獅子王の姿・新しい武器・木の将棋駒・「歩」の文字・文字全般・背景・床・影・相手・光彩・グロー・速度線・紙の繊維・3D・写実・ピクセル・分割した手足・複数案。武器や角を画面外へ切らない。`} ;
 const file=root+'prompts/effect_'+p.id+'_v1.json';write(file,j);m.jobs.push({id:p.id,version:1,promptFile:file});
 references.push({key:p.id,name:p.name,copy:ref,origin:'%USERPROFILE%/OneDrive/デスクトップ/あにあにの遊び場/images/gacha/oshikoma/'+p.original,
 sha256:crypto.createHash('sha256').update(fs.readFileSync(ref)).digest('hex'),identityRecord:'trial-gacha-data.js: '+p.description,
 note:'原作の容姿を参照。頭上からの攻撃ポーズは本作のクロスオーバー表現。元フォルダは読み取りのみ。'});
}
write(root+'effects-manifest.json',m);write('docs/art_reference/king_supports/sources.json',{schemaVersion:1,references});
console.log('Named support jobs planned: Leila, Shion, Noa; ungenerated generic pawn cancelled.');

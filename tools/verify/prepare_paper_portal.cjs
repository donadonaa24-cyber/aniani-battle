// One-time migration used for the 20c5a1f portal checkout. Do not rerun on a released checkout.
// Prepare the latest public portal checkout. Original game artwork is read-only.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'../..'),portal=path.resolve(process.argv[2]||'TestResults/portal_release');
if(!portal.startsWith(root+path.sep))throw Error('portal checkout must stay inside workspace');
const sharp=require('sharp'),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const read=p=>fs.readFileSync(path.join(portal,p),'utf8'),write=(p,s)=>{fs.mkdirSync(path.dirname(path.join(portal,p)),{recursive:true});fs.writeFileSync(path.join(portal,p),s);};
const map=new Map();
function metas(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const p=path.join(dir,e.name);if(e.isDirectory())metas(p);else if(e.name.endsWith('.meta')){const m=fs.readFileSync(p,'utf8').match(/^guid: ([a-f0-9]+)/m);if(m)map.set(m[1],p.slice(0,-5));}}}metas(path.join(root,'AniBato/Assets/_Project'));
function linked(text,field){const guid=text.match(new RegExp('^  '+field+': \\{[^}]*guid: ([a-f0-9]+)','m'))?.[1];if(!guid||!map.has(guid))throw Error('Missing linked '+field);return map.get(guid);}
const people=[['Touma','toma','透真','星の終わりに君は生きる'],['Ren','ren','蓮','推し駒battle'],['Chizuru','chizuru','千鶴','Battle à la carte'],['Minato','minato','湊','星の終わりに君は生きる'],['Shiro','shiro','シロ','星の終わりに君は生きる'],['Yui','yui','結衣','推し駒battle'],['King','king','黒獅子王','推し駒battle'],['Akatsuki','akatsuki','暁','Battle à la carte'],['Ryuta','ryuta','龍太','Battle à la carte']];
const support=[['alma','ALMA','星の終わりに君は生きる','意識フィールドを展開し、相手の移動を5秒間60％にする。攻撃速度は変わらない。'],['shion_car','志遠の車','EchoShion','赤い車で舞台を走り抜け、命中した相手を2秒間麻痺させる。'],['saki_hana','咲と花','花散るさきの、幸せのかたち','もやしカレーを届け、5秒間スタミナ消費をなくす。'],['sara','紗良','星の終わりに君は生きる','緊急箱を持って登場。ハートを1つ回復する（上限まで）。'],['mai','舞依','Battle à la carte','弓道着で登場し、相手へハート1つ分の矢を放つ。'],['tsuyoshi','剛','Battle à la carte','財布を舞台のどこかへ落とす。財布はどちらも拾え、スタミナを全回復する。'],['mio','美桜','推し駒battle','5秒間、または通常3回・特殊1回分までのバリア。超必殺は貫通する。'],['hota','歩太','推し駒battle','上空から6個の歩の駒を降らせる。相手に当たると1個につきスタミナ15減少。']];
async function main(){
const sourceCatalog=require(path.join(portal,'trial-gacha-data.js'));const oldCards=sourceCatalog.cards.slice(0,159);assert.equal(oldCards.length,159);
const oldHash=sha(Buffer.from(JSON.stringify(oldCards))),entries=[],cards=[];
async function image(source,key){
 const target=`images/gacha/anibato-paper/${key}.webp`;fs.mkdirSync(path.dirname(path.join(portal,target)),{recursive:true});
 await sharp(source).trim({threshold:1}).resize({width:768,height:1024,fit:'inside',withoutEnlargement:true}).webp({quality:88}).toFile(path.join(portal,target));
 entries.push({image:target,source:path.relative(root,source).replaceAll('\\','/'),sourceSha256:sha(fs.readFileSync(source)),webSha256:sha(fs.readFileSync(path.join(portal,target))),usage:'Web用縮小派生。ゲームの原画は変更しない。'});return target;
}
for(const [asset,key,name,work] of people){
 const data=fs.readFileSync(path.join(root,`AniBato/Assets/_Project/Data/${asset}.asset`),'utf8'),motion=fs.readFileSync(linked(data,'PaperMotion'),'utf8'),source=linked(motion,'Sprite');
 const img=await image(source,key);
 cards.push({rarity:'SSR',category:'紙キャラ',title:`${name}（紙の舞台）`,image:img,sourceWork:work,role:`${work} / 紙キャラ`,generatedWithAI:true,description:`『${work}』から、あにあにバトル！！に参戦する${name}。ゲームで採用している待機原画を使ったコレクションカード。カードの所持でゲームの強さは変わりません。`});
 // The homepage uses the same currently adopted waiting image as the new card.
 fs.copyFileSync(path.join(portal,img),path.join(portal,`images/anibato/chars/${key}-idle.webp`));
}
const adopted=JSON.parse(fs.readFileSync(path.join(root,'art_source/paper_support_items/adoption.json'),'utf8'));
for(const [key,name,work,description] of support){const entry=adopted.entries.find(x=>x.key===key);assert(entry,key);const source=path.join(root,entry.source);assert.equal(sha(fs.readFileSync(source)),entry.sha256);cards.push({rarity:'SR',category:'紙サポート',title:`${name}（紙サポート）`,image:await image(source,key),sourceWork:work,role:`${work} / 紙サポート`,generatedWithAI:true,description:description+' あにバト独自のクロスオーバー演出です。ガチャの所持と試合内の出現・効果は連動しません。'});}
let data=read('trial-gacha-data.js');data=data.replace('"推し駒battle": "OSK"','"推し駒battle": "OSK", "あにあにバトル！！": "ANI"');
data=data.replace(/\n    \/\/ PAPER_CARDS_BEGIN[\s\S]*?\/\/ PAPER_CARDS_END\n/,'\n');
const addition='\n    // PAPER_CARDS_BEGIN\n'+cards.map(c=>`    add("あにあにバトル！！", ${JSON.stringify(c.rarity)}, ${JSON.stringify(c.category)}, ${JSON.stringify(c.title)}, ${JSON.stringify(c.image)}, ${JSON.stringify(Object.fromEntries(Object.entries(c).filter(([k])=>!['rarity','category','title','image'].includes(k))))});`).join('\n')+'\n    // PAPER_CARDS_END\n';
data=data.replace('    return Object.freeze({',addition+'\n    return Object.freeze({');write('trial-gacha-data.js',data);
write('docs/paper-cards-assets.json',JSON.stringify({sourceGameCommit:'24ddaa0',existing159Sha256:oldHash,newCards:17,fighterRarity:'SSR',supportRarity:'SR',entries},null,2)+'\n');
await sharp(path.join(root,'docs/screenshots/paper_support_web/support_pickup.png')).resize({width:1280,withoutEnlargement:true}).webp({quality:87}).toFile(path.join(portal,'images/anibato/battle-20261022.webp'));
let html=read('index.html');html=html.replaceAll('aniani.css?v=20261018a1','aniani.css?v=20261022a1').replaceAll('aniani.js?v=20261018a1','aniani.js?v=20261022a1').replaceAll('trial-gacha-data.js?v=20261012b1','trial-gacha-data.js?v=20261022a1');
html=html.replace(/(<button class="game-case anibato-case"[\s\S]*?<\/button>)/,m=>m.replace('data-status="試作版・開発中"','data-status="無料ブラウザ試作版"').replace('DEVELOPING / WINDOWS','PLAY / PC BROWSER').replace('試作版・開発中 / プレイ不可','無料ブラウザ試作版 / 9人チーム戦'));
html=html.replace(/(<article class="game-card active anibato-preview">[\s\S]*?<\/article>)/g,m=>m.replaceAll('試作版・開発中。プレイ版・ダウンロードは現在未公開です。','無料ブラウザ試作版を公開。PC・キーボード／ゲームパッド向けです。').replace('href="anibato/">紹介ページへ','href="anibato/">ホームページへ').replace('</div>\n                    <p class="rights-notice">','<a class="btn btn-mini btn-primary" href="games/anibato/">ブラウザで遊ぶ</a></div>\n                    <p class="rights-notice">').replaceAll('ステージ外に落ちたら即敗北','場外ラインを越えると即敗北。崖つかまり・ジャンプ・復帰技で舞台へ戻れます'));
html=html.replace('<button type="button" data-trial-filter="推し駒battle" aria-pressed="false">推し駒battle</button>','<button type="button" data-trial-filter="推し駒battle" aria-pressed="false">推し駒battle</button>\n                            <button type="button" data-trial-filter="あにあにバトル！！" aria-pressed="false">あにバト・紙の舞台</button>');
write('index.html',html);
let page=read('anibato/index.html');page=page.replaceAll('プレイ版・ダウンロードは現在未公開です。','無料ブラウザ試作版を公開中です。').replaceAll('試作版・開発中で、プレイ版・ダウンロードは未公開です。','無料のPCブラウザ試作版を公開しています。').replaceAll('Windows／PCブラウザ（開発中）','PCブラウザ（試作版）／Windows版は開発中').replaceAll('（ジャブ→アッパー）','（ジャブ→蹴り）').replaceAll('ステージの外に落ちたら即敗北','場外ラインを越えると即敗北。落ちてもジャンプ・復帰技・崖つかまりで戻れます').replaceAll('Windows版とPCブラウザ版の試作（どちらも未公開）','PCブラウザ試作版を無料公開。Windows版は開発中').replaceAll('走り・回避・やられ姿などの動作イラスト（順に追加中）','オンライン試験版の実回線での接続・遅延の確認').replaceAll('公開の時期・方法は未定','対人戦でのバランスと遊びやすさの調整').replaceAll('サポートキャラやオンライン対戦は最初の版には入りません。','サポート8種類を実装。オンラインは無料の直接通信による招待対戦の試験版です。回線によって接続できない場合があります。').replaceAll('anibato.css?v=20261018b1','anibato.css?v=20261022a1');
page=page.replace('<p class="status-badge"><strong>試作版・開発中</strong> 無料ブラウザ試作版を公開中です。</p>','<p class="status-badge"><strong>無料ブラウザ試作版</strong> PC・キーボード／ゲームパッド向け</p><p class="play-actions"><a class="play-link" href="../games/anibato/">ブラウザで開演！</a><a href="../#trial-gacha">紙キャラの無料ガチャへ</a></p>');
page=page.replace('</main>',`<section id="support" class="section"><h2 class="section-title"><span>SUPPORT</span>舞台にやってくる8組のサポート</h2><div class="card-grid">${cards.filter(c=>c.category==='紙サポート').map(c=>`<article class="paper"><img class="support-art" src="../${c.image}" alt="紙人形風に描かれた${c.title}" loading="lazy"><h3>${c.title}</h3><p>${c.description}</p></article>`).join('')}</div><p class="note">試合内のアイテムは拾うと発動。ガチャは観賞用コレクションで、所持や抽選が対戦の強さに影響することはありません。</p></section></main>`);
write('anibato/index.html',page);write('anibato/anibato.css',read('anibato/anibato.css')+'\n.play-actions{display:flex;align-items:center;gap:18px;flex-wrap:wrap}.play-link{display:inline-block;padding:16px 28px;background:#ffd879;color:#302017;border:3px solid #755133;border-radius:6px;box-shadow:4px 5px 0 #866244;font-weight:800;text-decoration:none}.support-art{display:block;max-height:210px;max-width:100%;width:auto;margin:auto;object-fit:contain}\n');
const article={id:'2026-10-22-anibato-paper-release',title:'あにあにバトル！！ 無料ブラウザ試作版と紙の舞台ガチャ',category:'アップデート',body:['9人・3作品のペープサート風チームバトルを、PCブラウザで遊べる無料試作版として公開しました。3人ずつ選び、先に2勝したチームの勝ちです。','崖つかまりとスタミナの駆け引き、8種類のサポート、専用の動作原画、紙の舞台を実装。オンラインは無料の直接通信による招待対戦の試験版です。回線によって接続できない場合があります。','無料試験ガチャにも紙キャラ9枚（SSR）とサポート8組（SR）を追加。既存159枚の所持データとレアリティ排出率を維持しています。同じレアリティ内の1枚あたりの確率は枚数の増加に応じて変わります。ガチャは観賞用で、ゲーム内のキャラやサポートを解放するものではありません。'].join('\n\n'),screenshots:[{src:'images/anibato/battle-20261022.webp',alt:'紙の舞台で透真と蓮が戦い、サポート効果の説明が表示されている画面',title:'小さな紙HUDとサポート',caption:'9人の専用原画・紙の舞台・スタミナ管理で戦う無料試作版。'}],links:[{href:'anibato/',label:'あにバトのホームページへ'},{href:'games/anibato/',label:'PCブラウザで遊ぶ'}],createdAt:'2026-10-22T23:00:00+09:00'};
let js=read('aniani.js');js=js.replace('const officialArticles = [','const officialArticles = [\n        '+JSON.stringify(article,null,4)+',');write('aniani.js',js);
// The original catalog remains byte-for-byte equivalent as data, and collection IDs stay stable.
delete require.cache[require.resolve(path.join(portal,'trial-gacha-data.js'))];const next=require(path.join(portal,'trial-gacha-data.js'));assert.equal(sha(Buffer.from(JSON.stringify(next.cards.slice(0,159)))),oldHash);assert.equal(next.cards.length,176);
console.log(JSON.stringify({existingCardsUnchanged:159,newCards:17,total:176,assetEntries:entries.length}));
}
main().catch(e=>{console.error(e);process.exit(1);});

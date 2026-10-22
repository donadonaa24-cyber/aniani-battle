// One-time migration used for the 20c5a1f portal checkout. Do not rerun on a released checkout.
const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../..'),portal=path.resolve(process.argv[2]||'TestResults/portal_release');if(!portal.startsWith(root+path.sep))throw Error('unsafe checkout');
function edit(file,fn){const p=path.join(portal,file);fs.writeFileSync(p,fn(fs.readFileSync(p,'utf8').replaceAll('\r\n','\n')));}
edit('index.html',s=>s.replace('あにあにバトル！！（試作版・開発中、プレイ不可）','あにあにバトル！！（無料ブラウザ試作版）').replace('Windows・PCブラウザ向けの試作版を開発中です。','PCブラウザで遊べる無料試作版です。').replace(/(<article class="game-card active anibato-preview">[\s\S]*?<\/article>)/g,m=>m.replace('<div class="game-status">試作版・開発中</div>','<div class="game-status">無料ブラウザ試作版</div>').replace(/(<div class="game-actions">[^\n]*?)(<\/div>)/,(_,a,b)=>a+'<a class="btn btn-mini btn-primary" href="games/anibato/">ブラウザで遊ぶ</a>'+b)));
edit('aniani.js',s=>s.replace('"bunko-detail", "anibato-detail"]','"bunko-detail", "anibato-detail", "trial-gacha"]'));
edit('anibato/index.html',s=>s.replace('サポート8種類を実装。オンラインは無料の直接通信による招待対戦の試験版です。','サポート8種類を実装。オンラインは無料の直接通信による招待対戦の試験版として準備・検証中です。').replace('オンライン試験版の実回線での接続・遅延の確認','オンライン試験版の接続設定・実回線での接続と遅延の確認'));
edit('trial-gacha.test.cjs',s=>s.replace("catalog.cards.length, 159, 'catalog should include 135 published cards and 24 Oshikoma characters'","catalog.cards.length, 176, 'catalog includes original 159 and 17 paper cards'").replace(".size, 159, 'card IDs must be unique'",".size, 176, 'card IDs must be unique'").replace('const oshikoma = catalog.cards.slice(135);','const oshikoma = catalog.cards.slice(135,159);'));
edit('portal.test.cjs',s=>s.replace("anibatoCase.includes('data-status=\"試作版・開発中\"')","anibatoCase.includes('data-status=\"無料ブラウザ試作版\"')").replace("anibatoCase.includes('DEVELOPING / WINDOWS') && anibatoCase.includes('プレイ不可')","anibatoCase.includes('PLAY / PC BROWSER') && anibatoCase.includes('無料ブラウザ試作版')").replaceAll('aniani.css?v=20261018a1','aniani.css?v=20261022a1').replaceAll('aniani.js?v=20261018a1','aniani.js?v=20261022a1').replace("view.includes('試作版・開発中。プレイ版・ダウンロードは現在未公開です。')","view.includes('無料ブラウザ試作版を公開。PC・キーボード／ゲームパッド向けです。')").replace("m[1] === 'anibato/' ||", "m[1] === 'anibato/' || m[1] === 'games/anibato/' ||").replace("['試作版・開発中', 'プレイ版・ダウンロードは現在未公開',", "['試作版・開発中', '無料ブラウザ試作版を公開中',").replace("'ステージの外',","'場外ライン',").replaceAll('trial-gacha-data.js?v=20261012b1','trial-gacha-data.js?v=20261022a1').replace('anibato preview without play/download links','anibato browser release with an allowlisted play link').replace('// あにあにバトル！！の紹介ページ（anibato/）：ルール・9人・ステージを載せ、プレイ・配布への導線は置かない。','// 2026-10-22公開依頼：9人と3舞台の紹介・無料ブラウザへの許可済み導線。Windows配布等は追加しない。'));
const assets=JSON.parse(fs.readFileSync(path.join(portal,'docs/paper-cards-assets.json'),'utf8'));
fs.writeFileSync(path.join(portal,'paper-release.test.cjs'),`const assert=require('node:assert/strict'),fs=require('node:fs'),{createHash}=require('node:crypto');
const catalog=require('./trial-gacha-data.js'),gacha=require('./trial-gacha.js'),assets=require('./docs/paper-cards-assets.json');
assert.equal(createHash('sha256').update(JSON.stringify(catalog.cards.slice(0,159))).digest('hex'),${JSON.stringify(assets.existing159Sha256)});
assert.equal(catalog.cards.length,176);assert.equal(new Set(catalog.cards.map(c=>c.id)).size,176);
const paper=catalog.cards.slice(159);assert.equal(paper.filter(c=>c.category==='紙キャラ').length,9);assert.equal(paper.filter(c=>c.category==='紙サポート').length,8);
for(const [i,c] of paper.entries()){assert.equal(c.id,'ANI-'+(160+i));assert.equal(c.work,'あにあにバトル！！');assert(c.generatedWithAI);assert(c.description&&c.sourceWork);assert(fs.existsSync(c.image));assert(assets.entries.find(a=>a.image===c.image));}
assert.deepEqual(catalog.rates,{C:60,SR:25,SSR:10,UR:5});assert.equal(catalog.storageKey,'aniani.trial-gacha.v1.inventory');
const previous=Object.fromEntries(catalog.cards.slice(0,159).map((c,i)=>[c.id,i+1]));const inventory=gacha.addToInventory(previous,paper);for(const [id,n] of Object.entries(previous))assert.equal(inventory[id],n);
const html=fs.readFileSync('index.html','utf8'),page=fs.readFileSync('anibato/index.html','utf8');assert(html.includes('data-trial-filter="あにあにバトル！！"'));assert(page.includes('../games/anibato/'));assert.equal((html.match(/href="games\\/anibato\\/"/g)||[]).length,2);assert.equal((page.match(/class="support-art"/g)||[]).length,8);
assert(!/sb_secret_|service_role|supabaseKey|publishableKey/.test(fs.readFileSync('games/anibato/online-config.json','utf8')));assert(fs.existsSync('games/anibato/payload-manifest.json'));
const payload=require('./games/anibato/payload-manifest.json');for(const data of Object.values(payload.files)){let size=0;const hash=createHash('sha256');for(const p of data.parts){const bytes=fs.readFileSync('games/anibato/'+p.file);assert(bytes.length<100*1024*1024);assert.equal(createHash('sha256').update(bytes).digest('hex'),p.sha256);hash.update(bytes);size+=bytes.length;}assert.equal(size,data.length);assert.equal(hash.digest('hex'),data.sha256);}
console.log('PASS: 176 cards, original 159 unchanged, old inventory preserved, 17 originals credited, play links, 8 supports, credential-free config, exact chunked payload.');
`);
edit('package.json',s=>{const p=JSON.parse(s);if(!p.scripts.test.includes('paper-release.test.cjs'))p.scripts.test+=' && node paper-release.test.cjs';return JSON.stringify(p,null,2)+'\n';});
edit('AGENTS.md',s=>s.replace('あにあにバトル！！（7番目、試作版・開発中、プレイ導線なし）','あにあにバトル！！（7番目、無料PCブラウザ試作版）').replace('- あにあにバトル！！は紹介のみ（ポータル内の紹介ページ `anibato/` と詳細モーダル）。プレイ版・ダウンロードは未公開で、配布・exe起動・GitHubリポジトリへのリンクを追加しない。内容はゲーム本体リポジトリ `aniani-battle` の文書を読み取りだけで参照し、そちらへは書き込まない。','- 2026-10-22所有者指定：あにあにバトル！！のPCブラウザ試作版を `games/anibato/` で無料公開。紹介ページは `anibato/`。Windows配布・exe起動・リポジトリ導線は追加しない。ゲームと画像は静的配信し、Supabase Storageへ置かない。ゲーム本体は別リポジトリで開発し、検証済みWebGLだけを反映する。'));
for(const doc of ['CURRENT_STATE','CHANGELOG','PROJECT_SPEC','ARCHITECTURE'])edit('docs/'+doc+'.md',s=>`# 2026-10-22 紙の舞台の無料ブラウザ試作版・ガチャ追加（反映準備）

所有者が公開・ポータルとホームページ更新・ガチャ追加を依頼。anibato/からgames/anibato/へクリック読込で移動。9人の3人チーム・3舞台・8サポート・崖復帰・スタミナ管理。紙待機9枚SSR/サポート8組SRを追加し全176枚、既存159枚のデータ・保存キー・レア率を保持。AI生成と原画由来はdocs/paper-cards-assets.jsonに記録。カードはコレクションで試合の強さと連動しない。

WebGLの大きなデータを8MiBの静的ファイルに分割し、ゲーム範囲だけのService Workerで元バイト列として配信。各片SHA-256を検証、原画とゲーム内容は変更しない。ゲーム本体/画像はGitHub Pages、Supabase Storageは未使用。接続用の公開窓口URLだけを持ち、APIキー等をソースへ追加しない。オンラインの本番反映は自動承認審査で停止中、利用可能と確定する前に準備中表示を維持する。最新の公開・検証結果は作業完了時に追記する。

以下は従来の記録。

${s}`);
console.log('portal release checks and documentation prepared');

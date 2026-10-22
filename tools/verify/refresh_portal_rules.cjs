// One-time migration used for the 20c5a1f portal checkout. Do not rerun on a released checkout.
const fs=require('node:fs'),path=require('node:path');const root=path.resolve(__dirname,'../..'),portal=path.resolve(process.argv[2]||'TestResults/portal_release');if(!portal.startsWith(root+path.sep))throw Error('unsafe checkout');
function edit(name,fn){const file=path.join(portal,name);fs.writeFileSync(file,fn(fs.readFileSync(file,'utf8')));}
edit('index.html',s=>s.replace('あにあに文庫3作品・架空運輸の車両・推し駒battleのイラストは、生成AIを使用して制作しています。','あにあに文庫・架空運輸・推し駒battleと、今回追加したあにバトの紙キャラ／サポートのイラストは、生成AIを使用して制作しています。'));
edit('anibato/index.html',s=>{
 const changes=[
 ['<span>対応予定</span>','<span>対応</span>'],
 ['特殊攻撃</strong>はキャラごとに1種類。','特殊攻撃</strong>は方向入力で技が変わります。上方向は復帰、下方向は設置・回復・強化など。攻撃型の特殊が'],
 ['<li><strong>特殊攻撃</strong>はキャラごとに1種類。当たると相手のハートを1つ減らします。</li>','<li><strong>特殊攻撃</strong>は方向で変化。上は復帰、下は設置・回復・強化など。攻撃型の特殊は命中すると相手のハートを1つ減らします。</li>'],
 ['押している間すべての攻撃を防ぐ。','押している間、スタミナを消費して攻撃を防ぐ。'],
 ['空中ジャンプ・ダッシュ・回避・ガードで<strong>スタミナ</strong>を使います。地上で止まっていると早く回復。攻撃にスタミナは要りません。','通常攻撃・特殊攻撃・空中ジャンプ・ダッシュ・回避・ガードで<strong>スタミナ</strong>を使います。消費後は回復まで間があり、地上で止まっていると回復しやすくなります。飛び道具にも消費と発射後の隙があります。'],
 ['長く溜めるほど遠くへ吹っ飛ばす。','長く溜めるほど遠くへ吹っ飛ばす。最大溜めは画面外まで強く飛ばしますが、場外ラインに届く前なら復帰の余地があります。'],
 ['画面の外から相手の頭上へロケットを落とす','画面外からロケット。上段足場を通過して下段で爆発し、長く溜めるほど爆発範囲が広がる'],
 ['<dt>通常</dt><dd>義手で打撃</dd>','<dt>通常</dt><dd>1・2段目は素手、3段目は義手で殴り飛ばす</dd>'],
 ['霧のような意識状態になり、触れるとダメージを受けるブラックホールのような空間を出す','霧のような意識状態になり、強い引力のブラックホールを出す。吸い込まれるとハート2つ。溜めるほど引力の届く範囲が広がる'],
 ['可愛い肉球パンチ（3段目は魔法）','ステッキで可愛く叩く（3段目は魔法で吹っ飛ばす）'],
 ['王の駒に変身し、部下3人を呼んで頭上から攻撃','裂け目から部下3人を呼び、「突撃しろ！！」の号令で向いている側の舞台端まで行進させる'],
 ['カードで攻撃（カード投げ）','1・2段目はカードで切りつけ、3段目はカードを投げる'],
 ['砂をこねてピラミッドを作り、相手の頭上へ落とす','溜めてピラミッドを落とす。上段を通過して下段に着地し、壁は10秒で消える'],
 ['英語の本を開き、謎の呪文を唱えて攻撃','英語の本を開いて詠唱し、20秒間の速度強化。特殊下では5秒間の速度1.5倍'],
 ['Windows版の初期設定です。設定の「キーコンフィグ」で変更できます。PCブラウザ版は一部のキー（P2・ポーズなど）が異なります。','公開PCブラウザ版の初期設定です。設定の「キーコンフィグ」で変更できます。オンラインは2人ともP1の操作を使います。'],
 ['<td>右Ctrl</td>','<td>テンキー0</td>'],['<td>Delete</td>','<td>テンキー1</td>'],['<td>End</td>','<td>テンキー2</td>'],['<td>PageDown</td>','<td>テンキー3</td>'],['<td>右Shift</td>','<td>テンキー4</td>'],['<td>PageUp</td>','<td>テンキー5</td>'],['<td>Home</td>','<td>テンキー6</td>'],['<td>右Alt／左右2回</td>','<td>テンキーEnter／左右2回</td>'],['<td>Esc</td>','<td>P</td>'],['<td>Backspace</td>','<td>O</td>'],
 ['対戦中もポーズから開けます。','ローカル対戦中はポーズから開けます。オンライン中はポーズできません。'],
 ['<a href="#status">開発状況</a>','<a href="#support">サポート</a><a href="#status">開発状況</a>']
 ];
 // Keep each replacement explicit; do not rewrite historical articles or other games.
 for(const [a,b]of changes.slice(0,1).concat(changes.slice(2)))s=s.replaceAll(a,b);
 s=s.replace('<section id="controls"',`<section id="online-guide" class="section"><h2 class="section-title"><span>ONLINE</span>友だちを招待して対戦</h2><div class="paper"><ol><li>ゲームを読み込み、「音を有効にして開演！」を押す。</li><li>上の「オンライン対戦」を開き、自分のチーム3人を選ぶ。</li><li>1人が部屋を作り、表示された招待コードを相手に渡す。相手はコードを貼り付けて参加。</li><li>両者が「対戦の準備完了」を押すと開幕。ラウンド後も両者が決定して次へ進む。</li></ol><p>招待待ちは同時4室・1日80室・2分まで。接続後の連続対戦は20分までです。利用者に料金はかからず、有料中継には切り替えません。直接通信できない回線では接続できない場合があります。</p></div></section><section id="controls"`);
 s=s.replace('<section id="modes"',`<figure class="shot"><img src="../images/anibato/battle-20261022.webp" alt="小型の紙HUDと、下部のサポート効果説明が表示された最新対戦画面" width="1280" height="720" loading="lazy"><figcaption>紙HUD・待機原画アイコン・サポート取得時の効果説明。</figcaption></figure><section id="modes"`);
 s=s.replace('<section id="status"',`<section id="ledge" class="section"><h2 class="section-title"><span>RECOVERY</span>落ちても、舞台へ戻る</h2><div class="paper"><p>ジャンプ・特殊上の復帰技で崖へ近づくと、片手でつかまれます。ジャンプか舞台側への入力で復帰。つかまり中はスタミナを毎秒25使い、最大5秒で離れます。着地するまで再つかまりはできません。</p><p>つかまり中は通常の横攻撃は当たらず、下向き判定を持つ攻撃だけが届きます。深く落ちても場外ラインを越えるまで復帰を狙えます。</p></div></section><section id="status"`);
 return s;
});
console.log('current stamina, directional specials, recovery, key bindings and support artwork reflected');

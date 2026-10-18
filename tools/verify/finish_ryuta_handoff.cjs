const fs=require('fs'),tag=process.argv[2],root='art_source/paper_roster_action/ryuta/',webRoot='docs/screenshots/rework_r4_ryuta_web/';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,'')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const a=read(root+'adoption.json'),v=read(webRoot+'verification.json'),b=read(webRoot+'build-report.json'),summary=read('art_source/paper_roster_action/single-key-adoption.json'),padding=read(webRoot+'pixel-audit.json');
if(a.tag!==tag||summary.tag!==tag||summary.completedDedicatedCharacters.length!==9||summary.remainingDedicatedCharacters.length||v.nativeTag!==tag||v.consoleErrors.length||v.audio!=='running'||b.result!=='success'||!padding.entries.every(e=>e.rgbaPreserved)||!fs.existsSync(webRoot+v.normalTopScreenshot))throw Error('Native and browser proofs required');
const heading='## 最新（2026-10-18）：龍太まで9人の攻撃原画を実装、Unity全4工程とWeb確認済み';
const message=`${heading}\n\n今回の龍太は通常1・2段の左ジャブ共有2枚、3段の右アッパー2枚、上2枠／下1枠／空中N1枠／バット2枠／英語の本と呪文2枠＝12表示枠／10人物原画。上は通常3段を原寸共有。打撃・バットの軌跡・呪文の予備と決めの4効果／7接続。原画・全文条件・参照役割・版・SHA・候補の不採用理由はart_source/paper_roster_action/ryuta/。12枠RGBA一致、再サンプル0。頭髪と靴の投影の微差、アッパー上7px／バット右9pxなど狭い余白はpart-reviewに記録。美術最終承認とは区別。\n\n呪文の予備のABCは本の近く、発動の大きい紙片は前方へ、表示だけの予備位置指定を追加。文字効果は向きで位置を変え、文字を左右反転しない。既存効果の既定動作は保持。特殊15／5／22、超必殺34／20／30・遅延8で42tick切替を維持。戦闘シミュレーション・45半減判定・ゲージ14/10・千鶴の保護カバン13原画は不変。\n\n今回の実Unity ${tag}：BuildAll成功／Edit${a.edit.passed}/${a.edit.total}／Play${a.play.passed}/${a.play.total}／Windows成功、失敗・スキップ0。実入力と左右、通常3段・全技の予備／有効／硬直／解除、空中重力、呪文の遅延と位置・文字、1通常被弾／特殊1ハート／超必殺2ハート、余韻の追加被弾なし、反復描画でtickと位置が変わらないことを検証。${summary.screenshotCount}PNGはdocs/screenshots/${tag}/、龍太専用${a.screenshotCount}PNG。WindowsはBuild/${tag}/AniBato.exe。\n\nWebGL ${v.webTag}は実ビルド成功（${b.bytes.toLocaleString('en-US')}bytes／${b.files}ファイル／${b.seconds.toFixed(3)}秒）、Windowsへ復帰済み。内蔵Chromiumで集合トップ・9人2段選択から龍太を選び、通常攻撃・バット・呪文の構えから発動と実被弾を確認。音声running、console error 0。${padding.entries.length}画像の元RGBA保存を監査。Web用LOD／DXTは非可逆の表示処理であり、原画保存と別。実機パッドと通常Chrome／Edge個別は今回未再検査。証跡は${webRoot}。\n\n現在の攻撃原画は9人とも接続済み。全員通常4ユニーク原画で1・2段共有、下／空中Nは各1枚。透真・蓮の未生成の旧ピクセル姿勢は本人紙待機へ置換済み。黒獅子王の部下は既存の豹妬レイラ・銀狼シオン・鹿角ノアで、汎用駒へ戻さない。採用画像はイラスト確認/現在ゲームで採用中/の各人物名→日本語動作名。候補と現在採用を分離。追加の溜め・被弾・他差分は最新指定によりユーザー追加指示後。今回の範囲は完了。\n\nClaudeへ：最初にtasklist・Git差分・実行中Unity／WebGL・最新ログを確認。今回の全4工程と保存済み原画を重複実行しない。既存変更を破棄せず、追加作業は新しい指示から。通常commit／pushの許可条件は全4で満たした。\n\n`;
for(const file of ['docs/RESUME.md','IMPLEMENTATION_STATUS.md']){const text=fs.readFileSync(file,'utf8').replace(/^\uFEFF/,'');if(text.startsWith(heading))throw Error('Already written');fs.writeFileSync(file,(message+text).trimEnd()+'\n');}
fs.writeFileSync('docs/IMPLEMENTATION_ORDER.md',`2026-10-18現在：9人の通常4原画・上2枠・下と空中N各1枚・特殊／超必殺まで接続済み。${tag}全4合格（Edit${a.edit.passed}／Play${a.play.passed}）、Web実動確認済み。追加の溜め／被弾／他差分は追加指示後。詳細はRESUME冒頭。\n\n`+fs.readFileSync('docs/IMPLEMENTATION_ORDER.md','utf8'));
const p=read('docs/codex_tasks/rework_progress.json');p.latestVerification=summary;p.activeVerification=null;p.current='latest_requested_scope_complete';p.stages.R4='complete_latest_scope';p.currentObjective.status='complete_latest_scope';p.nextTask=null;
p.currentArtwork={character:'ryuta',bodySlots:12,uniqueBodyImages:10,effectBindings:7,uniqueEffects:4,gameConnected:true,verifiedTag:tag,webVerifiedTag:v.webTag,next:null,extraVariants:'追加指示後'};
p.latestWebVerification={nativeTag:tag,tag:v.webTag,build:b,screenshots:webRoot,consoleErrors:0,mipmaps:true,paddingAudit:padding.entries.length};p.webGL={...b,...p.latestWebVerification,browser:'Codex in-app Chromium',runtime:'passed'};p.displayFixes.allOtherPropsComplete=true;
if(!p.attempts.some(e=>e.tag===tag))p.attempts.push({...summary,reason:'龍太と呪文の位相別位置・非反転文字を含め全4合格。9人の最新原画仕様が接続済み。'});
write('docs/codex_tasks/rework_progress.json',p);write(root+'adoption.json',{...a,webVerification:p.latestWebVerification});
fs.appendFileSync('docs/ASSET_SOURCES.md','\n## 龍太のジャブ・アッパー、バット、英語の本と呪文（2026-10-18）\n\n本人の採用待機としゃがみv2を参照して内蔵image_genで生成。12表示枠／10人物原画と4効果原画。上は通常3段を原寸共有。全文条件・参照役割・版・SHA・候補の修正理由はart_source/paper_roster_action/ryuta/。手の拡大資料は確認用でゲーム用原画数に数えない。透明梱包とRGBA保存を監査。原寸の投影の微差や狭い余白を美術最終承認と区別。バットと英語の呪文はユーザー指定に基づく本作独自演出。新たな第三者アセットなし。\n');
fs.appendFileSync('docs/codex_tasks/PAPER_PROP_ART_INVENTORY.md',`\n2026-10-18最終：龍太12枠／10人物原画・4効果を接続し${tag}全4合格、Web更新済み。9人の現在指定の攻撃と固有小物に未接続はなし。既存部下はレイラ／シオン／ノア。追加原画は追加指示後。\n`);
let inventory=fs.readFileSync('docs/codex_tasks/PAPER_PROP_ART_INVENTORY.md','utf8');
const tableRows={
 '黒獅子王':'本人12枠・爪／咆哮・王変身と既存の豹妬レイラ／銀狼シオン／鹿角ノアを接続・検証済み',
 '暁':'本人12枠・正式背面カードの実投射・砂とピラミッドを接続・検証済み',
 '龍太':'本人12枠・ジャブとアッパー、バット、本と英語呪文の効果を接続・検証済み'
};
for(const [name,status] of Object.entries(tableRows))inventory=inventory.replace(new RegExp('^\\|'+name+'\\|.*$','m'),'|'+name+'|'+status+'|現在の攻撃・小物の不足なし。追加差分は追加指示後|');
fs.writeFileSync('docs/codex_tasks/PAPER_PROP_ART_INVENTORY.md',inventory);
const order=fs.readFileSync('docs/IMPLEMENTATION_ORDER.md','utf8').replace(/^\| R4 \|.*$/m,`| R4 | 9人の攻撃・小物原画（追加差分は指示後） | 完了。${tag}でEdit${a.edit.passed}／Play${a.play.passed}・全4合格。通常4原画／下と空中N各1枚、Web更新・9人採用監査済み |`);
fs.writeFileSync('docs/IMPLEMENTATION_ORDER.md',order);
fs.mkdirSync('docs/codex_logs',{recursive:true});fs.writeFileSync('docs/codex_logs/rework_r4_ryuta_final.md',message.trimEnd()+'\n');
console.log(JSON.stringify({tag,edit:a.edit.passed,play:a.play.passed,web:v.webTag,completed:summary.completedDedicatedCharacters,next:'on_user_request'}));

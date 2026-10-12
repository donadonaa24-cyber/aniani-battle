# 実装状況：ART1（千鶴スプライト・制作パイプライン・PlayMode修正）

更新日：2026-10-12（JST）。指示書：`docs/codex_tasks/ART1.md`。原本：`docs/SPEC_ORIGINAL.md`。

**実装・画像制作・SpriteForge加工・.NET代替検証を完了。ART1のUnity BuildAll／EditMode／PlayMode／Windowsビルドは未実行です。Unityでの合格はClaude確認待ちです。** git commit/push/公開はしていません。シェフ服PNG/.metaは同じフォルダ内でAwakeningへ移し、元データを保持しています。

## 実装済み

- 既存フェーズ3の3キャラ・純C#60Hz戦闘・入力ラッチ・自前AABB・攻防・各固有技を維持。戦闘の数値・判定サイズは変更なし。
- SpriteForge：外部パッケージ不要のWindows PowerShell＋C#画像加工。キー/かぶり除去、自動列分割、固定の体アンカー/足元、全クリップ同倍率の最頻色縮小、共通32色、二値アルファ、検証、2倍ニアレスト一覧。他キャラのマニフェスト・胴体色・手動アンカーにも対応。
- 千鶴の私服 **23アニメ・75フレーム** と小物 **3種・10フレーム**。全26シート＋JSON、私服カットイン。採用済みidleを完全コピーで再利用。原画・生成履歴はAssets外、加工済みPNGはArt/Sprites/chizuru。
- CharacterAnimationSet/Editorビルダー：状態対応、fps/loop/攻撃持続姿勢、SpriteRect IDの保持、Point/無圧縮/Mipなし/PPU50のスライス、CharacterDataへの接続。BuildAllから生成。
- AniBato.Presentation：純C#アニメ選択。攻撃の発生→持続→硬直に同期し、固定tickで着地/カウンター成功/KO→敗北/勝利を再生。ヒットストップ・暗転で停止、flipX、再戦リセット。勝敗後は戦闘を進めず演出のみ更新。描画は選択済みSampleを読む。
- 実スプライト、頭上UI、爆弾おにぎり弾、もぐもぐ、カレー波、被弾の透過点滅。透真/蓮は仮図形にフォールバック。千鶴のコック帽は表示しません。
- F9判定表示：Hurtbox緑／有効近接Hitbox赤／弾黄。第二Hitboxも表示。
- PlayModeのテストランタイムをrunInBackground=true・ApplicationFocusへ設定。パッケージのinternalプロパティにテスト専用reflectionでアクセスし、フォーカス通知・Update後にKeyboard生成。旧アサーションを維持し、私服の確認と新規2件を追加。

## 生成・加工結果

- 新規採用 **26枚**＝キャラ原画22＋小物3＋カットイン1。idleは既存1枚を再利用。
- **作り直し10枚**、内蔵image_gen出力計36枚。途中版10枚はUnity外/Git管理外に保存。
- 最終 **26シート・85フレーム**。超必殺は体格・余白を確保するため目安6枚から4枚へ変更し、鍋を掲げてふるまう演出を維持。
- 加工：224×160セル、足元159、基準idle112px、全クリップ共通倍率112/675、32色、キー許容90、PPU50。個々の姿勢を同じ全高へ拡縮していません。
- 全一覧：`docs/art_wip/chizuru_contact.png`、分割一覧 `_01`〜`_06`。全絵・顔・服・小物・輪郭・分割数・左右を目視確認。

## 検証結果

| 項目 | ART1結果 | 証跡 |
| --- | --- | --- |
| 純C# EditModeソースの.NET代替実行 | **178成功、0失敗、0スキップ**（既存143＋アニメ35） | TestResults/art1-dotnet-battle.xml・.log |
| Unity API参照の全ソース.NETコンパイル | **警告0、エラー0** | TestResults/art1-dotnet-unity-compile.log |
| SpriteForge合成入力テスト・全シート検証 | **85フレーム合格**：マゼンタ0、二値アルファ、共通32色、地上足元、idle112±2px、寸法・枚数一致 | TestResults/art1-spriteforge-*.log |
| 加工の再現性 | 再生成PNG/JSONのSHA-256一致 | TestResults/art1-spriteforge-reproducibility.json |
| 既存原画/保管画像 | idleコピー・移動したシェフ服のSHA-256一致 | docs/ASSET_SOURCES.md |
| ART1 Unity BuildAll/EditMode/PlayMode/Windows/実画面 | **未実行**。AGENTS.mdに従いClaudeへ引継ぎ | 今回のUnity結果はありません |
| ART1前のClaude Unity XML | phase3 EditMode145/145、PlayMode0/8（全SetUpのkeyboard.enabled失敗）を確認 | TestResults/phase3_EditMode.xml・phase3_PlayMode.xml |

状態・通常段数/空中・特殊4・超必殺・発生/持続/硬直・ヒットストップ/暗転・左右・着地・カウンター・勝敗・再戦・調整値・30/60/144/240FPSを代替テストで確認。Unity生成アセット4件とPlayMode10件はコンパイルのみ。**Unity標準想定はEditMode182件、PlayMode10件**。Unityの成功と代替検証を区別しています。過去のXML/ログを上書きしていません。

## 変更ファイル概要

- tools/SpriteForge（コード・README）、art_source（原画・マニフェスト・プロンプト履歴）。
- Art/Sprites/chizuru（26PNG・JSON・加工報告）、Art/Characters（私服Portrait、Awakening保管）。
- Scripts/Presentation、Game/Data/CharacterAnimationSet、Editor/AnimationSetBuilder、Game/HitboxDebugViewを追加。
- CharacterData/PresentationSettings、BattleRunner/FighterView/ProjectileView/HUD/InputController、ProjectBuilder、各asmdef、FighterStateの読み取り専用freeze公開を更新。
- AnimationSelectionTests35件、AnimationAssetTests2件、PlayMode入力ヘルパー・既存8件の修正・新規2件。Validationの参照・対象更新。
- README/BALANCE_CONFIG/ARCHITECTURE/ASSET_SOURCES、本ファイル、codex_logs/art1_final、制作一覧・在庫情報を更新。

## 既知の問題・未実装

- ART1のUnityインポート・実アセット生成・シーン・Sprite ID維持・画面は未確認。現存BattleはBuildAllで更新が必要です。新規.metaもUnityで生成してください。
- InputFixtureFocusは現在のInputTestFixture内部プロパティ名に依存します。変更時は設定を省略せず明示的に失敗させます。バッチ実行合格はUnity待ち。
- 空中通常3段は同じair_attackを共有。頭上UIは鍋/おたまを含む不透明上端に追従するため、実画面で余白を確認してください。
- 千鶴の姿勢と見た目の判定一致、HUD/状態アイコン/カットイン、日本語、解像度、実パッドはUnityでの確認が必要です。バランス変更は行っていません。
- 透真・蓮のスプライトはART2。タイトル/選択/ポーズ、作品3ステージ・縮小、CPU、音、サポートは後続フェーズ。
- 既存の弾の地形消滅・パッド再割当UI・オンラインは未実装。プラットフォーム間floatのビット一致は保証しません。

## ClaudeにUnityで確認してほしい点

1. SpriteForge→BuildAll→EditMode→PlayMode→Windows再ビルドを、同プロジェクトをエディタで開かず **-nographicsなし** で実行。ART1名のXML/ログへ保存。期待EditMode182／PlayMode10。
2. ChizuruのAnimationSet（26クリップ・85Sprite・24状態）、私服Portrait、BattleのF9表示、Point/PPU50等、新規asmdef/.metaを確認。
3. 旧8件のテスト入力・current Keyboard・移動・ジャンプ・防御・Fキー/R・全3キャラ操作を確認。再インポート後のSprite GUID/local ID、AnimationSet.Timingと戦闘調整保持も確認。
4. 千鶴をP1/P2に配置し全23アニメ・左右flip・打撃持続姿勢・停止・着地・カウンター成功失敗・KO/勝敗・再戦を実操作で確認。
5. F9で絵と判定、足元、頭上数値、状態アイコン・波・カットインの重なり、日本語/比率/F11/パッドを確認。数値調整する場合はBALANCE_CONFIGへ記録。

詳しい手順：`docs/codex_logs/art1_final.md`。フェーズ3の詳細実装履歴は `docs/codex_logs/phase3-status-before-art1.md` と `phase3_final.md` に保存しています。Unityレビュー後はART2（透真・蓮）へ進みます。

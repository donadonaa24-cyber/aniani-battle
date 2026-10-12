> **最新（Claude追記）**：フェーズ5B 完了・Claude検証済み：BuildAll成功、EditMode 243/243、PlayMode 23/23、Windowsビルド成功（`Build/phase5b/`）。次は**フェーズ6**（`docs/REVIEW_NOTES.md` の全項目：中割りの描き直し、背景・カメラ、PS表記・キーコンフィグ、操作表示、必須テスト24項目の総点検、最終ビルド、資料）。

# 再開メモ：フェーズ5BのUnity検証待ち（2026-10-12）

原本 docs/SPEC_ORIGINAL.md、指示書 docs/codex_tasks/PHASE5B.md。**0→A→Bのソース実装・素材生成・オフライン検証は完了。Unity実行／実機確認はClaudeへ引き継ぐ。** commit／push／公開なし。

## 完了・結果

- 前フェーズClaude結果：BuildAll成功、EditMode225/225、PlayMode15/20、Windowsビルド成功。
- 回帰5件：ボタン・デバイス単位の解放待ち、押下の重複防止、自動遷移時のガード撤去、Removed通知でもパッド席予約を保持。既存5失敗のアサーションは維持。HeldConfirmの準備はP1の実際のREADYキーEnterを保持する形に更新し、別キー／別パッドの即時受付も追加検査する。Unityの合格は未確認。
- RenderSnapshotsの前後tick補間をキャラ・弾ID・エフェクト・頭上UI・カメラ目標へ接続。停止／KO／反転／位置ジャンプ／再戦はスナップ、アニメは現tick基準。VSync既定ON、同期・60・120・144・無制限、5tick上限と残時間保持。
- 3キャラ各14クリップ更新、245→368F、42GIF。idle8F/12fps、walk/dash8F/15fps、jump/fall/attack1〜3/air_attack/特殊4種/guard6F/15fps。単一基準idleの胴・足固定、頭頂±1px。その他は既存2キーの局所中割り・微小整合・共通32色。原画保持、旧加工結果art_source/phase5b_keys/。AI新規生成は行っていない。
- 特殊発生+12F／硬直+18F／CD120F／ガード時48F以上。上復帰発生維持・CD90F、食事240F維持。弾1人1発、ハート減少後75F無敵と最低(12,8)吹っ飛び。通常3回=1、特殊1、超必殺2、ハート3、70秒・最後10秒縮小は保持。
- CPU防御・攻撃後の間合い・無敵相手への無駄撃ち・空振り連発を改善。ゲージ14/10/20/18→12/8/16/14。ルール・CPU・キャラSchema5移行は旧既定値に一致する項目だけ変更し独自設定を保持。
- .NET230成功／0失敗／0スキップ、全ソース／プレイヤーコンパイルは警告0／エラー0、asmdef6件（新規なし）。無敵・CD・弾制限・設定移行・現行値の30/60/144/240Hz決定性・180試合目標を追加検証。旧原本1〜18の単独回帰も維持。
- 144Hz相当の描画差分0.06597185〜0.06597233u、単調。368F素材、42GIF全コマ復号、idle胴/足一致・頭頂±1px、PNG/JSON/contact/GIF217ファイル再生成のSHA-256一致を検査。千鶴ジャブの持続姿勢は6F化で添字2／attack1_02へ更新し、厳密な一致テストを維持。
- CPU全9組×30seed×3難易度810試合：全試合両者攻撃／ハート減少、自滅近似0。ふつう平均4.83→45.51秒、縮小0→21.9%、超必殺67.8→56.7%。afterは純C#作成既定値、実SO集計ではない。

## 次の手順（Claude）

1. Unityを閉じ、外部実行可能な環境で powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase5b。-nographics禁止。
2. BuildAll後にTitle/Battle実在、ルール/CPU/キャラSchema5、無敵75F・特殊CD・ゲージ12/8/16/14、Presentation接続、アニメセット125/118/125Fと持続姿勢を確認。NewScene前判定／後アセット再読込・Timing保持を維持。
3. Unity想定EditMode243／PlayMode23。**想定件数を合格と報告しない**。フェーズ5の5失敗を含め全件確認。
4. AniBato.EditorTools.CpuBalanceReportBuilder.Writeで実アセット810試合をTestResults/phase5b-cpu-unity.csv/.mdへ保存。ふつうの平均35〜55秒／縮小20〜40%／超必殺半数前後を再確認。
5. ActualFighterViewMovesUniformlyWith144HzFrameTimeSequenceは実FighterViewを計測しphase5b-render-144hz-unity.csvを保存する。144Hz実機のフレーム時間／移動／ジャンプ／弾／カメラ／頭上数値も確認。
6. キーボード・DualSense：確定後StageSelectに留まる、決定保持中もEsc／OPTIONSが有効、ポーズと操作説明の停止、再開J/×の攻撃・ジャンプ漏れなし、再戦、逆順再接続。
7. GIF／無敵点滅／特殊の隙／CPU攻防を目視。結果と所見をIMPLEMENTATION_STATUS／本書へ追記してからフェーズ6へ。

## 再現・残課題

オフラインは tools/Validation/RunPhase5B.ps1、素材生成は tools/SpriteForge/BuildPhase5B.ps1、検査は VerifyPhase5B.ps1（-Rebuildで全3キャラの再現性も検査）／ValidateArt2.ps1。42GIFは docs/art_wip/<character>_<clip>_preview.gif、[リンク一覧](art_wip/phase5b_previews.md)とcontact画像も更新。

Unityのシーン／.asset／.meta／実行ファイルは5Bで再生成していない。PNG/JSONとソース更新済み、最初にBuildAllが必要。workspace-write／承認不可でUnity IPCへ接続できないためAGENTS.mdとHANDOFF §2に従って外部担当へ引き継ぐ。

中割りはローカル処理で、姿勢差が大きい部分は近いキーポーズへ切り替える。原画由来の顔・衣装・ポーズ差、144Hzの快適さ、対人難易度・キャラ相性・音は実機待ち。UnitySetUpの入力分離／UnityTearDown復元を維持。

フェーズ6の残りは背景／カメラ最終調整、操作表示整理、PS表記／キーコンフィグ、必須24項目棚卸し・リリースビルド。サポート／オンライン／正式音源は未実装。詳細は IMPLEMENTATION_STATUS.md、BALANCE_CONFIG.md、docs/CPU_BALANCE_REPORT.md。

# AGENTS.md — あにあにバトル！！（あにバト！！）

## プロジェクト
- 正式タイトル：**あにあにバトル！！**（略称：**あにバト！！**）。旧仮称「ANIANI ALL-STARS」は使わない。
- オリジナル2D横スクロール対戦アクション（プラットフォームファイター）。Unity 6000.6.0f1、2D。
- Unityプロジェクト本体：`AniBato/`（製品名は「あにあにバトル！！」、companyName は `aniani`）。
- 原本仕様：`docs/SPEC_ORIGINAL.md`（必ず参照。迷ったらこれに従う）。
- 役割：**Codex＝メイン実装**（構成、戦闘ロジック、操作、当たり判定、CPU、テスト）。**Claude＝レビュー・検証・取りまとめ**。Claudeからの指示書は `docs/codex_tasks/` にある。

## 作業開始時に読むもの
1. この `AGENTS.md`
2. `IMPLEMENTATION_STATUS.md`（あれば。現状・既知の問題）
3. 今回の指示書 `docs/codex_tasks/PHASE*.md`
4. 必要に応じて `docs/SPEC_ORIGINAL.md`、`GAME_DESIGN.md`、`BALANCE_CONFIG.md`、`docs/ARCHITECTURE.md`

## 絶対ルール
- 書き込みはこのフォルダ（`anianiバトル！！`）配下のみ（指示書で明示された場合を除く）。他のゲームのフォルダ（`推し駒battle`、`battle-a-la-carte*`、`あにあにの遊び場` 等）は**読み取り専用の参考資料**。
- GitHub：`https://github.com/donadonaa24-cyber/aniani-battle`（**公開リポジトリ**、ブランチ `main`）。ユーザー許可済み（2026-10-12）：**Unity検証（BuildAll・EditMode・PlayMode・ビルドが全合格）を通った区切りで commit・push してよい**。force push、履歴の書き換え、ブランチ削除、リポジトリ設定の変更はしない。
- 公開リポジトリなので、秘密情報・個人情報（メールアドレス、PCユーザー名を含む絶対パス）を書かない。パスは `%USERPROFILE%` や相対パスで書く。`docs/codex_logs/*_run.log` はGit管理外。
- 既存ファイルの破壊的削除はしない。他プロジェクト（あにあにポータル等）への書き込み・公開は、その都度ユーザー許可を得る。
- APIキー・トークン・秘密情報を書かない。外部サーバーへデータを送らない。有料アセット・出典不明の第三者アセットを入れない。
- 画像・音は自作の仮素材（コード生成テクスチャ、単色シェイプ、手続き生成SE）でよい。原作素材を使う場合は出典を `docs/ASSET_SOURCES.md` に記録する。
- 資料で確認できない原作設定を「正史」として確定しない。本作独自の演出は独自表現と明記する。
- Unityエディタがこのプロジェクトを開いている時はバッチモードが失敗する。その場合は無理に進めず報告する。

## 技術方針（全フェーズ共通）
- **戦闘シミュレーションは純C#（MonoBehaviour非依存）**で `AniBato.Battle` 名前空間・独立asmdefに置く。60Hz固定tick（`Tick(FrameInput p1, FrameInput p2)`）で進み、EditModeテストから直接駆動できること。
- Unity側（`AniBato.Game`）は入力取得・描画・UI・音のみ。入力は `Update` でラッチして次tickで消費（押下取りこぼし防止）。描画はシミュレーション状態を読むだけ。
- 物理は自前の簡易AABB（Rigidbody2D非依存）で決定的に。すり抜け足場、場外判定もシミュレーション側。
- 攻撃は攻撃インスタンスID＋ヒット済み対象集合で多重ヒットを防止。
- 数値はすべて ScriptableObject（`CharacterData`、`AttackData`、`BattleRules` 等）から。シミュレーションは ScriptableObject から変換した純C#の定義構造体を使う（テストではコード上で定義を作れる）。
- シーン・アセットは手書きYAMLではなく、Editorスクリプト（`AniBato.EditorTools.ProjectBuilder.BuildAll` 等、`-executeMethod` で実行可能）で生成する。
- 巨大な1ファイルにしない。システムごとにクラスを分ける。
- テスト：`Assets/_Project/Tests/EditMode`、`Assets/_Project/Tests/PlayMode`。

## Unityコマンド（参考）
```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath AniBato -runTests -testPlatform EditMode -testResults TestResults\editmode.xml -logFile TestResults\editmode.log
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -quit -projectPath AniBato -executeMethod AniBato.EditorTools.ProjectBuilder.BuildAll -logFile TestResults\build.log
```
- **`-nographics` は付けない**（Personalライセンスではヘッドレス権限が無く exit 198 で失敗する）。
- Codexのサンドボックス内ではUnityライセンスIPCに接続できないため、**Unityでの BuildAll／テスト／ビルドはClaudeが実行**して結果を返す。Codexは .NET 代替テスト（`tools/Validation`）で可能な範囲を検証し、Unity未検証の点を最終報告に明記する。
- Claudeの検証結果は `TestResults/`（EditMode.xml、PlayMode.xml、各 .log）に置かれる。

## 容量の管理（2026-10-12 ユーザー要望）
- Windowsビルド（`Build/<作業名>/`）は1つ100〜160MB。**最新2つだけ残す**（`tools/verify/run_unity.ps1` が自動で古いものを削除）。過去の版は Git／GitHub から再現できるので、ビルドを溜めない。
- `TestResults/` は直近2〜3回分の結果だけ残し、古い作業名のファイルは削除してよい（`TestResults/dotnet`・`dotnet-home` は .NET 検証用なので残す）。
- `docs/codex_logs/*_run.log`（Git管理外）は、完了した作業のものを削除してよい。報告の `*_final.md` は残す。

## 作業終了時
- `IMPLEMENTATION_STATUS.md` を更新（実装済み／未実装／テスト状況／既知の問題／次の作業）。
- 最終メッセージに：変更ファイル概要、検証結果（実行できたか、できなかったか正直に）、Claudeに確認してほしい点。

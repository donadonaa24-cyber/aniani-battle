# PHASE 6B 完了記録・担当交代の指示書（2026-10-12）

## 目的とユーザー指定

ユーザーから「6bは終わったよ」と確認を受けた。完了した実装を再作成せず、保存済みの検証証跡を確認して資料を最新化する。今後はCodexが実装・レビュー・Unity検証・取りまとめを担当し、Claudeにもいつでも交代できる状態を保つ。

## 作業範囲

1. tasklistとCodexの稼働状況で同時編集がないことを確認する。
2. TestResults/phase6b_runner.txt、phase6b_EditMode.xml、phase6b_PlayMode.xml、生成・ビルドログ、Title/BattleシーンとWindows実行ファイルを確認する。
3. AGENTS.md、HANDOFF_TO_CODEX.md、RESUME.md、IMPLEMENTATION_STATUS.md、README.md、REVIEW_NOTES.md、phase6b_final.mdの古い検証待ち・担当限定の記述を更新する。過去の提出時記録は履歴として残す。
4. 自動Unity検証、.NET代替検証、手動の実機確認を区別する。既存の比較画像はUnityの実画面と扱わない。
5. ゲームコード・アセット・テストのアサーションは変更しない。資料差分と公開情報を確認し、完了記録をcommit・origin mainへpushする。

## 確認済みの基準

- 6B：BuildAll exit 0、EditMode 320/320、PlayMode 29/29（失敗・スキップ0）、Windowsビルド exit 0・Success。
- 実装コミット：9560ed4。作業開始時のHEADとorigin/mainは一致、作業ツリーはclean。
- 最新ビルド：Build/phase6b/AniBato.exe。保持中はphase6a・phase6bの2つ。
- 今回は保存済み証跡を読む作業。Unity再実行と実キー入力による撮影は行わない。

## 交代・次の作業

- 次の具体的な実装修正を始める前にdocs/codex_tasks/へ指示書を作る。
- Unity実行はtools/verify/run_unity.ps1 -Tag <作業名>をサンドボックス外で承認申請して行う。-nographics禁止。.NETのみで完了扱いにしない。
- play_and_capture.ps1実行前は必ずユーザーへ「PCに触らないで」と声をかける。
- CodexとClaudeは同時編集しない。交代時はRESUMEに作業名・差分・検証結果・残課題・次の手順を残す。

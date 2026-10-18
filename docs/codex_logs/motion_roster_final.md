# 全8人の専用7動作・56原画（2026-10-19）

全員に走り・ジャンプ上昇・落下・回避・カウンター成功・通常被弾・強被弾を各1原画で実装。通常4原画、下/空中N各1枚、千鶴と保護カバン、判定と戦闘値は維持。選択版・全文条件・参照役割・SHA・整数梱包と原画RGBA監査はart_source/paper_roster_motion/。日本語の採用画像はイラスト確認/現在ゲームで採用中/。元の半減判定とゲージ14/10を変更していない。

龍太の通常被弾はユーザーが添付し「これでいい」と承認した握り拳v5。v4へ推測で戻さず、開いた掌v6も不採用。承認参照と理由を保存し、この手を再解釈して変更しない。透真の反撃の腕、暁の左手・3脚・6指の指摘も修正版を維持。人物別の部位差と候補不採用理由をpart-reviewへ記録。技術テストと全画像の最終美術承認は区別する。

各人物の全4検証・採用SHAはmotion_roster_native_verification.json。龍太の承認版motion_ryuta_2はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。Playは全8人の左右・7動作・反撃・通常/強の分岐・解除・回転・重力・ヒットストップ・描画非干渉を検査。古いTestResultsは保存済み実件数・ログSHAと人物別18PNGを残して整理し、過去再実行とは区別。

Web2はビルドと426画像監査が通っても実起動OOMで不採用。ビルド報告のテクスチャ1.3GBと既存WASM上限2048MBから、起動中の展開ピークが上限に達した可能性を考えた。WebGLBuilderの初期512MB・動的拡張上限4096MBでWeb3へ修正。原画縮小やミップマップ削除は行っていない。参考はUnity6000.6公式メモリ説明（指示書にURL）。

motion_roster_web_3：ビルド成功、119,935,208bytes／6ファイル／190.338秒、Windows復帰成功。426/426画像の原RGBAと透明余白を監査。Web用DXTとミップ表示は非可逆であり、原画保存と別。実ブラウザはCodex内蔵Chromiumで新規起動・トップ・9人2段選択・透真のDash/Jump・龍太の通常Hitstunと1/3を確認。音声running、Web3起動以降console error/warn 0。スクショとブラウザ状態はdocs/screenshots/motion_roster_web_3/。全56動作の全状態は実Unityで検査し、ブラウザは標本確認。実機パッド、通常Chrome/Edge個別、低メモリ端末は未再検査。

最終motion_web_memory_1：BuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。全8人の左右・7動作・通常/強被弾・反撃・回転・解除・重力・ヒットストップ・描画非干渉を検査。進行表complete。最新WindowsはBuild/motion_web_memory_1/AniBato.exe。最終機械可読証跡はmotion_roster_final_verification.json。通常commit/pushで保存する。

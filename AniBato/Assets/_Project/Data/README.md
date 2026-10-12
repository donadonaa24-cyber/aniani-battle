# フェーズ3の調整用アセット

Unityメニュー「**あにバト → フェーズ3プロジェクト生成**」、または `AniBato.EditorTools.ProjectBuilder.BuildAll` で生成します。

旧22個を保持し、Touma/Ren/Chizuruそれぞれ独立の地上通常3・空中通常3・特殊4・超必殺1・CharacterDataの12個と、BattleSetupを追加します。標準計59個。既存の調整値は保持します。初期値と全数値はルートの `BALANCE_CONFIG.md` を参照してください。BattleSetupの標準順序は透真・蓮・千鶴、P1Index=0/P2Index=1です。

フェーズ2の実生成はClaudeが確認済みです。フェーズ3の生成・再実行による調整保持・画像インポートはClaudeのUnity検証待ちです。手書きYAMLでは生成しません。

ART1：BuildAllはchizuruAnimationSet.assetを追加し、Chizuru.Animationsと私服Portraitへ接続します（標準データ60アセット）。PNG/JSONはArt/Sprites/chizuru。Sprite IDを保持してスライスし、AnimationSet.TimingのInspector調整も保持します。コード検証済み、Unityによる生成はClaude待ちです。

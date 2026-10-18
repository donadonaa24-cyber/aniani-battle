const fs=require('fs'),root='art_source/paper_roster_action/akatsuki/',read=p=>JSON.parse(fs.readFileSync(p,'utf8')),write=(p,v)=>fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');
const effects=read(root+'effects-manifest.json');const old=effects.entries.find(e=>e.id==='card'&&e.version===1);
old.rejectionReason='プレビューでは外周に光彩が見えたため版2を生成。後のRGBA確認で外側の色はalpha0のRGBと判明。v1は版比較で未採用、背景不透明と断定しない。';
const chosen=effects.entries.find(e=>e.id==='card'&&e.version===2);chosen.artNote='外側の灰色RGBはalpha0。白背景への診断合成で硬い透明境界を確認。原画そのものは加工しない。';write(root+'effects-manifest.json',effects);
const m=read(root+'manifest.json');for(const job of m.jobs){const j=read(job.promptFile);
 if(j.pose==='super')continue;
 if(j.phase==='impact'&&['normal3','special'].includes(j.pose)){
  j.references=[j.references[0]];j.referenceRoles=['本人の採用待機、空手の投げ終わりの編集対象'];
  j.prompt=j.prompt.replace('画像2は持つカードの正式な背面意匠。画像3は本人左手の薄いカードのつまみ持ちの手元参考。','')
   .replace(/カードを持つのは必ず本人左手、右手には持たせない。[\s\S]*?1189×1323の真の透明PNG一枚。/,
    '右向きの斜め横で胸と両腕両脚が見える。左右の腕2本と手2個、脚2本、各手は親指1本と4指。投げた後なので両手は空手、カードを描かない。1189×1323の真の透明PNG一枚。');
 }else{j.references[1]=chosen.source;j.referenceRoles[1]='正式背面を元に生成した本作の投射カード、保持中も同じ意匠';}
 write(job.promptFile,j);
}
write(root+'card-selection.json',{id:'card',version:2,source:chosen.source,sha256:chosen.sha256,artApproved:false,note:chosen.artNote,diagnosticOnly:'review/card_on_white.png'});

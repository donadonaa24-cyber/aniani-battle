const fs=require('fs'),root='art_source/paper_roster_action/akatsuki/',m=JSON.parse(fs.readFileSync(root+'manifest.json','utf8'));
for(const job of m.jobs){const p=job.promptFile,j=JSON.parse(fs.readFileSync(p,'utf8'));
 j.prompt=j.prompt.replace('画像3は本人左手の薄いカードのつまみ持ちの手元参考。','').replace('髪の幅約350px','髪の幅約335px');
 if(j.references.length>2){j.references=j.references.slice(0,2);j.referenceRoles=j.referenceRoles.slice(0,2);}
 j.prompt+='\n最優先：画像1の採用待機の頭・髪・眼鏡・顔と左右の靴の実寸を厳守。全身を大きく描き直さない。1189×1323キャンバス、頭髪の幅335px、接地は靴底1291。両膝を曲げて身体の高さを下げ、頭や髪を大きくしない。上側には透明余白を残す。';
 fs.writeFileSync(p,JSON.stringify(j,null,2)+'\n');
}

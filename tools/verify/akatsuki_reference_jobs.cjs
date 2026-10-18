const fs=require('fs'),sharp=require('sharp'),root='art_source/paper_roster_action/akatsuki/';
const m=JSON.parse(fs.readFileSync(root+'manifest.json','utf8'));
if(m.entries.length)throw Error('References must be assigned before generation');
const card='art_source/title_paper/references/battle_a_la_carte_card_back_user_20261017.png',grip='art_source/title_paper/references/akatsuki_left_card_grip_v1.png';
for(const job of m.jobs){const j=JSON.parse(fs.readFileSync(job.promptFile,'utf8'));
 if(j.pose==='super'){
  j.references=[m.reference];j.referenceRoles=['本人の採用待機、容姿・部位実寸の基準'];
  // The super has no card; never encourage an item from a reference to leak into it.
  j.prompt=j.prompt.replace('画像2は持つカードの正式な背面意匠。画像3は本人左手の薄いカードのつまみ持ちの手元参考。','')
   .replace(/カードを持つのは必ず本人左手、右手には持たせない。[\s\S]*?1189×1323の真の透明PNG一枚。/,
    '右向きの斜め横で胸と両腕両脚が見える。左右の腕2本と手2個、脚2本、各手は親指1本と4指。カードは持たない。1189×1323の真の透明PNG一枚。');
 }else{j.references=[j.references[0],card,grip];j.referenceRoles=['本人の採用待機、直接の編集対象','正式カード背面の意匠','本人左手のカード下端のつまみ方だけ'];}
 if(!j.references[0])j.references[0]='art_source/paper_roster_idle/generated/akatsuki_idle_v1.png';
 fs.writeFileSync(job.promptFile,JSON.stringify(j,null,2)+'\n');
}
(async()=>{const {data,info}=await sharp('art_source/paper_roster_idle/generated/akatsuki_idle_v1.png').ensureAlpha().raw().toBuffer({resolveWithObject:true});
 function span(top,bottom){let l=info.width,r=-1;for(let y=top;y<bottom;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]>=32){l=Math.min(l,x);r=Math.max(r,x);}return {left:l,right:r,width:r-l+1};}
 console.log(JSON.stringify({canvas:{width:info.width,height:info.height},hair:span(0,310),shoes:span(1160,info.height)}));})();

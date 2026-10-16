// Update only the adopted normal2 mapping; preserve all source images and bag backups.
const fs=require('fs');const file='art_source/paper_cutout/runtime-manifest.json',m=JSON.parse(fs.readFileSync(file));
const first=m.poses.find(p=>p.key==='attack1'),index=m.poses.findIndex(p=>p.key==='attack2'),old=m.poses[index];
if(!first?.frames||first.frames.length!==2||index<0)throw Error('Missing adopted first pair');
m.poses[index]={...structuredClone(first),key:'attack2',states:old.states,artNote:'2026-10-16指定：通常1・2段は同じ予備／当てる。3段のみ大振り、計4原画。旧2段原画は予備として保持。'};
m.note='通常1・2段の2原画を共用し、3段の大振り2原画と合わせ計4原画。本人左手フライパン。その他の姿勢・数値とカバン予備は維持。';
fs.writeFileSync(file,JSON.stringify(m,null,2)+'\n');
const catFile='art_source/paper_cutout/review-catalog.json',cat=JSON.parse(fs.readFileSync(catFile));
for(const row of cat.entries)if(row.key==='attack2'||row.key==='attack2_impact'){
 row.source=first.frames[row.key.endsWith('_impact')?1:0].source;row.note='通常1・2段で共有。3段と合わせて4原画。';
}
fs.writeFileSync(catFile,JSON.stringify(cat,null,2)+'\n');console.log('Chizuru normal2 now uses the first pair. Originals preserved.');

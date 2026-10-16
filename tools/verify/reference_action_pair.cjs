// Add a saved previous key as an explicit reference before generating its paired image.
const fs=require('fs'),path=require('path');const [key,id,source,note]=process.argv.slice(2);
if(!/^[a-z]+$/.test(key)||!fs.existsSync(source)||path.isAbsolute(source))throw Error('Character key and workspace-relative saved reference required');
const root='art_source/paper_roster_action/'+key+'/',m=JSON.parse(fs.readFileSync(root+'manifest.json','utf8'));
const file=m.jobs.find(j=>j.id===id).promptFile,j=JSON.parse(fs.readFileSync(file,'utf8'));
if(fs.existsSync(j.source))throw Error('Do not modify a generated job');
if(!j.references.includes(source)){j.references.push(source);j.referenceRoles.push(note);j.prompt+='\n追加参照の役割：最後の画像は'+note+'。人物の寸法と武器・衣装の同一性を維持し、指定の次のポーズだけに変える。';fs.writeFileSync(file,JSON.stringify(j,null,2)+'\n');}
console.log(JSON.stringify(j));

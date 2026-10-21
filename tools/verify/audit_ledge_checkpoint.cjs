// Compare existing numeric attack parameters to the saved HUD checkpoint. New ledge eligibility is separate.
const fs=require('fs'),path=require('path'),cp=require('child_process');
const root=path.resolve(__dirname,'../..'),baseline='eace794';
function values(text){
 const stack=[],result={};
 for(const line of text.split(/\r?\n/)){
  const m=line.match(/^(\s*)([\w]+):\s*(.*?)\s*$/);if(!m)continue;
  const depth=m[1].length;while(stack.length&&stack[stack.length-1].depth>=depth)stack.pop();
  const key=[...stack.map(s=>s.key),m[2]].join('.');
  if(m[3]==='')stack.push({depth,key:m[2]});
  else if(/^-?\d+(\.\d+)?(e[-+]?\d+)?$/i.test(m[3]))result[key]=Number(m[3]);
 }
 return result;
}
const files=cp.execFileSync('git',['diff','--name-only',baseline,'--','AniBato/Assets/_Project/Data'],{cwd:root,encoding:'utf8',stdio:['ignore','pipe','pipe']}).trim().split(/\r?\n/).filter(Boolean);
let attacks=0,fields=0;const differences=[];
for(const file of files){
 const current=fs.readFileSync(path.join(root,file),'utf8');if(!current.includes('AniBato.Game::AniBato.Game.AttackData'))continue;
 const old=values(cp.execFileSync('git',['show',baseline+':'+file],{cwd:root,encoding:'utf8'})),now=values(current);attacks++;
 for(const [key,value]of Object.entries(old)){
  fields++;if(now[key]!==value)differences.push({file,key,before:value,after:now[key]});
 }
}
const result={baseline,attacksCompared:attacks,existingNumericFieldsCompared:fields,differences};
fs.writeFileSync(path.join(root,'TestResults/ledge-balance-preservation.json'),JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify({attacks,fields,differences:differences.length}));if(differences.length)process.exitCode=1;

const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..');
const walk=dir=>fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(dir,e.name)):[path.join(dir,e.name)]);
const rel=p=>path.relative(root,p).replaceAll('\\','/');
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function fromLog(name){const text=fs.readFileSync(path.join(root,'TestResults',name),'utf8');return [...text.matchAll(/^\s+[\d.]+\s+[kmgt]?b\s+[\d.]+%\s+(Assets\/.+)$/gmi)].map(m=>m[1].trim());}
const art=walk(path.join(root,'AniBato/Assets/_Project/Art')).filter(p=>p.endsWith('.png')).map(p=>({path:rel(p),bytes:fs.statSync(p).size,sha256:hash(p)}));
const data=walk(path.join(root,'AniBato/Assets/_Project/Data')).filter(p=>p.endsWith('.asset')).map(p=>({path:rel(p),sha256:hash(p)}));
const win=walk(path.join(root,'Build/mobile_final_2')).map(p=>({path:rel(p),bytes:fs.statSync(p).size}));
const used=fromLog('mobile_final_2_build.log');
const baseline={date:'2026-10-23',sourceCommit:'d308812',windows:{expandedBytes:win.reduce((n,f)=>n+f.bytes,0),zipBytes:509049136,largest:win.sort((a,b)=>b.bytes-a.bytes).slice(0,6),usedAssets:used,usedPngs:used.filter(p=>p.endsWith('.png')).length},android:{apkBytes:161961932,usedAssets:fromLog('android_ascii_Android.log')},web:{bytes:138814613},art,data};
fs.writeFileSync(path.join(root,'TestResults/size_optimization_baseline.json'),JSON.stringify(baseline,null,2)+'\n');
console.log(JSON.stringify({windowsBytes:baseline.windows.expandedBytes,windowsUsedPngs:baseline.windows.usedPngs,originalPngs:art.length,originalPngBytes:art.reduce((n,f)=>n+f.bytes,0),androidUsedPngs:baseline.android.usedAssets.filter(p=>p.endsWith('.png')).length,gameDataAssets:data.length}));

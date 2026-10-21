// Record build/pixel proofs only after the real-browser screenshots and console capture exist.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const root=path.resolve(__dirname,'../..'),tag='paper_ledge_A_web';
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
const build=read('TestResults/webgl-build.json'),pixels=read('docs/codex_logs/paper_ledge_web_pixels.json');
if(build.result!=='success'||pixels.checked!==454||pixels.failed!==0)throw Error('Build or pixel audit incomplete');
for(const suffix of ['WebGL','RestoreWindows'])if(!fs.readFileSync(path.join(root,`TestResults/${tag}_${suffix}.log`),'utf8').includes('Application will exit with return code 0'))throw Error('Unity exit not confirmed');
const folder='docs/screenshots/paper_ledge_A_web',consoleRows=read(folder+'/browser_console.json');
if(consoleRows.length)throw Error('Review browser warnings/errors before recording success');
const screenshots=['chizuru_left_hang.png','chizuru_jump_return.png'].map(file=>({file:folder+'/'+file,sha256:sha(folder+'/'+file)}));
const report=read('docs/codex_logs/paper_ledge_verification.json');
if(report.nativeTag!=='paper_ledge_A'||report.handPattern!=='A'||report.thumbSide!=='image-left')throw Error('Earlier B art verification cannot be reused');
report.web={status:'verified',tag,build,windowsRestored:true,nativeTexturesChecked:454,rgbaPreserved:true,
 browser:{method:'Real keyboard inputs through in-app browser after reloading the new build',scenario:'Practice / Chizuru vs Ren / Star stage / ledge hang with ST drain / Space ledge jump and return to Idle',warnings:0,errors:0,screenshots}};
fs.writeFileSync(path.join(root,'docs/codex_logs/paper_ledge_verification.json'),JSON.stringify(report,null,2)+'\n');
const progress=read('art_source/paper_ledge/progress.json');progress.status='user_A_native_and_web_verified';progress.webTag=tag;
fs.writeFileSync(path.join(root,'art_source/paper_ledge/progress.json'),JSON.stringify(progress,null,2)+'\n');
console.log('Corrected-palm native, WebGL, pixel and real-browser proofs recorded.');

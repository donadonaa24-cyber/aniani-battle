mergeInto(LibraryManager.library,{
 AniBatoOnlineSend:function(ptr){if(window.AniBatoOnline)window.AniBatoOnline.send(UTF8ToString(ptr));},
 AniBatoOnlineUi:function(ptr){if(window.AniBatoOnline)window.AniBatoOnline.ui(UTF8ToString(ptr));},
 AniBatoResumeAudio__deps:['$WEBAudio'],
 AniBatoResumeAudio:function(){
  if(typeof WEBAudio!=='undefined'&&WEBAudio.audioContext){WEBAudio.audioContext.resume();window.AniBatoAudioState=WEBAudio.audioContext.state;}
 },
 AniBatoStatus__deps:['$WEBAudio'],
 AniBatoStatus:function(ptr){
  if(typeof WEBAudio!=='undefined'&&WEBAudio.audioContext)window.AniBatoAudioState=WEBAudio.audioContext.state;
  if(window.AniBatoStatus)window.AniBatoStatus(UTF8ToString(ptr));
 }
});

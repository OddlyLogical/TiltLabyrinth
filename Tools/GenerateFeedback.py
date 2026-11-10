from pathlib import Path
import math,wave,struct
root=Path(__file__).resolve().parents[1]/'Audio'/'Effects'
rate=22050
for name,duration in [('fall',1.1),('checkpoint',1.8)]:
 data=bytearray()
 for i in range(int(rate*duration)):
  t=i/rate
  if name=='fall':
   v=.13*math.sin(2*math.pi*(380*t-90*t*t))*(1-math.exp(-t*35))*math.exp(-t*4)
  else:
   v=sum(.1*math.sin(2*math.pi*f*t)*math.exp(-t*(2.8+j))*(1-math.exp(-t*100)) for j,f in enumerate([659.25,987.77]))
  data.extend(struct.pack('<h',int(max(-1,min(1,v))*32767)))
 with wave.open(str(root/(name+'.wav')),'wb') as w:
  w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(data)
print('Created original falling and checkpoint feedback WAV files')

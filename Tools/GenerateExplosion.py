from pathlib import Path
import math,wave,struct,random
root=Path(__file__).resolve().parents[1]
random.seed(82);rate=22050;data=bytearray();noise=0
for i in range(int(rate*.6)):
 t=i/rate;noise=noise*.82+random.uniform(-1,1)*.18
 v=(.24*math.sin(2*math.pi*(100*t-45*t*t))+.22*noise)*(1-math.exp(-t*450))*math.exp(-t*8)
 data.extend(struct.pack('<h',int(v*32767)))
with wave.open(str(root/'Audio/Effects/explosion.wav'),'wb') as w:
 w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(data)

# Original deterministic PCM audio. Every loop is constructed from integer cycles
# on its exact duration, so both the waveform and its derivatives are periodic.
from pathlib import Path
import random, math, wave, struct
p=Path(__file__).resolve().parents[1]
random.seed(118)
rate=22050
def wav(name,seconds,fn):
    f=p/'Audio'/name;f.parent.mkdir(parents=True,exist_ok=True)
    with wave.open(str(f),'wb') as w:
        w.setparams((1,2,rate,0,'NONE','not compressed'))
        data=bytearray()
        for i in range(int(rate*seconds)):
            v=max(-1,min(1,fn(i/rate)))
            data.extend(struct.pack('<h',int(v*32767)))
        w.writeframes(data)
tau=2*math.pi
def zen(t):
    pad=sum(math.sin(tau*round(f*32)/32*t)*(0.65+0.35*math.cos(tau*t/32+j)) for j,f in enumerate([130.81,196,261.63,329.63,392]))*0.045
    # Smooth periodic bell envelopes, no discontinuities at the loop seam.
    bells=sum(0.025*(0.5+0.5*math.cos(tau*t/32+j*1.7))**14*math.sin(tau*round(f*32)/32*t) for j,f in enumerate([523.25,659.25,783.99]))
    return pad+bells
wav('Music/zen.wav',32,zen)
phases=[random.random()*tau for _ in range(70)]
wav('Ambience/wind.wav',8,lambda t:sum(math.sin(tau*(35+i*19.125)*t+phases[i])/(1+i*0.08) for i in range(70))*0.008*(0.8+0.2*math.cos(tau*t/8)))
wav('Effects/roll.wav',2,lambda t:sum(math.sin(tau*(55+i*27.5)*t+phases[i]) for i in range(26))*0.018*(0.85+0.15*math.cos(tau*3*t)))
wav('Effects/impact.wav',0.7,lambda t:(1-math.exp(-t*900))*math.exp(-t*14)*(math.sin(tau*1320*t)+0.5*math.sin(tau*2117*t)+0.2*math.sin(tau*3511*t))*0.32)
wav('Effects/victory.wav',3,lambda t:sum((1-math.exp(-max(0,t-j*0.23)*100))*math.exp(-max(0,t-j*0.23)*2.8)*math.sin(tau*f*max(0,t-j*0.23))*0.13 for j,f in enumerate([523.25,659.25,783.99])))

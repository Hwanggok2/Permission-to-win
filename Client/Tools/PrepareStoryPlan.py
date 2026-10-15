"""Prepare localized story edits from the character brief; Excel remains the source."""
from pathlib import Path
import json,re
repo=Path(__file__).resolve().parents[2]
doc=(repo/'Docs/Design/CHARACTERS.md').read_text(encoding='utf-8')
out=repo/'Client/Evidence/pixel-art-20261015'
stages=json.loads((out/'stages-before.json').read_text(encoding='utf-8'))['stages']
dialogs={}
for line in doc.splitlines():
    cells=[c.strip() for c in line.strip('|').split('|')]
    if len(cells)==4 and re.fullmatch(r'C\d\d',cells[0]): dialogs[cells[0]]=cells[1:]
names=['강민재','문시온','오준호','정한결','남기석','최건우','한유진','박정민','한태식','배성훈','김준서','서지훈','장태오','서강현','이도현','백승우','임재훈','윤정호','조은찬']
english=['Kang Min-jae','Moon Si-on','Oh Jun-ho','Jung Han-gyeol','Nam Gi-seok','Choi Geon-woo','Han Yu-jin','Park Jeong-min','Han Tae-sik','Bae Seong-hun','Kim Jun-seo','Seo Ji-hoon','Jang Tae-o','Seo Gang-hyeon','Lee Do-hyeon','Baek Seung-woo','Lim Jae-hoon','Yoon Jeong-ho','Jo Eun-chan']
ranks=['훈련병','이병','일병','상병','병장','하사','공군 중사','상사','원사','준위','소위','소대장','중대장','작전참모','대대장','연대장','부사단장','사단장','일병']
eranks=['Recruit','Pvt.','Pfc.','Cpl.','Sgt.','Staff Sgt.','Air Force Tech Sgt.','Master Sgt.','Sgt. Major','Warrant Officer','2nd Lt.','Platoon Cmdr.','Company Cmdr.','Operations Officer','Battalion Cmdr.','Regiment Cmdr.','Deputy Division Cmdr.','Division Cmdr.','Pfc.']
en_dialogs=[
 ['They say you were a pro. I think I can beat you.','Was that pro story made up?','A pro really is different. Next time, I win.'],
 ['Wait, am I really playing a pro?','I can make moves like that too!','Oh, I missed that one space.'],
 ['I heard the rumors. Let me see for myself!','Now I have a story to tell!','Better than the rumors. Respect!'],
 ['I studied your moves. This time I can read you.','My preparation paid off.','I need to write that move down.'],
 ['Relax. I plan to win at my own pace.','I may look relaxed, but I watch the board.','Now that move woke me up!'],
 ['Tell me why you chose that move.','Let us review the move you missed.','Good judgment. I learned something.'],
 ['An exchange match? Time to calculate.','I saw that route first.','Unexpected. I need to recalculate.'],
 ['Relax. Take your time.','I spotted it a little sooner today.','Ha! A fine way to lose!'],
 ['No rush. Look a little longer.','There was still another move to see.','You read deeply. Well played.'],
 ['This one space may change everything.','A small difference mattered, right?','Aha! So that is how it connects!'],
 ['I practiced a lot. Let us have a good game!','That was the move I practiced!','I was not ready for that. More to learn!'],
 ['Show me the skill everyone talks about.','Good move. Look one step further.','The company commander should see this.'],
 ['Let us see if the rumors are true.','This round is mine.','Good! Those rumors were well earned.'],
 ['Let us consider the other possible moves.','That was the best of the moves I compared.','I overlooked that option.'],
 ['I will meet your best move head-on.','My move got there first.','A clean move. Well played.'],
 ['Everyone has a style. What is yours?','My style worked this round.','Interesting. You have your own style.'],
 ['Is it my turn to learn something today?','My instincts still work!','I learned a lot. That was fun.'],
 ['A match with a pro. I look forward to it.','A good game. Challenge me again.','An excellent move. Let us play once more.'],
 ['I care more about this board than your fame.','The next move matters more than fame.','Good move. Play your game in the final.']]
ids=list(range(1,13))+[n for n in range(13,19) for _ in range(2)]+[2,3,4,5,19,1]
rounds=['64강','32강','16강','8강','4강','결승']
erounds=['Round of 64','Round of 32','Round of 16','Quarterfinal','Semifinal','Final']
tournament_en=[
 ['I am not nervous now. I practiced!','This time I followed every move!','I showed what I learned. I am cheering for you!'],
 ['Less talk today. Let my moves speak!','This game will be my story today!','I lost, but what a game. Keep going!'],
 ['I reviewed every move from my last loss.','That review paid off.','More moves to learn. Reach the final!'],
 ['We came this far. Time to get serious.','I want to go all the way too.','Respect. Play a good game for me too.'],
 en_dialogs[18],
 ['I only had confidence before. Now I am ready.','This time I won! Come back for a rematch.','You are the champion. Play me again sometime.']]
tournament_ko=[]
for n in range(25,31):
    line=next(l for l in doc.splitlines() if l.startswith(f'| {n} ·'))
    tournament_ko.append([c.strip() for c in line.strip('|').split('|')][1:])
bridges={13:('좋아, 한 판 더! 이번엔 다른 수를 봐야겠군.','Good! One more. I will try another move.'),15:('그 선택은 놓쳤군. 다음 판에서 다시 비교해 보자.','I missed that choice. Let us compare again.'),17:('깔끔했네. 한 판 더 부탁하지.','Cleanly played. One more game, please.'),19:('흥미로운 기풍이군. 조금 더 두어 보세.','An interesting style. Let us play again.'),21:('정말 한 수 배웠네! 한 판 더 괜찮겠나?','I learned a lot! How about one more?'),23:('훌륭한 수였네. 한 판 더 두어 보세.','An excellent move. Let us play again.'),24:('내가 졌군. 사단 병사 대회에서도 실력을 보여 주게.','You beat me. Show your skill in our enlisted tournament.')}
strings=[]
def add(key,ko,en):
    strings.append({'key':key,'ko':ko,'en':en})
    return key
for stage,cid in zip(stages,ids):
    n=stage['id'];code=f'C{cid:02}';idx=cid-1
    ko_rank=ranks[idx] if n!=30 else '이병'
    en_rank=eranks[idx] if n!=30 else 'Pvt.'
    label=f'{ko_rank} {names[idx]}'
    elabel=f'{en_rank} {english[idx]}'
    if n>=25:
        label=rounds[n-25]+' · '+label;elabel=erounds[n-25]+' · '+elabel
    stage['characterId']=code
    stage['rankKey']=add(f'story_opponent_{n:03}',label,elabel)
    if n==1: desc=('프로 출신 입대자라는 소문을 듣고 동기가 도전한다.','A fellow recruit challenges you after hearing you were a pro.')
    elif n==2: desc=('훈련을 마치고 이병이 된 당신. 부대에도 프로 소문이 퍼졌다.','Training is over. Word of your pro career spreads through the unit.')
    elif n>=25: desc=(f'사단 병사부 {rounds[n-25]}. 다시 만난 상대와 실력으로 겨룬다.',f'The enlisted tournament: {erounds[n-25]}. Let your moves speak.')
    elif n==24: desc=('사단장과 두 번째 대국. 마지막 수까지 집중하자.','Your second game with the division commander. Focus to the last move.')
    else: desc=(f'{names[idx]}와의 부대 대국. 한 수씩 서로의 기풍을 읽는다.',f'A friendly match with {english[idx]}. Learn each other\'s style.')
    stage['descriptionKey']=add(f'story_stage_desc_{n:03}',*desc)
    if n==1: continue
    kd=tournament_ko[n-25] if n>=25 else dialogs[code].copy()
    ed=tournament_en[n-25] if n>=25 else en_dialogs[idx].copy()
    if n in bridges: kd[2],ed[2]=bridges[n]
    for field,suffix,k,e in zip(['aiIntroKey','aiWinKey','aiLoseKey'],['intro','win','lose'],kd,ed):
        stage[field]=add(f'story_{n:03}_{suffix}',k,e)
add('action_to_round_64','64강으로','To the round of 64')
add('story_champion','사단 병사부 4목 대회 우승!','Division enlisted tournament champion!')
assert len(strings)==149 and len({s['key'] for s in strings})==149
(out/'story-plan.json').write_text(json.dumps({'stages':stages,'strings':strings},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f'Prepared {len(stages)} stages / 19 characters / {len(strings)} added bilingual keys')

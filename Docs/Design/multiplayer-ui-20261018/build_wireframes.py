"""Generate review-only SVG/HTML wireframes; does not touch Unity or game data."""
from pathlib import Path
from html import escape

HERE = Path(__file__).resolve().parent
BG, INK, PANEL, OLIVE, GOLD = '#ded7bd', '#26291f', '#f4ebcc', '#586443', '#b18239'

def rect(x,y,w,h,fill=PANEL,stroke='#716348',sw=2):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>'

def text(x,y,value,size=30,fill=INK,anchor='middle',weight=400):
    return f'<text x="{x}" y="{y}" font-size="{size}" fill="{fill}" text-anchor="{anchor}" font-weight="{weight}">{escape(value)}</text>'

def lines(x,y,values,size=30,step=44,fill=INK):
    return ''.join(text(x,y+i*step,v,size,fill) for i,v in enumerate(values))

def button(x,y,w,label,primary=False,disabled=False,h=108):
    fill = '#d6d2c1' if disabled else OLIVE if primary else PANEL
    color = '#8a897f' if disabled else '#fff9e5' if primary else INK
    if label == '상대방 응답 대기 중':
        label_svg = lines(x+w/2,y+h/2-8,['상대방 응답','대기 중'],30,38,color)
    else:
        label_svg = text(x+w/2,y+h/2+11,label,32,color,weight=600)
    return rect(x,y,w,h,fill)+label_svg

def header(name='멀티 배틀'):
    return rect(0,0,720,1280,BG,'none')+rect(0,0,720,96,'#e9e1c4','none')+text(32,65,name,40,anchor='start',weight=700)+button(592,0,104,'×',h=96)

def profile():
    return rect(24,112,672,90)+text(52,146,'내 계급',26,anchor='start')+text(52,184,'이등병 · 레이팅 200',34,anchor='start',weight=600)

def board(x=24,y=300,size=672):
    s=rect(x,y,size,size,'#986833','#4c3924',10)
    for i in range(16):
        s+=rect(x+12,y+12+i*(size-24)/16,size-24,7,'#b98845','none')
    left,top,step=x+size*.08,y+size*.16,size*.14
    for i in range(7):
        s+=f'<path d="M{left+i*step} {top}v{step*5}" stroke="#433b29" stroke-width="3"/>'
    for i in range(6):
        s+=f'<path d="M{left} {top+i*step}h{step*6}" stroke="#433b29" stroke-width="3"/>'
    for col,row,white in [(1,5,False),(3,5,True),(4,5,False),(3,4,True),(5,5,False),(2,5,True),(2,4,False)]:
        s+=f'<circle cx="{left+col*step}" cy="{top+row*step}" r="{step*.29}" fill="{"#f8f5df" if white else "#2d3027"}" stroke="#1f241d" stroke-width="5"/>'
    return s

def player_pair():
    return rect(24,108,328,112)+rect(368,108,328,112)+text(44,150,'● 나',26,anchor='start')+text(44,194,'이등병 · 200',30,anchor='start',weight=600)+text(388,150,'○ 상대',26,anchor='start')+text(388,194,'이등병 · 200',30,anchor='start',weight=600)+text(360,251,'내가 선공 · 내 돌: 흑',28)

def footer(result='',notice='',finished=False,requested=False,left=False):
    s=rect(0,1060,720,220,'#e9e1c4','none')
    s+=text(360,1095,result or '레이팅 반영 ON',32,weight=600)
    if notice:s+=text(360,1135,notice,28)
    if finished:s+=button(24,1152,328,'상대방 응답 대기 중' if requested else '다시하기',True,requested or left)+button(368,1152,328,'나가기',primary=left)
    else:s+=button(24,1152,672,'놓기',True)
    return s

def game(result='',notice='',finished=False,requested=False,left=False):
    s=header()+player_pair()+rect(24,260,672,64,'#586443','none')+text(360,302,'대국 종료' if finished else '내 차례 · 42초',32,'#fff9e5',weight=600)
    return s+board(y=340)+footer(result,notice,finished,requested,left)

screens={}
screens['01-menu']=('멀티 메뉴',header()+profile()+lines(360,270,['비슷한 레이팅의 상대와 겨뤄 보세요.','친선전은 점수 반영을 선택할 수 있어요.'])+button(24,366,672,'무작위 매치',True,h=150)+button(24,540,672,'친선전',h=150))
screens['02-search']=('상대 찾는 중',header()+profile()+rect(24,350,672,290)+text(360,438,'상대 찾는 중',40,weight=700)+lines(360,510,['레이팅 차이 100점 이내에서','찾는 중…'])+button(24,1152,672,'매칭 취소'))
screens['03-host']=('친선전 · 초대하는 사람',header()+profile()+lines(360,270,['친구를 초대하세요.','경기가 시작되면 점수 설정은 고정됩니다.'])+button(24,366,672,'레이팅 반영 ON',True,h=104)+button(24,494,328,'친구에게 공유',True)+button(368,494,328,'초대 URL 복사')+rect(24,626,672,80)+lines(360,658,['https://hwanggok2.github.io/Permission-to-win/','#invite=…'],23,28))
screens['04-invite']=('친선전 · 초대받은 사람',header()+profile()+rect(24,360,672,300)+text(360,428,'친구 레이팅 200',40,weight=600)+text(360,502,'레이팅 반영 OFF',34,OLIVE,weight=600)+lines(360,570,['입장하면 경기가 시작됩니다.'])+text(360,1080,'레이팅 반영 OFF',30)+button(24,1152,672,'이 설정으로 입장',True))
screens['05-playing']=('대전 중',game())
screens['06-result']=('승패 결과',game('승리! 레이팅 +16 · 현재 216점','',True))
screens['07-rematch']=('상대 리매치 요청',game('승리! 레이팅 +16 · 현재 216점','상대방이 리매치를 요청하였습니다',True))
screens['08-wait']=('내가 리매치 요청한 상태',game('승리! 레이팅 +16 · 현재 216점','상대방 응답 대기 중',True,True))
screens['09-left']=('상대방 퇴장',game('승리! 레이팅 +16 · 현재 216점','상대방이 나갔습니다',True,left=True))
confirm=game()+rect(0,0,720,1280,'#15180fc9','none')+rect(24,416,672,430)+lines(360,500,['지금 나가면 패배로 처리됩니다.','나가시겠습니까?'],32,52)+button(48,704,300,'취소',True)+button(372,704,300,'나가기')
screens['10-leave-confirm']=('진행 중 나가기 확인',confirm)

style='<style>@font-face{font-family:Pretendard;src:url(../../../Client/Assets/WebGLTemplates/StageGamePortrait/Fonts/Pretendard-Regular.woff2)}text{font-family:Pretendard,Arial,sans-serif}</style>'
for slug,(title,content) in screens.items():
    (HERE/(slug+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="720" height="1280" viewBox="0 0 720 1280" role="img" aria-label="{title}">{style}{content}</svg>',encoding='utf-8')

def compact(english=False,waiting=False,error=False):
    s=rect(0,0,320,403,BG,'none')+rect(0,0,320,44,'#e9e1c4','none')
    s+=text(10,29,'Multiplayer' if english else '멀티 배틀',24,anchor='start',weight=700)
    s+=rect(270,0,44,44)+text(292,30,'×',25)
    s+=rect(6,44,148,40)+rect(166,44,148,40)
    s+=text(14,59,'● You · 700' if english else '● 나 · 200점',11,anchor='start')
    s+=text(14,76,'Sergeant First Class' if english else '이등병',12,anchor='start',weight=600)
    s+=text(174,59,'○ Opponent · 1900' if english else '○ 상대 · 200점',11,anchor='start')
    s+=text(174,76,'Lieutenant General' if english else '이등병',12,anchor='start',weight=600)
    s+=text(160,96,'You go first · Your stones: black' if english else '내가 선공 · 내 돌: 흑',12)
    s+=rect(6,100,308,28,OLIVE,'none')+text(160,120,'Game finished' if english else '대국 종료',14,'#fff9e5',weight=600)
    bsize=159 if error else 175
    s+=board((320-bsize)/2,132,bsize)+rect(0,295 if error else 311,320,108 if error else 92,'#e9e1c4','none')
    s+=text(160,312 if error else 328,'You win! Rating +16 · Now 716' if english else '승리! 레이팅 +16 · 현재 216점',14,weight=600)
    notice=('Waiting for opponent' if waiting else 'Your opponent requested a rematch') if english else ('상대방 응답 대기 중' if waiting else '상대방이 리매치를 요청하였습니다')
    if error:s+=lines(160,331,['Could not reconnect. Please leave','and open multiplayer again.'],12,17)
    else:s+=text(160,349,notice,12)
    s+=rect(6,355,148,48,'#d6d2c1' if waiting or error else OLIVE)
    if waiting:s+=lines(80,375,['Waiting for','opponent'] if english else ['상대방 응답','대기 중'],14,17,'#77776c')
    else:s+=text(80,385,'Rematch' if english else '다시하기',15,'#77776c' if error else '#fff9e5',weight=600)
    s+=rect(166,355,148,48)+text(240,385,'Leave' if english else '나가기',15,weight=600)
    return s

compact_cards=''
for slug,title,en,wait,error in [('11-compact-ko','320×403 · 상대 요청',False,False,False),('12-compact-en','320×403 · 영어 긴 계급',True,False,False),('13-compact-en-wait','320×403 · 영어 응답 대기',True,True,False),('14-compact-en-error','320×403 · 긴 오류 안내',True,False,True)]:
    content=compact(en,wait,error)
    (HERE/(slug+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="320" height="403" viewBox="0 0 320 403" role="img" aria-label="{title}">{style}{content}</svg>',encoding='utf-8')
    compact_cards+=f'<article><h2>{escape(title)}</h2><a href="{slug}.svg"><svg xmlns="http://www.w3.org/2000/svg" width="320" height="403" viewBox="0 0 320 403" style="max-width:320px" role="img" aria-label="{escape(title)}">{content}</svg></a></article>'

cards=''.join(f'<article><h2>{i+1:02}. {escape(title)}</h2><a href="{slug}.svg"><svg xmlns="http://www.w3.org/2000/svg" width="360" height="640" viewBox="0 0 720 1280" role="img" aria-label="{escape(title)} 와이어프레임">{content}</svg></a></article>' for i,(slug,(title,content)) in enumerate(screens.items()))
(HERE/'index.html').write_text('''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>이겨도 됩니까? · 멀티 UI 와이어프레임</title><style>
@font-face{font-family:Pretendard;src:url(../../../Client/Assets/WebGLTemplates/StageGamePortrait/Fonts/Pretendard-Regular.woff2)}*{box-sizing:border-box}body{margin:0;background:#282d22;color:#f4ebcc;font:16px/1.6 Pretendard,Arial,sans-serif}header,main,footer{max-width:1600px;margin:auto;padding:28px}h1{font-size:30px;margin:0 0 12px}p{max-width:1000px}header{border-bottom:1px solid #65704e}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:30px}article{margin:0}h2{font-size:18px;font-weight:500}svg{width:100%;height:auto;max-width:360px;box-shadow:0 8px 24px #12140c}svg text{font-family:Pretendard,Arial,sans-serif}a{color:#f1d496}.tag{display:inline-block;padding:4px 12px;border:1px solid #8c9776;margin-right:8px}.warning{color:#ecd7a0}code{color:#ffd698}
</style><header><h1>이겨도 됩니까? · 멀티 UI 와이어프레임</h1><span class="tag">720 × 1280 설계 단위</span><span class="tag">2026-10-18 KST</span><span class="tag">배치 검토용 초안</span><p>고정 헤더 → 양측 플레이어 정보 → 차례 → 정사각형 판 → 결과와 상대 상태 → 하단 행동. 결과·다시하기·나가기를 항상 함께 보여줍니다. 전체 화면 스크롤을 제거하고 높이가 부족하면 판 영역을 줄입니다.</p><p class="warning">색·폰트·목재 인상은 기존 자산 계열을 유지합니다. 이 도형 보드는 배치 설명이며 새 게임 그래픽이 아닙니다. 예시 문구 중 프로필 분할·부가 안내는 기존 Excel 키를 재사용하거나 새 키를 등록해야 합니다. SVG는 고정 크기 참고이며 실제 반응형·한영 렌더링 검증은 Unity 구현에서 수행합니다.</p><a href="layout-spec.md">구체적 치수·상태별 요구·공식 출처 보기</a></header><main class="grid">'''+cards+'''</main><header><h1>가용 높이403px · 압축 표본</h1><p>320×568 화면에서 호스트 상하157px와 위8px를 제외한 실효 높이. 한/영 결과·상대 상태·다시하기/나가기를 함께 유지합니다. 아래는 CSS 크기320×403의 정지 설계 표본입니다.</p></header><main class="grid">'''+compact_cards+'''</main><footer>출처는 layout-spec.md에 정리. 외부 이미지/폰트/게임 자산을 복사하지 않았습니다. 본 산출물은 실제 게임 구현·사용자 수용 완료를 의미하지 않습니다.</footer></html>''',encoding='utf-8')
print(f'Wrote {len(screens)+4} SVG wireframes and index.html to {HERE}')

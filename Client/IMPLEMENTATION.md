# 스테이지 상세 중앙 배치 — 2026-10-16 준비 완료

**Client 소스 동결, Unity 종료. 총괄 WebGL 통합 빌드 시작 가능.** 상세 화면의 헤더와 본문에 같은 상하 여백을 주어 안전영역 중앙에 배치했다. 루트의 가로 슬라이드 위치와 분리하여 열기·닫기 및 이전/다음 스테이지 이동 후에도 위치를 유지한다. 작은 화면은 기존 터치 크기와 스크롤을 유지하며 영어 설명 공간을 두 줄 높이로 늘렸다. Detail 프리팹의 직렬화 참조가 RankAppRoot의 중첩 프리팹에도 적용된다.

Unity **7/7**, **58캡처**. 한영·6개 화면 크기·Toss 상하 inset 59/34 조합에서 중앙 배치, 스크롤 끝 시작 버튼 클릭, 슬라이드, 이전/다음 이동과 30개 설명의 잘림 여부를 검사했다. 최소 터치 크기는 48.88px 이상이다. 저장된 진행이 없는 최초 실행은 **1스테이지에서 규칙 패널을 연 상태로 시작**, 기존/복구 저장이 있으면 로비에서 시작함을 다시 확인했다. 새 빌드·배포·실기기 검사는 하지 않았다. 인계: [HANDOFF.md](Evidence/detail-centered-20261016/HANDOFF.md), `verification.json`, `source-freeze.json`.

---

# PC 중앙 세로 화면 — 2026-10-16 준비 완료

**Client 소스 동결, Unity 종료. 총괄 WebGL 통합 빌드 시작 가능.** PC는 최대480 CSS px의 세로 게임 영역을 중앙에 두며1280×720에서는405px로 높이에 맞춘다.800×1200의 큰 세로 창도480px 제한을 적용한다. 모바일320/390/454 세로 화면 폭은 유지하고 가로 회전은320px 폭과 기존 세로 스크롤을 사용한다. HTML/host-layout/Unity viewport·좌우 inset 좌표를 함께 맞추고 에디터 가로 창 확대도 방지했다. 기존 최초 규칙/중앙 안내/물음표 버튼 변경은 보존했다.

Unity **12/12**, Node VM **19/19**, Unity **100캡처**에서 목록/입력/스크롤/안전영역/DPR/기존 안내 회귀 검사 통과. 실제 브라우저 확인은 로컬 연결 시간 초과와 도구 URL 정책 차단으로 미완료다. 새 빌드·배포·실기기 검사는 하지 않았다. 총괄이 재빌드·Pages 배포 후 공개 사이트를 확인해야 한다. 변경 파일·한계·viewport별 기대값과 동결 정보: [HANDOFF.md](Evidence/desktop-centered-20261016/HANDOFF.md), `verification.json`, `source-freeze.json`.

---

# 앱 실행별 착수 안내·대국 도움말 — 2026-10-16

하단의 정상 열 선택 확인 문구를 제거했다. 중앙 착수 안내는 앱 실행 후 첫 대국의 첫 사람 턴에서만 표시하며 다시하기·되돌리기·다음 판·로비 왕복·씬 재로드·도움말 닫기에는 반복하지 않는다. 최초 실행 규칙 패널이 있으면 닫은 뒤 표시한다. 진행 저장과 별개의 세션 상태를 Unity 런타임 초기화 훅에서 재설정한다.

대국 좌측 상단에 우측 일시정지와 같은 Y/크기의 `?` 버튼을 Match/RankAppRoot에 저장하고 기존 도움말 패널과 클릭 효과음에 연결했다. 규칙을 다시 보는 동안 타이머·AI·착수는 멈추고 BGM은 계속하며 닫으면 선택/남은 시간을 유지한다. 기존 최초 실행 시 한 번의 자동 규칙 안내도 유지한다.

Unity **16/16 통과**, 한영 네 화면 크기 등 **27캡처**. 소스와 프리팹 반영, Unity 종료. 실제 기기 앱 재실행·새 빌드/배포는 미실행이다. 자세한 범위/검증/동결 정보: [HANDOFF.md](Evidence/session-guide-help-20261016/HANDOFF.md), `verification.json`, `source-freeze.json`.

---

# 최초 규칙 안내·중앙 착수 안내·작은 버튼 — 2026-10-16

사용자가 선택한 “최초 실행 때 한 번”에 맞춰 기존 진행 저장이 없는 첫 시작에서 Help 프리팹을 띄운다. 규칙 패널이 열려 있으면 타이머/착수/대사/AI와 뒤쪽 버튼을 막고 BGM은 계속한다. 닫으면 같은 남은 시간으로 이어가며 이후에는 로비 도움말로 다시 열 수 있다. Excel Strings C138/E138만 “돌은 아래에서부터 쌓여요” / “Stones stack from the bottom”으로 변경하고 기존 4개 연결·가로/세로/대각선 도식과 함께 표시한다. 다른 셀과 서식/시트 기능은 보존했다.

착수 안내는 기존 토스트를 사용해 화면 중앙에 흰색 굵은 글자와 짙은 외곽선으로 3초간 표시한다. 열 선택 시 즉시 사라지고 하단의 동일 안내는 숨긴다. 되돌리기/다시하기 배경을336×165→255×114 설계 단위로 줄이고 원래 터치 영역은 유지했다. Match/TurnToast/RankAppRoot를 저장했고 Rank/Pixel 제작 도구에도 반영했다. 원본 아트·음원·AI·스테이지·광고·씬·다른 프리팹은 유지했다.

Unity **16/16**, 최종 입력 차단 변경의 영향 항목 **4/4 재검사**, 데이터 **46/46**, exporter와 JSON 동기화 검사 통과. 한영 네 화면 크기에서 규칙/안내/버튼을 캡처하고 중앙 위치·하단 잘림·투명한 버튼 여백 클릭을 검사했다. 실제 기기/사람 플레이·새 WebGL/Android 빌드·배포는 미실행이다. Client 배치 종료 및 소스 동결. 자세한 인계: [HANDOFF.md](Evidence/first-rules-ui-20261016/HANDOFF.md), `verification.json`, `source-freeze.json`.

---

# WebGL 짧은 효과음·모바일 여백 수정 — 2026-10-16

카카오톡 아이폰 웹뷰의 짧은 AAC 디코드 오류와 화면이 작고 아래로 밀리는 현상을 수정했다. 원본 TypingKey/ButtonClick을 보존하고 앞 2048샘플 보호 무음을 포함하는 0.2초 사본을 RankAppRoot에 직렬화했다. 실제 Unity WebGL AAC 결과에서 원본 대비 peak는 타이핑 99.06%, 버튼 99.83%, 에너지는 각각 99.58/99.66%다. 약 46ms의 소리 시작 지연을 수반한다. 돌 소리와 BGM은 유지했다.

일반 웹뷰의 불필요한 호스트 64 CSS px를 제거하고 명시적 Toss inset은 유지했다. visualViewport 크기/offset을 컨테이너에 반영하며, CanvasScaler 이후 폭을 다시 제한하던 축소와 DPR별 터치 크기 계산을 고쳤다. 저장된 1080×1920 UI·아트·상하 여백 배치는 유지했다. 390×664 대국 제목 중심은 위에서110.92px, 판 폭337.36px, 최소 버튼49.29 CSS px이며 하단 버튼이 들어온다.

최종 고유 Unity **23/23**, Node 어댑터 **10/10**, 한영 네 화면 크기와 DPR 1/1.5/2를 포함한 **27캡처**를 확인했다. 초기 PCM 동일성 검사 2실패와 수정 후 5/5 재검사는 증거에서 구분한다. 원본 음원/메타·아트·데이터·Core·씬·다른 프리팹·기존 빌드19파일은 그대로다. **Client 소스 동결·Unity 종료**, 새 WebGL 재빌드·배포·실제 아이폰 카카오톡 재검증은 총괄 후속 단계다. 자세한 인계와 한계: [HANDOFF.md](Evidence/web-audio-fsb-20261016/HANDOFF.md), `verification.json`, `source-freeze.json`, `codec-report.json`.

---

# 전 레이팅 공격·방어 곡선 조정 — 2026-10-16

사용자의 전체 레이팅 점검 요청에 따라 200~1600의 15개 프로필을 같은 조건에서 비교했다. 200~400(첫 6단계)은 유지하고 500은 안전 후보 무작위 선택 75→70%로 조금 보강했다. 600~1100은 깊이 4·예산 4,000~16,000·무작위 60→16%, 1200~1600은 깊이 6·예산 30,000·무작위 10→0%다. 700 이상의 전술 누락 0%와 양쪽 위협 방어를 유지한다. 생산 AiSearch 코드·시간 제한·숨은 첫 수 보정은 변경하지 않았다. Excel E/F/G의 70개 셀을 수정하고 Generated/Resources에 반영했다. 자세한 15개 프로필과 수치는 `../GameData/DIFFICULTY-CONTRACT.md`에 있다.

검증은 실제 생산 AI를 사용했다. 독립적인 승/패 탐색으로 생성한 48개 전술 문제와 96개 추가 문제에서 조정한 뒤, 최종 프로필에 새 48개 문제를 적용했다. 마지막 문제 집합의 공격/방어 평균 정답률은 Rating 200 **37.8/59.9%**, 800 **70.3/94.3%**, 1600 **100/100%**이며 전 레이팅에서 두 평균이 감소하지 않았다. 범위는 1/3/5수 공격과 2/4/6수 방어, 판마다 16개 seed(확률 0인 1600은 1회)다. 제한 깊이 문제의 정답률이며 실제 Elo·사람 승률·모든 판의 완벽한 판단을 뜻하지 않는다.

고정 상대와 같은 시작판/seed/선후공 교대로 최종 15구간×128판 = **1,920판**을 검사했다. 대국 점수(무승부 0.5)는 200 0%, 700 23.8%, 800 35.9%, 1200 74.6%, 1600 81.6%였다. 1500의 82.8%보다 1600 표본이 약간 낮으므로 모든 100점 증가가 매번 승률 증가를 보장한다고 기록하지 않는다. 700~1600 모두에서 양쪽 선공·각 2개 seed·사람의 네 번째 착수까지 열거한 **101,374분기/15,980응수에 7/8수 조기 패배 0회**였다.

최종 Unity **44/44**, 데이터 **46/46**, exporter `--check`, JSON 동기화·원본 Excel의 다른 값/서식/시트 기능 보존을 확인했다. Unity 검사는 모든 15구간의 즉시 공격/방어, 700~1600의 무작위 선택 시 양쪽 위협 방어, 최상위의 5수 공격과 6수 패배 회피를 다룬다. 실제 코루틴의 0.65초 기한에서도 두 강제 공격을 찾았으며 1초 응수 연출·일시정지·중단·힌트·턴 타이머를 확인했다. 중간 설정 검사 `unity-tests.xml`과 최종 `unity-final-tests.xml`을 구분한다.

최종 인계: `Evidence/rating-curve-20261016/verification.json`, `source-freeze.json`, `unity-final-tests.xml`, `validation-tactics.json`, `final-secondary-comparison.json`, `final-matches.json`, `final-short-games.json`, `workbook-changes.json`. 기존 프리팹·아트·음원·씬·빌드를 유지하고 Unity 배치 종료 후 인계한다. 새 WebGL/Android 빌드·배포·기기/사람 플레이·커밋·푸시는 이 클라이언트 세션에서 하지 않았다. 총괄은 이 동결본을 기준으로 후속 빌드/테스트 배포와 공용 요약을 진행할 수 있다.

---

# 작은 일시정지 배경·7수 패배 원인 수정 (이하 이전 프로필의 기록)

큰 `||`가 배경과 섞여 보인다는 요청에 따라 일시정지 버튼 안에 96×96 설계 단위의 밝은 정사각형 Backdrop을 저장했다. 기존 픽셀 패널 자산을 재사용하고 아이콘 크기 120×120, 버튼 전체 터치 영역, 아이콘의 누름/비활성 피드백은 유지했다. 픽셀 자산 자체는 수정하지 않았다. 변경 도구는 기존 `RankUiBuilder.ApplyMatchPauseIcon`이며 Match 프리팹에 반영된다.

Rating 800의 7수 패배 원인은 1수 안전 후보 중 무작위 선택이 상대의 열린 3연속/양쪽 승리 위협을 허용하는 것이었다. `AiSearch`에서 깊이 2 이상 후보를 추가 검사해, 다음 상대 착수로 막을 수 없는 두 승리 열을 허용하는 수를 제외한다. 즉시 이길 수 있는 반격은 허용하고 모든 후보가 나쁘면 기존 합법 후보를 쓴다. 검사도 노드/Step 할당량과 중단을 지킨다. Excel에서 Rating 700 이상 전술 누락을 0으로 하고 Rating 700 평가 깊이를 1→2로 조정했다. Rating 200~600·나머지 깊이·노드 예산·안전 후보 무작위 비율·표시 Rating·시작판은 유지했다.

정확히 보관한 직전 AI 코드로 7수 패배가 재현됐다. Rating 700/800, 플레이어/AI 선공, 각각 32개 seed에서 플레이어의 네 번째 착수까지 모든 합법 선택을 검사했다. Rating 800 플레이어 선공은 이전 조기 패배 경로 166개→0개(현재 89,599개 플레이어 분기), AI 선공은 2개→0개였다. Rating 700은 각각 207→0, 24→0이었다. 현재 합계 329,051개 플레이어 분기·51,136회 AI 응수에서 같은 범위의 7/8수 패배가 없었고 최대 탐색 노드는 112, 측정된 최장 탐색은 약 2.8ms였다. 이것은 빈 시작판의 제한된 초반 경로 검사이며 사람 승률이나 중후반 무패 보장이 아니다.

관련 Unity **40/40**, 데이터 **46/46**, exporter `--check`, Generated/Resources 세 JSON 일치를 확인했다. 새 회귀 검사는 사진과 좌우 대칭 위협을 Rating 700/800/900 및 무작위 선택 100%에서 반복하고, 기본 승리/방어·노드/프레임 예산·첫 AI 응수·턴 타이머·음악/클릭·한영 1080/390/320 폭을 기존 검사로 확인했다. 1080×1920·390×844 화면에서 작은 배경이 `||`를 감싸고 전체 버튼 중앙/양옆 여백 클릭이 유지된다. Excel 변경은 16개 셀뿐이며 다른 값·서식·시트 기능은 보존했다.

근거: `Evidence/pause-square-20261015/unity-apply.log`, `Evidence/fork-defense-20261015/verification.json`, `unity-tests.xml`, `short-game-comparison.json`, `workbook-changes.json`, `source-freeze.json`, `screens/`. 재현 도구: `Tools/ShortGameAudit.cs`, `Evidence/fork-defense-20261015/CompareShortGames.ps1`. 아트·음원·씬 및 기존 빌드 17파일은 유지했다. 새 빌드·사람 플레이·기기 검증·커밋·푸시는 하지 않았다. 총괄은 현재 방어 프로필과 양쪽 위협 검사 정책을 공용 요약에 반영해야 한다.

---

# 승리 문구·일시정지 아이콘·기본 방어 보강 — 2026-10-15 (아래 방어 수치는 이전 상태)

일반 승리 결과를 기존 Excel `result_win` 키의 `승리!` / `You win!`로 연결했다. 최종 대회 우승 문구는 유지한다. 인게임 일시정지 아이콘은 60×60→120×120 설계 단위로 키우고 배경 이미지를 투명하게 저장했다. 버튼 전체의 터치 영역은 유지하며 누름/비활성 피드백은 아이콘에 적용한다. `RankUiBuilder.ApplyMatchPauseIcon`으로 Match 프리팹을 저장했고 전체 UI/픽셀 헤더 재생성에도 같은 구성을 반영한다. 공통 버튼·다른 화면은 변경하지 않았다.

사용자가 Rating 700에서도 방어 없이 세로 연결로 끝나고 앞 단계에서도 비슷한 일이 잦다고 알려, Excel의 전술 검사 생략 확률을 낮췄다. Rating 200은 65%→40%, Rating 700은 25%→5%이며 이후 단계도 점차 낮춘다. 탐색 깊이·예산·안전 후보 선택 비율·표시 Rating·시작판은 유지한다. 변경 셀은 Stages T5:T32의 28개이고 원본의 다른 데이터·서식·시트 기능을 보존했다. 현재 전체 표는 `../GameData/DIFFICULTY-CONTRACT.md`를 따른다.

검증: 관련 Unity **7/7**, 그중 실제 UI 레이캐스트 검증을 보강한 화면 검사 **1/1 재실행 통과**, 데이터 **46/46**, exporter `--check`와 Generated/Resources 세 JSON 일치. 한영 1080×1920·390×844·320×568에서 일시정지 버튼 중앙과 양옆 투명 여백의 클릭 대상을 검사하고 화면 캡처를 확인했다. 1080×1920에서는 전체 동작 버튼이 화면 안에 들어온다. Unity 첫 실행은 샌드박스의 라이선스 IPC에 막혀 종료했고 정상 사용자 권한의 재실행으로 저장·검증했다.

기존 고정 반응형 봇으로 전후 30단계×128판씩 총 **7,680판** 비교했다. 가로/세로 단일 위협의 256개 seed 검사에서 Rating 700의 방어는 각각 204→245회, Rating 200은 117→170회·118→171회로 증가했다. 2단계 봇 플레이어 승리는 113/128→91/128이었다. 전체 봇 대국의 AI 승수는 26단계 증가·3단계 동일·25단계에서 115→111로 소폭 감소했다. 표본 결과를 사람 승률이나 실측 Elo로 해석하지 않는다.

근거: `Evidence/turn-timer-rating-20261015/defense-ui/verification.json`, `unity-tests.xml`, `unity-pointer-tests.xml`, `defense-comparison.json`, `workbook-changes.json`, `source-freeze.json`, `screens/`. 기존 아트·음원·씬 및 빌드 17파일은 보존했다. 사람 플레이·새 Android/WebGL 빌드·기기 검증·커밋·푸시는 하지 않았다. 총괄은 최신 방어 프로필을 공용 요약에 반영해야 한다.

---

# 레이팅 문구 후속 수정 — 2026-10-15

사용자 요청에 따라 Excel Strings `ai_rating`의 한국어·영어를 모두 `Rating {0}`으로 통일했다. 로비 카드·선택 상세창·인게임은 동일 키를 사용하므로 함께 반영된다. 원본 C427/E427 두 셀만 수정하고 Generated/Resources 문자열을 동기화했다. 나머지 값·서식·시트 기능과 단계/광고 JSON은 유지했다.

검증: 데이터 검사 46/46, exporter `--check`, Generated/Resources 세 JSON 바이트 일치 확인. 문구만 변경해 Unity 검사는 재실행하지 않았다. 근거: `Evidence/rating-display-speech-20261015/rating-label/workbook-changes.json`, `verification.json`. 아래 Unity 검증은 이 문구 수정 이전의 기능 검증 기록이다.

---

# 레이팅 표시·판 상단 시간 바·타이핑음 보강 — 2026-10-15

로비의 30개 스테이지 카드, 선택 상세창 제목, 인게임 상대 패널에 Excel `ai_rating` 키로 AI 레이팅을 표시한다. 현재 선택한 단계의 `aiRating`을 사용하며 한국어/영어 전환 시 함께 갱신한다. AI 밸런스 수치는 바꾸지 않았다. 레이팅 표시만 Strings에 추가해 현재 423개이며, Excel 기존 값·서식·행/열 크기·고정창·유효성은 보존했다.

사용자가 지정한 판 상단의 나무 여백으로 제한 시간 표시를 옮겼다. 작은 남은 초를 가운데 위에 놓고, 그 아래에 남은 시간/30 비율의 가로 슬라이더를 저장했다. 슬라이더는 읽기 전용이며 5초 이하에서는 숫자와 채움이 붉게 변한다. 기존 상단의 시간 박스는 이동했고, 판의 선/돌/열 입력 위치와 1080×1920 하단 버튼 배치는 보존했다. 30초 자동 착수와 일시정지·확인창·힌트·백그라운드의 시간 정지 규칙은 유지한다.

타이핑음은 버튼 클릭을 0.18로 재생하던 방식에서 전용 `TypingKey.wav`를 0.75로 재생하는 방식으로 바꿨다. 35ms의 짧은 키 타격음을 프로젝트 도구로 생성했고 다음 40ms 글자 출력 전에 끝나게 했다. BGM과 실제 돌 녹음, 기존 버튼 클릭 파일은 변경하지 않았다. 글자별 효과음 ON/OFF, 대사 정지/재개, 화면 이동 취소도 유지한다. 파일: `Assets/StageGame/Audio/TypingKey.wav`, SHA-256 `948CA89DDAD1278D61F9D2897BB009B669EF8F5685D4D8C6478A9C42E5EF3DC0`.

검증: 최초 Unity 전체 **114개 중 113개 통과**. 새 카드 레이팅의 명시적 동적 텍스트 소유자를 기존 번역 검사 목록에 추가한 뒤 번역 검사 **9/9**를 통과했다. 최신 합산 커버리지는 **114/114**, 최종 실패·건너뜀 0이다. 데이터 검사 **46/46**, exporter `--check`, Generated/Resources 세 JSON 해시 일치도 통과했다. 한영 1080×1920·390×844·320×568의 로비/선택창/인게임 렌더링을 확인했고 긴 상대 이름과 레이팅이 겹치지 않는다. 자동 검사는 시간 바가 판 내부·격자 위에 머무르고 텍스트가 바 위에 있는 것도 확인한다.

Unity AudioListener 측정에서 타이핑 RMS는 0.00521→0.03358(약 6.44배), peak는 0.02888→0.26326이었다. 음악+타이핑 RMS 0.04307, 음악만 0.02755이며 음악/타이핑/착수 동시 출력 peak는 0.49294로 클리핑 범위 아래다. 이는 엔진 출력 측정이며 사람의 청취 평가를 의미하지 않는다. 기존 아트·음원·씬·빌드 17파일은 보존했고 새 빌드·브라우저/기기 검증·커밋·푸시는 수행하지 않았다.

근거: `Evidence/rating-display-speech-20261015/verification.json`, `typing-listener-output.json`, `test-coverage.json`, `source-freeze.json`. 저장 적용: `RankUiBuilder.ApplyRatingsAndTypingSound`. 도구: `Tools/SynthesizeGameAudio.py`, `Tools/VerifyRatingLabelWorkbook.py`, `Tools/VerifyRatingDisplaySpeech.py`. 총괄은 공용 요약에서 타이머 위치와 표시 레이팅을 갱신해야 한다.

---

# 30초 턴 제한·AI 레이팅 완화 — 2026-10-15 (타이머 위치는 위 후속 기록 적용)

사용자 선택에 따라 매 플레이어 턴에 30초를 주고, 시간이 끝나면 선택한 합법 열에 자동 착수한다. 선택이 없거나 만열이면 중앙부터 빈 열을 고른다. 기존 착수 함수로 연결해 돌 소리·승리 판정·AI 응수·되돌리기·진행 저장을 공유한다. 일시정지·확인창·설정/도움말·힌트/광고·앱 포커스 이탈/백그라운드에서는 시간이 멈춘다. 선택·언어 전환·확인 취소·힌트는 남은 시간을 유지하고, 새 턴·되돌리기·재시작은 30초로 시작한다. `RankUiBuilder.ApplyTurnTimer`가 Match와 RankAppRoot에 타이머 및 직렬화 참조를 저장한다. 1080×1920에서 기존 상단 여백과 전체 버튼 배치는 유지한다.

AI는 Excel의 **내부 레이팅 200~1600, 15구간**으로 나눴다. 같은 레이팅은 같은 깊이·예산·안전 후보 무작위 비율·전술 누락 확률을 사용한다. 1~2단계는 200으로 승리/방어를 놓치기도 하는 입문 설정이다. 처음 5번의 AI 응수에 깊이 4 이상을 강제하던 보정은 게임 경로에서 제거했다. 재도전마다 seed를 바꾸며 힌트에는 실수 확률을 적용하지 않는다. 사람 대전에서 측정한 Elo나 사용자 성적에 따라 바뀌는 보정은 아니다. 전체 표와 신규 JSON `aiRating`/`aiOversightPermille` 계약은 `../GameData/DIFFICULTY-CONTRACT.md`를 따른다.

검증: Unity 전체 110개 중 최초 108개 통과, 이전 AI의 필승 착수/고정 재시작을 전제한 2개 검사 조건을 새 정책에 맞게 수정한 뒤 관련 4개 재검사를 모두 통과했다. 최신 결과를 합친 커버리지는 **110/110**, 최종 실패·건너뜀 0이다. 자동 착수 경계/만열/승리, 턴 초기화, 모달/힌트/포커스 정지, 실시간 타이머, 음악 유지, 레이팅별 전술 정확도, 저장 프리팹과 한영 1080/390/320 폭을 검사했다. 실제 Unity Play 렌더링을 눈으로 확인했고 1080×1920에서 하단 버튼이 잘리지 않는다. 데이터 검사 **46/46**, exporter `--check`, 세 JSON 원본/Resources 해시 일치, 기존 Excel 서식·행/열 크기·고정창·유효성 보존 검사를 통과했다.

고정 반응형 봇으로 실제 이전 초반 보정까지 포함해 **15,360판**을 비교했으며 30개 단계 모두 플레이어 측 승리가 증가했다. 2단계는 전후 256판씩 **43승→216승**이다. 이 결과는 사람 승률이나 실측 Elo를 의미하지 않는다. 인간 플레이·새 Android/WebGL 빌드·기기/브라우저 검사는 하지 않았다. 기존 빌드 파일 17개와 모든 아트/오디오/씬 바이트를 보존했고 커밋·푸시는 하지 않았다.

근거: `Evidence/turn-timer-rating-20261015/verification.json`, `test-coverage.json`, `bot-comparison.json`, `source-freeze.json`. 재현 도구: `Tools/CompareRatedDifficulty.ps1`, `Tools/VerifyRatingWorkbook.py`, `Tools/VerifyTurnTimerRating.py`. 이전 결과와 최종 재검사 XML을 함께 보관한다. 총괄은 Overview/Docs Management의 게임 규칙·JSON 계약을 갱신해야 한다.

---

# 묵직한 실제 착수음·대사 타이핑 — 2026-10-15

실제 운자 바둑돌 녹음의 다섯 번째 착수(신카야 판)를 0.300초로 추출해 `GoStonePlaceHeavy.wav`로 저장하고 RankAppRoot의 moveSound에 연결했다. 이전 착수음과 최대 진폭은 동일한 약 0.65다. 저중음(100~1500Hz) 에너지 비중은 4.74%에서 39.24%, 에너지 중심 주파수는 3213Hz에서 2258Hz로 달라져 더 낮은 판 울림이 있는 샘플을 선택했다. 이는 신호 분석이며 사람 청취 평가가 아니다. 피치 변경·EQ·합성음을 추가하지 않았으며 이전 착수음도 보존했다. CC0 원본·편집 구간·해시는 `Archive/Audio/RecordedStone/heavy-provenance.json`, 비교 근거는 `Evidence/heavy-stone-comparison.json`에 있다.

대국 시작과 승리·패배 시 상대 대사는 기존 Excel 문자열을 한 글자씩 표시한다. 한 글자 간격은 최소 0.04초이며 공백을 제외한 글자마다 기존 UI 클릭음을 배율 0.18로 재생한다. 보이지 않는 뒷부분도 투명 텍스트로 배치해 줄바꿈과 자동 글자 크기가 출력 중 바뀌지 않는다. 한글 완성형, 결합 문자, 서로게이트 문자를 텍스트 요소 단위로 처리한다. 동일 대사를 새로고침하면 진행 위치를 유지하고 언어 변경·재시작·다음 단계·결과 대사에서는 새 문장으로 시작한다. 로비 이동·루트 비활성화 때 이전 대사를 취소한다.

일시정지·재시작/나가기 확인창·앱 백그라운드·포커스 이탈·광고 대기 중에는 타이핑 진행과 새 타이핑 소리를 멈춘다. 음악은 기존 정책대로 게임 내 확인창에서도 계속 재생한다. 효과음 OFF에서는 글자 표시가 계속되며 소리만 나지 않는다. 런타임 UI 트리 생성이나 데이터 원문 변경은 없다.

검증: Unity 전체 104개 중 최초 101개 통과·3개 실패 후, 배치 검사에서 대사 완료를 기다릴 때 앱 포커스를 명시하도록 검사 도우미를 보정했다. 실패 3개와 신규 대사 4개를 재실행해 7/7이 통과했다. 총 104개 검사 케이스 모두 마지막 실행 결과가 통과이며, 전체를 다시 한 번 실행한 결과로 표기하지 않는다. 최초 캡처의 일회성 NullReferenceException은 해당 검사 코드를 바꾸지 않은 재실행에서 해소됐다. 한·영 부분/완료 캡처 4개를 생성하고 대표 2개를 직접 확인했다. 결과: `Evidence/heavy-stone-speech-tests.xml`, `Evidence/heavy-stone-speech-retest.xml`, `Evidence/heavy-stone-speech-verification.json`. 기존 빌드 17개는 해시가 같다. 사람 청취·새 빌드·브라우저·실기기 확인과 커밋·푸시는 수행하지 않았다.

---

# 실제 바둑돌 녹음·버튼 효과음·확인창 음악 유지 — 2026-10-15

사용자가 합성 착수음을 거부해 billhails2의 [GoStoneSounds.ogg](https://freesound.org/people/billhails2/sounds/588317/) 실제 녹음(CC0 1.0)으로 교체했다. 운자 바둑돌을 나무 바둑판에 놓은 세 번째 착수를 공개 HQ MP3에서 추출했다. 0.280초 자르기, 일정한 음량 조정, 끝 10ms 페이드만 적용했으며 합성음·피치 변경·EQ·리버브는 추가하지 않았다. 최종 파일은 `Assets/StageGame/Audio/GoStonePlace.wav`이며 저장된 RankAppRoot의 moveSound가 직접 참조한다. 거부된 합성 착수음은 Assets 밖 Archive로 옮겼고 기존 Move.wav와 사용자 음원은 보존했다. 출처·CC0·가공 내역은 게임 내 라이선스 고지에도 포함했다.

로비↔대국뿐 아니라 일시정지·다시하기·로비 나가기 확인창에서도 같은 배경음악이 끊김 없이 이어진다. 게임 조작/AI 일시정지와 음악 일시정지를 분리했고 앱 백그라운드·포커스 이탈·광고 대기·사용자 음악 OFF 동작은 유지한다. 저장 프리팹의 모든 버튼에 직렬화된 참조로 짧은 클릭음을 연결했다. 효과음 OFF와 비활성 버튼에서는 소리가 나지 않으며, 설정을 다시 켤 때에도 클릭음은 한 번만 나온다.

음악·버튼 변경 후 전체 Unity 검사 100/100이 통과했다. 이는 실제 녹음 교체 전 결과다. 실제 녹음으로 교체한 최종 상태에서는 음악·효과음 관련 **11/11 통과, 실패·건너뜀 0**이다. 최종 AudioListener 출력으로 착수/버튼 재생, OFF 무음, 중복 클릭 방지, 확인창의 음악 시간 증가와 게임 조작 정지, 광고/앱 전환, 저장 참조를 확인했다. 준비 도구를 두 번 실행해 최종 WAV가 같은 바이트로 재생성됨도 확인했다. 기존 빌드 파일 17개는 그대로이며 사람 청취·새 빌드·브라우저·실기기 검증은 수행하지 않았다.

근거: `Evidence/stone-ui-sounds-verification.json`, `Evidence/recorded-stone-tests.xml`, `Evidence/recorded-stone-listener-output.json`. 원음·해시·정확한 편집 구간: `Archive/Audio/RecordedStone/provenance.json`. 재현: Unity `RecordedStoneAudioBuilder.DecodeSource` → `Tools/PrepareRecordedStone.py` → `RankUiBuilder.ApplySoundEffects`. 커밋·푸시는 하지 않았다.

---

# 로비·대국 배경음악 연속 재생 — 2026-10-15 (이전 기록, 확인창 정지는 위 기록에서 해제됨)

로비에서도 Easy Lemon이 재생되도록 변경했다. 로비↔대국 이동과 결과 화면에서 로비 복귀 시 같은 저장 AudioSource와 현재 재생 위치를 유지한다. 음악 음량 0.30과 착수음 1.00은 유지하고, OFF 설정·일시정지/확인창·앱 백그라운드/포커스 이탈·광고 대기 동작도 유지한다. 설정·도움말·스테이지 상세를 보는 동안 로비 음악이 이어진다.

Unity 음악 관련 검사 **8/8 통과, 실패·건너뜀 0**. 곡의 12초 지점부터 로비/대국을 3회 왕복하며 재생 위치가 초기화되지 않고 계속 증가함을 검사했고, 결과→확인→로비 복귀, 양쪽 화면의 OFF 유지와 로비에서의 백그라운드 복귀도 검증했다. 프리팹·음원·데이터·기존 빌드 파일은 변경하지 않았다. 사람 청취와 새 빌드/브라우저/실기기 검증은 하지 않았다. 근거: `Evidence/continuous-music-verification.json`, `Evidence/continuous-music-tests.xml`.

---

# Unity Play 음악·착수음 음량 보정 — 2026-10-15 (이전 기록, 로비 정지는 위 연속 재생으로 대체)

사용자가 Unity Play에서 음악·착수음이 잘 들리지 않는다고 알려 음량을 보정했다. 음악 AudioSource는 0.12→0.30, 착수음 PlayOneShot 배율은 0.30→1.00으로 변경했다. 원본 MP3/WAV와 사용자 ON/OFF 저장 설정은 그대로다. `RankUiBuilder.ApplyAudioLevels`가 현재 RankAppRoot 프리팹에 음량만 저장하며 전체 생성의 음악 기본값도 0.30으로 맞췄다.

검사 시점 Unity 배치에서 에디터 전체 음소거는 false, AudioListener 음량은 1이었다. 실제 사람이 듣던 순간의 OS 출력 장치·음량을 확인한 것은 아니다. 최종 AudioListener 출력 샘플을 측정한 음악 RMS는 0.01005→0.02540(약 2.53배), 착수음 peak는 0.03794→0.12645(약 3.33배)였다. 두 설정 OFF 시 출력은 0이었다. 관련 Unity 검사 **7/7 통과**이며 아래의 전체 95개 결과는 음량 보정 이전 기록이다. 음악은 기존 요청대로 대국 중에 재생하고 로비에서는 멈춘다.

음원·픽셀 자산·다른 프리팹·데이터와 기존 빌드 17개를 보존했다. 사람 청취, 웹/Android 빌드와 실기기 검증은 수행하지 않았다. 근거: `Evidence/audio-levels-20261015/verification.json`, 출력 측정: `Evidence/audio-levels-20261015/listener-output.json`.

---

# Easy Lemon 배경음악 연결 — 2026-10-15 (최초 연결 기록, 현재 음량은 위 보정 적용)

확정된 Easy Lemon 원본 MP3를 `Assets/StageGame/Audio/EasyLemon.mp3`로 바이트 변경 없이 복사했다. 저장된 RankAppRoot의 별도 AudioSource가 음량 0.12·2D·반복 재생을 담당한다. 대국에서 재생하고 로비에서는 정지한다. 일시정지/재시작·나가기 확인창, 앱 백그라운드·포커스 이탈, 보상형·전면 광고 대기 중에는 일시 중지하며 해제 시 같은 위치에서 재개한다. 광고 실패·미충전·취소·시간 초과 뒤에도 정상 복귀하고 사용자의 OFF 설정은 유지한다.

저장된 Settings 프리팹에 효과음과 독립적인 배경음악 버튼을 추가했다. `ProgressData.musicMuted`는 기존 v1 저장에서 기본 false이므로 기본 켜짐이며, OFF 설정은 재실행 후에도 복원된다. 표시 문구는 총괄이 추가·export한 Excel `settings_music_on`/`settings_music_off` 두 키를 사용한다. 이 작업에서는 GameData 원본이나 Resources/GameData JSON을 수정하지 않았다. 현재 문자열은 420개다.

`StreamingAssets/Notices/Easy-Lemon-Attribution.txt`에 제공된 저작자·원본 URL·CC BY 4.0·가공 안내를 추가하고 `LocalizationBuilder.ExportLegalNotices`로 기존 라이선스 화면용 텍스트를 재생성했다. 기존 고지 파일은 그대로 유지했다. `RankUiBuilder.ApplyBackgroundMusic`는 Settings/RankAppRoot만 좁게 저장하며 픽셀 그림과 대국 화면 프리팹은 기존 해시를 유지한다. 런타임 UI 생성은 없다.

Unity 에디터 전체 **95/95 통과, 실패·건너뜀 0**. 그중 신규 음악 검사 6개가 실제 AudioSource의 재생 상태·정지된 시간·재개 후 시간 증가, 설정/언어 복원, 광고 전후 동작, 저장 참조, 원본 MP3 해시와 전체 라이선스 포함을 검증했다. 설정 ON/OFF 한영 1080/390/320 및 고지 화면의 네이티브 캡처 13개를 생성하고 대표 화면을 직접 확인했다.

**사람 청취, 웹·실기기 검증, WebGL/Android 빌드, 커밋·푸시는 실행하지 않았다.** 기존 빌드 파일 17개는 동일하다. 실제 기기의 체감 음량·루프 경계와 브라우저 오디오 시작 조건은 별도 확인 대상이다. 근거: `Evidence/easy-lemon-verification.json`, `Evidence/easy-lemon-tests.xml`, `Evidence/easy-lemon-source-freeze.json`. 변경 목록과 인계: `BGM-IMPLEMENTATION.json`.

---

# 초반 난이도 완화 — 2026-10-15

2단계부터 너무 어렵다는 피드백을 반영했다. 2·3·5·6·7·8단계는 빈 판·플레이어 선공으로 바꿨고 유리한 연습 국면 4·9·10단계는 유지했다. 초반 10단계 AI는 깊이 1, 무작위 안전 수 선택 비율 85%→45%로 조정했다. 중반은 깊이 상승을 늦췄으며 25~30단계 대회에서 깊이 5~6으로 올라간다. 즉시 승리·방어 처리와 게임 규칙은 그대로다.

Excel 원본에서 exporter를 거쳐 Resources에 적용했다. 데이터 45/45, Unity 코어 25/25, exporter `--check` 및 3개 JSON 일치 검사가 통과했다. 실제 AI를 사용한 5,120판의 이전/이후 비교에서 초반 모든 단계의 단순 봇 승리가 유지 또는 증가했다. 사람 플레이 밸런스 확정이나 새 빌드 검증을 뜻하지 않는다. [수치 계약](../GameData/DIFFICULTY-CONTRACT.md), [검증 기록](Evidence/difficulty-20261015/verification.json)을 따른다.

---

# 대국 제목·일시정지·상대 정보 배치 수정 — 2026-10-15

사용자 화면 피드백을 반영했다. 대국 제목 배경을 숨기고 글자를 60→78로 키웠으며 중심을 4.5단위 위로 옮겼다. 제목과 일시정지 버튼은 높이 156, 같은 상하단 좌표로 맞췄다. 이름·계급·난이도 패널은 630×270→600×216으로 줄여 제목 행과 말풍선 사이에 각각 7.5단위 여백을 확보했다. 수치는 저장 프리팹의 1080 기준이며 화면에 맞춰 함께 축소된다. 기존 상단 제외 영역과 하단 조작은 유지한다.

`PixelArtBuilder.ApplyMatchHeader`로 저장 프리팹에 적용했다. Unity 레이아웃 검사 **4/4 통과** 및 320/390/1080 한영 캡처를 확인했다. 전체 82개 검사를 이번에 다시 실행한 것은 아니다. 그림 원본·런타임 게임 코드·GameData는 직전 통합 해시와 동일하다. 새 빌드·브라우저·실기기 검증은 수행하지 않았다.

근거: [검증 기록](Evidence/pixel-header-20261015/verification.json), [검사 XML](Evidence/pixel-header-20261015/layout-tests.xml), [1080 적용 화면](Evidence/pixel-header-20261015/screens/ko-1080-plain-match.png). 아래는 이전 통합 검증 기록이다.

---

# 픽셀 캐릭터·목재 게임판·24+6 서사 적용 — 2026-10-15

사용자가 이 기획 세션의 Client/GameData 수정과 Unity 적용·검증을 승인했다. 기존 저장 프리팹에 19명·57상태, 내무반 배경, 목재 보드, 픽셀 흑백 돌과 올리브/크림 버튼을 연결했다. 하사는 세 자세 모두 아래 방향 V인 cap-v2를 사용한다. 원본 이미지와 기존 C06 복사본은 보존했다. 런타임 UI 트리 조립은 추가하지 않았다.

부대 대국 24단계 뒤 병사 대회 64강~결승 6단계를 배치했다. 인물별 이름·보직·군종·대사는 Excel ko/en 418개 문자열과 characterId로 연결한다. 24승리에서 사단장이 대회를 알리고 30승리에서 우승을 표시한다. 기존 269문구·시작판·AI·별점·저장 의미를 유지한다.

최종 Unity 전체 **82/82**, 데이터 **45/45**, exporter `--check`와 세 JSON 일치 검사가 통과했다. 320/390/1080 한영 렌더, 44px 입력 영역과 1080 전체 화면 맞춤을 확인했다. 말풍선을 인물 오른쪽에 배치하고 차례 안내는 게임판 안의 글자로 유지했다. 320은 기존 스크롤을 사용한다. 새 WebGL/Android 빌드·브라우저·실기기 검증은 수행하지 않았다.

검증: [verification.json](Evidence/pixel-art-20261015/verification.json), [Unity XML](Evidence/pixel-art-20261015/unity-tests-verified.xml), [현재 소스 해시](Evidence/pixel-art-20261015/source-freeze.json), [하사 화면](Evidence/pixel-art-20261015/screens/ko-stage-6.png). 현재 제작 원본과 총괄 문서 인계는 [PIXEL-INTEGRATION.md](../Docs/Design/PIXEL-INTEGRATION.md)를 따른다. 아래는 이전 이력이다.

---

# 1080×1920 인게임 높이 수정 — 2026-10-15

사용자가 지적한 하단 잘림을 수정했다. 상단 216단위 여백을 유지하고 하단 48단위를 확보한 뒤, 저장된 대국 화면 전체를 남은 높이에 맞게 비례 조정한다. 일반 화면의 되돌리기·놓기·다시하기와 결과 화면의 다음 단계·다시하기·로비가 1080×1920에서 스크롤 없이 모두 보인다. 일반 안전 영역과 호스트 상하단 여백을 넣은 조건을 확인했다.

`Match.prefab`에 `RankMatchFitter`와 직렬화된 Scroll/보드 열 참조를 저장했다. 런타임에는 기존 콘텐츠의 크기와 위치만 조정하며 UI 트리를 만들지 않는다. `RankUiBuilder.ApplyMatchScreenFit`과 전체 생성에 같은 설정을 반영했다. 보드는 정사각형을 유지한다. 320×568처럼 작은 화면은 보드 열의 44 렌더 픽셀 입력 영역을 유지하기 위해 스크롤을 허용한다.

전체 Unity EditMode **78/78 통과, 실패·건너뜀 0**. 134개 네이티브 캡처를 생성했고, 1080 일반 대국/호스트 여백 대국/영문 승리 결과와 390 대국/320 하단 접근 캡처를 직접 확인했다. 로비 프리팹·GameData·기존 그림·게임 로직은 이전 해시와 동일하다. 기존 빌드 파일 17개도 동일하며 새 빌드·브라우저·실기기 검증은 하지 않았다. 캐릭터 적용은 계속 보류한다. 현재 근거: `Evidence/match-fit-1080-verification.json`, 소스 해시: `Evidence/match-fit-1080-source-freeze.json`.

---

# 인게임 상단 여백 — 2026-10-15 (이전 기록, 아래의 1080 스크롤 방식은 위 수정으로 대체)

사용자 요청에 따라 인게임에도 로비와 같은 216단위(1080 기준) 상단 여백을 적용했다. `Match.prefab`의 Scroll 상단 offset만 변경했으므로 제목·일시정지·AI 헤더·대사·보드·결과 화면이 함께 내려가고, 스크롤 중에도 상단 제외 영역은 유지된다. `RankUiBuilder`의 전체 생성과 별도 `ApplyMatchTopGap` 메뉴가 같은 값을 저장한다. 보드·버튼 크기와 런타임 게임 로직은 바꾸지 않았다.

확보한 여백만큼 표시 높이가 줄어들므로 1080×1920 호스트 화면 및 작은 화면에서 하단 조작 버튼은 스크롤로 접근한다. 한국어·영어 1080/390/320 네이티브 캡처와 기존 레이아웃 테스트 3/3이 통과했다. 로비/인게임 헤더 높이 일치, 정사각 보드, 44px 입력 영역, 스크롤 후 하단 버튼 접근, 결과·모달 화면을 확인했다. 이번에 전체 78개를 재실행한 것은 아니며 아래 진행 개편 결과는 이전 기록이다.

저장 프리팹 변경이 Scroll 상단 offset 하나뿐임을 이전 해시와 역대조했다. 로비·GameData·기존 그림·런타임 C#은 유지했다. 캐릭터 적용은 계속 중단 상태이며, 새 빌드·브라우저·실기기 실행은 하지 않았다. 근거와 현재 소스 해시: `Evidence/match-top-gap-verification.json`, `Evidence/match-top-gap-source-freeze.json`.

---

# 로비 여백·난이도·계급 진행 수정 — 2026-10-15 (이전 기록)

사용자 승인으로 Client와 GameData를 함께 수정했다. 로비의 4목 제목·도움말·설정과 목록 상단을 216단위(1080 기준) 내리고, 하단 대국 시작 영역과 목록 하단을 같은 216단위 올려 배너용 빈 공간을 확보했다. 실제 배너 SDK 연결은 포함하지 않는다.

Excel 원본의 30단계는 별 1→8을 0.5씩 올리며 각 난이도를 두 단계에 배정한다. AI 깊이·노드 예산은 감소하지 않고 무작위 선택 비율은 증가하지 않는다. 계급 표시 18종은 처음 6종에 1단계, 나머지 12종에 2단계씩 배정하며 29·30단계는 사단장이다. 30개 ID·시작판·starter·chapter·저장 진행은 보존했다. 한영 문자열은 269개이며 세 JSON은 Generated와 Client에서 바이트가 일치한다.

로비·상세·대국 헤더의 별은 8개의 저장된 이미지 쌍과 직렬화된 참조로 표현한다. 반 별은 왼쪽 50%를 채우고 빈 부분은 연하게 표시한다. 런타임 UI 생성은 없다. 긴 영문 계급명은 작은 화면에서 두 줄을 허용한다. 기존 모자 그림은 유지하며 별도 검토 중인 픽셀 캐릭터는 적용하지 않았다.

전체 Unity EditMode 78/78, 데이터 자동 검사 45/45와 Excel/Generated/Client 대조가 통과했다. 최종 화면 검증과 소스 해시는 `Evidence/progression-verification.json`을 따른다. 기존 WebGL/Android 산출물은 이전 버전이며 이번 변경을 새로 빌드하거나 브라우저·실기기에서 실행하지 않았다. 체감 난이도는 사람 플레이 검증 전이다. 총괄은 통합 시 `Docs/Management/CONTRACT.md`와 Overview의 기존 10계급·정수 별 계약을 18계급·float 반 별로 조정해야 한다.

---

# 로비 상단 위치 수정 — 2026-10-15 (이전 중간 기록)

사용자가 토스 닫기 버튼과의 겹침을 지적해 로비 제목·도움말·설정을 저장 프리팹에서 216단위(1080 기준) 내렸다. 제목/버튼 중심 Y는 -82.5에서 -298.5, 목록 상단 여백은 217.5에서 433.5로 바뀐다. 경고 문구도 함께 내리고 하단 버튼은 유지했다. 이전의 로비/게임 헤더 Y 일치 요구는 이번 요청으로 대체된다.

Unity Apply와 프리팹 검증은 성공했다. 1080·390 화면의 일반/호스트 여백 캡처에서 배치를 확인했다. 320 일반 캡처 검사에서 상단 여백 68px을 측정했고, 검사 기대값을 고정 추가 여백 64px에 맞게 바로잡았다. 재실행은 새로 열린 Client 에디터의 프로젝트 잠금 때문에 시작하지 못했으며 통과로 기록하지 않는다. 아래 76/76은 수정 전 v10 결과다. 근거: `Evidence/lobby-header-verification.json`, 캡처: `Evidence/lobby-header-20261015/`. 새 빌드·브라우저 실행은 하지 않았다.

---

# v10 최종 상태 — 2026-10-15, 소스·에디터 검증 완료

요청된 다섯 변경을 Client 안에서 완료했다. `내 차례`는 배경·테두리 없는 글자만 표시하고 기존 페이드와 입력 비차단 동작을 유지한다. AI 탐색은 턴 시작부터 약 1초의 응수 대기와 겹쳐 실행되며 일시정지·확인창 시간은 제외한다. 최고 예산 단계의 측정값은 1.001275초다. Undo·Restart·Leave 취소와 힌트의 강제 대기 없음도 검사했다.

Canvas 기준, 저장 프리팹 좌표·크기·폰트는 실제 1080×1920 단위다. 보드 1056×1056, 놓기 버튼 165×165, 제목 60단위이며 1080 원본 렌더에서 하단 세 버튼까지 안전 영역 안에 들어온다. 390 화면은 한 화면, 320 화면은 저장된 스크롤로 접근하고 44CSS 입력 영역을 유지한다. 로비와 게임의 제목·아이콘 Y 좌표 및 한영 제목의 실제 픽셀 표시를 확인했다. 첫 계급 모자는 새 `RecruitCapV10.png`를 사용하며 기존 모자 PNG 10개는 원본 해시 그대로다.

표시명은 Excel의 **4목 / Four in a Row**다. 237문구·30단계의 세 JSON이 Generated와 일치한다. 최종 Unity **76/76 통과, 실패·건너뜀 0**, 원본 해상도 캡처 **116개**, Assets **317개 동결 및 재적용 동일**이다. 근거는 `Evidence/rank-v10-tests.xml`, `Evidence/rank-v10-source-freeze.json`, `Evidence/rank-v10-verification.json`, `Evidence/rank-v10-20261015/`에 있다.

**v10 WebGL·Android 빌드, 브라우저 실행, 물리 기기 시험은 하지 않았다.** 기존 WebGL v9 / Android v8 파일 17개의 해시가 그대로이며 이 파일들은 v10 결과물이 아니다. 새 빌드·브라우저 실행은 사용자 승인 전까지 보류한다. 다른 역할을 재개하거나 외부 QA 결과를 기다리지 않았다. 아래는 이전 버전의 기록이다.

---

# v9 최종 상태 — 승인 지시 수신 뒤 추가 실행 보류

최종 Unity73/73,한영 캡처87개,311 Assets 동결·재적용 동일을 확보했다. 소스 manifest는 `Evidence/rank-v9-source-freeze.json`, 검사 `Evidence/rank-v9-tests.xml`, UI/모자/두줄 대사는 `Evidence/rank-v9-ui-audit.json`이다. QA-014의 실제 Unity 파괴 후 지연 결과 callback 회귀가 통과했고 독립 QA 판정은 별도로 이어진다.

**이번 웹 빌드를 승인 후 실행하라는 최신 지시는 이 Client 세션이 소유한 WebGL 프로세스의 성공 종료 결과를 이미 받은 뒤 도착했다.** Unity 로그 생성23:46:41 KST,BuildReport 갱신23:47:18,종료 로그23:47:20이다. 로그 생성 시각은 실행 근거이며 별도 프로세스 시작 시각을 측정한 값은 아니다. WaitForExit/exit0와 Succeeded를 확인했으므로 그 프로세스에서 중지할 작업은 남아 있지 않았다. 이후 새 WebGL/Android 빌드나 브라우저 검증을 시작하지 않았다. 총괄이 최종 Unity 화면/검사 결과를 보여주고 승인을 받는 절차를 기다린다.

완료된 개발 WebGL은 오류/경고0이며 `Builds/WebGL`의 data/wasm/loader3개가 v8 대비 바뀌었다. data SHA256 `04DEA2A7570BAD089438166A0E40943A6773F1ABD027DD9143B134DF0611C472`,wasm `402BF4A39C73B3A712489EDFA76F535AF54B07821C12892BA824D5CA904722B0`. index/framework/ui-text/host-layout는 이전과 같다.311동결 자산은 빌드 후 그대로,237문구/Stage1서사의3JSON은 Generated와 일치,8고지의 WebGL 원문 일치를 확인했다. 전체 산출물 해시·시각·제한은 `Evidence/rank-v9-verification.json`.

새 Android APK나 물리 기기 시험은 하지 않았다. 기존 v8 APK와 검증 기록을 보존한다. 로컬 개발 산출물의 존재가 승인·브라우저 검증·출시 완료를 뜻하지 않는다.

---

v9 QA 연결 필드: `confirm`은 기존 Pause, 새 `actionConfirm/actionCancel/actionAccept/actionQuestion/actionBody/actionAcceptLabel`이 파괴 행동 확인창이다. private `pendingAction`=None/Retry/Lobby, `QueueAction/CancelAction/AcceptAction`가 전이한다. `matchScroll/detailScroll`은 저장된 스크롤이며 `turnToast/turnToastLabel`, private `toastRoutine`, `ShowTurnToast/CancelToast`가 알림을 제어한다. 수명 가드는 private `destroyed/adGeneration/rewardGeneration/matchId`, 결과 경계 `ResultAdBoundary`, 힌트는 `RequestHint/CancelPendingReward`다. `OnDestroy`는 광고 취소·identity 무효화·모든 코루틴 정리를 수행한다. `SafeAreaFitter.widthFitTopAligned`는 새 RankRoot만 true, 기존 UI는 기존 경로를 유지한다.

# v9 진행 중 — 최신 사용자 수정 반영

v8 소스·프리팹·검사 자료는 `Archive/RankV8-20261014/source-and-evidence.zip`에 보존했다. 아래 v8 67/67·APK/WebGL 수치는 v9 통과를 뜻하지 않는다. 현재 v9는 별도 `Evidence/rank-v9-*`에서 검증 중이다. 사용자의 이번 검증 범위는 브라우저이며 실제 폰 시험과 새 Android APK는 진행하지 않는다.

- 공통 ActionConfirm 저장 프리팹: 게임/결과 Restart,결과 Lobby,Pause Leave를 대기 동작으로 전환한다. 취소는 같은 보드·선택·AI 코루틴을 유지하며 Pause에서 연 경우 Pause로 돌아간다. 확인은 pendingAction을 먼저 소비해1회만 실행한다. Next는 바로 진행한다.
- 폭 기준 크기+상단8CSS 정렬,짧은 화면은 저장된 Match/Detail ScrollRect로 접근한다. 일반 버튼/보드열44CSS이상.704×704외곽,dx=dy=720/7인7×6교차점,여유세로여백. UniformBoardLines는 실제 Canvas pixel비율로 모든 검정선의 얇은 변을 같은 정수 픽셀 두께로 맞춘다.
- AI 얼굴/계급/별 그룹은 전체 폭을 재서 중앙 정렬,10개자체도형 모자. 플레이어 얼굴/상시Turn숨김,원형Place와텍스트형Undo/Restart. 저장된 비차단 TurnToast는 새 플레이어 턴에만0.85초 표시하며 착수·모달·새판·결과·파괴에서취소한다.
- 최종237문구와Stage1전용intro/win/lose참조를동기화했다. 첫 지정 대사는줄임없이그대로,AI승리/플레이어패배역할유지. 확인문구는V9-UI-CONTRACT의ui_cancel/confirm_exit*/confirm_retry*/confirm_leave/retry키다.
- QA-014수정: destroyed플래그,matchId무효화,adGeneration,OnDestroy광고취소로파괴후전면광고callback의저장을차단한다. rewardGeneration은취소/새요청의오래된힌트callback이문구까지변경하지못하게한다.최종소스동결후독립QA재실행이필요하다.

# Rank UI 현재 구현 — 2026-10-14

새 활성 대상은 `Assets/StageGame/Scenes/Main.unity`의 `Prefabs/RankUI/RankAppRoot.prefab`와 `RankGameApp`이다. 이전 `GameApp` 프리팹 인스턴스는 비활성 보존한다. 이전 Tabletop 디자인은 사용자 시안 반려 상태이며 아래 과거 통과 기록을 새 UI 승인으로 해석하지 않는다.

- 한 수 진급 / Rank by Rank: 흰 교차점7×6판, 흑백 돌, 원형 얼굴, 중앙 제목,30개2열 저장 카드. 카드 top anchor,328×236 크기,710높이 세로 ScrollRect. 상단 도움말/설정 아이콘, 하단 멀티 준비중 버튼은 비활성이다.
- 상세는 저장된 `DetailBlocker` 위의 `Detail`이0.22초 우측 슬라이드한다. `RankDetailBlocker`가 열기/열림/닫기 전체에서 아래 로비를 차단한다. 닫은 뒤 목록 스크롤 유지, 좌우 단계 탐색, 실제 openingMoves 흑백 미리보기, 잠금 Start 차단을 검증했다.
- 선택은 `selectedColumn`/`movePreview`만 변경하고 Place가 실제1수를 둔다. 가득 찬 열은 선택/preview를 지우고 Place를 끈다. `undoHistory`는 플레이어 착수 직전 `ConnectFourBoard.Clone()` 목록이다. Undo는 AI 완료 후2수 또는 탐색 중1수를 복원하고 `searchGeneration`을 증가시켜 이전 코루틴을 취소한다. opening 이전으로 가지 않으며 같은 matchId·RewardGate.Granted·통계를 유지한다. AI seed는 기존 stage×997+MoveCount×31이다.
- 최초 판정은 기존 키 `stagegame.progress.v1` 부재다. fresh 진행 저장 후1단계 자동 시작, 새 판의 `select_column` 한줄 안내. 기존1단계/진행 저장은 로비이며 새 flag를 강제하지 않는다. 손상 저장은 recovery 보존 후 로비/오류 안내이며 새 이용자 자동판으로 처리하지 않는다. 실제 저장 실패 환경 주입 검사는 하지 않았다.
- 마지막 돌은400ms1.45→1배/34상승 복귀,500ms 관찰 후 결과/광고 경계. 확대 동안 최상위 sibling, 종료·취소 시 좌표/크기/순서 복원. 최상단 돌과 겹치는 Turn 문구는 종료 시작에 숨긴다. 승리는 Next+Retry+Lobby,30단계/패배/무승부는 Retry+Lobby. 동시2방향 승리선은 같은 승자의 연속 직선 중 유효한 하나를 선택한다.
- Hint는 일시정지 메뉴에 남아 있다. NoFill은 진행을 막지 않으며 완료1회만 소비한다. Undo는 사용한 힌트 권한을 복구하지 않고 추천/대기 callback만 지운다. 실제 광고 SDK/IAP/서버는 미연결이다.
- 최종228개 Excel 문구/30단계 복사본은 Generated3JSON과 일치한다. `GameData.ValidateRankData()`가 새 참조5개와 계급/별 구간을 거부 검증한다. 초기 안내, 결과/AI 대사, WebGL 제목은 이 문자열 표에서 나온다.

검증: `Evidence/rank-ui-tests.xml` **67/67 PASS,0실패/0skipped**. 처음64/66 실행의2하네스 오류는 `rank-ui-tests-initial.xml`에 별도 보존했다. 실제 Main 씬 PlayMode 진입을 포함하며 저장/광고/AI 코루틴·스냅샷·raycast·한영320/390 host영역 검사다. 전체67 검사 후 Editor Apply에 Web문구 export 호출만 추가했으며 runtime은 동일하다. 재적용281 Assets 바이트 동일(`rank-ui-idempotence.json`), 동결 manifest `rank-ui-source-freeze.json`.

최신 캡처는 `Evidence/rank-ui-20261014/`의 `*-lobby-end.png`를 포함한다. `*-lobby-middle.png`4개는 초기 잘못된 카드 anchor의 역사 자료로 최종 manifest에서 제외한다. 회색 Place/Next 초기 캡처는 Selectable0.1초 색 전환 전 촬영 문제였고, 갱신/색 전환을 기다린 최종 캡처에서는 검정 활성 스타일이다. 브라우저/실기기 판정은 이 Unity 검사와 별개다.

`ProjectSettings/PackageManagerSettings.asset`는 작업 시작 전부터 untracked였고 현재 Unity PackageManager UI 내부 클래스/기본 packages.unity.com registry/펼침 상태를 담는다. 생성 주체를 확정할 이력은 없으며 이번 기능이나 빌드 재현에 새로 요구된 파일이 아니다. 수정/삭제하지 않았고 Git 포함은 총괄이 별도 판단하도록 권한다. 패키지 재현 기준은 기존 manifest.json/packages-lock.json이다.

두 개발 빌드가 오류/경고0으로 완료됐다. APK 35568872bytes, SHA256 `282C16157C42421DA474724B8C750D24992506E9F408898D705EC06B7D6DEAC7`. WebGL wasm SHA256 `CA66C107F9F6F3283F13971B2FBEA8CAC6860B9048A74D856E4098D381603FBE`.281동결 자산은 두 빌드 후에도 그대로이고,8개 고지 파일이 APK/WebGL에서 원문과 바이트 일치한다.3개 데이터 JSON도 Generated와 일치한다. 전체 경로/해시/빌드 요약은 `RANK-UI-REQUEST.json` 및 `Evidence/rank-ui-verification.json`. 총괄 브라우저·독립 QA는 별도 진행 중이다. 릴리스 서명·스토어 제출·앱인토스 실제 SDK·물리 기기 검사는 미완료다.

---

Rank runtime 후속: 최상단 마지막 돌의 1.45배/34높이 연출이 Turn 상태 문구와 겹치지 않도록 Finish 시작 시 해당 상태 문구를 숨긴다. 양측 row5 캡처/일시정지 검사를 추가했다.

2026-10-14 Rank UI runtime 후속 변경: `RankGameApp.cs`는 새 경기의 짧은 `select_column` 안내를 표시하도록 변경했다. 숨겨진 화면의 직접 이벤트/중복 Next가 새 경기나 힌트를 시작하지 않도록 match/result 활성 가드를 추가했다. QA 후보 snapshot 이후 변경이므로 최종 snapshot에서 영향 검증이 필요하다. `RankUiBuilder.cs`는 카드 top anchor·3logical 격자·중앙 모달 제목을 수정했고, 로비 도움말/설정은 상단 아이콘·준비중 멀티는 하단 고정으로 이동 중이다.

# StageGame 클라이언트 구현 기록

최신 작업: **한 수 진급 / Rank by Rank 재설계 진행 중**, `RANK-UI-REQUEST.json` 기준. 이전 Tabletop의 자동 검사·빌드·브라우저 기능 검증은 보존하되 최종 디자인 수용/완료로 주장하지 않는다.

이전 Tabletop 기술 검증 기준선(사용자 시각 불수용으로 대체됨): **2026-10-14, 보드게임 그래픽 리모델링·마지막 승리 착수·가로 결과2버튼 구현 완료, 최종 Unity59/59 통과. 코드/프리팹/에셋 동결. Android/WebGL 개발 빌드 모두 오류/경고0이며 총괄의 최종 실제 포인터 브라우저 검사도 통과했다. 실기기는 연결0대로 미실행이며 아래 최신 절/GRAPHICS-REFRESH-REQUEST.json 기준.** Unity **6000.3.15f1**, 프로젝트 `C:\Github\StageGame\Client`.

## 보드게임 그래픽과 보드를 유지하는 결과 (계약 v7, 최신)

- 새 `Art/TabletopUI/`의21개 PNG와 `TabletopTheme.asset`을 저장 프리팹에서 참조한다. 크림 `#F5F2E9`, 청록 `#173E43`, 민트 `#67D6B0`, 산호 `#F18C78`, 금색 `#E4B65C`를 사용하며, 9-slice 패널·버튼·보드 테두리/짧은 그림자, 홈, 고리/마름모 말,8개 아이콘을 코드에서 직접 작성했다. 예전 자산은 보존했고 외부 그림 복사나 신규 고지 대상은 없다. 출처·파일 해시: `Evidence/tabletop-ui-assets.json`.
- 네 말 엠블럼과 큰 제목,5×2 단계 지도,선택 카드,큰 시작 CTA를 정돈했다. 노드는 선택 금색·현재 민트·완료 옅은 민트·잠금 회색과 play/check/lock 아이콘으로 구분한다. 돌마다 나/AI 글자를 반복하지 않고 대국 범례를 한 번 표시한다. 일시정지는 저장된 아이콘 버튼이며110단위 hit target을 유지한다.
- 승리를 결정한 플레이어/AI의 마지막 말은 같은 실제 coroutine에서1.45배·위34단위로 시작해400ms 동안 제자리/1배로 복귀한다. 이후500ms 동안 원색/고리·마름모를 보존한 금색 테두리 승리 줄을 보여준다. 그 뒤 광고 기회를 처리하며 완료·실패·NoFill·4초 타임아웃을 거쳐 결과 행동을 활성화한다. resolving 동안 열/힌트/일시정지/Escape를 차단하고 판 ID·결과 가드로 저장·광고 콜백 중복을 막는다. 화면 전환 시 coroutine을 취소하고 말 위치/배율을 복구한다.
- **모든 승리/패배/무승부/최종 완료는 보드를 남기고 하단에 ‘다시 도전’과 ‘로비로’ 두 버튼만 가로 배치한다.** 큰 결과 배경과 프레임은 투명/비 raycast로 저장하고 Next/Close는 비활성 보존한다. 위 상태 영역에 결과 제목, 보드 아래 별도72단위 영역에 기존 Excel 결과 문구를 표시한다. 진행 해금은 유지하며 로비 복귀 시 최고 해금 단계를 선택한다. 이전 v5의 ‘다음 단계 주요 버튼’은 사용자 최신 지시로 폐기했다.
- 720×960 기준과 SafeArea/64CSS 호스트 여유, 최소44CSS 열 입력을 유지한다. 결과 본문 공간을 위해 보드 높이는514, 행 간격82로 정돈했다. 결과 버튼은각308×110, 단계 노드는120×120이며 추가 게임 루프/문자열 없이137개 Excel 키를 그대로 사용한다. WebGL 로딩 배경도 새 팔레트로 맞췄다.
- 첫 전체59개 실행은55통과/4실패였으며 네 실패는 큰 로비 제목의 사각형 높이80 대비 preferredHeight84였다. 높이90으로 수정한 전체59/59가 통과했다. 이후 총괄 시각 검토의 확대 말 가림/결과 Pause 보임을 수정하고 최종 전체59/59를 다시 통과했다. 연출 말은 기존 sibling index를 저장해 맨 앞에 그린 뒤 완료/취소 때 원래 순서로 복구하며, 결과에서 Pause를 숨기고 새 대국에서 복원한다. 초기2개 preview 실패의 AdStatus 높이24도36으로 수정했다. 실패 실행을 통과로 기록하지 않는다.
- 프리팹/아트/씬60개 파일은 두 번 적용 전후 SHA-256이 전부 동일하다(`Evidence/graphics-refresh-idempotence.json`). 동결 소스 전체 Assets 해시는 `Evidence/graphics-refresh-source-freeze.json`이며 빌드 전후 일치를 확인한다. 원본3개 데이터 JSON과137키는 그대로다.
- `Evidence/graphics-refresh-20261014/animation-timeline.csv`와 `animation-{player,ai}-{0,1,2}.png`는 실제 저장 씬의 플레이 검사다. 중간/완료 프레임 및 timestamp, 크기/입력 잠금/광고 호출/버튼 활성 순서를 기록했다. 테스트용 합법 국면을 주입하며 사람의 전체 대국이나 실기기 증거는 아니다.
- 최신 자산 적용 순서는 필요한 기존 마이그레이션 뒤 **`TabletopUiBuilder.Apply`를 마지막으로** 실행한다. 이전 SoftUiBuilder/StageMapBuilder만 재실행하면 과거 색·구성으로 돌아가므로 최신 에셋 제작의 단독 진입점은 TabletopUiBuilder다. 이미 존재하는 PNG를 덮어쓰지 않는다.

- 최종 WebGL/Android 개발 빌드가 모두 오류/경고0으로 성공했다. APK 36461106bytes이며 파일별 해시와62개 캡처는 `Evidence/graphics-refresh-verification.json`에 있다.193개 동결 Assets는 두 빌드 후에도 동일하고,137개 키/3개 데이터 원본과 복사본이 일치한다. APK와WebGL의8개 고지 파일 모두 원문 바이트와 일치한다. 총괄의 현재 빌드 실제 포인터 브라우저 검사도 PASS이며 이전 그래픽 기준선의 PASS와 구분한다.

- 미저장 씬 보호를 위해 마지막으로 에디터 제작 도구에 Play/프리팹 편집/dirty scene 사전 중단 조건을 추가했다. 별도1/1 검사와 깨끗한 배치 Apply가 통과했으며, 전체59 런타임 검사와 합산해 한 번의60 검사로 표현하지 않는다. 최초 안전 검사의 additive 임시씬 생성 제한을 테스트 준비 방식에서 수정했다. 동결 이후 바뀐 파일은 Editor builder/test2개뿐이고 게임 코드·프리팹·그림·양 플랫폼 빌드는 동일하다. 구/신 동결 해시는 source-freeze 기록에 남겼다.

- 총괄의 최종 실제 WebGL 포인터 검사가 통과했다([보고서](../Docs/Management/Evidence/graphics-refresh-browser-20261014/README.md)).390 한국어에서1단계 실제9수 승리·같은 단계 재도전·로비의 최고 해금2단계 선택,2단계 실제10수 AI 세로 승리,잠금 진입 차단을 확인했다.320 영어에서 로비/대국/설정/규칙/라이선스 중간 스크롤·닫기,같은 열 반복 입력,NoFill 뒤 착수,재로드 언어·진행 복원을 확인한 뒤390 한국어로 복원했다. 결과는 보드와 원래 말 색·모양/금색 테두리를 보존하며 다음/닫기/일시정지 없이 두 가로 버튼을 표시한다. 조회 콘솔 경고/오류는0건이다.
- Client는 보고서·대표 캡처·빈 콘솔을 읽고 총괄 WebGL6개 파일과 자체7개 산출물,수정된 동결 Assets193개 해시의 일치를 재확인했다. 이 마감은 문서만 수정했고 재빌드/테스트/브라우저 조작을 하지 않았다.400ms 연속 프레임·양측 입력 잠금·광고 순서의 근거는 별도 Unity 타임라인이며 브라우저 정지 캡처로 전 프레임을 확인했다고 주장하지 않는다. 이번 브라우저는1단계 재승리와2단계 세로 패배이며 전체30단계 사람 플레이나 실제 신규 해금 획득 시험으로 확대하지 않는다. 라이선스 원문 전체 포함은8개 배포 파일 대조 근거이며,휴대폰은 연결0대로 미실행이다.

## 이전 완료 기준선 (v5/v6, 아래 빌드·브라우저 수치는 새 그래픽 검증 아님)

7×6 중력식 4목의 오프라인 AI 스테이지 초기 버전이다. **30개 스테이지, 한국어/영어 설정과 즉시 전환·저장·복원, 저장된 씬·프리팹, Unity 전체57/57 검사, Android 개발 APK, 일반 WebGL 개발 빌드**를 확보했다. IAP와 서버는 계획 상태다.

| 항목 | 확인 결과 | 증거 |
|---|---|---|
| Unity 컴파일·회귀 | 초기 전체57/57 + 승인된 문구 변경 후 관련11/11. 두 실행을 하나의68개 검사로 합산하지 않음 | `Evidence/stage-map-tests.xml`, `Evidence/loss-copy-tests.xml` |
| Android 개발 APK | 성공, BuildReport 오류/경고 각각 0, APK 39,792,595 bytes | `Builds/Android/StageGame-development.apk`, `Evidence/android-build-summary.txt`, `Logs/android-stage-map-final.log` |
| 일반 WebGL 개발 빌드 | 성공, BuildReport 오류/경고 각각 0 | `Builds/WebGL/index.html`, `Evidence/webgl-build-summary.txt`, `Logs/webgl-stage-map-final.log` |
| 최신 한영 PlayMode 화면 | 실제 저장 씬 실행·카메라 렌더 720×1280, 영어 전환과 재실행 후 해금/언어 복원 | `Evidence/ko-lobby.png`, `ko-settings.png`, `ko-match.png`, `en-lobby.png`, `en-settings.png`, `en-match.png`, `en-restored.png` |
| 추가 화면·UI 감사 | 영어 도움말·일시정지·결과, 버튼 영역 및 색 대비 계산 | `Evidence/en-help.png`, `en-confirm.png`, `en-result.png`, `ui-ux-audit.json` |
| 독립 코어 QA | 29묶음 통과/0실패. Unity API 대체 harness이며 실제 기기 검증 아님 | `Docs/QA/Evidence/qa-l10n-core-v1-20261014/run.log` |
| 최종 지도·문구 브라우저 | 총괄 실행 PASS. 최초 한영/챕터/잠금/실제 승패 흐름과 최종 한국어 세로 패배 문구 재시험을 구분 | `Evidence/stage-map-verification.json`; 총괄 `Docs/Management/Evidence/reference-ux-browser-20261014/README.md` |
| 이전 호스트 전용 브라우저 | 총괄 실행 PASS. 본문24 기능 검사와 최종 본문30 스크롤/닫기/resize 재시험을 분리 기록 | `Evidence/host-ui-verification.json`; 총괄 `Docs/Management/Evidence/host-ui-browser-20261014/README.md` |
| 이전 독립 브라우저 QA-008 | 새로고침 없이 390→320→390, 설정 크기·닫기 입력·언어 유지 통과. error/warn 없음 | `Evidence/layout-reflow-check.json`; 총괄 `Docs/Management/Evidence/localization-browser-20261014/final-*` |
| 이전 빌드 브라우저 기준선 | 모바일 폭 잘림 수정 및 9수 승리→2/30 해금→새로고침 복원 통과. 한영 변경 전 결과 | `Evidence/responsive-check.json`, `webgl-browser-match.jpg`, `verification.json`의 `rootIndependentRetest` |
| 최종 데이터 동기화 | 3개 JSON이 GameData/Generated와 SHA-256 일치 | `Evidence/verification.json` |
| 배포 고지 포함 | APK ZIP의 Notices 항목 및 WebGL StreamingAssets 확인 | `Evidence/apk-notices.json`, `Assets/StreamingAssets/Notices/` |

**실기기 Android 설치·플레이, 앱인토스 SDK/.ait 패키징, 실제 광고 송출, 릴리스 서명, 스토어 제출/승인은 미검증·미완료다.** 현재 산출물은 개발 빌드이며 출시 승인이나 수익 연동 완료를 뜻하지 않는다.

## 실행

- Unity Hub에서 `Client`를 Unity 6000.3.15f1로 열고 `Assets/StageGame/Scenes/Main.unity`를 Play한다.
- Android APK는 ARM64, 패키지 `com.stagegame.prototype`, 버전 `0.1.0`/1, min SDK 25, target SDK 36으로 생성했다. 앱 식별자와 서명은 출시 전에 확정해야 한다.
- WebGL은 `file://` 대신 HTTP 서버로 실행한다. 예: `python -m http.server 8765 --bind 127.0.0.1 --directory C:\Github\StageGame\Client\Builds\WebGL` 후 `http://127.0.0.1:8765/` 접속.
- `StageGamePortrait` 템플릿의 캔버스와 배경은 viewport 전체를 채운다. Unity 내부 720×960 콘텐츠만 OS/호스트 안전영역 안에 맞춘다. 모바일 브라우저 실기기 성능은 측정하지 않았다.

## 한국어·영어 구현과 이전 UI 검사

- `LOCALIZATION-REQUEST.json`으로 필요한 70개 UI 문자열을 먼저 공유했고, 총괄의 중계와 `GameData/LOCALIZATION-CONTRACT.md`로 계약을 확정했다. 기존 key/ko 58개를 보존하고 새 UI 키 65개를 추가한 **123개 ko/en**, 기존 `schemaVersion:1`을 사용한다. stages/ads는 변경하지 않았다. Excel 생성물을 Sync 도구로 복사했으며 JSON을 수동 편집하지 않았다.
- 설정의 저장된 한국어/English 버튼으로 화면 문구를 즉시 갱신한다. 선택 표식은 ✓이며 언어명은 양쪽 언어에서 모국어로 유지한다. `ProgressData.language`를 저장 v1에 추가했고, 구형 저장·누락·알 수 없는 값은 해금/소리 설정을 보존하며 ko로 복귀한다. 영어 선택 후 씬 재실행에서 영어와 2/30 해금을 함께 복원하는 PlayMode 회귀를 통과했다.
- 정적 문구는 AppRoot의 직렬화된 `LocalizedLabel[]`, 동적 제목·턴·수·결과·힌트·오류는 동일 `StringTable` 키를 사용한다. 저장된 모든 Text는 키 바인딩 또는 명시적 동적 필드에 포함되는지 검사한다. 숫자 형식은 코드에서 준비하고 문자열은 `{0}`, `{1}`만 사용한다. 법적 원문·개발 로그·문자열 데이터 자체를 읽지 못했을 때 최소 복구 안내만 예외다.
- WebGL 로딩/오류/캔버스 설명도 빌드 시 Excel 출력에서 `ui-text.js`로 생성한다. 언어 브리지는 실제 선택을 다음 로딩 화면에도 전달하며 게임 내 설정의 기준은 PlayerPrefs다.
- 설정의 SDK/수익/Made with Unity/폰트 설명을 제거하고 라이선스 전문 메뉴를 유지했다. NoFillAds에서는 로비·결과 배너와 결과의 광고 상태 문구를 숨긴다. 실패·취소·저장 안내는 짧게 표시한다. 민트/산호 및 돌의 나/You·AI 문자 표식을 유지했다.
- 기존 프리팹을 `LoadPrefabContents`로 제한 수정했다. 언어 버튼도 공통 PrimaryButton의 저장된 중첩 인스턴스이며 공통 CloseButton을 유지한다. 런타임에서 UI 오브젝트를 만들지 않는다. 호스트 개선 이전 기준 100×100 및 320px 보드 열 39.05px의 제약은 아래 새 구성에서 수정했다. 현재 일반 버튼/닫기는 110단위, 보드 열은 720/7단위이며 최종 CSS 크기는 호스트 절의 지원 범위에 따른다.
- 긴 영어 제목 검사에서 `Review connections and defense`의 잘림을 발견했다. 로비 제목 폭 580/글자 32, 대국 제목 글자 28로 조정한 뒤 두 언어의 30개 제목을 모두 검사했다. 설정·로비·결과·일시정지 버튼 겹침과 영어 본문 높이 검사를 통과했다.
- 불투명 색상 기준 대비는 본문/패널 13.20:1, 보조문구/패널 6.18:1, 짙은 글자/민트 12.01:1, 흰 글자/보조버튼 10.09:1이다. 비활성/장식 요소는 제외한 계산이며 전체 접근성 인증이나 사람 사용성 시험이 아니다.

## 챕터 지도·그림 도움말·완료 기록 (계약 v5)

- 로비는3개 챕터를 기존 이전/다음 버튼으로 넘기며 한 페이지에5열×2행의10개 노드만 표시한다. 공통 `StageNode.prefab`의 저장된 중첩 인스턴스이며 런타임 생성이나 스크롤은 없다. 입문/전술/도전은 기획 분류이며 난이도의 실측 보장이 아니다.
- 각 노드는120×120 논리 단위로 최소 지원 조건320×568·상단59/하단34·추가64에서51.375 CSS px다. 상태 라벨은28단위(약12px)이며 완료 체크+문구, 현재/잠금 문구, 선택 숫자의 괄호를 함께 쓴다. 잠긴 노드를 선택해 이름·해금 조건을 볼 수 있지만 시작 버튼과 `StartStage`가 진행을 막는다. 선택한 완료 단계는 큰 ‘다시 도전’ CTA를 제공하고 전체 완료 수를 따로 표시한다.
- `ProgressData.highestClearedStage`를 기존 `saveVersion:1`에 추가했다. 구형 저장은 `unlockedStage-1`까지만 완료를 추론하며,30단계 해금만으로 마지막 완료를 주장하지 않는다. 실제30단계 승리 후30을 저장하고 재도전/패배/중복 결과로 이를 낮추거나 중복 집계하지 않는다. 기존 처리한 판 ID·대국/광고 카운터·언어·소리 설정과 복구 경로를 유지한다. native Unity JSON/PlayerPrefs 검사와 실제 GameApp 결과→로비→재도전 흐름을 통과했다.
- 도움말은 저장된3×4 중력 예시(밑의 산호 돌 위로 민트 돌이 놓임)와 가로·세로·대각선 방향별 정확히4개 돌의 추상 패턴을 사용한다. 방향 패턴에는 빈 격자를 넣지 않아 떠 있는 돌이나5목 판으로 오해하지 않게 했다. 모든 캡션은 Excel 키를 사용하고 두 언어의 높이/경계를 검사했다.
- 결과는 일반 승리 때 다음 단계, 패배·무승부·최종 단계 승리 때 재도전을 민트로 강조한다. 로비는 항상 보조 색이다. 재도전 위치는 저장된 `PrimarySlot`/`SecondarySlot` 참조로 바뀌며 UI 트리를 다시 만들지 않는다. 테스트의 승/패/무승부 종료 국면 주입은 실제 사람 플레이 증거와 구분한다.
- `StageMapBuilder.Apply`는 기존 프리팹을 제한 수정하고 누락된 지도/도형만 생성한다. 예전 장식과 도움말 본문은 비활성화해 보존했다. 새 그래픽은 기존 원본 SoftUI 스프라이트와 직접 작성한 Unity 도형이며 참고 이미지의 질감·캐릭터·테두리 자산을 복사하지 않았다. 두 번 적용한5개 프리팹의 SHA-256이 동일하다(`Evidence/stage-map-idempotence.json`). 재적용 순서는 필요할 때 기존 LocalizationBuilder→SoftUiBuilder→StageMapBuilder다.
- 새14개 키를 데이터 담당의 Excel 생성물에서 동기화해137개 ko/en 키를 사용한다. stages/ads는 변경하지 않았으며 JSON3개의 원본/복사본 해시가 일치한다. 원본 Excel이나 외부 담당 폴더는 수정하지 않았다.
- 문구 수정 전 전체 EditMode57/57 통과, `Evidence/stage-map-tests.xml`. 저장 프리팹·모든 노드 참조·그림의 돌 개수/방향·한영30개 제목·잠금 진입 차단·최종 완료와 재도전·승패 강조·호스트 최소44px/경계·390→320→390·라이선스 전문 스크롤을 포함한다. `Evidence/host-*.png`28개와 `Evidence/map-*.png`12개는 현재 저장 씬의 Unity 카메라 렌더다. 이전 호스트 기록의 같은 이미지 경로는 이번 렌더로 갱신됐으므로 현재 해시는 `Evidence/stage-map-verification.json`을 기준으로 한다.
- 두 개발 빌드는 오류/경고0이며 APK 고지8개가 원문 해시와 일치한다. 최신 소스·프리팹·40개 캡처·산출물 해시는 `Evidence/stage-map-verification.json`, 전체 상태는 `STAGE-MAP-REQUEST.json`이다. 이전 본문30 빌드의 총괄 브라우저 결과는 별도 이전 범위로 남기며 새 지도 브라우저 결과로 사용하지 않는다. 총괄의 지도 기능시험과 승인된 패배 문구 수정 후 최종 안내 재시험을 모두 통과했다. 두 빌드의 검사 범위를 나누어 기록한다.
- 총괄의 실제 브라우저 입력(2026-10-14, 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`)에서 한영·390↔320·챕터1→2→3→1·잠금 살펴보기와 시작 차단·그림 도움말·언어/진행 재로드가 통과했다. 이미 완료한1단계를9수로 재승리한 뒤 다음2단계로 진입했고,2단계10수 패배 후 강조된 재도전 버튼으로 초기4수 배치가 복원됐다. 새 완료 증가나30단계 완주를 이 브라우저에서 검사한 것으로 기록하지 않는다. 쉼·라이선스 진입/스크롤/닫기와 콘솔 오류/경고0도 보고됐으며 고지 끝 문단은 이번에 반복하지 않았다. 출처: [총괄 브라우저 기록](../Docs/Management/Evidence/reference-ux-browser-20261014/README.md). 실제 Toss SDK/휴대폰/사람 사용성 시험과 구분한다.
- 패배 팝업이 판을 가리므로 총괄 승인에 따라 `result_loss_horizontal/vertical/diagonal`의 둘째 문장만 ko/en에서 제거했다. Excel 생성 JSON을 동기화했고137개 중3개만 변경, 나머지134개와 stages/ads·코드·프리팹은 동일하다. 이전 결과샷 `Docs/Management/Evidence/reference-ux-browser-20261014/en-loss-retry-390.jpg`는 문구 수정 전 증거로 보존한다. `Evidence/loss-copy-change.json`에 전후 문구와 불변 소스 해시를 기록했다.
- 최종 문자열로 별도 Unity11/11을 통과했다. 문자열 조회/포맷/저장 언어, 모든 저장 텍스트 바인딩·데이터/프리팹 유효성, 한영 승패/무승부·최종 완료 결과 렌더를 포함한다(`Evidence/loss-copy-tests.xml`). 초기57/57과 합산하지 않고 변경 없는 기존 기능의 증거를 유지한다. 320px 영어 결과 캡처에서 AI의 연결 방향 한 문장만 표시됨을 확인했다.
- 총괄의 최종 WebGL 재로드 후 한국어 로비·그림 도움말,2단계10수 세로 패배를 다시 확인했다. 안내는 ‘AI가 세로로 네 개를 이었어요.’ 한 문장이며 재도전이 주요 행동으로 표시되고 로비 복귀도 정상이다. 최종 관찰 콘솔은 `[]`다. [최종 패배](../Docs/Management/Evidence/reference-ux-browser-20261014/final-ko-loss-390.jpg) · [최종 로비](../Docs/Management/Evidence/reference-ux-browser-20261014/final-ko-map-390.jpg) · [최종 도움말](../Docs/Management/Evidence/reference-ux-browser-20261014/final-ko-help-390.jpg). 최종 브라우저 범위는 한국어 세로 패배 하나이며 가로/대각선·영어 패배까지 모두 재현했다고 기록하지 않는다. Client는 총괄 보고·캡처·빈 콘솔을 읽고 현재7개 산출물과 Generated/Client 문자열 `57E392...`의 해시 일치를 확인했다. 추가 코드 변경·브라우저 실행·빌드·테스트 없이 문서만 마감했고, 실제 Toss SDK·실기기는 미검증으로 유지한다.
## 호스트 영역·자체 그래픽·라이선스 메뉴 (이전 구현 및 유지 계약)

- 계약 v4에 따라 WebGL 콘텐츠 상단은 **OS 상단 inset + 64 CSS px** 아래에 둔다. 64는 StageGame 설계 여유이며 공식 Toss 버튼 높이나 실기기 측정값이 아니다. 총괄이 확인한 [공식 Safe Area 문서](https://developers-apps-in-toss.toss.im/documentation/common/screen/safe-area), 확인일 2026-10-14. 실제 Toss SDK는 아직 연결하지 않았다.
- HTML 캔버스/배경은 전체 viewport를 유지하고 CSS에서 inset을 빼지 않는다. `HostLayout.ContentArea`는 각 변의 Unity OS safe inset과 브리지 CSS inset 중 큰 값을 취한 후 상단 64를 한 번 추가한다. CSS→Unity 변환은 각 축의 실제 Screen/viewport 비율을 사용한다. 선택적 실제 호스트 컨트롤 바닥 좌표가 있으면 `max(OS top+64, controlsBottom+8)`을 쓴다. Android 단독 앱에는 추가64를 적용하지 않는다.
- 입력 경계는 `window.stageGameHostLayout.setInsets({left,top,right,bottom}, actualControlsBottom=-1)`이다. 단위는 CSS px, OS inset은 각 viewport 가장자리로부터 거리이며 controlsBottom은 viewport 상단부터의 거리다. future adapter가 `SafeAreaInsets.get()`과 `subscribe()` 결과를 전달한다. OS inset 대신 이미 예약한 전체 높이를 넣거나 외부 wrapper가 viewport를 다시 줄이면 안 된다. `host-layout.js`가 CSS env를 기본값으로 사용하며 명시적 값은 이를 대체한다.
- 모의 막대는 `?hostPreview=1&hostTop=59&hostBottom=34`에서만 표시한다. 일반 WebGL은 막대가 숨겨져 있어도64를 예약한다. 모의36px 버튼은 실제 Toss UI의 치수나 작동을 재현한 것이 아니며 포인터를 가로채지 않는다. 실제 SDK/호스트 확인과 분리한다.
- 논리 콘텐츠를 720×960으로 줄이고 일반 버튼·닫기를 최소110×110, 보드7열을720/7 폭으로 저장했다. **320×568 / OS top59·bottom34·left0·right0 / 추가64**의 가용 영역은320×411이며 일반 버튼47.094px, 열 입력44.036px다. 390×844의 동일 inset에서는 각각59.583px/55.714px다. 모든 모달·화면의 활성 그래픽 상하 경계와 각 버튼의44px를 렌더 좌표로 검사했다. 사용 가능한 폭308px 미만 또는 높이410.667px 미만이면 보드44px를 충족하지 못할 수 있어 검증 지원 범위 밖이다. 가로/실기기 사용성은 미검증이다.
- `SoftUiBuilder.Apply`는 기존 저장 프리팹의 배치·스프라이트 참조만 수정하며 런타임 UI 트리를 생성하지 않는다. `Art/SoftUI/RoundRect96.png`(9-slice border26)와 `SoftDisc128.png`는 코드로 그린 원본 도형이다. 둥근 패널·버튼·보드와 민트/산호 돌, 기존 문자 표식을 통일했다. 이전 `Disc.png`를 보존하고 기존 새 자산이 있으면 픽셀을 덮어쓰지 않는다. 생성 코드·파일 해시·참조·출처는 `Evidence/soft-ui-assets.json`에 기록했다.
- 라이선스 메뉴는 설정 하단의26단위 보조 텍스트이며 배경만 투명한535×110 Button이다. 클릭 영역은 줄이지 않았다. 총괄 브라우저 가독성 피드백에 따라 본문만24→30단위(최소 지원 화면에서12.844 CSS px)로 키웠다. 기존8개 고지 파일과 묶음 전문을 유지하고 ScrollRect 마지막 문단까지 렌더했다. `Evidence/host-en-320-notices-bottom.png`는 렌더 타깃 변경 후 스크롤 끝을 다시 맞춘 최종 이미지다.
- 새 Unity 전체 회귀 **49/49**에서 한국어 제목의 높이 부족을 수정한 뒤 통과했다. QA-009의 NaN/Infinity·JS float32 초과 입력은0으로 정규화하고64 예약은 유지한다. 이 마지막 변경 뒤 관련 Unity 검사 **8/8**을 실행했다. 두 실행의 **고유 테스트50개**가 통과했으며 단일50개 실행으로 기록하지 않는다. XML은 `Evidence/unity-tests.xml`, `host-ui-boundary-tests.xml`; 로그는 `Logs/host-ui-tests.log`, `host-ui-boundary-tests.log`다. 최종 본문30 변경 후 호스트 렌더·스크롤 검사1/1을 추가 실행해28개 캡처를 갱신했다(`Evidence/host-ui-final-render.xml`, `Logs/host-ui-final-render.log`). 변경 없는 코어·데이터는 마지막 경계 수정 후 반복하지 않았다.
- `Evidence/host-{ko|en}-{320|390}-{lobby|settings|match|help|confirm|result|notices-bottom}.png` 28개는 저장 씬을 실행한 실제 Unity 카메라 렌더다. 390→320→390 재배치, 두 언어의30개 제목, 버튼 겹침·크기·상단 예약·라이선스 스크롤·중첩 닫기 프리팹을 검사했다. 브라우저 검증은 총괄이 실행했다. 본문24 빌드의 기능 검사는 전체 캔버스/상단123 영역, 390→320→390 설정 크기·영어 유지, 라벨 밖(52,440) 라이선스 클릭, 보드 가장자리 입력·AI 응수, 쉼·도움말을 통과했다. 본문30 최종 빌드를 다시 로드한 뒤320×568에서 진입·본문 확대·55회 드래그 후 마지막 uGUI 문단·닫기 및390 복귀를 통과했다. 관찰 콘솔은 `[]`이고 재로드 후 한국어·2/30 진행 유지, 기본 실행의 모의 표시 숨김도 보고됐다. 첫 기능 검사와 최종 본문 검사는 서로 다른 빌드의 범위이며 새 스테이지 승리를 재검증했다고 기록하지 않는다.
- 출처는 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의2026-10-14 보고와 [최종 브라우저 기록](../Docs/Management/Evidence/host-ui-browser-20261014/README.md), `final-artifacts.json`, `final-*` 캡처다. Client는 보고/최종 이미지/빈 콘솔을 읽고5개 최종 산출물의 SHA-256·크기가 현재 파일 및 Client 명세와 모두 일치함을 확인했다. 별도 브라우저를 실행하지 않았다. Unity의44px 좌표 검사와 총괄의 실제 포인터 검사를 함께 사용하며 실제 Toss SDK·휴대폰 시험으로 확대하지 않는다. 이번 마감은 문서만 갱신했고 코드·빌드·기존 테스트를 반복하지 않았다.

## 구조·자료구조 감사

- 42칸 `int[]`와 7열 높이 배열은 작은 고정 보드에 적합하며 착수 위치를 바로 구한다. 별도 그래프나 범용 보드 계층을 추가하지 않았다.
- 현재137개 문자열은 키 기반 Dictionary를 한 번 구성하고 언어 변경 시 기존 Text를 갱신한다. 화면 수가 적어 기존 GameApp의 직렬화 참조와 명시적 갱신으로 충분하며 전역 이벤트 버스나 MVVM 프레임워크를 추가하지 않았다.
- `IAdService`는 플랫폼 경계를 나누고 RewardGate의 요청 토큰은 늦은/중복 콜백을 거부한다. Status와 저장 경고는 UI 문자열 키로 변환했다. 실 SDK가 연결됐다는 뜻은 아니다.
- 결과 중복 거부에는 저장 List와 조회 HashSet을 사용한다. 완료 판 ID가 계속 쌓이므로 장기 저장 크기/압축 정책은 후속 검토 대상이다.
- AI는 기존 iterative deepening/alpha-beta 및 노드·시간 제한을 유지한다. 분기마다 Board.Clone, Score 객체, iterator가 할당되므로 GC 부담 가능성이 있다. 실기기 할당량·프레임 p95를 측정하기 전 최적화 완료로 주장하지 않으며 이번에 풀링·undo 보드 등으로 재작성하지 않았다.

## 이전 QA-008 크기 변경 결함과 수정 이력

- 총괄의 독립 브라우저 검사에서 390→320×568 전환 직후 설정 프레임이 약 232px, 닫기가 약 36px로 축소되고, 새로고침하면 약 284px/44px로 복구되는 결함을 확인했다. Canvas DOM은 이미 319.5×568이었으므로 HTML 폭 제한과 별개의 Unity 내부 배율 문제였다.
- 설치된 uGUI의 CanvasScaler는 `preWillRenderCanvases`에서 배율을 갱신한다. 기존 SafeAreaFitter.Update가 그 이전의 rect를 읽고 Screen 크기만 캐시해 이후 실제 rect 변경을 놓쳤다. 저장 씬에서 Screen은 고정한 채 Canvas 배율을 바꾸는 회귀로 수정 전 **기대 0.375 / 실제 1.0** 실패를 재현했다 (`Evidence/layout-before.xml`).
- SafeAreaFitter를 `Canvas.willRenderCanvases`에서 실행하고 Screen/safe-area와 별도로 실제 가용 rect 크기도 추적한다. OnDisable에서 이벤트를 해제하며, 가용 크기가 같으면 배율을 다시 쓰지 않는다. 프리팹·게임·언어·광고 로직은 변경하지 않았다.
- 수정 후 같은 검사에서 내용 배율이 **1→0.375→1**로 정상 복귀했고 전체 Unity 회귀 **42/42**를 통과했다 (`Logs/layout-final-tests.log`, `Evidence/unity-tests.xml`, `Evidence/layout-reflow-check.json`). Android와 WebGL도 최종 재빌드에서 오류/경고 각각 0으로 성공했다.
- 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의 2026-10-14 최종 보고: `webgl-layout-final`을 로드한 뒤 390px 설정 프레임 약 346px/닫기 54px→새로고침 없이 320px에서 프레임 284px/닫기 44px→English 선택→390px 복귀에서 동일한 크기와 영어 유지→X로 로비 복귀를 확인했다. error/warn은 빈 배열이며 한국어로 복귀하고 뷰포트 설정을 복원했다. **QA-008 최종 독립 재시험 통과**로 기록한다. 직접 증거는 `Docs/Management/Evidence/localization-browser-20261014/final-ko-settings-390.jpg`, `final-en-settings-320.jpg`, `final-en-settings-390-resized.jpg`, `final-console.json`이다. 이 항목은 총괄의 실제 브라우저 보고이며 클라이언트의 별도 실행이나 실제 모바일 기기 검증으로 표기하지 않는다.

## 게임과 코드

- `ConnectFourBoard`: 7열×6행, 행 0은 바닥. 플레이어 1/AI -1. 가로·세로·양 대각선 4개 이상 승리, 만판 무승부, 만열/범위 밖/종료 후 착수 거부.
- `AiSearch`: 즉승과 피할 수 있는 즉패 방어를 우선하고 iterative deepening negamax/alpha-beta를 사용한다. 실수율도 즉승·즉패 방어를 무시하지 않는다. 원본 보드는 수정하지 않는다.
- AI는 깊이/노드 예산 외에 프레임 약 4ms, 전체 활성 사고시간 650ms의 시험 상한을 둔다. 전체 시간은 프레임 사이 대기를 포함하고 일시정지 시간을 제외한다. 매 16노드 단위로 검사하며 중단 시 마지막 완료 깊이의 합법 수를 사용한다. 250ms의 별도 연출 대기가 있다. 하드 실시간 보장이나 실기기 p95 수치가 아니다.
- 이번 Editor 배치 smoke에서 최고 예산의 빈 판 AI 응답은 연출 대기를 포함해 약 280ms였다. 이는 단일 개발 PC 검사이며 모바일 성능으로 일반화하지 않는다.
- `GameData`: JsonUtility wrapper 계약 v1을 읽고 정확히 30개, 연속 ID/챕터/문자열 참조/AI 범위/시작측/합법 시작 수열을 검사한다. 실패하면 로비에 명시적 오류를 표시하고 시작 버튼을 막는다.
- `GameApp`: 로비/스테이지 탐색→대국→결과→재도전/다음 스테이지, 일시정지·로비 복귀, 승리 줄과 패배 방향, 효과음·설정·도움말·고지를 연결한다.
- `ProgressStore`: 게임 데이터와 별도 저장 버전1, 최고 해금/실제 최고 완료 단계/완료 판수/설정/광고 간격/처리한 판 ID를 PlayerPrefs로 저장한다. 비연속 A→B→A 및 재실행 후 같은 결과 중복 처리를 막는다. 저장 실패는 경고를 표시하고 현재 플레이/결과 화면 이동을 계속 허용한다.
- 서버/로그인/계정/결제 없이 대국과 해금을 진행한다.

## 실제 저장 UI

씬: `Assets/StageGame/Scenes/Main.unity`.

| 저장 프리팹 | 역할 |
|---|---|
| `Prefabs/AppRoot.prefab` | Canvas, SafeArea, GameApp의 모든 직렬화 참조 |
| `UI/Common/CloseButton.prefab` | 결과/설정/도움말/확인/고지에서 중첩 재사용 |
| `UI/Common/PrimaryButton.prefab` | 공통 행동 버튼 |
| `UI/Common/ModalFrame.prefab` | 배경·제목·공통 닫기 버튼 |
| `UI/Common/StageCard.prefab` | 스테이지 정보 |
| `UI/Screens/LobbyScreen.prefab`, `GameScreen.prefab` | 로비 및 대국 화면 |
| `UI/Screens/ResultModal.prefab`, `SettingsModal.prefab` | 결과 및 설정 |
| `UI/Screens/HelpModal.prefab`, `ConfirmModal.prefab`, `NoticesModal.prefab` | 도움말·일시정지 확인·스크롤 가능한 라이선스 전문 |
| `Board/Board.prefab`, `Board/Cell.prefab` | 저장된 42칸 및 7개 열 입력 영역 |

위 경로는 `Assets/StageGame/Prefabs/` 기준이다. 런타임에는 저장된 UI를 바인딩·활성화하며 UI 트리를 생성하지 않는다. 단순 원형 이미지와 착수 효과음은 Editor 도구에서 최초 생성해 파일로 저장했다.

`ProjectBuilder.Create`는 존재하는 프리팹을 재생성하지 않는다. 초기 누락 스프라이트/SafeArea 참조 보완과 고지 버튼 추가는 `LoadPrefabContents`/`UnloadPrefabContents`를 사용한 제한된 수정으로 수행했다. 모든 공통 CloseButton이 원본 프리팹을 중첩 참조하는지 검사한다.

## 광고 상태와 제한

- **실제 SDK나 광고 계정 ID는 연결하지 않았다.** 플랫폼 공통 `IAdService` 계약과 기본 `NoFillAds`만 있다. 기본 상태에서는 광고 수익이 발생하지 않는다.
- `EditorTestAds`는 `UNITY_EDITOR`에서만 컴파일한다. 성공/실패/취소/중복 콜백 검증을 위한 도구다.
- 힌트는 자발적 요청, 성공 콜백 1회/판만 지급한다. 미준비/실패/취소는 무보상이며 재도전은 무료다.
- 리워드 대기 중 보드와 중복 요청을 잠근다. 6초 응답 제한, 모달/백그라운드/판 이동 취소, 늦은·중복 콜백 거부 후 진행 복구를 검사했다.
- 전면 광고 기회는 첫 3판 제외, 마지막 실제 표시 후 2판 및 90초 조건을 모두 만족하는 결과 경계에서만 처리한다. 4초 응답 제한 후 결과 버튼을 복구한다.
- 향후 실제 SDK 어댑터는 `CancelPending()`에서 콜백뿐 아니라 **늦은 광고 표시 자체**도 취소해야 한다. 현재 no-fill 검증이 실제 SDK의 수명주기 검증을 대신하지 않는다.

## 다음 MVP 범위: 광고 제거 IAP (미구현)

2026-10-14 총괄이 전달한 사용자 결정으로 광고 제거 IAP를 다음 구현 범위에 추가했다. **이번 한영/UI 작업에 결제 구현은 포함하지 않았다. 결제 기능은 구현·연동·검증되지 않았다.**

- 상품 기획안: **3,900원 1회 결제**, 전면·배너 광고 제거 및 리워드 힌트 광고 면제. 구매자의 힌트도 **판당 1회**로 제한한다.
- 판매 노출 조건: 원스토어 광고 미연결 버전에서는 구매 버튼을 숨긴다. 아직 제공하지 않는 광고의 제거 상품을 판매하지 않는다.
- 구현 예정: 플랫폼별 결제 어댑터, 서버 검증을 거친 구매 권리 적용, 구매 복구, 저장된 Unity 프리팹 기반 구매/복구 UI. 로컬 값만으로 구매 권리를 확정하지 않는다.
- QA 예정: 구매·복구 성공/실패, 환불 후 권리 갱신, 광고 제거 적용, 판당 힌트 제한, 광고 미연결 버전의 판매 버튼 비노출을 검증한다. 아직 수행하지 않았다.
- 최신 총괄 계약은 v5이며 서버 계획은 **Cloudflare Workers Free + D1 Free + Secrets/mTLS**다. 무료 한도 내 운영 방향이고 실제 토스 mTLS·소유권·SDK 검증 및 배포는 미실행이다. 상세 조건은 `Server/README.md`를 따른다. JSON schema v1 및 오프라인 대국 구조는 유지한다.

## 한영 개선 이전의 검증·수정 이력

- 390×844 뷰포트에서 기존 높이 우선 CSS의 캔버스가 474.75×844로 커져 오른쪽 버튼이 잘리는 문제를 수정했다. 안전영역을 뺀 가로·세로 공간의 최솟값으로 크기를 제한하고 grid 최소 크기를 0으로 설정했다. 실제 브라우저 DOM 측정은 390×844→390×693.33, 320×568→319.5×568, 1280×720→405×720이며 모두 9:16 및 화면 내부 경계를 유지한다. 390×844에서 설정 버튼과 닫기 버튼도 포인터로 검증했다. 조회한 브라우저 오류·경고는 없었다. 이는 데스크톱 IAB의 뷰포트 검사이며 실기기 검사와 구분한다.
- 총괄의 최종 독립 브라우저 재시험은 390×844에서 캔버스 390×693.333, x=0/y=75.333, 로비·7열 보드·결과 화면 잘림 없음을 확인했다. 0 기준 열 수순 `P3 AI3 P3 AI3 P1 AI1 P2 AI1 P4`로 1스테이지 9수 승리 후 '연결 성공' 결과→로비 2/30 해금→새로고침 후 STAGE 02 '두 돌 잇기' 및 2/30 복원을 확인했다. 조회한 console error/warn은 빈 배열이며 뷰포트 설정도 복원했다. 이 항목은 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의 2026-10-14 전달 결과를 기록한 것으로, 클라이언트의 별도 재실행이나 실제 모바일 기기 검사가 아니다. QA007은 클라이언트 수정 및 총괄 직접 재시험을 통과했으며 QA 문서 종결은 해당 담당 영역에서 반영한다.
- 반응형 수정 후 WebGL 재빌드는 오류/경고 0으로 성공했다. WASM·data·Android APK 해시는 이전 검증본과 동일하고 WebGL index.html만 변경됐다. 이번 변경은 템플릿 CSS이며 기존 Unity 30/30 결과를 보존하고 브라우저 검사를 추가했다. Android 재빌드나 Unity 전체 테스트 재실행은 하지 않았다.
- 규칙 4방향 승리/만열/종료 잠금/복사 불변성, AI 즉승·방어·예산, 30개 데이터 재생 및 합법 AI 수, 잘못된 데이터, 광고 실패·취소·중복·간격, 결과 저장 중복, 저장 프리팹 참조를 검사했다.
- EnterPlayMode 테스트로 실제 저장 UI의 클릭, AI 응수, 결과/다음 스테이지, 로컬 해금 복구, 광고 대기/시간 초과/모달/백그라운드/판 전환, 모의 중복 보상, 최고 AI 예산을 검사했다.
- 라이선스 X는 `EventSystem.RaycastAll`에서 최상위 입력 대상으로 판정되고 PointerClick 경로로 닫히는 회귀 검사를 통과했다. 실제 1280×720 브라우저에서도 관찰된 X 중심 `(783,85)` 클릭 후 설정 복귀를 확인했다. 별도 보고된 `(783,118)`은 이 빌드에서 버튼 아래였으며 상세 증거는 `Evidence/notices-close-check.txt`, `webgl-notices-close.jpg`다.
- 총괄도 동일 빌드에서 `(783,85)`로 즉시 닫힘을 독립 재확인하고 최초 보고를 좌표 판독 오류로 정정했다. 제품 결함으로 남기지 않으며 이 정정 때문에 코드 변경·재빌드는 수행하지 않았다. 최종 회귀 30/30, 런타임 소스·세 JSON·빌드 파일 해시 일치를 마지막으로 재확인했다.
- QA 독립 harness v4는 21개 통과/0개 실패 (`Docs/QA/Evidence/qa-core-v4-20261014/run.log`). 저장 실패 주입 등은 Unity 대체 API 검사이며 실제 기기 증거와 구분한다.
- 초기 테스트에서 A→B→A 결과 중복, 재실행 후 중복, 저장 예외 시 결과 화면 정지, 1개 스테이지 잘림 허용을 수정했다.
- 초기 캡처의 사각형 돌은 SpriteImportMode.Single과 누락 참조를 보완했다. 흐린 글자는 작은 GameView에서 생성된 Text 캐시를 캡처 해상도로 다시 생성해 수정했다.
- 첫 일반 WebGL 빌드는 컴파일 성공했지만 브라우저에서 자홍색 화면이 나왔다. 새 프로젝트의 `m_AlwaysIncludedShaders`가 비어 있어 런타임 기본 uGUI 셰이더가 누락된 문제였다. `UI/Default`, `Sprites/Default`를 명시 포함하고 실제 브라우저에서 로비·착수·AI 응수를 재검증했다. 임시로 시도했던 Editor 전용 `GUI/Text Shader`는 플레이어에 포함하지 않는다.
- 최초 sandbox Unity 실행은 라이선스 IPC 접근 실패로 중단했다. 자신이 생성한 Client 배치 프로세스만 경로 검증 후 종료하고 승인된 배치 실행으로 검증했다. 별도 SimpleGame 에디터는 조작하지 않았다.

## 최종 데이터 및 산출물 식별

10/12/20/22/24/26 스테이지의 수정 국면을 포함한 최종 데이터다.

| 파일 | SHA-256 |
|---|---|
| `stages.json` | `2D04B124A42F0F3798AD942E9AACAB97EE34A25F05B380144CD70F75C3904992` |
| `ads.json` | `D19F181FAF52DB28287BBED1B42291770DC3BF83D35E5CCA8AF4A6DCCE9E194A` |
| `strings.json` (한영137개, 승인 패배문구3개 수정) | `57E392CF68464898829946F17030E4AE3008AAB86370452BACF6DBE56D68A291` |
| Android APK (39,792,595 bytes) | `865DAD68BA5A648829F83428C53F7BC343C4AB60595F54BE9080D402B932EC95` |
| WebGL WASM | `D093BE45525525A092B3569D65CAE23B0A7A63C701A50859C41FFAE3ECC344C0` |
| WebGL data | `FA73330ECB9D638D63BBA6B2B4403E5F23A8A5BAF6F93CF48173E3469C9B17EE` |
| WebGL index.html | `53BDBF4813EA2CFE5A19E5C21B3B711CD6AD08AD1200254146D1AB3BA3C6C3CA` |
| WebGL ui-text.js | `2D958DA273B7A6ACC2DFBD37EE24B034F0A82078A7BCA7755858A41C33A70FC8` |

전체 산출물·런타임 소스 해시는 `Evidence/verification.json`에 기록했다. 빌드 파일/Library/Logs는 Git 제외 대상이며, 작은 검증 요약/XML/이미지는 Evidence에 별도로 남겼다. 커밋/푸시는 수행하지 않았다.

## 재현 명령

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe'
$project = 'C:\Github\StageGame\Client'
& "$project\Tools\SyncGameData.ps1"
# 최초 누락 자산만 생성; 기존 사용자 프리팹 재생성 금지
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.ProjectBuilder.Create -quit -logFile "$project\Logs\create.log"
# 기존 프리팹의 한영 키 바인딩/입력 영역 변경을 재적용할 때만 실행
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.LocalizationBuilder.Apply -quit -logFile "$project\Logs\localization-prefabs.log"
# 호스트 UI는 LocalizationBuilder 이후 적용하며 기존 자산을 보존한다
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.SoftUiBuilder.Apply -quit -logFile "$project\Logs\host-ui-prefabs.log"
# 챕터 지도/그림 도움말은 마지막에 적용한다
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.StageMapBuilder.Apply -quit -logFile "$project\Logs\stage-map-prefabs.log"
# 테스트에는 -quit를 추가하지 않는다
& $unity -batchmode -projectPath $project -runTests -testPlatform EditMode -testResults "$project\Logs\editmode-results.xml" -logFile "$project\Logs\tests.log"
& $unity -batchmode -nographics -projectPath $project -buildTarget Android -executeMethod StageGame.Editor.ProjectBuilder.BuildAndroid -quit -logFile "$project\Logs\android-build.log"
& $unity -batchmode -nographics -projectPath $project -buildTarget WebGL -executeMethod StageGame.Editor.ProjectBuilder.BuildWebGL -quit -logFile "$project\Logs\webgl-build.log"
```

## 사용 자산과 기존 프로젝트 조사

- Noto Sans CJK KR Regular 공식 OTF를 사용했다. 폰트 SHA-256은 `6BCB2A0703AA137E874FC2DFFA85F6C21BA9A67FA329E81B8C801663AF7E992A`다. Adobe 저작권 및 OFL 원문, uGUI 저작권과 Unity Companion License 전문, 개발 도구 고지를 함께 포함했다.
- 설정→오픈소스 라이선스에서 고지를 스크롤해 읽을 수 있다. 원문은 `Assets/StreamingAssets/Notices`, UI용 묶음은 `Resources/LegalNotices.txt`다. `Tools/PrepareNotices.py`는 내려받은 공식 UCL HTML과 로컬 고지 원문을 묶는다.
- 공식 출처: https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf 및 https://unity.com/legal/licenses/unity-companion-license . 고지 포함은 사용자의 엔진 계약/계정 자격 확인을 대신하지 않는다.
- 설치 조사: 2022.3.62f3는 Android/WebGL 모듈 없음, 6000.3.15f1은 양쪽 모듈과 SDK/NDK/JDK 있음, 6000.5.9f1은 WebGL만 있음.
- SimpleGame ProjectVersion은 6000.3.15f1, uGUI 2.0.0, Input System 1.19.0, URP 17.3.0, 앱인토스 SDK release/v3.5.0이었다. 구조만 읽고 복사/수정하지 않았다. SimpleGame의 OneStore 어셈블리는 WebGL용이므로 원스토어 Android 광고 구현으로 재사용하지 않았다.
- StageGame은 uGUI 2.0.0, Test Framework 1.6.0 및 필요한 Unity 기본 모듈만 사용한다. 다른 담당 폴더를 수정하거나 사용자 자산을 덮어쓰지 않았다.

# 이겨도 됩니까? — MVP 데이터

**2026-10-19 현재 원본:** 30스테이지·509문자열·광고·멀티 설정 16개. 멀티 결과·레이팅·착수 확인 한영 7키를 추가하고 Client Resources의 문자열 JSON을 동기화했다. 기존 502키와 전 시트의 기존 값·서식, 나머지 생성 데이터는 보존했다. exporter 일치·47개 자동 검사 통과. [이번 변경·보존·검증 기록](Validation/multiplayer-feedback-20261019/README.md).

2026-10-18 광고 변경: 사용자 확정에 따라 최초 제외 0판·매 1판·시간 간격 0초로 변경하고 광고 제거 구매 안내 한영 12키를 추가했다. 스토리는 정상 완료 매판, 멀티는 정상 대국 이후 방에서 나가 로비로 돌아가는 경계에서 최대 1회가 대상이다. 당시 기존 490문자열과 스테이지·멀티 설정·기존 셀 스타일을 보존했다. [광고 변경·보존·검증 기록](Validation/ad-offer-20261018/README.md).

`Multiplayer` 시트 → `Generated/multiplayer.json`은 서버가 직접 읽는다. Unity에는 기존 세 JSON만 복사한다. 10월 18일 광고 데이터 작업은 Client 파일을 수정하지 않았으며 런타임 적용·광고 SDK·구매 동작 검증은 클라이언트 담당이다. 앞선 2~30단계 대사 정리의 [수정/보존 증거](../Client/Evidence/multiplayer-20261017/excel-preservation.json)와 [멀티 계약](../Docs/Management/MULTIPLAYER-CONTRACT.md)은 별도 기록이다.

2026-10-16 제목 확정: **이겨도 됩니까? / Permission to Win**. 제목·웹 접근성 문구 4키를 Excel에서 갱신했다. [적용 기록](../Docs/Design/GAME-TITLE.md). 아래 4목 / Four in a Row 표기는 이전 표시명 변경 이력이다.

현재 기준(2026-10-15): **부대 24단계 + 병사 대회 6단계, 캐릭터 19명, ko/en 문자열 418개**다. 사용자가 이 기획 세션의 Client/GameData 수정과 Unity 적용·검증을 승인했다. [TOURNAMENT-CONTRACT.md](TOURNAMENT-CONTRACT.md)가 단계 배치와 CharacterId·인물별 대사 계약이다. 추가 피드백으로 초반 시작판 6개와 AI 수치를 완화했고 [DIFFICULTY-CONTRACT.md](DIFFICULTY-CONTRACT.md)에 현재 수치를 기록했다. 문자열·별점·진행 저장 의미는 보존한다. exporter `--check`, Python 45개, Unity 코어 25개 검사 통과. UI와 그림의 직전 검증은 [적용 기록](../Docs/Design/PIXEL-INTEGRATION.md)에 구분한다.

## 직전 계급·난이도 개편 기록

2026-10-15 사용자 승인으로 Excel 원본을 30단계·18개 계급 표시·별 1~8의 반 개 단위 난이도로 변경했다. 처음 6계급은 1단계씩, 나머지는 2단계씩이며 마지막 두 단계는 사단장이다. 별은 같은 값으로 두 단계씩 진행한다. AI 깊이·노드 예산은 점차 높이고 무작위 선택 비율은 줄였다. 7×6 중력식 4목과 기존 30개 시작판·ID·chapter·starter를 유지하며, 한국어·영어 문자열은 269개다. 별은 기획된 단계이며 사람 플레이로 검증된 난이도 등급은 아니다. 표시명은 4목 / Four in a Row, 상점명은 미확정이다.

현재 규격은 `RANK-UI-CONTRACT.md`와 `FORMAT.md`, 검증 자료는 `Validation/progression-20261015/`를 따른다. 아래 제목·서사 변경 기록은 이전 v10 상태이며 이번에는 AI 설정과 계급별 참조도 변경했다. 새 빌드·실광고 활성화는 하지 않았다.

이전 v10 표시명 변경 당시의 해시는 [TITLE-V10-CONTRACT.md](C:/Github/StageGame/GameData/TITLE-V10-CONTRACT.md)를 따른다. v9의 237개 키 중 제목 관련 4키의 7셀만 수정하고 나머지 233개 문자열·Stage·광고를 보존했다. 웹·Android 빌드는 실행하지 않았다. v9 확인 패널·대결 서사는 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md)를 따른다. 프로 출신 플레이어는 훈련병으로 유지하며 계급 표시는 도전자 계급이다. 계급별 대사 30개와 첫 대국 지정 문구 3개를 보존했다. Stage 1의 대사 참조 3개만 전용 키이며 30개 시작판·AI·ID·titleKey·chapter·설명은 그대로다. 일반 대사 최대 ko 23자/en 54자, 첫 시작 대사 ko 30자/en 61자이며 말풍선은 1–2줄을 허용한다. 최종 UI는 Client/총괄 검증 범위이며 실물 폰 시험은 보류한다.

## 파일과 편집 흐름

| 파일 | 역할 |
| --- | --- |
| [GameData_MVP.xlsx](C:/Github/StageGame/GameData/GameData_MVP.xlsx) | 유일한 콘텐츠 원본. `Stages`, `AdConfig`, `Strings`, `Multiplayer` 4개 시트 |
| [FORMAT.md](C:/Github/StageGame/GameData/FORMAT.md) | SimpleGame 관례 조사, 차이점, 실제 v1 필드 매핑과 검증 규칙 |
| [export_data.py](C:/Github/StageGame/GameData/export_data.py) | Excel 읽기, 전체 검사, 재현 가능한 JSON 생성, `--check` |
| [requirements.txt](C:/Github/StageGame/GameData/requirements.txt) | 기존 설치를 사용한 읽기 라이브러리 `openpyxl==3.1.5` |
| [test_export_data.py](C:/Github/StageGame/GameData/test_export_data.py) | Python 자동 검증 47개 |
| [audit_openings.py](C:/Github/StageGame/GameData/audit_openings.py) | 휴리스틱 없이 제한 깊이 승패를 분석하는 독립 bitboard 검사 |
| [Validation](C:/Github/StageGame/GameData/Validation/README.md) | QA 지적 6개 국면 수정 전후 수순·보존 검사·화면·재현 스크립트 |
| [Generated/stages.json](C:/Github/StageGame/GameData/Generated/stages.json) | 기존 30개 국면·점진적 AI 설정과 계급 UI 참조 |
| [Generated/ads.json](C:/Github/StageGame/GameData/Generated/ads.json) | 최초 제외 0판·매 1판·시간 간격 0초, 싱글 힌트 1회/판 |
| [Generated/strings.json](C:/Github/StageGame/GameData/Generated/strings.json) | 계급·설명·대사·UI 문구 총 509개, 각각 ko/en |
| [Generated/multiplayer.json](C:/Github/StageGame/GameData/Generated/multiplayer.json) | 서버가 읽는 시작 레이팅·Elo·매칭·시간 설정 16개 |

소량의 데이터이며 제목 참조와 광고 설정을 한 번에 검증해야 하므로 원본 파일은 하나로 묶었다. 4행 메타 구조는 SimpleGame을 참고했지만 콘텐츠·코드는 새로 작성했다. Excel에는 enum이나 텍스트로 변환한 숫자를 넣지 않았으며, `OpeningMoves`는 열 수순을 표현하는 텍스트다.

Excel을 수정한 뒤 exporter를 실행한다. JSON을 직접 고치지 않는다. export 성공 뒤 클라이언트 담당이 **세 JSON을 함께** `Client/Assets/StageGame/Resources/GameData/`에 복사하고 런타임 검증을 실행한다. 10월 19일 데이터 작업은 변경된 문자열 JSON을 동기화했고 다른 생성물은 불변이다. 런타임 적용 검증은 총괄이 별도로 진행한다.

## 실행

검증 환경은 bundled Python 3.x와 기존 설치된 openpyxl 3.1.5다. 새 패키지 다운로드는 하지 않았다. 아래 명령은 PowerShell 기준이며 작업 디렉터리에 의존하지 않는다.

```powershell
$stagePython = 'C:\Users\JW\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
& $stagePython C:\Github\StageGame\GameData\export_data.py
& $stagePython C:\Github\StageGame\GameData\export_data.py --check
& $stagePython -m unittest discover -s C:\Github\StageGame\GameData -p test_export_data.py -v
```

다른 환경은 Python 3.10 이상과 requirements에 고정한 openpyxl 버전을 준비한 후 같은 스크립트를 실행한다. 설치된 Python을 사용하면 명령의 실행 파일만 바꾼다. `--source <xlsx경로>`는 검사할 원본을 바꿀 수 있지만 출력 디렉터리는 항상 스크립트 옆 `GameData/Generated/`다. `--check`는 쓰지 않고 원본을 검증한 후 현재 JSON과 바이트 단위로 비교한다. 누락·오래된 JSON은 종료 코드 1이다.

모든 원본 검증을 마친 후 새 파일 4개와 이전 파일 백업을 임시 경로에 준비하고 파일별로 교체한다. 검증 실패 시 쓰지 않으며, 교체 중 처리 가능한 예외가 나면 바뀐 파일의 이전 내용 또는 원래의 부재 상태를 복구한다. 복구 실패 시 `Generated/.export-incomplete`와 이 파일이 가리키는 `.export-*` 백업을 보존하고 `--check`와 재export를 거부한다. 백업을 확인해 이전 묶음을 복원·대조한 뒤 표시를 제거하고 exporter와 `--check`를 다시 실행한다. 프로세스 강제 종료·전원 손실·동시 읽기까지 묶음 원자성을 보장하지 않으므로 배포용 복사 전에 `--check`가 성공해야 한다.

## 콘텐츠 의도와 증거 수준

| 학습 의도 | 스테이지 ID | 확인 범위 |
| --- | --- | --- |
| 중앙과 양옆 비교 | 1, 5, 8, 13, 19, 24 | 합법 국면, 즉시 승패 위협 없는 도입/복습용 |
| 두 돌 연결 | 2, 7 | 연결 발판을 관찰하는 연습 |
| 즉시 연결 찾기 | 4, 15, 21, 27 | 한 수 연결 기회가 존재하는 국면 |
| 상대 즉시 위협 차단 | 3, 6, 10, 12, 20, 22, 29 | 상대 즉승 열 하나, 플레이어 즉승 없음 |
| 대각 연결/방어 | 11, 16, 17, 23, 26 | 중력을 고려한 대각 국면, 연결과 방어를 번갈아 소개 |
| 두 위협 만들기 | 9, 14, 18, 25, 28, 30 | 한 수 뒤 복수 위협을 만드는 후보가 있는 국면 |

국면은 결정적 난수 후보 탐색과 전술 특징 필터로 새로 작성한 다음 합법 수순으로 확정했다. 최초 빈 보드는 1개, 중간 국면은 29개이며 모든 재생 후 플레이어가 착수한다. `starter=-1`인 스테이지는 8, 15, 22, 27이고, AI 선공 수순을 재생한 뒤 시작한다. `starter`를 첫 대화면 착수자와 혼동하지 않는다.

현재 깊이는 1~6, 노드 예산은 500~30000, 무작위 선택 비율은 300→0‰다. 같은 별 두 단계는 같은 설정을 사용하며, 단계가 높아질수록 깊이·예산은 유지 또는 증가하고 무작위 선택은 유지 또는 감소한다. 체감 난이도와 전체 퍼즐 밸런스는 사람 플레이 검증이 필요하다.

**30개 전체의 퍼즐·밸런스 인증은 완료되지 않았다.** QA 지적 후 교체한 10/12/20/22/24/26은 독립 제한 깊이 8 분석에서 승리 전략 존재를 확인했다. 전체 30개의 최적 플레이 결과·유일 해답·사람 학습 효과·재미·45~120초 목표는 검증하지 않았다. Excel의 보수적인 `강제승리 미검증` 문구는 다른 필드와 함께 보존했고, 추가 계산 증거는 아래 수정 기록에 분리했다. 실제 기기와 클라이언트 AI를 통한 검증이 필요하다.

## 2026-10-15 난이도·계급 변경 검증

Python 자동 검사 45/45, exporter `--check`, 원본 서식·기존 시작판 보존, Generated와 Client 세 JSON 바이트 대조가 통과했다. 현재 해시와 변경 셀은 [verification.json](C:/Github/StageGame/GameData/Validation/progression-20261015/verification.json), Excel 전후 렌더는 같은 디렉터리에 보관한다. 반 별과 사단장 표시는 Client Unity 검사에서 확인하며 실제 기기 체감 난이도 평가는 별도다.

## 2026-10-14 실행 결과 (이전 기록)

| 검사 | 결과 |
| --- | --- |
| Artifact Tool로 Excel 작성·언어 확장 및 재계산 | 완료. 3개 시트, 30스테이지, 10계급, ko/en 237개, 수식 오류 검색 0건 |
| Excel 화면 렌더 | 3개 시트 모두 직접 확인. Stages 좌/우·후반, 광고 전 필드, Strings 메타·제목·긴 도움말/UI 문구 확인. 한글/메타/긴 수순 잘림 없음 |
| 저장된 Excel 재열기 | openpyxl로 원본을 읽어 타입·메타·키·참조·합법 수순 검사 통과 |
| 실제 exporter | 성공. 3개 wrapper 객체, UTF-8, schemaVersion 1 |
| 자동 테스트 | 44개 통과. 기존 경계 검사와 신규 참조·계급/별 경계·AI 대사 역할·Stage 필드 보존 포함 |
| 반복 생성 및 행 순서 변경 | 동일 JSON 바이트. 문자열 키/스테이지 ID 정렬 확인 |
| 잘못된 데이터 입력 | 생성 실패 및 기존 정상 JSON 보존 확인 |
| `--check` | 현재 원본과 생성물 일치. 누락/변경 JSON 거부도 테스트 |

자동 검사는 30개 수순의 **모든 접두 수순**을 독립적인 전체 보드 4칸 탐색으로 다시 확인한다. production validator의 마지막 착수점 검사와 별도 방식이다. 추가로 양방향 대각선, 수직/수평 승리, 종료 뒤 착수, 만열 7번째 착수, 41수 유효/42수 무승부, 잘못된 열·starter·구분 문법, 숫자 경계·비정수·NaN/무한대, 중복/누락 ID·문자열, 잘못된 챕터, 없는 titleKey, 메타행 변경, NONE 행의 수식, 광고 계약 위반을 검사한다. 테스트는 원본 Excel을 저장하지 않으며 전후 SHA-256 동일성도 확인한다.

스프레드시트 제작은 bundled Artifact Tool, export는 openpyxl의 읽기 기능만 사용했다. 분석용 Node 의존성 junction은 권한 제한으로 만들지 못해 설치된 패키지의 공개 entrypoint를 읽기 전용으로 resolve했다. 패키지 디렉터리는 수정하지 않았다.

원본과 검증한 생성물의 SHA-256:

| 파일 | SHA-256 |
| --- | --- |
| `GameData_MVP.xlsx` | `f2def3d974982b9361040c3b3e242e9fe3cfcbe72737212b782355b2b56401a1` |
| `stages.json` | `6f76d1575f71fd0240cf7b5774bd2a3fed41fb1828bd0cb85464f34e72122e7e` |
| `ads.json` | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` |
| `strings.json` | `9519b8ea2dfa94e45ac1273226ffb0a95634779d3de9d02d6817f17c9e6f74f1` |

이 해시는 해당 검증 시점의 기록이다. 원본 변경 후에는 exporter, 테스트, `--check`를 다시 실행하고 갱신한다.

## QA 지적 국면 6개 수정

QA core-v2는 10/12/20/22/24/26에서 실제 C# AI 상대 승리 예시를 12시도 안에 찾지 못했다. 별도 bitboard 전수 탐색으로 이전 국면을 검사한 결과, 각각 8/6/6/6/18/22 ply 이내의 최적 대응 패배를 확인했다. 시작 시 단순 양방향 즉승만 검사해서는 검출되지 않는 문제였다.

학습 의도를 유지하면서 **해당 6개 OpeningMoves 셀만** 교체했다. 기존 수순 길이를 고정하지 않았으며 AI 값·제목·starter·학습 의도·문구·광고 설정은 유지했다. 다른 24개 국면, 전 시트 서식·고정창·행/열 크기·입력 유효성도 비교하여 보존을 확인했다. 새 6개는 제한 깊이 8에서 모두 `bounded_win`이고, 회귀 테스트에 포함했다. 실제 클라이언트 AI의 0.65초 제한과 사람의 선택은 이 분석에 포함되지 않는다.

| Stage | 새 openingMoves |
| --- | --- |
| 10 | `3,3,4,1,3,1,1,5,3,0,4,0,1,2` |
| 12 | `6,1,6,6,5,3,4,5,1,3,3,2,0,6,6,2` |
| 20 | `2,5,5,3,6,5,3,0,5,6,1,4,3,2,6,2,0,1` |
| 22 | `2,1,5,0,0,2,0,6,3,3,6,6,6,3,2,1,5` |
| 24 | `0,6,5,0,3,6,5,1,1,1,2,4,6,1,4,4` |
| 26 | `0,4,6,3,3,5,0,4,4,3,2,3,3,0,4,1,0,2,2,4` |

재현 명령:

```powershell
& $stagePython C:\Github\StageGame\GameData\audit_openings.py --depth 8
& $stagePython C:\Github\StageGame\GameData\Validation\verify_repairs.py
```

입력과 결과는 [Validation/README.md](C:/Github/StageGame/GameData/Validation/README.md)에 정리했다. exporter에는 한 수 후 피할 수 없는 즉시 패배를 거부하는 검사도 추가했다. 깊은 패배는 별도 audit와 QA에서 검사하며, exporter 성공을 게임의 승리 가능성 증명으로 취급하지 않는다.

## QA-004 출력 교체 실패 수정

두 번째 `os.replace`에 오류가 나면 stages만 새 버전으로 남던 문제를 수정했다. 1·2·3번째 교체 오류, 첫 생성 중 오류, 복구 자체 실패를 주입해 기존 묶음 보존 또는 불완전 표시/백업 보존과 `--check` 거부를 검증했다. 데이터 내용과 Excel은 변경하지 않았다.

재현: `& $stagePython C:\Github\StageGame\GameData\Validation\verify_export_failure.py`. [결과](C:/Github/StageGame/GameData/Validation/export-failure-results.json)에는 exporter 해시와 전후 데이터 해시를 기록한다. 프로세스 강제 중단까지 완전 원자성을 보장하는 구현은 아니다.

후속 [QA core-v4 경로](C:/Github/StageGame/Docs/QA/Evidence/qa-core-v4-20261014/stage-paths.json)는 이 생성물 해시로 30/30 플레이어 승리 예시를 확인했다. 이는 해당 C# AI·고정 시드·노드 예산 탐색의 예시이며 실제 기기 시간 제한과 사람 플레이 증거는 아니다.

## 남은 확인

1. 클라이언트가 세 JSON을 함께 복사하고 JsonUtility 로딩·오류 경로·모든 스테이지 진입을 확인한다.
2. 실제 AI에서 즉승/즉패 방어 우선, 예산 상한, 30국면의 승패와 학습 의도를 검토한다. 깊이 5~6의 저사양 기기 응답 시간은 특히 실측한다.
3. 최고 해금 저장, 데이터 업데이트 호환, Android/WebGL의 종료/재개와 저장 복원을 검증한다.
4. 사용자 제공 원스토어/토스 계정 준비 상태와 실제 SDK 연결·광고 승인·보상 검증을 구분한다. 구매 검증 무료 서버 계획은 [Server/README.md](C:/Github/StageGame/Server/README.md)를 따른다.

이번 결과에 Unity 실행/컴파일, Android/WebGL 빌드, 실제 광고, 스토어 제출, 기기 또는 사람 플레이테스트 통과를 포함하지 않는다.

## 한국어·영어 전환 데이터 (이전 137개 시점)

Strings의 기존 A:D 뒤에 Content_Eng(E)를 추가했다. 기존 한국어 58개를 보존·번역하고 Client 요청 신규 65개를 추가한 뒤, 단계 선택/그림 도움말 14개를 더해 당시 총 137개였다. `strings.json`은 schemaVersion 1의 `key/ko/en`이며 stages/ads JSON은 바이트가 동일하다. 총괄과 클라이언트의 [합의 계약](C:/Github/StageGame/GameData/LOCALIZATION-CONTRACT.md)을 따른다.

누락 번역·중복 키·양 언어 포맷 인자 불일치·불완전 중괄호·지원하지 않는 포맷을 거부한다. 허용 범위는 `{0}`, `{1}` 등과 `{{`/`}}`다. 순서 변경·반복을 허용하며 인덱스는 0부터 연속이다. 검증 실패 시 이전 JSON 묶음을 보존한다.

최초 언어 확장 당시 변경 전/후 Strings 렌더(메타, 긴 도움말, 설정, 동적 인자, 추가 UI)를 직접 확인했다. 당시 XLSX를 다시 읽어 원래 전 셀의 값·스타일, 행/열 크기, 고정창·검증 규칙 보존과 Client 요청 70개의 문구·인자 수 일치를 확인했다. [당시 결과](C:/Github/StageGame/GameData/Validation/localization-results.json). 현재 검증 명령은 `& $stagePython C:\Github\StageGame\GameData\Validation\verify_localization.py`이며 아래 승인 수정 3개를 반영해 대조한다. 변경 전 원본을 `--before`로 주면 승인된 셀 값 변경만 허용하여 나머지 보존 비교도 실행한다.

언어 전환 UI·언어 저장·화면 잘림·WebGL 로딩 문구는 Client 담당이 이 원본의 생성물을 연결하여 검증한다. 이 문서는 데이터 준비 완료만 기록한다.

후속 14개는 `Strings!A128:E141`에 추가했다. 기존 120개 보존 + 방향별 패배 안내 3개 승인 수정 + 신규 14개 = 총 137개다. 총괄의 실제 브라우저 검사에서 결과 팝업에 가려진 금색 줄을 보라는 안내를 발견해 `result_loss_horizontal/vertical/diagonal`의 둘째 줄만 ko/en에서 삭제했다. 변경 셀 C101:C103·E101:E103 외 기존 값과 전체 서식·크기·고정창·입력 유효성을 보존했다. [승인 전후 문구](C:/Github/StageGame/GameData/Validation/approved-string-revisions.json)를 반영한 ko/en·포맷 인자 수 검증, 반복 export와 `--check`, 기존 테스트 36개가 통과했다. [추가·수정 문구·최종 해시·보존 증거](C:/Github/StageGame/GameData/Validation/stage-map-strings-results.json). 입문/전술/도전은 1–10/11–20/21–30의 챕터 표시명이며 사람 난이도 검증 결과가 아니다. 변경 전 Excel로 보존 비교를 재실행할 때는 `verify_localization.py --before <변경 전 원본> --report <새 결과 경로>`를 사용해 과거 증거를 유지한다.

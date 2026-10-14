# StageGame MVP 데이터

2026-10-14 기준 Excel 원본과 JSON v1 생성물을 작성하고 검증했다. 7×6 중력식 4목, 3챕터×10스테이지, 광고 설정 1행, 한국어 문자열 58개다. 전용 게임 서버는 사용하지 않는다.

## 파일과 편집 흐름

| 파일 | 역할 |
| --- | --- |
| [GameData_MVP.xlsx](C:/Github/StageGame/GameData/GameData_MVP.xlsx) | 유일한 콘텐츠 원본. `Stages`, `AdConfig`, `Strings` 3개 시트 |
| [FORMAT.md](C:/Github/StageGame/GameData/FORMAT.md) | SimpleGame 관례 조사, 차이점, 실제 v1 필드 매핑과 검증 규칙 |
| [export_data.py](C:/Github/StageGame/GameData/export_data.py) | Excel 읽기, 전체 검사, 재현 가능한 JSON 생성, `--check` |
| [requirements.txt](C:/Github/StageGame/GameData/requirements.txt) | 기존 설치를 사용한 읽기 라이브러리 `openpyxl==3.1.5` |
| [test_export_data.py](C:/Github/StageGame/GameData/test_export_data.py) | Python 자동 검증 28개 |
| [audit_openings.py](C:/Github/StageGame/GameData/audit_openings.py) | 휴리스틱 없이 제한 깊이 승패를 분석하는 독립 bitboard 검사 |
| [Validation](C:/Github/StageGame/GameData/Validation/README.md) | QA 지적 6개 국면 수정 전후 수순·보존 검사·화면·재현 스크립트 |
| [Generated/stages.json](C:/Github/StageGame/GameData/Generated/stages.json) | 30개 국면과 AI 시험값 |
| [Generated/ads.json](C:/Github/StageGame/GameData/Generated/ads.json) | 최초 3판 제외, 이후 2판 AND 90초, 힌트 1회/판 |
| [Generated/strings.json](C:/Github/StageGame/GameData/Generated/strings.json) | 제목 30개와 한국어 UI/안내 문구 28개 |

소량의 데이터이며 제목 참조와 광고 설정을 한 번에 검증해야 하므로 원본 파일은 하나로 묶었다. 4행 메타 구조는 SimpleGame을 참고했지만 콘텐츠·코드는 새로 작성했다. Excel에는 enum이나 텍스트로 변환한 숫자를 넣지 않았으며, `OpeningMoves`는 열 수순을 표현하는 텍스트다.

Excel을 수정한 뒤 exporter를 실행한다. JSON을 직접 고치지 않는다. export 성공 뒤 클라이언트 담당이 **세 JSON을 함께** `Client/Assets/StageGame/Resources/GameData/`에 복사하고 런타임 검증을 실행한다. 이 세션은 `Client/`에 쓰지 않았다.

## 실행

검증 환경은 bundled Python 3.x와 기존 설치된 openpyxl 3.1.5다. 새 패키지 다운로드는 하지 않았다. 아래 명령은 PowerShell 기준이며 작업 디렉터리에 의존하지 않는다.

```powershell
$stagePython = 'C:\Users\JW\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
& $stagePython C:\Github\StageGame\GameData\export_data.py
& $stagePython C:\Github\StageGame\GameData\export_data.py --check
& $stagePython -m unittest discover -s C:\Github\StageGame\GameData -p test_export_data.py -v
```

다른 환경은 Python 3.10 이상과 requirements에 고정한 openpyxl 버전을 준비한 후 같은 스크립트를 실행한다. 설치된 Python을 사용하면 명령의 실행 파일만 바꾼다. `--source <xlsx경로>`는 검사할 원본을 바꿀 수 있지만 출력 디렉터리는 항상 스크립트 옆 `GameData/Generated/`다. `--check`는 쓰지 않고 원본을 검증한 후 현재 JSON과 바이트 단위로 비교한다. 누락·오래된 JSON은 종료 코드 1이다.

모든 원본 검증을 마친 후 새 파일 3개와 이전 파일 백업을 임시 경로에 준비하고 파일별로 교체한다. 검증 실패 시 쓰지 않으며, 교체 중 처리 가능한 예외가 나면 바뀐 파일의 이전 내용 또는 원래의 부재 상태를 복구한다. 복구 실패 시 `Generated/.export-incomplete`와 이 파일이 가리키는 `.export-*` 백업을 보존하고 `--check`와 재export를 거부한다. 백업을 확인해 이전 묶음을 복원·대조한 뒤 표시를 제거하고 exporter와 `--check`를 다시 실행한다. 프로세스 강제 종료·전원 손실·동시 읽기까지 묶음 원자성을 보장하지 않으므로 배포용 복사 전에 `--check`가 성공해야 한다.

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

깊이는 1~6, 노드 예산은 500~26000, 실수 비율은 0~250‰의 초기 시험값이다. 챕터 사이와 복습 단계에서 낮추기도 하므로 매 단계 숫자만 증가하는 구조가 아니다. 생성된 AI 파라미터가 실제 클라이언트에서 어느 정도의 응답 속도·난이도를 만드는지는 측정하지 않았다.

**30개 전체의 퍼즐·밸런스 인증은 완료되지 않았다.** QA 지적 후 교체한 10/12/20/22/24/26은 독립 제한 깊이 8 분석에서 승리 전략 존재를 확인했다. 전체 30개의 최적 플레이 결과·유일 해답·사람 학습 효과·재미·45~120초 목표는 검증하지 않았다. Excel의 보수적인 `강제승리 미검증` 문구는 다른 필드와 함께 보존했고, 추가 계산 증거는 아래 수정 기록에 분리했다. 실제 기기와 클라이언트 AI를 통한 검증이 필요하다.

## 2026-10-14 실행 결과

| 검사 | 결과 |
| --- | --- |
| Artifact Tool로 신규 Excel 작성 및 재계산 | 완료. 3개 시트, 30스테이지, 한국어 58개, 수식 오류 검색 0건 |
| Excel 화면 렌더 | 3개 시트 모두 직접 확인. Stages 좌/우·후반, 광고 전 필드, Strings 메타·제목·긴 도움말/UI 문구 확인. 한글/메타/긴 수순 잘림 없음 |
| 저장된 Excel 재열기 | openpyxl로 원본을 읽어 타입·메타·키·참조·합법 수순 검사 통과 |
| 실제 exporter | 성공. 3개 wrapper 객체, UTF-8, schemaVersion 1 |
| 자동 테스트 | 28개 통과. 아래 주요 경계와 불변성 포함 |
| 반복 생성 및 행 순서 변경 | 동일 JSON 바이트. 문자열 키/스테이지 ID 정렬 확인 |
| 잘못된 데이터 입력 | 생성 실패 및 기존 정상 JSON 보존 확인 |
| `--check` | 현재 원본과 생성물 일치. 누락/변경 JSON 거부도 테스트 |

자동 검사는 30개 수순의 **모든 접두 수순**을 독립적인 전체 보드 4칸 탐색으로 다시 확인한다. production validator의 마지막 착수점 검사와 별도 방식이다. 추가로 양방향 대각선, 수직/수평 승리, 종료 뒤 착수, 만열 7번째 착수, 41수 유효/42수 무승부, 잘못된 열·starter·구분 문법, 숫자 경계·비정수·NaN/무한대, 중복/누락 ID·문자열, 잘못된 챕터, 없는 titleKey, 메타행 변경, NONE 행의 수식, 광고 계약 위반을 검사한다. 테스트는 원본 Excel을 저장하지 않으며 전후 SHA-256 동일성도 확인한다.

스프레드시트 제작은 bundled Artifact Tool, export는 openpyxl의 읽기 기능만 사용했다. 분석용 Node 의존성 junction은 권한 제한으로 만들지 못해 설치된 패키지의 공개 entrypoint를 읽기 전용으로 resolve했다. 패키지 디렉터리는 수정하지 않았다.

원본과 검증한 생성물의 SHA-256:

| 파일 | SHA-256 |
| --- | --- |
| `GameData_MVP.xlsx` | `e236710c67014a3c4f6aced8121977a1185b447b2c7b1f9620c9edde2c2e5bc4` |
| `stages.json` | `2d04b124a42f0f3798ad942e9aacab97ee34a25f05b380144cd70f75c3904992` |
| `ads.json` | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` |
| `strings.json` | `f307e1900a7e0cdf72a1b50d33cbfb0eece4a6f46e69910978286a6cef2d7752` |

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
4. 사용자 제공 원스토어/토스 계정 준비 상태와 실제 SDK 연결·광고 승인·보상 검증을 구분한다. 서버 없이 시작하는 범위는 [Server/README.md](C:/Github/StageGame/Server/README.md)를 따른다.

이번 결과에 Unity 실행/컴파일, Android/WebGL 빌드, 실제 광고, 스토어 제출, 기기 또는 사람 플레이테스트 통과를 포함하지 않는다.

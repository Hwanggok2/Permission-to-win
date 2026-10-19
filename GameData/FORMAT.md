# StageGame GameData 형식

2026-10-15 추가 적용: AI 전체가 어렵다는 피드백에 따라 200~1600 내부 레이팅 프로필과 전술 누락 확률을 추가했다. Stages S/T열과 JSON의 `aiRating`/`aiOversightPermille`가 추가되며 wrapper는 v1을 유지한다. 현재 밸런스·검증은 [DIFFICULTY-CONTRACT.md](DIFFICULTY-CONTRACT.md)를 따른다.

확인일: 2026-10-14 (Asia/Seoul). 상태: 형식 조사 및 MVP Excel·Python exporter/validator·JSON v1 작성 완료.

Excel을 수치와 문자열의 원본으로 삼고, 개발 시 검증·변환한 데이터를 클라이언트에 포함한다. 2026-10-15 사용자 승인으로 부대 대국 24단계와 병사 대회 6단계, 19명의 인물을 배치했다. [대회 계약](TOURNAMENT-CONTRACT.md)이 현재 배치 기준이다. 별은 1~8이며 2단계마다 0.5씩 높아진다. 기존 저장 호환용 chapter 필드의 3×10 값은 유지한다.

이 문서의 **관찰**은 SimpleGame 파일에서 확인한 내용이고, **StageGame 기준안**은 적용할 원칙이다. 참조 구현을 복사하거나 모든 예외의 호환성이 검증되었다는 뜻은 아니다. 현재 구현 범위는 아래 9절과 [실행 결과](C:/Github/StageGame/GameData/README.md)에 명시한다. 런타임 타입·출력은 총괄의 [JSON v1 계약](C:/Github/StageGame/Docs/Management/CONTRACT.md)을 따른다.

## 1. 확인한 자료와 범위

- [SimpleGame 구조 안내](C:/Github/SimpleGame/GameData/GameData_StructureGuide.md:143): 공통 메타행·ID·소유권 규칙. 문서 내 과거 설명과 현재 코드가 일부 다르다.
- [공통 importer와 ExcelTable](C:/Github/SimpleGame/Assets/Game/Editor/GameDataExcelImporter.cs:3053): 필드 탐색, 메타행 인식, 행 추출, 값 파싱을 정적으로 확인했다.
- [Open XML reader](C:/Github/SimpleGame/Assets/Game/Editor/OpenXmlWorkbookReader.cs:46): ZIP/XML 읽기, 수식·오류 셀 거부, Excel Table 메타데이터 검사.
- [문자열 파서](C:/Github/SimpleGame/Assets/Game/Editor/GameDataExcelImporter.cs:1142): 별칭, 다중 시트 병합, 문자열 키와 포맷 인자 검사.
- [import 적용 단계](C:/Github/SimpleGame/Assets/Game/Editor/GameDataExcelImporter.cs:2801): 참조 검증 후 에셋 변경, Undo 등록과 실패 복구 시도. 디스크 저장 전체의 원자성을 실행 검증한 것은 아니다.
- [추가 도메인 파서](C:/Github/SimpleGame/Assets/Game/Editor/PermanentProgressionExcelParser.cs:164)와 [별도 Endgame importer](C:/Github/SimpleGame/Assets/Game/Editor/EndgameExcelImporter.cs:65): 공통 규칙과 예외를 구분하는 근거.

`C:/Github/SimpleGame/GameData` 바로 아래 `.xlsx` 11개에서 시트명·메타행·수식/병합 존재 여부를 읽었다. `Legacy/`의 엑셀 2개는 목록만 확인했고 기준으로 채택하지 않았다. 콘텐츠 행, 밸런스 수치, 소스, 에셋, 비밀정보는 StageGame에 복사하지 않았다.

대표 메타행은 bundled Artifact Tool로 다음 범위를 읽어 확인했다.

| 원본 | 범위 | 확인 내용 |
| --- | --- | --- |
| [GameData_String.xlsx](C:/Github/SimpleGame/GameData/GameData_String.xlsx) | `GameString!A1:I4` | `ExportType`, 문자열 필드, `none`, `Client`, `None` |
| [GameData_Enemy.xlsx](C:/Github/SimpleGame/GameData/GameData_Enemy.xlsx) | `EnemyBalance!A1:H4` | 일반 수치 시트의 4행 구조와 `All` |
| [GameData_Stage.xlsx](C:/Github/SimpleGame/GameData/GameData_Stage.xlsx) | `StageSpawn!A1:H4` | 시트 안의 난이도 열, 정수/실수 메타, 단위 설명 |

나머지 구조는 bundled Python의 ZIP/XML 읽기로 확인했다. 원본 Excel 저장·변환은 실행하지 않았다. 아래 규칙은 조사 결과이며 Unity importer 실행 성공이나 게임 동작 검증 결과가 아니다.

## 2. 파일과 시트 관례

**관찰**

- 파일은 `GameData_<주제>.xlsx`로 구분된다. 확인한 주제는 Balance, Character, Economy, Endgame, Enemy, Lobby, Map, Player, Skill, Stage, String이다. 이 목록은 StageGame의 필요 파일 목록이 아니다.
- 시트는 책임별 영문 이름을 쓴다. 일반 수치, 문자열, 보고/설명 시트가 한 통합 문서에 공존한다.
- `_Info`, `_SheetCatalog`, `_ColumnCatalog`, `_Validation`, `_DomainPlan`, `README` 같은 설명 시트가 일부 파일에 있다. 보고 시트에는 수식이나 병합 셀이 있다.
- 공통 parser는 등록된 시트명을 명시적으로 읽는다. 접두사 `_`나 범위 `None`만으로 자동 제외되는 범용 시트 탐색기가 아니다.
- 원본 Excel은 프로젝트 루트 `GameData/`, Unity 생성 결과는 SimpleGame의 `Assets/Game/Data/Generated/`에 있다. 런타임은 manifest를 통해 생성 데이터를 소비한다.

**StageGame 기준안**

- 최소한의 주제별 파일과 실제 사용하는 시트만 만든다. 게임 확정 전에 참고 게임의 파일 구성이나 빈 시트를 미리 복제하지 않는다.
- 파일명은 같은 `GameData_<주제>.xlsx` 규칙을 사용한다. MVP는 `GameData_MVP.xlsx`의 `Stages`, `AdConfig`, `Strings`, `Multiplayer` 네 시트를 허용 목록으로 관리한다.
- 문자열·밸런스 입력·검토용 계산을 구분하되, 용도가 없으면 설명/보고 탭을 추가하지 않는다.
- 클라이언트의 Excel 경로를 SimpleGame 기준으로 하드코딩하지 않는다. StageGame은 Unity 프로젝트가 `Client/` 아래에 있으므로 프로젝트 루트와 원본 경로의 기준점을 계약에 명시해야 한다.
- Excel parser는 개발 도구에 둔다. Android/WebGL 런타임은 `.xlsx`나 Excel 라이브러리에 의존하지 않는다. `GameData/Generated/`에 UTF-8 JSON을 만들고 클라이언트 담당이 Resources로 인계한다.

## 3. 공통 4행 메타 포맷

| Excel 행 | 역할 | 규칙 |
| ---: | --- | --- |
| 1 | 기계 필드명 | 영문 PascalCase 기본, 중복 금지. `/Description`, `Content_Kor` 등 참조 형식의 예외는 명시적으로 허용 |
| 2 | 자료형 | 공통 지원값 `string`, `int`, `float`, `bool`, `none` |
| 3 | 추출 범위 | `All`, `Client`, `Server`, `None` |
| 4 | 한글 설명 | 모든 이름 있는 열에 기재. 단위·허용 범위·빈값 의미 포함 |
| 5 이후 | 데이터 | 한 행이 하나의 레코드. 빈 행과 비활성 행의 처리는 아래 규칙 적용 |

**관찰**: 공통 `ExcelTable`은 첫 번째 비어 있지 않은 행을 헤더로 삼는다. 뒤의 3행이 허용 타입·범위·설명을 모두 갖추면 메타행으로 건너뛴다. 그렇지 않으면 구형 단일 헤더 표로 처리한다. 헤더가 빈 열은 메타 인식 검사에서 제외하며, 완전히 빈 데이터 행도 건너뛴다. 시트명·열명 비교는 대소문자를 구분하지 않는 곳이 있다.

**StageGame 기준안**: 신규 데이터 표는 A1부터 위의 4행을 고정한다. 잘못된 타입/범위/빈 설명을 구형 형식으로 조용히 해석하지 않고 오류로 보고한다. 작성 시 대소문자와 필드명을 계약과 일치시키고, 중간 빈 열·병합 셀·제목행 삽입을 피한다. 데이터가 없는 필수 시트는 오류다. 선택 시트의 빈값 허용은 스키마에서 명시한다.

## 4. 타입과 값

| 타입/값 | 참조 구현의 관찰 | StageGame 작성·검증 기준안 |
| --- | --- | --- |
| `string` | 문자열을 읽고 앞뒤 공백을 제거함. 필수값 검사는 도메인 parser가 수행 | ID는 텍스트 셀. 필수/선택 여부를 필드별로 선언. 앞뒤 공백에 의미를 부여하지 않음 |
| `int` | 공통 파서는 InvariantCulture로 읽고 유한값·정수성·Int32 범위를 검사 | Excel 숫자로 저장. 소수 입력 거부. 양수/0 이상/최댓값은 필드별 정의 |
| `float` | 유한한 단정도 숫자만 허용. 범위 검사 함수 별도 | Excel 숫자로 저장. NaN/무한대 거부. 초·배율·확률 단위 명시 |
| `bool` | `TRUE/FALSE`, `true/false`, `1/0` 허용 | Excel 논리값 `TRUE/FALSE` 사용 권장. 허용 입력을 검사기에 명시 |
| `none` | 메타 타입으로 인식. 자체적으로 열을 제거하지는 않음 | 기획 설명/메모 열. 생성 데이터에 넣지 않음 |
| enum | 메타는 보통 `string`, C# enum parser가 값과 정의 여부 검사 | 언어 중립 이름 사용. 허용 목록과 미지원 값 오류를 명시 |
| 확률 | 공통 `Rate` 검사 범위는 0~1 | 10%의 원본값은 0.1. 표시 형식만 백분율로 지정 |
| 목록/관계 | 일부 도메인에 `|` 구분 문자열 처리 존재 | 범용 배열 타입으로 간주하지 않음. 관계 시트 필요 여부를 게임 확정 후 결정 |

메타 타입만 바꾸면 런타임 타입이 자동 생성되는 구조는 아니다. 공통 메타 인식과 도메인별 실제 값 파싱을 모두 구현해야 한다. 빈값을 임의로 0이나 기본 콘텐츠로 바꾸지 않는다. 0이 유효한 값인지 별도로 정의한다.

**예외**: [GameData_Endgame.xlsx](C:/Github/SimpleGame/GameData/GameData_Endgame.xlsx)의 `TowerCheckpoint!C2`, `TowerReward!C2`, `EndgameBalance!C2`는 `long`이다. 이 파일의 별도 importer는 공통 `ExcelTable`을 통하지 않고 5행부터 열 위치로 읽는다. 따라서 `long`은 공통 지원 타입이라는 근거가 되지 않는다. StageGame은 필요가 확정되기 전 추가하지 않는다.

## 5. 추출과 ID

**관찰**

- `ExportType` 열이 있으면 공통 `ExcelTable`은 `USE` 행만 포함하고 `NONE` 행을 제외한다. 빈값·알 수 없는 값은 오류다. 대소문자는 무시한다. 이 열이 없는 기존 수치 시트도 존재한다.
- `All/Client/Server/None`은 공통 메타 인식에서 허용된다. 그러나 읽은 공통 parser에는 대상 플랫폼에 따른 범용 열 필터가 없고, 도메인 코드가 필요한 열을 선택한다. `Server`를 적었다고 클라이언트 제외가 보장되지 않는다.
- `Id`는 관리용 숫자, 의미 ID와 `SearchKey`는 참조용 문자열로 쓰인다. 숫자 관리 ID가 없는 표도 있다. 의미 ID는 표시 문구나 행 번호와 분리된다.
- 일반 식별자 검사에는 `^[A-Za-z][A-Za-z0-9_]*$`가 사용된다. 키 중복과 다른 표 참조는 도메인별로 검사한다.

**StageGame 기준안**

- 신규 콘텐츠 표는 첫 열 `ExportType`을 두고 `USE/NONE`을 명시한다. 공통 도구가 행 필터를 지원하는 시점에 함께 도입한다. `Enabled` 같은 게임 내 조건은 추출 상태와 별개다.
- MVP 클라이언트 생성기는 `All`과 `Client`의 지정 데이터 필드를 포함하고, `None` 및 `none` 메모를 제외한다. 시트별 메타 스키마를 정확히 검사하므로 `Server`나 임의의 범위 변경은 오류다. 범용 서버 exporter는 구현하지 않았다.
- 의미 ID는 안정적으로 유지하고 삭제된 ID를 재사용하지 않는다. 표시 이름을 키로 쓰지 않는다. 숫자 `Id`는 이력 관리가 필요할 때만 추가한다.
- 활성 레코드의 참조 대상도 활성 출력에 존재해야 한다. 단일키·복합키 구성, 정렬 순서, 필수 참조, 대소문자 정책은 스키마에 적는다. `NONE` 이력도 ID를 재사용하지 않도록 작성 시 확인한다.
- Unity Prefab/Sprite/AudioClip은 별도 카탈로그에서 ID와 연결한다. Excel에 비밀키, 인증정보, Unity 계층이나 시각 배치값을 넣지 않는다.

## 6. 문자열 관례

참조의 지원 시트는 `GameString`, `GameMessage`, `CodexString`, `ErrorMessage`, `TutorialString`이다. 도메인 확장에는 별도 문자열 시트도 있다. StageGame은 실제 콘텐츠가 필요한 시트만 만든다.

- 대표 필드: `ExportType`, `Id`, `/Description`, `Category`, `SearchKey`, `Content_Kor`, `Content_Eng`, `FormatArgCount`, `Note`.
- 구형 별칭 `StringId/KoKR/EnUS`도 참조 parser가 읽지만, StageGame 신규 파일은 신형 이름만 사용한다.
- 문자열 키는 병합하는 모든 문자열 시트에서 유일해야 한다. 한국어·영어 필수값, 같은 포맷 인덱스 집합을 검사한다.
- 참조의 `FormatArgCount`는 등장 횟수가 아니라 가장 큰 인덱스 + 1이며 인자가 없으면 0이다. 동일 `{0}`가 반복되면 1이다.
- 런타임 계약이 한국어 전용을 선택하면 언어 열의 필수 여부를 별도로 확정한다. 공란 번역을 자동으로 완성된 번역처럼 처리하지 않는다.

## 7. import와 재현성 준비

참조 Reader는 요청한 시트의 셀을 읽을 때 수식과 Excel 오류 셀을 거부한다. `NONE` 행이나 메모 열도 Reader를 먼저 통과하므로 그 안에 수식을 넣어도 안전하게 제외된다고 가정할 수 없다. 보고 수식은 importer가 읽지 않는 별도 시트에 둔다. Excel Table 객체를 쓰면 표 범위·헤더명·열 개수·고유 ID/이름까지 일치해야 한다. 기존 원본의 보고 시트 수식이 존재한다는 사실은 런타임 수식 허용을 뜻하지 않는다.

현재와 후속 작업의 경계는 다음과 같다.

1. 총괄이 확정한 JSON v1 계약을 구현했다. 구체적인 필드 매핑은 9절을 따른다.
2. 30개 국면과 한국어 문구를 새로 작성하고 bundled Artifact Tool로 Excel을 제작했다. SimpleGame 콘텐츠나 코드를 복사하지 않았다.
3. 원본 읽기 → 메타/타입 검사 → 행 필터 → 키·참조·게임 조건 검사 → 네 JSON과 이전 파일 백업 준비 → 파일별 교체 순서다. 검증 실패 시 생성물에 쓰지 않고 교체 예외 시 기존 묶음을 복구한다. 복구 실패는 `.export-incomplete`와 백업을 남겨 `--check`/재export를 거부한다. 디스크 장애/프로세스 강제 중단까지 네 파일 전체의 원자성을 보장하는 트랜잭션은 아니다.
4. 생성물은 수동 편집하지 않는다. 같은 원본·도구에서 동일 바이트를 생성하며 단계 ID와 문자열 키로 정렬한다. 시간값을 삽입하지 않는다.
5. `schemaVersion=1`을 네 wrapper에 기록한다. 원본 SHA-256은 README의 검증 기록에 남긴다. 별도 contentVersion 필드는 계약에 없으므로 추가하지 않았다. 로컬 저장 버전은 클라이언트 담당이 별도로 관리한다.
6. 클라이언트 담당이 생성 데이터 로드와 플레이 반영을 검증한다. 데이터 담당은 `Client/`를 수정하지 않는다.

제작할 때 적용할 최소 검증 항목:

| 검사 | 기대 결과 |
| --- | --- |
| 메타행 누락/알 수 없는 타입·범위/중복 열 | 파일·시트·행·필드 위치를 포함한 오류 |
| 수식/오류 셀, 소수 정수, 범위 밖 값, 필수 빈값 | 생성 중단, 정상 생성물 보존 |
| `USE/NONE`, 빈 ExportType, 활성 데이터가 0건인 필수 표 | 명시된 추출 규칙 적용 또는 오류 |
| 중복 ID/복합키, 없는 참조, 제외된 행 참조 | 오류. 행 순서를 바꿔도 의미가 보존됨 |
| 한국어·영어 필수값/포맷 인덱스/키 중복 | 누락 또는 불일치 오류 |
| 두 번 생성, 동일 원본 재import | 데이터·순서·참조가 동일함 |
| 잘못된 Unity 카탈로그 참조 또는 반영 실패 | 기존 데이터 보존 확인 |
| 모든 제작 시트 렌더와 최종 XLSX 재열기 | 한글·메타행·숫자 가독성, 잘림·의도치 않은 변형 없음 |

위 목록은 일반 형식의 검증 기준이다. MVP는 한국어·영어를 지원하며 Unity 에셋 importer는 없다. 실제 실행한 45개 Python 테스트와 렌더 검증은 README에 구분하여 기록했다. 참조 테스트의 존재는 [파서 테스트](C:/Github/SimpleGame/Assets/Game/Tests/EditMode/GameDataExcelImporterTests.cs:2698)와 [참조 실패 보존 테스트](C:/Github/SimpleGame/Assets/Game/Tests/EditMode/GameDataImportTransactionTests.cs:14)에서 확인했으나 SimpleGame 테스트는 실행하지 않았다.

## 8. 문서와 코드의 차이 및 남은 결정

구조 안내의 일부 항목에는 ExportType이 미지원이고 모든 범위가 All이라고 적혀 있다. 현재 Excel과 공통 코드는 행 필터 및 네 가지 범위의 메타 인식을 포함한다. 반대로 범위 메타 인식을 범용 열 추출 기능과 동일시해서도 안 된다. 별도 Endgame의 `long`과 위치 기반 파싱도 공통 규칙과 분리했다.

후속으로 확인할 항목:

1. AI 깊이·노드 예산·실수 비율의 기기별 실제 성능과 난이도, 플레이 시간 및 학습 효과.
2. 클라이언트 복사/로딩 및 `Docs/Management/CONTRACT.md`와의 통합 확인.
3. 각 국면의 최적 플레이 결과, 실제 퍼즐 해답, 언어별 UI 표시, 콘텐츠 업데이트 시 기존 저장 처리.

## 9. 현재 MVP 원본과 JSON v1 매핑

원본은 `GameData_MVP.xlsx` 하나다. 함께 검증해야 하는 30스테이지·광고 설정·문자열·멀티 설정을 네 시트로 묶어 파일 간 버전 불일치를 줄였다. 한국어·영어를 지원하며 수식과 Excel Table 객체를 사용하지 않는다. 메타행은 위의 4행, 활성값은 대문자 `USE/NONE`, 모든 셀은 고정값이다.

| Stages 열 | JSON 필드 | 필수/제약 |
| --- | --- | --- |
| `ExportType` | 출력 안 함 | `USE/NONE`; 최종 USE는 정확히 30행 |
| `Id` | `id` | 1~30 고유, 숫자 셀 |
| `Chapter` | `chapter` | 1~10=1, 11~20=2, 21~30=3 |
| `TitleKey` | `titleKey` | 활성 Strings의 SearchKey 참조 |
| `AiDepth` | `aiDepth` | 정수 1~6 |
| `AiNodeBudget` | `aiNodeBudget` | 정수 100~30000 |
| `AiRandomPermille` | `aiRandomPermille` | 정수 0~1000, 250은 25% |
| `Starter` | `starter` | 1 또는 -1; openingMoves 첫 수의 주체 |
| `OpeningMoves` | `openingMoves` | 빈 셀은 빈 문자열. 그 외 `0,3,2`처럼 열 0~6을 쉼표로 연결 |
| `LearningGoal` | 출력 안 함 | 필수 학습 의도, `none/None` |
| `VerificationNote` | 출력 안 함 | 현재 `강제승리 미검증`, `none/None` |
| `RankKey` | `rankKey` | `story_opponent_001`–`story_opponent_030`, 계급/보직·이름·대회 라운드 |
| `DescriptionKey` | `descriptionKey` | `story_stage_desc_001`–`story_stage_desc_030`, 해당 단계 설명 |
| `DifficultyStars` | `difficultyStars` | float 1–8, 반 개 단위. 1–2는 1, 3–4는 1.5, …, 29–30은 8 |
| `AiIntroKey` | `aiIntroKey` | 해당 단계의 `story_NNN_intro`, 1단계 예외 아래 참조 |
| `AiWinKey` | `aiWinKey` | 해당 단계의 `story_NNN_win`, AI 승리/플레이어 패배 |
| `AiLoseKey` | `aiLoseKey` | 해당 단계의 `story_NNN_lose`, 플레이어 승리/AI 패배 |
| `CharacterId` | `characterId` | `C01`–`C19`, 24+6 배치에 맞는 캐릭터 ID |
| `AiRating` | `aiRating` | int/All, S열. 내부 실력 구간 200~1600, 100 단위. 실제 사람 Elo가 아님 |
| `AiOversightPermille` | `aiOversightPermille` | int/All, T열. 전술 검사 전 합법 수를 무작위로 고를 확률 0~1000‰ |

사용자 지정 서사 예외: **ID 1만** `AiIntroKey=story_first_intro`, `AiWinKey=story_first_win`, `AiLoseKey=story_first_lose`다. 다른 단계에 이 키를 넣거나 역할을 뒤바꾸면 exporter가 거부한다. 문자열 값에는 강제 개행이나 인자를 넣지 않으며 대사 UI의 자연 줄바꿈은 허용한다. 계급·보직은 도전자 표시이며 승리로 플레이어가 진급하지 않는다.

계급 UI 6개 필드는 L:Q, 캐릭터 ID는 R열이다. 숫자 별은 `float/Client`, 나머지는 `string/Client`다. 문자열 참조 5개는 활성 Strings에 존재하고 해당 단계/대사 역할과 일치해야 한다. 값에 개행·포맷 인자를 넣지 않는다. 기존 titleKey와 수순은 보존하며 AI 수치는 레이팅 계약을 따른다. 현재 상대 표시는 rankKey를 쓴다. 계급/별은 사람 난이도 검증 결과가 아니다. [필드·키·인계 계약](TOURNAMENT-CONTRACT.md).

중간 국면은 시작측부터 교대로 매 수 재생한다. 만열, 범위 밖 열, 잘못된 구분 문법, 재생 도중의 승리, 42수 무승부를 모두 거부한다. 매 합법 수가 상대의 다음 즉승을 허용하는 국면도 거부한다. 깊은 수읽기의 패배는 별도 `audit_openings.py`와 QA에서 검사한다. 현재 5·8·11·14·18·22·26·30단계는 AI 선공이다. 시작 수순과 선공은 레이팅 변경에서 보존했다.

`AdConfig`는 USE 한 행이며 2026-10-18 사용자 확정값을 엄격히 검사한다. 필드는 `InterstitialGraceMatches=0`, `InterstitialEveryMatches=1`, `InterstitialCooldownSeconds=0`, `BannerLobby=TRUE`, `BannerResult=TRUE`, `RewardedHintsPerMatch=1`이다. JSON은 첫 글자만 소문자로 바꾼 동명 필드를 사용하고 schemaVersion 1을 유지한다. 스토리는 정상 완료 매판이 전면 광고 대상이며, 멀티는 정상 대국 이후 방에서 나가 로비로 돌아가는 경계에서 최대 1회만 대상이다. 멀티의 경기 수 누적·방 퇴장 경계·중복 방지는 런타임에서 판정한다. 6판/180초 제안은 사용하지 않는다. 값 변경은 총괄 계약과 validator를 함께 갱신한다. 이 값은 게임의 노출 기준이며 플랫폼 정책이나 실제 광고 송출 완료를 뜻하지 않는다.

`Strings`는 `ExportType`, `SearchKey`, `Content_Kor`, `/Description`, `Content_Eng` 순서다. 마지막 영어 열을 추가하여 기존 열 위치를 보존했다. `SearchKey→key`, `Content_Kor→ko`, `Content_Eng→en`을 생성하며 두 언어 모두 필수다. 영어 메타는 `string/Client`, 설명행은 `영어 표시 문구`다. 키는 전역 고유 ASCII 식별자다.

포맷은 C# `string.Format`의 제한된 부분집합 `{0}`, `{1}` 등만 허용한다. 두 언어의 인자 인덱스 집합이 같고 0부터 연속이어야 한다. 인자 순서 변경·반복은 허용하고 `{{`/`}}`는 리터럴 중괄호다. 이름 인자, 정렬/형식 지정자, 불완전 중괄호를 거부한다. 숫자 자릿수 등은 호출자가 미리 형식화한다. 예: `챕터 {0} · 스테이지 {1}` / `CHAPTER {0} · STAGE {1}`.

wrapper/schemaVersion 1과 ko/en을 유지한다. 2026-10-19 현재 기존 502개에 멀티 결과·레이팅·착수 확인 7개를 더해 총 509개다. `mp_rating_result`의 `{0}`은 현재 레이팅, `{1}`은 부호를 포함한 변화량이며 양 언어 모두 `Rating {0}  ({1})`를 사용한다. `mp_rating_unchanged`는 레이팅 변화가 없는 경기를 표시하고 `{0}`은 현재 레이팅이다. 구매 가격은 `ad_offer_buy`의 `{0}`에 플랫폼에서 받은 표시 가격을 넣으며 Excel에 가격을 고정하지 않는다. 닫기는 기존 `ui_close`를 재사용하고 구매 취소는 별도 오류 문구 없이 돌아온다. 기존 `stage_tier_1/2/3`과 챕터용 문구는 구버전 호환을 위해 보존하며 새 화면은 `rankKey`와 계급 UI 키를 사용한다. 언어 코드는 `ko/en`, 최초·잘못된 저장값의 기본은 ko다. 언어 선택/저장·UI 갱신은 클라이언트가 구현한다. [현재 연결 계약](TOURNAMENT-CONTRACT.md).

생성물은 `Generated/stages.json`, `ads.json`, `strings.json`이며 각 최상위는 `schemaVersion:1`을 갖는 객체다. stages/strings는 객체 배열이다. 스키마 밖의 열·시트·타입·범위는 오류로 보고한다. `float`는 DifficultyStars에 사용하고 `long`, enum, 목록 메타는 사용하지 않는다. 범용 형식에서 허용될 수 있는 타입과 이번 MVP exporter의 지원 범위를 구분한다.

## 10. 멀티 설정 추가 (2026-10-18)

허용 시트는 `Stages`, `AdConfig`, `Strings`, `Multiplayer` 네 개이며 출력도 stages/ads/strings/multiplayer.json 네 개다. 기존 클라이언트 JSON 세 개는 기존 Resources로 인계하고, multiplayer.json은 별도 Worker가 import한다. 범용 Server 열 필터를 추가한 것이 아니라 시트별 고정 스키마와 소비자를 확장했다.

`Multiplayer` 열은 `ExportType:string:All`, `Key:string:All`, `Value:int:All`, `/Description:none:None`이다. 4행 메타와 USE/NONE 규칙은 동일하다. 16개 필수 키의 중복·누락·알 수 없는 키·정수 범위·검색 초기 범위≤최대 범위·계급 문자열 20개를 검사한다. 시작200/최저0/계급간격100/계급수20은 사용자 계약으로 고정한다. Elo·매칭·시간 제한 값은 [멀티 계약](../Docs/Management/MULTIPLAYER-CONTRACT.md)을 따른다. 레이팅과 점수 반영 여부는 서버가 결정한다. 클라이언트의 보낸 점수나 결과를 신뢰하지 않는다.

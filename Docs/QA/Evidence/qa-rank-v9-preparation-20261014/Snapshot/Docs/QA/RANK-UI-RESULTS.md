# 한 수 진급 UI·입력·무르기 QA

기준일: 2026-10-14 KST. **v8 후보 실제소스 독립 실행 완료: 서로 다른25묶음 중24통과·QA-014 한 건 실패. v9 추가 UI/확인 흐름은 영향 검사 준비 중이다.** v8 전체 실행24묶음(23통과/1실패)과 후속 영향9묶음(8통과/1실패)은 중복을 포함하며 단일25/25 실행이 아니다. [통합 범위](Evidence/qa-rank-runtime-impact-v3-20261014/coverage-rollup.json). 228키 데이터 대조는 유지하며 기존 Tabletop 증거를 새 디자인 합격으로 사용하지 않는다. 아래 초기 준비 기록과 미실행 표시는 해당 시점의 이력이고 최신 실행/제한은 마지막 절을 따른다.

## 변경된 기준

- 중력식7×6 4목을 유지한다. 흰 보드·검정 격자·플레이어 흑돌/AI 백돌(검정 외곽선), 중앙 제목 ‘한 수 진급 / Rank by Rank’를 확인한다. 이전 크림/청록·민트/산호 sprite 기대값은 폐기한다.
- 큰2열30단계 카드와10계급×3단계, 원형 AI 얼굴·계급별 intro/win/loss 대사,6단계마다1~5별을 확인한다. 단계 선택은 우측 슬라이드 preview, 한 줄 설명, 좌우 삼각, X, 시작 버튼으로 이어진다.
- 멀티 대국은 ‘준비 중’ 표시만 제공한다. 온라인 매칭·방 생성·계정·네트워크 대국을 추가하지 않는다.
- 신규 설치의 첫 실행만1단계 자동 진입하고 기존 저장을 보존한다. 열 선택은 최하 빈칸 preview만 바꾸며 Place를 눌러야 착수한다. 하단 Undo/Place/Retry를 제공한다.
- 무료 Undo는 응수 전 내 수1개, 응수 후 내 수+AI 수2개를 되돌리고 opening 이전으로 내려가지 않는다. 종료 연출/결과에서는 Undo를 금지한다. AI 진행 중 취소·늦은 응수 경합을 검사한다.
- 결과 Next를 복구하되 마지막30에서는 숨긴다. Retry/Lobby 접근을 유지한다. 이전 ‘결과 정확히2개’ 제한은 폐기하며 마지막 수 확대·보드가 보이는 결과는 유지한다.

## 보존하는 이전 증거

이전 동결 소스/프리팹/아트는 [graphics-frozen-v1](Evidence/qa-graphics-frozen-v1-20261014/source-manifest.json), 최종 Editor 보호 조건은 [build-link-v2](Evidence/qa-graphics-build-link-v2-20261014/source-manifest.json), 실제 포인터는 [browser-final](Evidence/qa-graphics-browser-final-20261014/README.md)에 그대로 보존한다. 이를 변경 중인 현재 Client 파일로 대체하지 않는다.

이전 QA-011/012 종결은 당시 버전에만 해당한다. QA-013 닫기 누락 후보는 **시안 폐기로 후속 판정 중단·미확정 보존**하며 수정 완료로 바꾸지 않는다. 새 preview/도움말/설정/일시정지의 닫기 표시와 실제 클릭을 신규 검증에 포함한다.

## 검증 행렬

| ID | 확인할 내용과 기대 결과 |
| --- | --- |
| RUI01 | 저장된 Scene/Prefab의 중앙 제목·흰 보드/검정 격자·흑백 돌·AI 원형 얼굴·큰2열 카드·preview/하단/결과 컨트롤과 sprite/glyph 참조가 연결된다. 사용자 자산 보존·런타임 UI 트리 조립 금지. 파일 존재만으로 시각 완성을 판정하지 않는다. |
| RUI02 |30카드의 번호/계급/별/완료·잠금·선택 상태와 실제 시작 대상이 일치한다. 마지막 행/30카드까지 스크롤 가능하고 잠금 단계를 preview해도 시작 우회가 없어야 한다. 순번 기준 계급은3단계씩10개, 별은6단계씩1~5이며 최종 Excel 계약과 대조한다. |
| RUI03 | 카드 선택→우측 슬라이드 preview→실제 opening 시작판/한 줄 설명/얼굴/별/좌우 삼각/X/시작하기를 확인한다.1/30·잠금 경계, 전환 중 연타와 오래된 listener를 검사한다. 열기/열림/닫기 중 모달 아래 카드·설정·도움말·멀티 입력 차단과 닫은 뒤 목록 스크롤 위치 보존, 실제 닫기 표시/클릭·입력 복원을 확인한다. |
| RUI04 | 저장키가 없는 최초 실행은1단계 자동 진입1회, 이후 로비 진입. 저장키가 있으면 진행1단계·첫 실행 flag 누락이어도 기존 이용자로 처리한다. 첫 판 중 종료/로비/재실행·저장 실패 정책을 확인하고 해금·실제완료·언어·음향·경기/광고카운터·중복ID·복구 사본을 보존한다. |
| RUI05 | 열 선택/변경은 보드·턴·경기ID·저장·광고에 영향을 주지 않는다. preview는 최하 빈칸과 일치하며 정식 돌/승리 판정에 포함되지 않는다. 꽉 찬 열 선택은 기존 preview 제거+Place 비활성화다. 선택 없음·AI 턴·중단/결과의 Place를 거부하고 합법 Place만1회 착수한다. |
| RUI06 | Place 연타/다른 열 연속 선택/키보드 입력/버튼 이벤트 중복에도 플레이어1수와 AI1응수만 발생한다. 정상 착수 시 선택/preview 처리 정책을 따르고 AI 응수 후 preview를 새 보드 기준으로 갱신한다. |
| RUI07 | AI 응수 완료 뒤 Undo는 내 수+AI 수를 되돌려 동일한 플레이어 턴의 직전 보드/수순/LastMove/승리표시/열 높이로 복원한다. 여러 번 Undo해도 opening 이전으로 내려가지 않고 무료이며 광고/경기완료/해금/결제 횟수를 늘리지 않는다. |
| RUI08 | AI 대기/탐색 중 Undo는 아직 둔 내 수만 되돌리고 AI 작업을 취소한다. AI 완료와 Undo 순서가 경합해도 결과는 직전 플레이어 턴의 보드이며 늦은 응수·이중 검색·새 판 오염이 없어야 한다. Retry/로비/다른 단계/앱 pause/언어 변경과의 경합도 분리해 확인한다. |
| RUI09 | Undo 뒤 다른 열 Place·원래 열 재착수·여러 pair 반복, AI 시작/opening 국면을 확인한다. history와 opening을 구분하고 preview/버튼활성·AI seed 정책과 대조한다. 사용한 힌트 권한은 계속 소비 상태이고 추천 표시만 지운다. 종료 연출/결과 Undo를 거부해 저장된 완료기록을 뒤집거나 중복 기록하지 않아야 한다. |
| RUI10 | 하단 Retry는 같은 단계의 opening으로 복원하고 오래된 AI/연출/광고 callback을 끊는다. 아직 끝나지 않은 판을 재시작하는 것만으로 완료 경기/해금이 증가하지 않아야 한다. 완료 결과의 Retry는 새 경기ID로 시작한다. |
| RUI11 | 양측 마지막 수 확대→승리 표시→광고 경계→결과와 보드 노출을 유지한다. 승리의 Next로 다음 해금 단계에1회 이동하고 마지막30에서는 숨긴다. 패배/무승부는 Retry/Lobby 두 가로 버튼이며 결과 Close는 없다. 종료 처리 동안 하단 Undo/Place/Retry·보드/힌트/pause 차단, NoFill/실패/timeout·늦은 callback·결과1회 저장을 확인한다. |
| RUI12 | 각 계급의 intro/win/loss 대사와 얼굴·별이 현재 AI/단계와 일치한다. aiWinKey는 AI 승리=플레이어 패배, aiLoseKey는 반대이며 한영 전환/reload에도 맞는다. 새 rankKey/descriptionKey/difficultyStars/aiIntroKey/aiWinKey/aiLoseKey와 확정228키(기존90보존·47승인변경·91신규)를 사용한다. 참조 누락/공란/미존재·별0/6/구간 오류의 runtime 거부와 포맷/하드코딩을 검사한다. 기존 opening/AI 설정 보존의 JSON 대조는 완료, UI 출력은 미검증이다. |
| RUI13 | 멀티 버튼은 준비 중 안내만 표시하고 실제 판 생성·매칭/네트워크·계정·진행 변경이 없다. 설정·도움말·라이선스·음향·언어는 유지한다. 현재 광고는 NoFill이며 실제 수익화 완료로 표현하지 않는다. |
| RUI14 |320×568/390×844, OS 위59/아래34와 WebGL 제품 호스트64 조건에서 카드/preview/하단3버튼/결과/닫기 hitbox44px·글자·스크롤·검정/흰돌 식별성을 확인한다.390↔320, preview 전환/얼굴 mask/한 줄 설명/별/모달 닫기의 실제 렌더와 포인터를 검토한다. |
| RUI15 | Client native와 총괄 브라우저 증거를 최종 소스/데이터/아트/빌드 해시에 연결한다. AI 경합·Undo는 독립 상태 및 실제 코루틴으로, 슬라이드·확대·sprite/레이캐스트는 native/실제 포인터로 구분한다. 실제 폰0개·Toss SDK/광고/IAP/서버 미연결을 유지한다. |
| RUI16 | 한 수로 두 직선 승리를 만드는 양측·좌우반전4수순에서 표시선이 마지막 착수칸을 포함한 같은 승자 돌의 수평/수직/45도 대각 연속4개 이상이어야 한다. 전체 승리칸 합집합의 최장 두 점이 다른 선을 가로지르는 오류를 거부한다. 독립 끝점 판정과 native 표시선/확대 캡처를 구분한다. |

## 독립 harness 준비

[입력 fixture](../../Tests/Fixtures/rank-ui-interaction-cases.json)는 **신규 제품 실행 전 입력/기대 상태**다. 열 번호는0부터, row0은 바닥이다. 새 runtime 필드명을 강제하지 않는 의미 기반 기대값이며 source API 확인 후 기존 실제소스 harness에 연결한다. `turnPairs`의 AI 열은 deterministic 입력 벡터로, 실제 AI가 그 열을 고른다고 주장하지 않는다.

실제 새 활성 대상은 **RankGameApp / RankTheme / RankStageCard**이며 이전 GameApp/프리팹은 보존된다. [RankHarness 준비 폴더](../../Tests/RankHarness/README.md)를 분리했다. 기존 whole-board oracle·수동 시계/중첩 coroutine·지연 광고 callback을 재사용하되 새 UI 실제 필드/메서드와 별도 실행 진입점에 연결한다. 동결 전 객체/저장 API를 추측해 만들거나 이전 Tabletop `MakeApp`·Mint/theme·두 버튼/137키 검사를 새 제품에 실행하지 않는다.

검사 우선순위는①preview 비파괴/Place1회 ②opening 경계와 pair Undo ③AI 취소/늦은 응수 ④첫 실행/기존 저장 ⑤단계/결과/광고/한영 ⑥직렬화·native/브라우저다. 보드/AI 코드가 같으면 이전1,000국면 탐색을 반복하지 않는다. Undo가 보드 코어를 바꾸면 합법 prefix 되감기/재생 oracle 비교 및 영향받은 규칙 검사를 추가한다.

## Client·데이터 동결 시 필요한 공유 자료

1. 새 RankGameApp/RankTheme/RankStageCard/Core/진행 저장의 파일 목록과 최종 해시, 실제 활성 scene/root prefab 식별자. 선택열/preview/Place/Undo/Retry·AI 취소 generation/token·history/opening 경계·종료 상태·승리선 두 끝점 또는 그 화면 좌표의 실제 필드/메서드/접근 경로.
2. **최초 실행 구현:** 저장키 없음=신규, 기존 저장/새 flag 누락=기존은 확정됐다. 소비 marker 저장 시점·실제 필드, 저장 실패/손상 복구, saveVersion/key 변경 여부와 native raw 저장 migration XML을 제공한다.
3. **Undo 구현:** 응수 전1수/후2수·opening floor·종료 연출/결과 금지·사용 완료된 힌트 권한을 되돌리지 않고 추천 표시만 지우기는 확정됐다. history/취소 identity, AI 시작 opening, 늦은 힌트 callback, AI 재선택 seed, 앱 pause 경합의 실제 구현을 제공한다.
4. **선택/결과 구현:** full열 선택→preview 제거/Place 비활성, 승리 Next(30 제외), 패배/무승부 두 버튼, Lobby 최고 해금 선택은 확정됐다. 선택 없음/Undo 후 preview 상태와 각 활성 guard·실제 API, preview 모달 배경 차단/스크롤 보존 경로를 제공한다.
5. **데이터 인계 수신/정적 대조 완료:** [RANK-UI-CONTRACT](../../GameData/RANK-UI-CONTRACT.md),228키,47승인변경 목록,Generated/Client3JSON 일치,30단계 기존 필드 보존. 데이터 담당42테스트는 제공 결과이며 QA가 재실행한 결과와 구분한다. 얼굴 매핑·실제 runtime 거부/대사 출력은 Client 동결 후 연결한다.
6. 저장된 Main/AppRoot·카드/preview/보드/AI 얼굴/하단/결과/공통닫기 prefab, 재적용 비파괴 로그·참조 검증,320/390 한영 캡처·UI hitbox, 우측 슬라이드/마지막 수/Undo·AI 취소 native 실행 XML·타임라인.
7. 마지막으로 두 개발 빌드/고지/소스 해시와 총괄 실제 포인터·콘솔 보고. 오래된 Tabletop 이미지/빌드를 새 시안 증거로 혼합하지 않는다.

이 자료 요청은 공유 문서로 남긴 구현 연결 조건이며 사용자 승인 요청이 아니다. QA가 Client·Excel·Unity·브라우저·커밋/푸시를 대신 조작하지 않는다. 초기 준비 당시 판정 전 상태와 후속 실제 런타임 결과를 구분한다.

## 준비 검증 결과

[준비 기록](Evidence/qa-rank-ui-preparation-20261014/preparation-results.json)에 보드13·생명주기5·모달2, 총20입력 사례와 기존 harness/계약 스냅샷을 보존했다. JSON·중복ID·기존 저장 fixture 참조와 기대 수순15개의 합법성/opening prefix만 검사했다. 이는 입력 자료의 일관성 확인이며 제품20검사 통과가 아니다. runner는 신규 fixture를 AuditInputs에 보존하도록 준비했지만 실제 제품 API 바인딩/새 테스트 실행은 Client·데이터 동결 후에 한다. Unity·브라우저·기기 실행은 없었다.

## 초기 소스 검토에서 추가한 경계 — 제품 검사 미실행

총괄이 작업 중 소스를 검토해 Client에 전달한 사항을 아래에 반영했다. QA가 초기 RankGameApp을 읽었지만 아직 동결되지 않았으므로 현재 미완성 코드를 제품 실패로 실행/등록하지 않는다.

- **복수 방향 동시 승리선:** WinningCells의 모든 점 중 가장 먼 두 점은 서로 다른 승리선을 가로지를 수 있다. 예시 열 수순 `[1,0,6,2,2,1,1,0,4,0,0,1,1,6,5,5,3]`은 마지막3열 착수에서 바닥 수평 `[3,4,5,6]`과 대각 `[21,15,9,3]`을 동시에 완성한다(cell=row×7+column). 가장 먼21↔6은4목 직선이 아니다. [전용 fixture](../../Tests/Fixtures/rank-ui-crossing-win-cases.json)에 양측 승자·좌우 반전4사례를 준비했고 독립 전체4칸 창 oracle로 기대 수순을 검산했다. 연속4목 이상의 한 직선을 선택하고 마지막 수를 포함하는지 제품 연결 후 검사한다. 실제 선이 여러 개면 각 선별로 유효성을 확인하며 엉뚱한 대각 연결은 허용하지 않는다.
- **추가 데이터 runtime 검증:** rankKey/descriptionKey/aiIntroKey/aiWinKey/aiLoseKey는 필수·공란/미존재 key 거부, difficultyStars는1~5 및6단계 구간과 일치해야 한다. 기존 GameData 검증 통과만으로 새 필드 검증을 대신하지 않는다. 입력 변이는 QA snapshot에만 적용하고 exporter와 runtime 거부/안전 복귀를 구분한다.
- **Undo와 힌트:** 사용한 힌트의 권한/판당1회 소비를 복원하지 않는다. Undo 전 Granted→후 Granted를 유지하며 추천열/강조 표시만 지운다. 무르기만으로 새 광고 요청·보상·경기/완료 통계를 만들지 않는다. 대기/늦은 보상 callback은 독립 사례로 확인한다.
- **슬라이드/모달 배경 차단:** 카드뿐 아니라 로비 Settings/Help/멀티 버튼까지 열기/닫기 슬라이드 중과 preview가 열린 동안 입력을 막는지 확인한다. 직접 listener guard와 실제 overlay raycast를 구분하고 닫은 뒤 정상 복원·스크롤 보존을 확인한다.

승리선 oracle와 입력 수순의 준비는 reference 검산이며 RankGameApp의 ShowWinLine 실행 증거가 아니다. 최종 native에서는 선 끝점·각도·돌 위 확대/복원·흑백 식별성을 실제 캡처로 함께 확인한다.

## 동결 데이터 정적 대조와 추가 입력 준비

[독립 결과](Evidence/qa-rank-data-preparation-v1-20261014/results.json)와 [19파일 스냅샷](Evidence/qa-rank-data-preparation-v1-20261014/source-manifest.json)을 보존했다. `Tests/check-rank-data-preparation.py`는 제품 소스/Excel을 실행하거나 수정하지 않고 다음 범위만 확인했다.

- Generated/Client의 stages·strings·ads3파일 바이트 일치 및 인계 해시 일치. stages `6e66c8b0…`, strings `e191ce67…`, ads `d19f181f…`이며 전체값은 결과에 있다. 검사 중 원본 변경0.
- 이전 동결 JSON과30단계 기존 필드 전체·광고 보존. 기존137문구 중90개 그대로,47개는 승인 목록의 before/after와 일치,91개 추가해228개. 한영 공란·중복키·포맷 인자 불일치 없음.
- 10계급×3단계·6단계마다 별1~5·AI intro/win/lose 참조 방향·설명 참조 일치. 설명/대사60개 최대 길이 한22자/영41자. 상세 `시작하기 / Start`,멀티 `대국 시작` 확인.
- 기존20입력에 힌트 소비 보존·전체 로비 차단·추가필드 거부3개를 더해23개를 준비했다. 별도 복수 승리선4수순의 모든 착수는 합법이며 마지막 수 전 종료 없음, 마지막 수에서2직선 동시 승리·합집합 최장선 오류를 독립 검산했다.

**미실행:** RankGameApp 런타임·C# 승리선 helper 컴파일·Unity·브라우저·실기기·Excel/exporter 테스트 재실행. 데이터 담당42테스트/반복 생성/원본 보존 결과는 제공 증거로만 보존한다.23+4입력 준비를 제품27검사 통과로 세지 않는다. 새 UI 합격과 출시 판정은 계속 보류한다.

## 동결 전 초기 native 캡처 재확인 항목

[보존한3캡처와 관찰](Evidence/qa-rank-ui-pre-freeze-visual-v1-20261014/README.md): ko390로비에서 카드가 표시되지 않고 en320중간 로비는 큰 빈 공간 뒤01/02가 표시된다. ko320승리 화면에서는 여러 내부 격자선이 보이지 않는다. QA는 사본을 직접 읽었으며 Unity/실제 입력을 실행한 것은 아니다.

총괄은 Client에 카드 상단 기준 배치와 격자선 두께 수정/재캡처를 요청했다. anchor 불일치·축소 raster 누락은 원인 후보이며 확정하지 않는다. RUI01/02/03/14에서 초기 첫 행,30단계 스크롤·선택/잠금,preview 종료 시 스크롤 보존,320/390 상세·대국·결과의 전체7×6격자 식별성을 다시 확인한다. 수정 후 native 캡처를 동결 소스/빌드와 연결하고 총괄 실제 포인터 증거를 받아야 한다. 현재 예비 문제를 해결 완료 또는 최종 제품 검사 통과로 표시하지 않는다.

## v8 후보 실제 런타임 실행

총괄의 후보 실행 요청에 따라 [후보v1](Evidence/qa-rank-candidate-v1-20261014/source-manifest.json)에 runtime12파일·Client/Generated JSON6개·계약/fixture5개를 고정했다. 실제 RankGameApp/RankTheme/RankStageCard/RankDetailBlocker와 Core/광고를 컴파일하며, 구 GameApp/Tabletop은 공유 LocalizedLabel 타입 의존성 때문에 함께 컴파일할 뿐 새 테스트 대상으로 사용하지 않았다. C# 승리선 helper도 컴파일해 실제 ShowWinLine의 위치·길이·각도에서 복원한 두 끝점을 판정했다.

| 실행 | 결과 | 구분 |
| --- | --- | --- |
| [runtime-v1](Evidence/qa-rank-runtime-v1-20261014/build.log) | 컴파일 실패, 제품 검사0 | QA Program의 Debug 이름 충돌. 제품 컴파일 결함 아님. 이후 using 별칭만 수정. |
| [runtime-v2](Evidence/qa-rank-runtime-v2-20261014/results.json) | **23통과·1실패,21,731 assertions** | 후보24묶음 전체. QA-014 파괴 후 전면광고 완료가 저장 변경. |
| [impact-v3](Evidence/qa-rank-runtime-impact-v3-20261014/results.json) | **8통과·1실패,18,535 assertions** | RankGameApp 마지막 돌 연출 시작 시 Turn 숨김 변경 영향 및 숨긴 화면/중복 Next 추가. QA-014 동일 재현. 검사 중28입력/도구파일 변경0. |

후보 RankGameApp은 `A63CAD6404834545BB425AD0CD4A927F946A18B027C74ED7A842553E209C9899`, 후속은 `2F36B95E8AD1F116D5C97D19AEBDB77C905934C3CDDE7B305445FB0F5922ECAE`다. 두 실행 사이 제품 변경은 Finish의 Turn 숨김뿐이며 나머지 runtime/JSON 해시는 같았다. 후속9묶음은 이전8묶음을 반복하고 hidden handler1묶음을 추가해 서로 다른25묶음24통과/1실패다. assertion 수를 합산하거나 준비23+4사례를 실행 건수에 더하지 않는다.

**통과 범위:** 신규1회 자동 시작·기존1단계/구형8저장/손상 recovery·저장 오류 중 플레이, preview 비파괴/최하칸/꽉 찬 열/Place 연타, 실제 AI1응수, 응수 전/후 Undo·opening floor·AI 시작·취소 generation·강제로 재개한 오래된 iterator, Retry/Lobby/다른 단계 취소, pause/언어 전환, 힌트 완료/중복/취소/시간초과/Undo 소비 보존, 양측 마지막 수 .4+.5 수동 시간 순서·입력 차단/1회 저장, 실제 Place/AI/무승부 종료,1/9/10/29/30의 Next/Retry/Lobby, 광고4결과/중복/시간초과/전환취소, 데이터 참조 변이20개·별 변이3개 안전 거부, 복수 승리4벡터, 카드/상세 경계·잠금/scroll 상태·blocker enable/disable, 한영 대사 방향/상태 보존.

**실패 QA-014:** 지연 전면광고 요청 후 OnDestroy→Completed에서 CancelPending 증가0이며 lastInterstitialMatch가0→4로 저장된다. [결함과 수정 요청](DEFECTS.md). 현재 NoFill 일반 플레이의 지연광고 재현이 아니라 실제 제품 C#의 종료 수명 관리 실패를 지연 어댑터로 재현했다. 동일 최신 소스에서도 재현되므로 종결하지 않는다.

**검증 한계:** Unity UI/JsonUtility/PlayerPrefs/시간/코루틴은 명시적 대체환경이며 실제 AI 탐색 코드는 실행했다. 입력들은 실제 진행 전체를 사람이 플레이한 결과가 아니고 합법 opening/terminal 주입도 포함한다. 물리 저장 실패/재시작 내구성, 모든 프리팹·문구·실제 레이캐스트/렌더, 기기 성능·실 SDK·광고/IAP·출시 검증은 아니다. 상세 아래 Settings/Help 포인터 차단은 blocker 활성만 확인했으며 실제 native/browser 입력 증거가 필요하다.

Client 제공 [67/67 XML](Evidence/qa-rank-runtime-impact-v3-20261014/ClientProvided/rank-ui-tests.xml)은23:17:56~23:17:58 KST 실행으로 읽어 보존했고 QA가 Unity를 실행하지 않았다. 초기64/66 실패 XML과 Assets freeze도 [제공 증거 manifest](Evidence/qa-rank-runtime-impact-v3-20261014/client-provided-manifest.json)에 별도 보존했다. 총괄은23:09 재캡처에서 카드/격자 수정 시각 확인을 보고했으나 이 절에서 QA 직접 최종 캡처/빌드 연결 완료로 승격하지 않는다. v8 WebGL 성공 보고와 최종 실제 포인터는 별도다.

## v9 후속 영향 검사 — 아직 실행 전

총괄이 전달한 사용자 추가 지시가 v8 직접 Retry/Lobby 및 기존 보드/헤더 배치를 대체한다. **휴대폰 시험은 사용자 지시로 보류하고 이번 확인은 브라우저 중심**이다. 실제 기기 검증 완료로 바꾸지 않는다. v8 소스/결과를 덮어쓰지 않고 새 구현 API를 확인해 변경 영향만 연결한다.

| ID | 추가 기대값 |
| --- | --- |
| RUI17 | 대국 중 Retry,결과 Retry/Lobby,일시정지 나가기 등 모든 사용자 로비로/다시하기는 확인 패널을 거친다. 취소하면 보드·턴·경기ID·Undo history·선택/힌트·언어·통계 및 기존 화면으로 복원한다. 수락은1회만 실행하고 중복 이벤트/숨겨진 버튼으로 새 판·저장을 중복 생성하지 않는다. Next는 즉시1회 이동한다. 내부 초기화용 ShowLobby/StartStage 직접 호출과 사용자 버튼 경로를 구분한다. |
| RUI18 | 확인 패널의 수락/취소 hitbox가 배경 blocker/남은 overlay에 가로막히지 않는다. 열기/닫기/AI 작업/광고 대기와의 경합, 취소 후 정상 입력·화면 복원, 재진입·언어 전환을 native/실제 포인터로 확인한다. 대체환경의 ButtonEvent.Invoke를 raycast 증거로 사용하지 않는다. |
| RUI19 | 외곽판 정사각형,7×6 교차점/내부6×5칸의 dx=dy,동일 선 두께·외곽 여백. 상단은 토스 예약영역 바로 아래,AI 모자 얼굴/계급/별 한줄 중앙+아래 말풍선. 플레이어 얼굴/상시 내차례 제거,원형 Place와 단순 Undo/Retry.320/390 및 가용높이411조건에서 clipping·스크롤·44px입력 타협안을 실제 측정한다. 가용높이 제한을 임의 전체통과로 처리하지 않는다. |
| RUI20 | 내 차례 알림은 큰 중앙의 잠깐 fade toast다. 판/턴이 바뀌거나 Undo/Retry/Lobby/Next/취소하면 이전 알림이 새 판을 침범하지 않는다. 알림이 필요한 Place/열/확인 입력을 가로막지 않고 pause/resume·한영 전환·연타에도 중복 coroutine/오래된 callback이 남지 않는다. |
| RUI21 | 사회에서4목 프로였던 주인공의 입대·소문을 들은 계급별 AI의 도전 대사를 확인한다. stage1 intro는 ‘이 게임의 프로라고 했지? 그런데 내가 이길 것 같아.’,AI승리=플레이어패배는 ‘프로라는 말, 거짓말이었나?’를 보존한다. AI패배는 실력을 인정하는 내용이다. aiIntroKey/aiWinKey/aiLoseKey를 재사용하되1단계 별도키 예외는 최종 Data 계약을 따른다. 사용자 원문을 이전22자/한줄 제한보다 우선하며1~2줄 말풍선의 잘림/누락·한영 승패 방향을 확인한다. |

QA-014는 v9와 무관하게 수정이 필요한 실제 재현 항목이다. v9 확인 패널/팝업의 새 필드·핸들러·취소 identity와 저장 프리팹/최종 native 캡처를 받은 후 v8 직접 Retry/Lobby 기대값을 승인된 새 경로로 바꾼다. 현재 소스명을 추정한 테스트를 제품에 실행하지 않는다.

**스토리 변경으로 데이터 동결 해제:** 기존228키/계급공통 AI키/설명·대사60개 길이 한22/영41 검사는 위 v8 snapshot에만 유효하다. 현재 GameData 변경 중에는 `data.frozen_rank_contract`와 고정해시 정적 checker를 v9 전체 합격 기준으로 실행하지 않는다. 최종 키 수·stage1 예외·승인 문구 변경 목록을 받은 뒤 실제 JSON 바인딩을 바꾸고 규칙/AI/opening/저장ID 보존을 대조한다. 이전 소스/JSON/실행 증거는 그대로 둔다.

### v9 동결 대기 중 준비한 입력

[v9 flow fixture](../../Tests/Fixtures/rank-v9-flow-cases.json)에 확인9·toast6·story4·파괴수명2·layout1,총22개 의미 사례를 준비했다. 이는 제품22검사 통과가 아니며 현재 바뀌는 소스에는 실행하지 않았다. 최종 문자열 수는228/중간233을 고정하지 않고 스토리 최종 계약을 따른다.

인계 뒤 순서는①새 소스/JSON/승인 변경 해시 고정 ②QA-014 원래 재현과 보상/AI/토스트/확인 수명 취소 ③실제 사용자 버튼의 확인 취소/수락/중복 Next ④턴 전환/언어/힌트에 따른 toast 중복·잔류 ⑤한영 첫 대사·승패 방향/기존 opening·AI 보존 ⑥Client native·Root 브라우저의 확인 버튼 raycast/말풍선/정사각형/짧은 스크롤 증거 연결이다. 확인 중 AI pause/resume·취소 시 원래 pause 복원과 toast duration은 최종 구현을 읽어 계약과 대조하며 임의 API/시간을 만들지 않는다.

총괄이 Client 준비 완료를 전달하기 전 런타임 재실행은 하지 않는다. QA-014는 수정 요청이 Client에 전달된 상태이며 수정 통과로 변경하지 않았다. 휴대폰 보류와 Root 브라우저 전담을 유지한다.

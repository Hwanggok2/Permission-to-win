# 한 수 진급 UI·입력·무르기 QA 준비

기준일: 2026-10-14 KST. **계약v8 계획/입력 준비만 완료, 새 제품 검사는 미실행**. 사용자가 Tabletop 시안을 거부하고 새 UI를 요청했다. 이전32묶음·Unity59/별도1·빌드/브라우저 증거는 이전 시안의 기록이며 이번 최종 합격으로 사용하지 않는다. Client와 데이터 담당이 변경 중이므로 동결 전 실제 제품을 반복 실행하지 않는다. [계약v8](../Management/CONTRACT.md)과 총괄 추가 답변을 아래 기대값에 반영했다.

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
| RUI03 | 카드 선택→우측 슬라이드 preview→실제 opening 시작판/한 줄 설명/얼굴/별/좌우 삼각/X/시작을 확인한다.1/30·잠금 경계, 전환 중 연타와 오래된 listener를 검사한다. 모달 아래 카드 입력 차단과 닫은 뒤 목록 스크롤 위치 보존, 실제 닫기 표시/클릭·입력 복원을 확인한다. |
| RUI04 | 저장키가 없는 최초 실행은1단계 자동 진입1회, 이후 로비 진입. 저장키가 있으면 진행1단계·첫 실행 flag 누락이어도 기존 이용자로 처리한다. 첫 판 중 종료/로비/재실행·저장 실패 정책을 확인하고 해금·실제완료·언어·음향·경기/광고카운터·중복ID·복구 사본을 보존한다. |
| RUI05 | 열 선택/변경은 보드·턴·경기ID·저장·광고에 영향을 주지 않는다. preview는 최하 빈칸과 일치하며 정식 돌/승리 판정에 포함되지 않는다. 꽉 찬 열 선택은 기존 preview 제거+Place 비활성화다. 선택 없음·AI 턴·중단/결과의 Place를 거부하고 합법 Place만1회 착수한다. |
| RUI06 | Place 연타/다른 열 연속 선택/키보드 입력/버튼 이벤트 중복에도 플레이어1수와 AI1응수만 발생한다. 정상 착수 시 선택/preview 처리 정책을 따르고 AI 응수 후 preview를 새 보드 기준으로 갱신한다. |
| RUI07 | AI 응수 완료 뒤 Undo는 내 수+AI 수를 되돌려 동일한 플레이어 턴의 직전 보드/수순/LastMove/승리표시/열 높이로 복원한다. 여러 번 Undo해도 opening 이전으로 내려가지 않고 무료이며 광고/경기완료/해금/결제 횟수를 늘리지 않는다. |
| RUI08 | AI 대기/탐색 중 Undo는 아직 둔 내 수만 되돌리고 AI 작업을 취소한다. AI 완료와 Undo 순서가 경합해도 결과는 직전 플레이어 턴의 보드이며 늦은 응수·이중 검색·새 판 오염이 없어야 한다. Retry/로비/다른 단계/앱 pause/언어 변경과의 경합도 분리해 확인한다. |
| RUI09 | Undo 뒤 다른 열 Place·원래 열 재착수·여러 pair 반복, AI 시작/opening 국면을 확인한다. history와 opening을 구분하고 preview/버튼활성·AI seed 정책과 대조한다. 종료 연출/결과 Undo를 거부해 저장된 완료기록을 뒤집거나 중복 기록하지 않아야 한다. |
| RUI10 | 하단 Retry는 같은 단계의 opening으로 복원하고 오래된 AI/연출/광고 callback을 끊는다. 아직 끝나지 않은 판을 재시작하는 것만으로 완료 경기/해금이 증가하지 않아야 한다. 완료 결과의 Retry는 새 경기ID로 시작한다. |
| RUI11 | 양측 마지막 수 확대→승리 표시→광고 경계→결과와 보드 노출을 유지한다. 승리의 Next로 다음 해금 단계에1회 이동하고 마지막30에서는 숨긴다. 패배/무승부는 Retry/Lobby 두 가로 버튼이며 결과 Close는 없다. 종료 처리 동안 하단 Undo/Place/Retry·보드/힌트/pause 차단, NoFill/실패/timeout·늦은 callback·결과1회 저장을 확인한다. |
| RUI12 | 각 계급의 intro/win/loss 대사와 얼굴·별이 현재 AI/단계와 일치한다. aiWinKey는 AI 승리=플레이어 패배, aiLoseKey는 반대이며 한영 전환/reload에도 맞는다. 새 rankKey/descriptionKey/difficultyStars/aiIntroKey/aiWinKey/aiLoseKey와 Excel 변경 목록을 사용해 누락/포맷/하드코딩을 검사한다.137개 고정 기대값은 폐기하고 기존 opening/AI 설정은 의미 비교로 보존을 확인한다. |
| RUI13 | 멀티 버튼은 준비 중 안내만 표시하고 실제 판 생성·매칭/네트워크·계정·진행 변경이 없다. 설정·도움말·라이선스·음향·언어는 유지한다. 현재 광고는 NoFill이며 실제 수익화 완료로 표현하지 않는다. |
| RUI14 |320×568/390×844, OS 위59/아래34와 WebGL 제품 호스트64 조건에서 카드/preview/하단3버튼/결과/닫기 hitbox44px·글자·스크롤·검정/흰돌 식별성을 확인한다.390↔320, preview 전환/얼굴 mask/한 줄 설명/별/모달 닫기의 실제 렌더와 포인터를 검토한다. |
| RUI15 | Client native와 총괄 브라우저 증거를 최종 소스/데이터/아트/빌드 해시에 연결한다. AI 경합·Undo는 독립 상태 및 실제 코루틴으로, 슬라이드·확대·sprite/레이캐스트는 native/실제 포인터로 구분한다. 실제 폰0개·Toss SDK/광고/IAP/서버 미연결을 유지한다. |

## 독립 harness 준비

[입력 fixture](../../Tests/Fixtures/rank-ui-interaction-cases.json)는 **신규 제품 실행 전 입력/기대 상태**다. 열 번호는0부터, row0은 바닥이다. 새 runtime 필드명을 강제하지 않는 의미 기반 기대값이며 source API 확인 후 기존 실제소스 harness에 연결한다. `turnPairs`의 AI 열은 deterministic 입력 벡터로, 실제 AI가 그 열을 고른다고 주장하지 않는다.

실제 새 활성 대상은 **RankGameApp / RankTheme / RankStageCard**이며 이전 GameApp/프리팹은 보존된다. [RankHarness 준비 폴더](../../Tests/RankHarness/README.md)를 분리했다. 기존 whole-board oracle·수동 시계/중첩 coroutine·지연 광고 callback을 재사용하되 새 UI 실제 필드/메서드와 별도 실행 진입점에 연결한다. 동결 전 객체/저장 API를 추측해 만들거나 이전 Tabletop `MakeApp`·Mint/theme·두 버튼/137키 검사를 새 제품에 실행하지 않는다.

검사 우선순위는①preview 비파괴/Place1회 ②opening 경계와 pair Undo ③AI 취소/늦은 응수 ④첫 실행/기존 저장 ⑤단계/결과/광고/한영 ⑥직렬화·native/브라우저다. 보드/AI 코드가 같으면 이전1,000국면 탐색을 반복하지 않는다. Undo가 보드 코어를 바꾸면 합법 prefix 되감기/재생 oracle 비교 및 영향받은 규칙 검사를 추가한다.

## Client·데이터 동결 시 필요한 공유 자료

1. 새 RankGameApp/RankTheme/RankStageCard/Core/진행 저장의 파일 목록과 최종 해시, 실제 활성 scene/root prefab 식별자. 선택열/preview/Place/Undo/Retry·AI 취소 generation/token·history/opening 경계·종료 상태·승리선 두 끝점 또는 그 화면 좌표의 실제 필드/메서드/접근 경로.
2. **최초 실행 구현:** 저장키 없음=신규, 기존 저장/새 flag 누락=기존은 확정됐다. 소비 marker 저장 시점·실제 필드, 저장 실패/손상 복구, saveVersion/key 변경 여부와 native raw 저장 migration XML을 제공한다.
3. **Undo 구현:** 응수 전1수/후2수·opening floor·종료 연출/결과 금지·사용 완료된 힌트 권한을 되돌리지 않고 추천 표시만 지우기는 확정됐다. history/취소 identity, AI 시작 opening, 늦은 힌트 callback, AI 재선택 seed, 앱 pause 경합의 실제 구현을 제공한다.
4. **선택/결과 구현:** full열 선택→preview 제거/Place 비활성, 승리 Next(30 제외), 패배/무승부 두 버튼, Lobby 최고 해금 선택은 확정됐다. 선택 없음/Undo 후 preview 상태와 각 활성 guard·실제 API, preview 모달 배경 차단/스크롤 보존 경로를 제공한다.
5. Excel FORMAT/런타임 계약과 계급10·대사 키·얼굴 매핑·별 계산/저장 여부, 한영 신규 총수·승인된 기존 문구 변경, Generated/Client 동일성 및 exporter 검사 결과. 기존 브랜드/도움말/착수 안내를 그대로 보존해야 한다고 오판하지 않도록 변경 목록이 필요하다.
6. 저장된 Main/AppRoot·카드/preview/보드/AI 얼굴/하단/결과/공통닫기 prefab, 재적용 비파괴 로그·참조 검증,320/390 한영 캡처·UI hitbox, 우측 슬라이드/마지막 수/Undo·AI 취소 native 실행 XML·타임라인.
7. 마지막으로 두 개발 빌드/고지/소스 해시와 총괄 실제 포인터·콘솔 보고. 오래된 Tabletop 이미지/빌드를 새 시안 증거로 혼합하지 않는다.

이 자료 요청은 공유 문서로 남긴 구현 연결 조건이며 사용자 승인 요청이 아니다. QA가 Client·Excel·Unity·브라우저·커밋/푸시를 대신 조작하지 않는다. 새 제품 통과·실패 및 출시 완료는 아직 판정하지 않았다.

## 준비 검증 결과

[준비 기록](Evidence/qa-rank-ui-preparation-20261014/preparation-results.json)에 보드13·생명주기5·모달2, 총20입력 사례와 기존 harness/계약 스냅샷을 보존했다. JSON·중복ID·기존 저장 fixture 참조와 기대 수순15개의 합법성/opening prefix만 검사했다. 이는 입력 자료의 일관성 확인이며 제품20검사 통과가 아니다. runner는 신규 fixture를 AuditInputs에 보존하도록 준비했지만 실제 제품 API 바인딩/새 테스트 실행은 Client·데이터 동결 후에 한다. Unity·브라우저·기기 실행은 없었다.

## 초기 소스 검토에서 추가한 경계 — 제품 검사 미실행

총괄이 작업 중 소스를 검토해 Client에 전달한 사항을 아래에 반영했다. QA가 초기 RankGameApp을 읽었지만 아직 동결되지 않았으므로 현재 미완성 코드를 제품 실패로 실행/등록하지 않는다.

- **복수 방향 동시 승리선:** WinningCells의 모든 점 중 가장 먼 두 점은 서로 다른 승리선을 가로지를 수 있다. 예시 열 수순 `[1,0,6,2,2,1,1,0,4,0,0,1,1,6,5,5,3]`은 마지막3열 착수에서 바닥 수평 `[3,4,5,6]`과 대각 `[21,15,9,3]`을 동시에 완성한다(cell=row×7+column). 가장 먼21↔6은4목 직선이 아니다. [전용 fixture](../../Tests/Fixtures/rank-ui-crossing-win-cases.json)에 양측 승자·좌우 반전4사례를 준비한다. 연속4목 이상의 한 직선을 선택하고 마지막 수를 포함하는지 독립 oracle로 검사한다. 실제 선이 여러 개면 각 선별로 유효성을 확인하며 엉뚱한 대각 연결은 허용하지 않는다.
- **추가 데이터 runtime 검증:** rankKey/descriptionKey/aiIntroKey/aiWinKey/aiLoseKey는 필수·공란/미존재 key 거부, difficultyStars는1~5 및6단계 구간과 일치해야 한다. 기존 GameData 검증 통과만으로 새 필드 검증을 대신하지 않는다. 입력 변이는 QA snapshot에만 적용하고 exporter와 runtime 거부/안전 복귀를 구분한다.
- **Undo와 힌트:** 사용한 힌트의 권한/판당1회 소비를 복원하지 않는다. Undo 전 Granted→후 Granted를 유지하며 추천열/강조 표시만 지운다. 무르기만으로 새 광고 요청·보상·경기/완료 통계를 만들지 않는다. 대기/늦은 보상 callback은 독립 사례로 확인한다.
- **슬라이드/모달 배경 차단:** 카드뿐 아니라 로비 Settings/Help/멀티 버튼까지 열기/닫기 슬라이드 중과 preview가 열린 동안 입력을 막는지 확인한다. 직접 listener guard와 실제 overlay raycast를 구분하고 닫은 뒤 정상 복원·스크롤 보존을 확인한다.

승리선 oracle와 입력 수순의 준비는 reference 검산이며 RankGameApp의 ShowWinLine 실행 증거가 아니다. 최종 native에서는 선 끝점·각도·돌 위 확대/복원·흑백 식별성을 실제 캡처로 함께 확인한다.

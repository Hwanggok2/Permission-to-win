# v9 독립 런타임 및 최종 소스 대조

- [실행 결과](results.json): **40/40 PASS**,실패0,43,988 assertions. 실제 후보C#·명시적 Unity/저장/시계/코루틴 대체환경·실제 AiSearch·지연 광고 callback. 네이티브/브라우저 실행이 아니다.
- [입력 manifest](source-manifest.json): 제품/데이터/계약/fixture와 실제 컴파일한 도구38개를 보존했다. [실행 후 대조](post-run-source-check.json)의 변경1개는 CONTRACT에 추가된 사용자 웹 승인 조건이며 제품 source/JSON변경0.
- [최종freeze 대조](runtime-freeze-link.json): runtime12+Client JSON3 모두 Client 최종freeze와 현재파일에 일치. RankGameApp `3FDF28A138D2F931C63BCC95B8904B0E42A101E0F26DD545EE986AA19F7393F9`.
- QA-014 원래 재현과 전면광고/보상/AI/확인/토스트 파괴·늦은 콜백 회귀 통과. 확인 취소/수락1회·중복 Next·AI정지/재개·Toast중복/잔류·한영첫대사·237키승인변경을 확인했다.
- [Client 제공](client-provided-manifest.json): 최종Unity73/73,모자 수정 전/초기 XML,Assetsfreeze. 최종검사는2026-10-14 23:45:44~23:45:46 KST이며 QA가 Unity를 실행한 결과가 아니다. 독립40개와 단순 합산하지 않는다.
- WebGL/Android 빌드·브라우저는 QA 미실행. 이번 웹 재빌드/새 브라우저 실행은 사용자 승인 대기,휴대폰은 사용자 지시로 보류. 제품 출시는 완료 판정하지 않는다.

재현: 같은 QA 도구 버전과 후보 입력으로 `Tests/run-rank-independent.ps1 -RunId <새ID> -CandidateId qa-rank-v9-candidate-v1-20261014`. 기존 증거를 덮어쓰지 않는다. 실제 프리팹/포인터·글자/격자 렌더/실기기·광고 SDK·저장 내구성은 이 결과 범위 밖이다.

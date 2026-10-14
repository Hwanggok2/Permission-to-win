# 참고 화면 분석의 권리 범위

2026-10-14 (KST). [REFERENCE-UX.md](../Docs/Design/REFERENCE-UX.md)의 **9장 시각 분석 및 이후 Client 생성 도구·저장 프리팹 참조 점검** 기록이다. 원본은 해당 문서에 식별한 사용자 지정 OneDrive 경로에서 읽기만 했다. 열람 요청은 제3자 화면 자산의 배포 허가로 해석하지 않는다.

- 참고 PNG·크롭·썸네일·추출 질감·로고·글꼴·아이콘·해칭 그림자·문구를 Client, 빌드, 상점 자료, Overview 이미지로 복사하지 않는다. 참고 이미지의 개별 자산 권리/재배포 허가는 확인되지 않았으며 게임 사용 자산으로 등록하지 않는다.
- 적용 대상은 상태를 색+문구로 구분하기, 다음 목표 제시, 반복 요소 정렬, 그림 규칙, 주요 행동 하나 강조 같은 설계 원칙이다. 표현은 기존 프로젝트의 자체 도형 `ART-SOFT-001/002`와 Noto `FONT-002`를 재사용한다. Noto OFL·uGUI/UCL 등 기존 고지 전문과 설정의 작은 진입 메뉴를 유지한다.
- 현재 파일 대조에서 `FONT-002`, 기존 Disc `PLAN-003`, `ART-SOFT-001/002`, 고지 묶음 `NOTICE-BUNDLE-001`의 SHA-256 5개가 대장과 일치했다. Noto CJK OFL·uGUI LICENSE·UCL 원문 3개가 고지 묶음에 줄바꿈 정규화 후 전부 포함됨도 재확인했다.
- **새 구현 근거:** [StageMapBuilder.cs](../Client/Assets/StageGame/Editor/StageMapBuilder.cs)는 기존 폰트·RoundRect96·SoftDisc128만 로드해 단계 노드, 중력 화살표/돌, 가로·세로·대각선 네 돌 패턴을 저장한다. 새 이미지 입력이나 외부 패키지 호출은 없다. 이는 내부 구성 코드이며 불필요한 제3자 라이선스 항목을 만들지 않는다. 기존 장식/설명은 삭제 대신 비활성화하고, 인식하지 못한 StageNode는 덮어쓰지 않도록 검사한다.
- **저장 참조 대조:** StageNode와 AppRoot/LobbyScreen/HelpModal/ResultModal의 직접 sprite/font GUID는 기존 도형 2개와 Noto다. LobbyScreen에 StageNode 원본 참조 10개, AppRoot의 버튼/번호/상태 직렬화 배열 각각 10개를 확인했다. [재적용 기록](../Client/Evidence/stage-map-idempotence.json)의 프리팹 5개 전후 해시가 현재 파일과도 일치한다. 이 검사는 모든 사용자 편집을 보존한다는 일반 보증이 아니다.
- **대장 갱신:** [ASSET-REGISTER.csv](ASSET-REGISTER.csv)는 **64행·16열**이다. 내부 생성 도구 `CODE-004`, 저장 노드 `UI-015`를 추가하고 변경된 기존 프리팹 4개와 런타임 코드군의 해시를 점검 시점 스냅샷으로 갱신했다. 생성 도구·프리팹의 상태는 `PRESENT_STAGE_MAP_SNAPSHOT`으로 최종 출하 승인과 구분한다. 기존 APK/캡처 행의 앞선 본문30 검증 이력은 새 단계 UI의 최종 검증 증거가 아니다.
- **추가 수정 시:** Client의 후속 소스/프리팹 조정이 있으면 해당 해시를 갱신한다. 최종 빌드·브라우저·권리 표시는 [단계 UX QA](../Docs/QA/STAGE-UX-RESULTS.md)와 [총괄 브라우저 기록](../Docs/Management/Evidence/reference-ux-browser-20261014/README.md)의 대상 빌드/결과에 연결한다. 참고 원본은 배포 자산으로 전환하지 않는다.

**확인 완료:** 생성 코드·저장 참조·기존 자산/고지 보존과 제공 Unity XML 57/57 통과 기록. **별도 진행:** 최종 빌드/브라우저 및 실제 토스·기기·사람 검증. 이번 역할은 Unity/브라우저 실행이나 전체 외부 코드 유사성·권리 전수 감사를 수행하지 않았다.

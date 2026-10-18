# 마지막 착수·대사·멀티 배틀 검증

작업 시작2026-10-17, 최종 확인2026-10-18 KST. **로컬 구현·검증**이며 공개 서버·새 웹/Android 빌드·실기기 공유 검증은 하지 않았다. 저장 프리팹의 공개 멀티 endpoint는 비워 두었다. 전체 계약은 [MULTIPLAYER-CONTRACT](../../../Docs/Management/MULTIPLAYER-CONTRACT.md)을 따른다.

| 실제 실행 | 결과 | 증거 |
| --- | --- | --- |
| Excel exporter 재생성 일치·변형 거절 | 47/47 | data-tests.txt |
| 서버 Arena·WebGL 공유 bridge·기존 구매 API 회귀 | 37/37 (멀티27, 구매10) | server-tests-final.txt |
| TypeScript noEmit | 종료0 | typescript.log |
| Worker 번들 dry-run | 종료0, 원격 배포 없음 | worker-bundle.txt |
| 실제 로컬 WebSocket·Durable Object SQLite | 11항목 통과 | server-smoke.txt |
| Unity 저장 프리팹·흰 테두리·되돌리기·멀티 UI·로비·버튼 | 6/6 | unity-tests-final.xml |
| Unity 실제 두 클라이언트 연결·한영 대사 타이핑 | 3/3 | unity-network-speech.xml |
| Unity 기존 보드·AI·광고/진행 코어 회귀 | 25/25 | unity-core.xml |

Unity 실행 합계34, 중복된 프리팹 검사를 제외하면33개 고유 테스트다. 모든 최종 실행의 실패/건너뜀은0이다. 최초 `unity-tests.xml`의 CanvasRenderer 누락 실패와 최초 `server-tests.log`의 Node 변환 옵션 누락 실패는 수정 전 이력이며 통과로 계산하지 않았다. 수정 후 XML/log를 우선한다.

실제 통합 검사는 게스트 인증·200점, 레이팅 ON/OFF와 입장 전 설정 변경, 잘못된 착수 거절, 일치하는 보드·결과, 요청만으로 시작하지 않는 리매치, 양쪽 동의 후 선공, 재접속, 상대 퇴장, 무작위 매칭을 확인했다. Unity 네이티브 전송 검사에서는 두 연결로 OFF 친선전을 끝내고 점수200 유지, 리매치, 상대 퇴장, 나가기→로비를 확인했다. 화면별 snapshot 검사는 ko/en ×320/390폭에서 메뉴·초대·대국·리매치·퇴장을 확인했으며 이것만으로 네트워크 검증을 주장하지 않는다.

화면 PNG27개:

- [AI 마지막 착수 흰 테두리](screens/opponent-last-move.png)
- [친선 레이팅 설정](screens/ko-390-online-invite.png)
- [리매치 알림](screens/ko-390-online-rematch.png)
- [상대 퇴장 알림](screens/ko-390-online-left.png)
- [Unity 실제 로컬 서버 대국 결과](screens/online-real-connection-result.png)
- [320폭 영어 초대](screens/en-320-online-invite.png)
- [보존한 첫 대사의 실제 표시](screens/ko-complete.png)

Excel은 Artifact Tool로 수정·렌더했다. 기존2850개 셀 스타일·메타와 Stages/AdConfig 값을 보존했다. 기존176개 언어 셀(88키)을 수정하고61개 문자열 키, Multiplayer 시트16개 설정을 추가했다. 원본/생성물 해시와 수량은 [verification.json](verification.json), 셀별 실제 변경은 [excel-preservation.json](excel-preservation.json)을 따른다. 전/후 대사와 새 설정 렌더는 excel-before.png, excel-after.png, excel-multiplayer.png, excel-config.png다.

한계: 실제 Android 공유·Kakao 대상 선택·모바일 초대 복귀, WebGL 컴파일과 실제 브라우저 공유, 공개 서버 계정/무료 한도 소진, 사람 대상 매칭 공정성·대기시간, 큰 부하·점수 담합 방지는 미검증이다. 웹 공유 단위 검사와 Unity 네이티브 연결 통과를 이 항목들의 통과로 기록하지 않는다.


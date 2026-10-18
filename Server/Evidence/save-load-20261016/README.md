# 게임 저장·불러오기 확인

확인일: 2026-10-16. 대상은 실제 배포된 [GitHub Pages 게임](https://hwanggok2.github.io/StageGame/)이다. Codex 내장 Chromium 브라우저에서 화면을 조작해 확인했다. 모바일 실기기/다른 브라우저 검증은 실행하지 않았다. 게임 코드를 수정하거나 저장소에 테스트 값을 직접 주입하지 않았다.

**검사 범위 보완:** 아래 통과 결과는 390×844 세로 뷰포트에서의 저장·불러오기 검사다. 이후 사용자 제보를 조사해 PC 가로 화면에서 단계 목록이 사라지고 UI가 겹치는 별도 배치 결함을 재현했다. [원인·재현 증거](../desktop-layout-diagnosis-20261016/README.md). PC에서 정상 플레이 가능하다는 판정으로 사용하지 않는다.

| 검사 | 실제 결과 |
| --- | --- |
| 설정 저장 | 한국어·효과음 켜짐·음악 켜짐에서 영어·효과음 꺼짐·음악 꺼짐으로 변경 |
| 설정 불러오기 | 페이지 새로고침 후 설정 화면의 영어 선택·효과음 꺼짐·음악 꺼짐 유지 확인: 통과 |
| 원래 설정 복원 | 한국어·효과음 켜짐·음악 켜짐으로 되돌림 |
| 진행도 저장 | 1단계를 실제 대국으로 승리하고 로비에서 1단계 완료 표시·2단계 해금 확인 |
| 진행도 불러오기 | 페이지 새로고침 후 1단계 완료 표시·2단계 해금 유지 확인: 통과. [화면 증거](progress-after-reload.jpg) |

진행도 검사 중 로딩 대기 도구가 한 차례 시간 초과를 반환했지만, 이어서 관찰한 실제 화면은 정상 로딩된 로비와 복원된 진행도를 표시했다. 대기 도구의 성공만으로 통과를 판단하지 않았다.

## 저장 위치와 범위

- 현재 게임은 `Client/Assets/StageGame/Scripts/Core/ProgressStore.cs`의 `stagegame.progress.v1` PlayerPrefs에 완료/해금 단계와 언어·효과음·음악 설정 등을 저장한다. `RankGameApp.cs`에서 설정 변경과 대국 완료 시 저장한다.
- Unity Web의 PlayerPrefs 저장 위치는 브라우저 IndexedDB다. 같은 사이트 origin·기기·브라우저 프로필에서 이어진다. 다른 브라우저/기기 또는 다른 도메인으로 자동 이전되지 않으며 사이트 데이터 삭제/시크릿 세션 종료 시 유지되지 않을 수 있다. [Unity PlayerPrefs 공식 설명](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerPrefs.html), [MDN IndexedDB](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API/Using_IndexedDB) (2026-10-16 확인).
- 진행 중인 대국의 돌 배치는 저장하지 않는다. 게임의 일시정지 안내에도 현재 대국은 저장되지 않고 해금한 스테이지는 유지된다고 표시된다.
- Cloudflare Workers + D1 개발 서버는 상태 확인 API와 `server_metadata`만 구현된 상태다. 게임 진행도 저장/불러오기 API 및 클라이언트의 D1 저장 연결은 없다. 이번 통과 결과는 브라우저 로컬 저장 검증이며 서버 저장 검증이 아니다.
- 기존 문서의 `shares-country-steel-fridge.trycloudflare.com` 임시 주소는 검사 시 `ERR_NAME_NOT_RESOLVED`로 접속되지 않았다. 현재 확인된 플레이 주소는 위 GitHub Pages 주소다.

테스트 후 이 검사 브라우저에는 1단계 완료·2단계 해금 기록이 남아 있다. 사용자 데이터를 지우는 초기화는 하지 않았다. 이 기록은 사용자가 별도로 여는 브라우저와 공유된다는 보장이 없다. 게임 빌드·배포·Git 커밋/푸시는 이번 검사에서 실행하지 않았다.

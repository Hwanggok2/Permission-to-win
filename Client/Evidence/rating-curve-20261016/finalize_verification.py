"""Read checked outputs, assert preservation, and record the final client handoff."""
import hashlib,json,xml.etree.ElementTree as ET
from datetime import datetime,timezone
from pathlib import Path
folder=Path(__file__).resolve().parent
root=folder.parents[2]
client=root/'Client'
def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest().upper()
def record(path,base):return dict(path=path.relative_to(base).as_posix(),bytes=path.stat().st_size,sha256=sha(path))
old=read(client/'Evidence/fork-defense-20261015/source-freeze.json')
changed=[entry['path'] for entry in old['files'] if sha(client/entry['path'])!=entry['sha256']]
assert set(changed)=={'Assets/StageGame/Resources/GameData/stages.json','Assets/StageGame/Tests/EditMode/OpeningTests.cs','Assets/StageGame/Tests/EditMode/RankTurnTimerTests.cs'},changed
assert sha(folder/'AiSearch-before.cs')==sha(client/'Assets/StageGame/Scripts/Core/AiSearch.cs')
builds=read(client/'Evidence/easy-lemon-before.json')['builds']
for entry in builds:assert sha(client/entry['path'])==entry['sha256']
hashes={}
for name in ('stages.json','strings.json','ads.json'):
    generated=root/'GameData/Generated'/name
    assert generated.read_bytes()==(client/'Assets/StageGame/Resources/GameData'/name).read_bytes()
    hashes[name]=sha(generated)
before=read(folder/'stages-before.json')['stages']
stages=read(root/'GameData/Generated/stages.json')['stages']
profiles=read(folder/'final-profiles.json')
assert stages[:6]==before[:6]
for i,(a,b) in enumerate(zip(before,stages)):
    for key,value in a.items():
        assert b[key]==(profiles[i//2][key] if key in ('aiDepth','aiNodeBudget','aiRandomPermille') else value),(i,key)
for name in ('unity-final-tests.xml',):
    tests=ET.parse(folder/name).getroot()
    assert tests.attrib['result']=='Passed' and tests.attrib['failed']=='0' and tests.attrib['skipped']=='0'
short=read(folder/'final-short-games.json')['results']
assert len(short)==20 and sum(r['result']['earlyPlayerWins'] for r in short)==0
matches=read(folder/'final-matches.json')['results']
validation=read(folder/'validation-tactics.json')['results']
for rows in (matches,validation,read(folder/'final-tactics.json')['results']):
    assert [r['profile'] for r in rows]==profiles
assert [r['rating'] for r in short]==[p['aiRating'] for p in profiles[5:] for _ in range(2)]
summary=[]
for tactical,match in zip(validation,matches):
    metrics=tactical['result']['metrics']
    summary.append(dict(rating=tactical['profile']['aiRating'],attackPercent=sum(m['percent'] for m in metrics if m['category'].startswith('attack'))/3,defensePercent=sum(m['percent'] for m in metrics if m['category'].startswith('defense'))/3,botScorePercent=match['result']['scorePercent']))
for key in ('attackPercent','defensePercent'):
    assert all(b[key]>=a[key] for a,b in zip(summary,summary[1:])),key
assert not (client/'Temp/UnityLockfile').exists()
stamp=datetime.now(timezone.utc).isoformat()
freeze=dict(atUtc=stamp,scope='Final authored 200..1600 rating curve; Unity closed; no player build',files=[record(p,client) for base in ('Assets','Packages','ProjectSettings') for p in sorted((client/base).rglob('*')) if p.is_file()],dataSources=[record(root/'GameData/GameData_MVP.xlsx',root)]+[record(p,root) for p in sorted((root/'GameData/Generated').glob('*')) if p.is_file()])
(folder/'source-freeze.json').write_text(json.dumps(freeze,indent=2)+'\n',encoding='utf-8')
verification=dict(status='ALL_RATING_CURVES_VERIFIED',atUtc=stamp,unityTests={k:int(tests.attrib[k]) for k in ('total','passed','failed','skipped')},unityResults='unity-final-tests.xml',dataTestsPassed=46,exportCheck=True,changedFrozenAssets=changed,preservedFrozenAssets=len(old['files'])-len(changed),unchangedBuildFiles=len(builds),workbookChangedCells=len(read(folder/'workbook-changes.json')['changedCells']),beginnerStagesUnchanged=[1,2,3,4,5,6],aiSearchUnchanged=True,generatedResourceHashes=hashes,finalProfiles=profiles,finalHeldoutTactics=dict(boards=48,seedsPerStochasticProfile=16,deterministicProfileTrialsPerBoard=1,source='validation-tactics.json',measuredAttackAndDefenseNondecreasing=True),finalMatches=dict(games=sum(r['result']['games'] for r in matches),source='final-matches.json',fixedOpponent='depth 4 / 10000 nodes / safe random 250 permille / oversight 0',scoreIsNotStrictlyMonotonic=True),shortGameAudit=dict(source='final-short-games.json',ratings='700..1600',seedsPerStarter=2,playerBranches=sum(r['result']['playerBranches'] for r in short),aiReplies=sum(r['result']['aiMoves'] for r in short),earlyLosses=0),summary=summary,unityClosed=True,newPlayerBuild=False,humanPlaytest=False,deviceValidation=False,notes=['Ratings are internal content labels, not measured Elo.','Tactical percentages cover bounded sampled positions, not perfect play.','Each rating has two stages; all initial layouts and metadata are preserved.','Unchanged candidate B audit rows are reused exactly; changed 600/700 profiles were rerun.','Source freeze is relative to Client for files and repository root for dataSources.'])
(folder/'verification.json').write_text(json.dumps(verification,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:verification[k] for k in ('status','unityTests','dataTestsPassed','preservedFrozenAssets','unchangedBuildFiles','workbookChangedCells','unityClosed')},ensure_ascii=False))

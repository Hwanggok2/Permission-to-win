"""Run exporter checks against snapshots; never write GameData/ or Client/."""
import argparse
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import shutil
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.dont_write_bytecode = True

def load_module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[name] = module
    spec.loader.exec_module(module)
    return module

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('run_id')
    args = parser.parse_args()
    if not args.run_id.replace('-', '').replace('_', '').isalnum():
        raise ValueError('safe run ID required')
    root = Path(__file__).resolve().parent.parent
    evidence = root/'Docs'/'QA'/'Evidence'/args.run_id
    evidence.mkdir(exist_ok=False)
    snapshot = evidence/'Sources'
    snapshot.mkdir()
    manifest = []
    sources = [root/'GameData'/n for n in ('export_data.py', 'test_export_data.py', 'GameData_MVP.xlsx')]
    sources += [root/'GameData'/n for n in ('audit_openings.py', 'Validation/opening-repairs.json') if (root/'GameData'/n).exists()]
    for source in sources:
        destination = snapshot/source.relative_to(root/'GameData')
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
        manifest.append({'source': str(source.relative_to(root)), 'sha256': hashlib.sha256(destination.read_bytes()).hexdigest()})
    original_generated = evidence/'GeneratedData'
    client = evidence/'ClientData'
    for source_dir, destination in ((root/'GameData'/'Generated', original_generated), (root/'Client'/'Assets'/'StageGame'/'Resources'/'GameData', client)):
        destination.mkdir()
        for name in ('stages.json', 'ads.json', 'strings.json'):
            shutil.copyfile(source_dir/name, destination/name)
            manifest.append({'source': str((source_dir/name).relative_to(root)), 'sha256': hashlib.sha256((destination/name).read_bytes()).hexdigest()})
    (evidence/'source-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    data = load_module('export_data', snapshot/'export_data.py')
    if (snapshot/'audit_openings.py').exists():
        load_module('audit_openings', snapshot/'audit_openings.py')
    tests = load_module('test_export_data', snapshot/'test_export_data.py')
    workspace = root/'Tests'/'.work'/args.run_id
    workspace.mkdir(parents=True, exist_ok=True)
    data.GENERATED = workspace
    log = io.StringIO()
    suite = unittest.defaultTestLoader.loadTestsFromModule(tests)
    result = unittest.TextTestRunner(stream=log, verbosity=2).run(suite)
    (evidence/'exporter-tests.log').write_text(log.getvalue(), encoding='utf-8')
    checks = []
    def check(name, function):
        try:
            detail = function()
            checks.append({'name': name, 'status': 'PASS', 'detail': detail})
        except Exception as error:
            checks.append({'name': name, 'status': 'FAIL', 'detail': str(error)})
    payloads = data.validate(data.read_source(data.SOURCE))
    def bytes_match():
        for name, expected in data.serialize(payloads).items():
            assert (original_generated/name).read_bytes() == expected, name+' differs from Excel/exporter'
        return 'All three generated JSON files match the snapshotted Excel and actual exporter byte-for-byte.'
    check('excel_to_generated_bytes', bytes_match)
    def imported_match():
        for name, expected in payloads.items():
            assert json.loads((client/name).read_text(encoding='utf-8-sig')) == expected, name+' differs in Client/'
        return 'All three Client imports are semantically identical.'
    check('generated_to_client_import', imported_match)
    def batch_failure():
        target = workspace/'write-fault'
        target.mkdir(exist_ok=True)
        old = copy.deepcopy(payloads)
        new = copy.deepcopy(payloads)
        old_key = new['stages.json']['stages'][0]['titleKey']
        new_key = old_key+'_qa_changed'
        new['stages.json']['stages'][0]['titleKey'] = new_key
        for entry in new['strings.json']['strings']:
            if entry['key'] == old_key:
                entry['key'] = new_key
        with patch.object(data, 'GENERATED', target):
            data.write_outputs(old)
            before = {name: (target/name).read_bytes() for name in old}
            actual_replace = data.os.replace
            count = 0
            def fail_second(source, destination):
                nonlocal count
                count += 1
                if count == 2:
                    raise OSError('QA injected second replace failure')
                return actual_replace(source, destination)
            try:
                with patch.object(data.os, 'replace', side_effect=fail_second):
                    data.write_outputs(new)
            except OSError:
                pass
            after = {name: (target/name).read_bytes() for name in old}
            changed = [name for name in old if before[name] != after[name]]
            written_stages = json.loads(after['stages.json'])
            written_keys = {row['key'] for row in json.loads(after['strings.json'])['strings']}
            broken = [s['titleKey'] for s in written_stages['stages'] if s['titleKey'] not in written_keys]
            assert not changed, f'Partial write after second replacement failure: changed={changed}; missing titleKey references={broken}'
            return 'Previous full output preserved on second replacement failure.'
    check('batch_write_failure_preserves_all_outputs', batch_failure)
    changed_sources = []
    for entry in manifest:
        current_hash = hashlib.sha256((root/entry['source']).read_bytes()).hexdigest()
        if current_hash != entry['sha256']:
            changed_sources.append(entry['source'])
    report = {'python': sys.version, 'exporterTests': {'run': result.testsRun, 'failures': len(result.failures), 'errors': len(result.errors)},
              'checks': checks, 'sources_changed_during_run': changed_sources,
              'limitations': 'No workbook authoring, source writes, visual layout checks, Unity JSON, device or live ads. Exporter existing test outputs redirected under Tests/.work. Test snapshots preserve exact input.'}
    (evidence/'results.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if result.wasSuccessful() and all(c['status']=='PASS' for c in checks) else 1

if __name__ == '__main__':
    raise SystemExit(main())

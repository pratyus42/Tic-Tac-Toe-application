const fs = require('node:fs');
const path = require('node:path');
const { XMLParser } = require('fast-xml-parser');

const rootDirectory = path.resolve(__dirname, '..', '..');
const reportsDirectory = path.join(rootDirectory, 'reports');
const backendReportPath = path.join(reportsDirectory, 'backend', 'backend-tests.trx');
const frontendReportPath = path.join(reportsDirectory, 'frontend', 'frontend-tests.xml');
const outputPath = path.join(reportsDirectory, 'test-report.html');
const parser = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '@_' });

function asArray(value) {
  if (value === undefined || value === null) {
    return [];
  }
  return Array.isArray(value) ? value : [value];
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function readXml(filePath) {
  if (!fs.existsSync(filePath)) {
    throw new Error(`Report not found: ${filePath}`);
  }
  return parser.parse(fs.readFileSync(filePath, 'utf8'));
}

function parseBackendTests() {
  const document = readXml(backendReportPath);
  const results = asArray(document.TestRun?.Results?.UnitTestResult);
  return results.map((result) => ({
    name: result['@_testName'] || 'Unnamed test',
    suite: 'Backend',
    status: result['@_outcome'] || 'Unknown',
    duration: result['@_duration'] || '-'
  }));
}

function parseFrontendTests() {
  const document = readXml(frontendReportPath);
  const suites = asArray(document.testsuites?.testsuite || document.testsuite);
  return suites.flatMap((suite) => asArray(suite.testcase).map((test) => {
    const failures = asArray(test.failure);
    const errors = asArray(test.error);
    const skipped = asArray(test.skipped);
    let status = 'Passed';
    if (failures.length > 0 || errors.length > 0) {
      status = 'Failed';
    } else if (skipped.length > 0) {
      status = 'Skipped';
    }

    return {
      name: test['@_name'] || 'Unnamed test',
      suite: test['@_classname'] || 'Frontend',
      status,
      duration: test['@_time'] ? `${test['@_time']} s` : '-'
    };
  }));
}

function summarize(tests) {
  return tests.reduce((summary, test) => {
    summary.total += 1;
    const status = test.status.toLowerCase();
    if (status === 'passed') summary.passed += 1;
    else if (status === 'failed') summary.failed += 1;
    else if (status === 'skipped' || status === 'notexecuted') summary.skipped += 1;
    return summary;
  }, { total: 0, passed: 0, failed: 0, skipped: 0 });
}

function statusClass(status) {
  const normalized = status.toLowerCase();
  if (normalized === 'passed') return 'status-passed';
  if (normalized === 'failed') return 'status-failed';
  return 'status-skipped';
}

function renderRows(tests) {
  return tests.map((test, index) => `
    <tr>
      <td class="number">${index + 1}</td>
      <td class="test-name">${escapeHtml(test.name)}</td>
      <td>${escapeHtml(test.suite)}</td>
      <td><span class="status ${statusClass(test.status)}">${escapeHtml(test.status)}</span></td>
      <td class="duration">${escapeHtml(test.duration)}</td>
    </tr>`).join('');
}

function renderSection(title, tests) {
  const summary = summarize(tests);
  return `
    <section class="report-section">
      <div class="section-heading">
        <div>
          <p class="eyebrow">${escapeHtml(title)} suite</p>
          <h2>${escapeHtml(title)} tests</h2>
        </div>
        <strong>${summary.passed}/${summary.total} passed</strong>
      </div>
      <div class="table-wrap">
        <table>
          <colgroup><col class="col-number"><col class="col-name"><col class="col-suite"><col class="col-status"><col class="col-duration"></colgroup>
          <thead>
            <tr><th class="number">#</th><th>Test method</th><th>Suite</th><th class="status-column">Status</th><th class="duration">Duration</th></tr>
          </thead>
          <tbody>${renderRows(tests)}</tbody>
        </table>
      </div>
    </section>`;
}

const tests = [...parseBackendTests(), ...parseFrontendTests()];
const summary = summarize(tests);
const generatedAt = new Date().toLocaleString();
const report = `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Tic Tac Toe Test Report</title>
  <style>
    :root { color-scheme: light; --ink: #17212b; --muted: #66727d; --line: #dce4e8; --paper: #ffffff; --canvas: #edf3f2; --teal: #087f73; --teal-light: #d9f2ed; --red: #b83b4b; --red-light: #fde5e8; --amber: #9a6b00; --amber-light: #fff3cf; }
    * { box-sizing: border-box; }
    body { margin: 0; background: linear-gradient(135deg, #edf3f2 0%, #f7f3eb 100%); color: var(--ink); font: 15px/1.5 Georgia, 'Times New Roman', serif; }
    .page { width: min(1180px, calc(100% - 40px)); margin: 0 auto; padding: 52px 0 72px; }
    header { display: flex; justify-content: space-between; gap: 24px; align-items: end; margin-bottom: 32px; }
    .kicker, .eyebrow { color: var(--teal); font: 700 12px/1.2 Arial, sans-serif; letter-spacing: .12em; text-transform: uppercase; }
    h1, h2 { margin: 0; font-weight: 700; letter-spacing: 0; }
    h1 { max-width: 650px; font-size: clamp(2.2rem, 5vw, 4.2rem); line-height: .98; }
    h2 { font-size: 1.45rem; }
    .subtitle, .generated { color: var(--muted); }
    .subtitle { max-width: 620px; margin: 16px 0 0; font-size: 1.08rem; }
    .generated { text-align: right; font: 12px/1.5 Arial, sans-serif; white-space: nowrap; }
    .summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 28px; }
    .metric { background: var(--paper); border: 1px solid var(--line); border-top: 4px solid var(--teal); padding: 18px 20px; box-shadow: 0 8px 24px rgba(33, 58, 63, .07); }
    .metric.failed { border-top-color: var(--red); }
    .metric.skipped { border-top-color: #d39b20; }
    .metric-label { color: var(--muted); font: 700 11px/1.2 Arial, sans-serif; letter-spacing: .1em; text-transform: uppercase; }
    .metric-value { display: block; margin-top: 7px; font: 700 2rem/1 Arial, sans-serif; }
    .report-section { background: var(--paper); border: 1px solid var(--line); margin-top: 22px; box-shadow: 0 10px 28px rgba(33, 58, 63, .06); }
    .section-heading { display: flex; align-items: center; justify-content: space-between; gap: 18px; padding: 22px 24px; border-bottom: 1px solid var(--line); }
    .section-heading .eyebrow { margin: 0 0 7px; }
    .section-heading strong { color: var(--teal); font: 700 14px/1.2 Arial, sans-serif; white-space: nowrap; }
    .table-wrap { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; table-layout: fixed; }
    .col-number { width: 7%; }
    .col-name { width: 47%; }
    .col-suite { width: 21%; }
    .col-status { width: 14%; }
    .col-duration { width: 11%; }
    th { background: #f4f8f7; color: var(--muted); font-size: 11px; letter-spacing: .08em; text-align: left; text-transform: uppercase; }
    th, td { border-bottom: 1px solid var(--line); padding: 12px 16px; vertical-align: middle; }
    tbody tr:last-child td { border-bottom: 0; }
    tbody tr:hover { background: #fbfdfc; }
    .number, .duration { color: var(--muted); white-space: nowrap; }
    .number { text-align: center; }
    .duration { text-align: right; }
    .status-column, td:nth-child(4) { text-align: center; }
    .test-name { overflow-wrap: anywhere; font-weight: 600; }
    .status { display: inline-block; min-width: 76px; padding: 4px 9px; border-radius: 999px; font-size: 11px; font-weight: 700; text-align: center; }
    .status-passed { background: var(--teal-light); color: var(--teal); }
    .status-failed { background: var(--red-light); color: var(--red); }
    .status-skipped { background: var(--amber-light); color: var(--amber); }
    footer { color: var(--muted); margin-top: 28px; font: 12px/1.5 Arial, sans-serif; }
    @media (max-width: 720px) { .page { width: min(100% - 24px, 1180px); padding-top: 28px; } header { display: block; } .generated { margin-top: 16px; text-align: left; } .summary { grid-template-columns: repeat(2, 1fr); } .section-heading { align-items: flex-start; flex-direction: column; } table { min-width: 760px; } .table-wrap { padding-bottom: 4px; } }
  </style>
</head>
<body>
  <main class="page">
    <header>
      <div>
        <p class="kicker">Quality report / Tic Tac Toe</p>
        <h1>Unit test report</h1>
        <p class="subtitle">A combined view of backend and frontend test execution, generated from the latest local test artifacts.</p>
      </div>
      <p class="generated">Generated<br><strong>${escapeHtml(generatedAt)}</strong></p>
    </header>
    <section class="summary" aria-label="Test summary">
      <div class="metric"><span class="metric-label">Total tests</span><span class="metric-value">${summary.total}</span></div>
      <div class="metric"><span class="metric-label">Passed</span><span class="metric-value">${summary.passed}</span></div>
      <div class="metric failed"><span class="metric-label">Failed</span><span class="metric-value">${summary.failed}</span></div>
      <div class="metric skipped"><span class="metric-label">Skipped</span><span class="metric-value">${summary.skipped}</span></div>
    </section>
    ${renderSection('Backend', tests.filter((test) => test.suite === 'Backend'))}
    ${renderSection('Frontend', tests.filter((test) => test.suite !== 'Backend'))}
    <footer>Source artifacts: reports/backend/backend-tests.trx and reports/frontend/frontend-tests.xml</footer>
  </main>
</body>
</html>
`;

fs.mkdirSync(reportsDirectory, { recursive: true });
fs.writeFileSync(outputPath, report);
console.log(`HTML test report written to ${outputPath}`);
console.log(`Total: ${summary.total} | Passed: ${summary.passed} | Failed: ${summary.failed} | Skipped: ${summary.skipped}`);

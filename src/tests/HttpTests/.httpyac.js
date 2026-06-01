/**
 * httpYac v6 — BSOFT universal token + sentinel injection
 *
 * Slots:   11111111 → capturedIds[0]   (primary entity ID)
 *          22222222 → capturedIds[1]   (secondary entity ID)
 *          33333333 → capturedIds[2]   (tertiary entity ID)
 *
 * Named-region → slot mapping (add new files here):
 *   AccessPolicy.http   : createPolicy→0  assignRole→1
 *   Geography.http      : createCountry→0  createState→1  createCity→2
 *   Currency.http       : createCurrency→0
 *   MiscMaster.http     : createMiscType→0  createMiscItem→1
 */

var fs  = require('fs');
var LOG = 'f:\\MyProjects\\Development\\ModulerMonolithic\\BSOFT\\src\\tests\\HttpTests\\httpyac-debug.log';

function log(msg) {
  try { fs.appendFileSync(LOG, new Date().toISOString().slice(11, 23) + ' ' + msg + '\n'); } catch (_) {}
}

function replaceAll(str, from, to) { return str.split(from).join(to); }

function substituteBody(body, from, to) {
  if (body === null || body === undefined) return body;
  if (typeof body === 'string') return replaceAll(body, from, to);
  if (typeof Buffer !== 'undefined' && Buffer.isBuffer(body))
    return Buffer.from(replaceAll(body.toString('utf8'), from, to), 'utf8');
  if (Array.isArray(body)) {
    return body.map(function (line) {
      if (typeof line === 'string') return replaceAll(line, from, to);
      if (line && typeof line.value === 'string') { line.value = replaceAll(line.value, from, to); }
      return line;
    });
  }
  return body;
}

// Sentinel strings for each slot index
var SENTINELS = ['11111111', '22222222', '33333333'];

// Named HTTP region → capture slot index
var CAPTURE_MAP = {
  // AccessPolicy.http (slot 0 = policy ID, slot 1 = role-assignment ID)
  'createPolicy': 0,
  'assignRole':   1,

  // Geography.http (slot 0 = country, slot 1 = state, slot 2 = city)
  'createCountry': 0,
  'createState':   1,
  'createCity':    2,

  // Currency.http (slot 0 = currency ID)
  'createCurrency': 0,

  // MiscMaster.http (slot 0 = MiscTypeMaster ID, slot 1 = MiscMaster ID)
  'createMiscType': 0,
  'createMiscItem': 1,
};

module.exports = {
  configureHooks: function (api) {

    // Reset log inside configureHooks (not top-level) to survive hot-reloads
    try { fs.writeFileSync(LOG, '=== configureHooks ' + new Date().toISOString() + ' ===\n'); } catch (_) {}
    log('START');

    var capturedToken  = null;
    var capturedIds    = [null, null, null];   // slots 0, 1, 2

    // ── 1. onResponse ─────────────────────────────────────────────────────────
    try {
      api.hooks.onResponse.addHook('bsoftCapture', function (response, context) {
        try {
          var name   = context && context.httpRegion && context.httpRegion.metaData && context.httpRegion.metaData.name;
          var status = response && response.statusCode;
          log('RESP name=' + (name || '(unnamed)') + ' status=' + status);

          var body = null;
          try {
            if (response && response.parsedBody && typeof response.parsedBody === 'object') {
              body = response.parsedBody;
            } else if (response && typeof response.body === 'string' && response.body.trim().charAt(0) === '{') {
              body = JSON.parse(response.body);
            }
          } catch (_) {}

          if (!body) { log('  no parseable body'); return; }

          // Log full error when status is 5xx for debugging
          if (status >= 500) { log('  5xx body=' + JSON.stringify(body).slice(0, 500)); }

          // Login: capture JWT and reset all slots (new file run)
          if (name === 'login') {
            var tok = body.data && body.data.token;
            if (tok) {
              capturedToken = tok;
              capturedIds   = [null, null, null];
              log('  TOKEN captured len=' + tok.length);
            } else {
              log('  login no token: ' + JSON.stringify(body).slice(0, 120));
            }
          }

          // Slot capture for any named region in CAPTURE_MAP
          var slot = CAPTURE_MAP[name];
          if (slot !== undefined) {
            var captured = null;

            // Format A: ApiResponseDTO  { isSuccess: true, data: <int> }  (AccessPolicy pattern)
            if (body.isSuccess === true && typeof body.data === 'number' && body.data > 0) {
              captured = body.data;

            // Format B: controller wrapper  { statusCode, data: { id: <int>, ... } }  (Geography / MiscMaster pattern)
            } else if (body.data && typeof body.data === 'object' && typeof body.data.id === 'number' && body.data.id > 0) {
              captured = body.data.id;

            // Format C: direct DTO  { id: <int>, ... }  (fallback)
            } else if (typeof body.id === 'number' && body.id > 0) {
              captured = body.id;

            // Format D: wrapper with plain numeric data, no isSuccess  { ..., data: <int> }  (Currency pattern)
            } else if (typeof body.data === 'number' && body.data > 0) {
              captured = body.data;
            }

            if (captured !== null) {
              capturedIds[slot] = captured;
              log('  slot[' + slot + ']=' + captured + ' (from ' + name + ')');
            } else {
              log('  ' + name + ' NOT captured: body=' + JSON.stringify(body).slice(0, 120));
            }
          }

        } catch (e) { log('RESP ERROR: ' + e.message); }
      });
      log('onResponse.addHook registered');
    } catch (e) { log('onResponse.addHook FAILED: ' + e.message); }

    // ── 2. onRequest ──────────────────────────────────────────────────────────
    try {
      api.hooks.onRequest.addHook('bsoftInject', function (request, context) {
        var injected = [];
        try {
          if (!request) return request;

          // Token injection into Authorization header
          if (capturedToken && request.headers) {
            var hasAuth = Object.prototype.hasOwnProperty.call(request.headers, 'Authorization') ||
                          Object.prototype.hasOwnProperty.call(request.headers, 'authorization');
            if (hasAuth) {
              request.headers['Authorization'] = 'Bearer ' + capturedToken;
              if (Object.prototype.hasOwnProperty.call(request.headers, 'authorization'))
                delete request.headers['authorization'];
              injected.push('token');
            }
          }

          // Sentinel substitution for each slot
          for (var i = 0; i < 3; i++) {
            if (capturedIds[i] !== null) {
              var sentinel = SENTINELS[i];
              var val      = String(capturedIds[i]);

              if (request.url && request.url.indexOf(sentinel) !== -1) {
                request.url = replaceAll(request.url, sentinel, val);
                injected.push('id' + i + '→' + val);
              }
              if (request.body !== null && request.body !== undefined) {
                var nb = substituteBody(request.body, sentinel, val);
                if (nb !== request.body) { request.body = nb; injected.push('body.id' + i + '→' + val); }
              }
            }
          }

        } catch (e) { log('REQ ERROR: ' + e.message); }

        if (injected.length)
          log('REQ [' + injected.join(' | ') + '] ' + (request.url || '?').slice(-70));
        return request;
      });
      log('onRequest.addHook registered');
    } catch (e) { log('onRequest.addHook FAILED: ' + e.message); }

    log('DONE');
  },
};

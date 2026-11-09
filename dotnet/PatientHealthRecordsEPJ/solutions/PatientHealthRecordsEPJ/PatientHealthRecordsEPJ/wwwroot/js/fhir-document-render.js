// Renders a FHIR Bundle (or single resource) JSON string as a readable clinical document.
(function () {
	"use strict";

	function esc(s) {
		return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
			return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
		});
	}
	function cap(s) { return s ? s.charAt(0).toUpperCase() + s.slice(1) : s; }

	function ccText(cc) {
		if (!cc) return null;
		if (cc.text) return cc.text;
		if (cc.coding && cc.coding.length) return cc.coding[0].display || cc.coding[0].code || null;
		return null;
	}
	function firstCoding(cc) { return cc && cc.coding && cc.coding[0] ? cc.coding[0] : null; }
	var SYS_NAMES = {
		'http://loinc.org': 'LOINC',
		'http://snomed.info/sct': 'SNOMED CT',
		'http://hl7.org/fhir/sid/icd-10': 'ICD-10',
		'http://hl7.org/fhir/sid/icd-10-cm': 'ICD-10-CM',
		'http://hl7.org/fhir/sid/icd-9-cm': 'ICD-9-CM',
		'http://www.whocc.no/atc': 'ATC',
		'http://www.nlm.nih.gov/research/umls/rxnorm': 'RxNorm',
		'http://unitsofmeasure.org': 'UCUM',
		'http://terminology.hl7.org/CodeSystem/v2-0203': 'v2-0203',
		'http://terminology.hl7.org/CodeSystem/v3-ActReason': 'v3-ActReason'
	};
	function codingLabel(coding) {
		if (!coding) return null;
		var sys = SYS_NAMES[coding.system];
		if (!sys && coding.system) {
			try { sys = new URL(coding.system).hostname.replace(/^www\./, ''); } catch (e) { sys = coding.system; }
		}
		var parts = [sys, coding.code].filter(Boolean);
		return parts.length ? parts.join(' ') : null;
	}
	function primaryCC(r, index) {
		if (index) {
			if (r.medicationReference) {
				var med = resolveRef(r.medicationReference, index).resource;
				if (med && med.code) return med.code;
			}
			if (r.resourceType === 'DeviceUseStatement' && r.device) {
				var dev = resolveRef(r.device, index).resource;
				if (dev && dev.type) return dev.type;
			}
		}
		if (r.resourceType === 'CarePlan') {
			var cpd = r.activity && r.activity[0] && r.activity[0].detail;
			if (cpd && cpd.code) return cpd.code;
		}
		if (r.resourceType === 'Consent' && r.provision && r.provision.code && r.provision.code[0]) return r.provision.code[0];
		return r.code || r.type || r.vaccineCode || r.medicationCodeableConcept || null;
	}
	function humanName(n) {
		if (!n) return null;
		var given = (n.given || []).join(' ');
		var parts = [given, n.family].filter(Boolean);
		if (parts.length) return parts.join(' ');
		return n.text || null;
	}
	function formatAddress(a) {
		if (!a) return null;
		var line1 = (a.line || []).join(', ');
		var cityLine = [a.postalCode, a.city].filter(Boolean).join(' ');
		return [line1, cityLine, a.district, a.state, a.country].filter(Boolean).join(', ') || null;
	}

	function buildIndex(resources, fullUrls) {
		var index = {};
		resources.forEach(function (r, i) {
			if (!r) return;
			if (fullUrls && fullUrls[i]) index[fullUrls[i]] = r;
			if (r.resourceType && r.id) index[r.resourceType + '/' + r.id] = r;
		});
		return index;
	}
	function resolveRef(refObj, index) {
		if (!refObj) return { resource: null, display: null };
		var res = null;
		if (refObj.reference) {
			res = index[refObj.reference] || null;
			if (!res) {
				var parts = refObj.reference.split('/');
				if (parts.length === 2) {
					for (var k in index) {
						if (index[k].resourceType === parts[0] && index[k].id === parts[1]) { res = index[k]; break; }
					}
				}
			}
		}
		var display = refObj.display || (res ? getTitle(res, index) : null);
		return { resource: res, display: display };
	}

	function getTitle(r, index) {
		if (!r) return null;
		switch (r.resourceType) {
			case 'Patient': case 'RelatedPerson':
				return humanName(r.name && r.name[0]) || r.resourceType;
			case 'Practitioner': {
				var pn = r.name && r.name[0];
				var pfx = pn && pn.prefix ? pn.prefix.join(' ') : null;
				var hn = humanName(pn);
				return hn ? [pfx, hn].filter(Boolean).join(' ') : r.resourceType;
			}
			case 'PractitionerRole': {
				var who = r.practitioner ? resolveRef(r.practitioner, index).display : null;
				var where = r.organization ? resolveRef(r.organization, index).display : null;
				var role = r.code && r.code[0] ? ccText(r.code[0]) : null;
				var base = who || role || r.resourceType;
				return base + (who && role ? ' (' + role + ')' : '') + (where ? ' ved ' + where : '');
			}
			case 'Medication':
				return ccText(r.code) || 'Legemiddel';
			case 'CarePlan': {
				var cpd = r.activity && r.activity[0] && r.activity[0].detail;
				return r.title || (cpd && (cpd.description || ccText(cpd.code))) || r.description ||
					ccText(r.category && r.category[0]) || 'Behandlingsplan';
			}
			case 'Consent': {
				var ccat = ccText(r.category && r.category[0]) || 'Samtykke';
				var prov = r.provision;
				var pc = prov && prov.code ? prov.code.map(ccText).filter(Boolean).join(', ') : null;
				if (!pc) return ccat;
				return ccat + ': ' + (prov.type === 'deny' ? 'avslår ' : prov.type === 'permit' ? 'samtykker til ' : '') + pc;
			}
			case 'Organization':
				return r.name || r.resourceType;
			case 'AllergyIntolerance':
				return ccText(r.code) || 'Allergi / intoleranse';
			case 'Condition':
				return ccText(r.code) || 'Tilstand';
			case 'MedicationStatement': case 'MedicationRequest': {
				var mres = r.medicationReference ? resolveRef({ reference: r.medicationReference.reference }, index).resource : null;
				return ccText(r.medicationCodeableConcept) || (r.medicationReference && r.medicationReference.display) ||
					(mres && ccText(mres.code)) || 'Legemiddel';
			}
			case 'Procedure':
				return ccText(r.code) || 'Prosedyre';
			case 'Immunization':
				return ccText(r.vaccineCode) || 'Vaksine';
			case 'Device':
				return (r.deviceName && r.deviceName[0] && r.deviceName[0].name) || ccText(r.type) || 'Utstyr';
			case 'DeviceUseStatement':
				return (r.device && resolveRef(r.device, index).display) || 'Bruk av utstyr';
			case 'Observation':
				return ccText(r.code) || 'Observasjon';
			default:
				return ccText(r.code) || ccText(r.type) || (r.id ? r.resourceType + ' / ' + r.id : r.resourceType);
		}
	}

	function getDetail(r, index) {
		if (!r) return null;
		switch (r.resourceType) {
			case 'AllergyIntolerance': {
				var kind = [r.type ? cap(r.type) : null, (r.category || []).join(', ')].filter(Boolean).join(' · ');
				var reactions = (r.reaction || []).map(function (rx) {
					var m = (rx.manifestation || []).map(ccText).filter(Boolean).join(', ');
					return m + (rx.severity ? ' (' + rx.severity + ')' : '');
				}).filter(Boolean).join('; ');
				var aparts = [kind || null, reactions ? 'Reaksjon: ' + reactions : null].filter(Boolean);
				return aparts.length ? aparts.join(' · ') : null;
			}
			case 'MedicationStatement': case 'MedicationRequest': {
				var mr = r.medicationReference ? resolveRef(r.medicationReference, index).resource : null;
				var form = mr && mr.form ? ccText(mr.form) : null;
				var dosage = r.dosage && r.dosage[0] && (r.dosage[0].text);
				var reason = null;
				if (r.reasonReference && r.reasonReference[0]) reason = resolveRef(r.reasonReference[0], index).display;
				else if (r.reasonCode && r.reasonCode[0]) reason = ccText(r.reasonCode[0]);
				var parts = [form, dosage, reason ? 'for ' + reason : null].filter(Boolean);
				return parts.length ? parts.join(' · ') : null;
			}
			case 'Immunization': {
				var iparts = [r.lotNumber ? 'Lot ' + r.lotNumber : null, r.route ? ccText(r.route) : null].filter(Boolean);
				return iparts.length ? iparts.join(' · ') : null;
			}
			case 'CarePlan': {
				var reasons = [];
				(r.activity || []).forEach(function (a) {
					var d = a.detail; if (!d) return;
					(d.reasonCode || []).forEach(function (c) { reasons.push(ccText(c)); });
					(d.reasonReference || []).forEach(function (x) { reasons.push(resolveRef(x, index).display); });
				});
				(r.addresses || []).forEach(function (x) { reasons.push(resolveRef(x, index).display); });
				reasons = reasons.filter(function (x, i, arr) { return x && arr.indexOf(x) === i; });
				return reasons.length ? 'for ' + reasons.join(', ') : null;
			}
			case 'DiagnosticReport': {
				var results = (r.result || []).map(function (x) {
					var rr = resolveRef(x, index);
					if (!rr.resource) return rr.display;
					var v = obsValue(rr.resource);
					return (getTitle(rr.resource, index) || 'Resultat') + (v ? ': ' + v : '');
				}).filter(Boolean);
				var dparts = [r.conclusion || null].concat(results);
				dparts = dparts.filter(Boolean);
				return dparts.length ? dparts.join(' · ') : null;
			}
			case 'Procedure': {
				var performer = r.performer && r.performer[0] && r.performer[0].actor ? resolveRef(r.performer[0].actor, index).display : null;
				var outcome = r.outcome ? (r.outcome.text || ccText(r.outcome)) : null;
				var pparts = [performer ? 'utført av ' + performer : null, outcome].filter(Boolean);
				return pparts.length ? pparts.join(' · ') : null;
			}
			case 'DeviceUseStatement': {
				var dreason = r.reasonReference && r.reasonReference[0] ? resolveRef(r.reasonReference[0], index).display : null;
				var dv = r.device ? resolveRef(r.device, index).resource : null;
				var udi = dv && dv.udiCarrier && dv.udiCarrier[0] && dv.udiCarrier[0].deviceIdentifier;
				var duparts = [dreason ? 'for ' + dreason : null, udi ? 'UDI ' + udi : null].filter(Boolean);
				return duparts.length ? duparts.join(' · ') : null;
			}
			case 'Observation': {
				var val = obsValue(r);
				var comps = (r.component || []).map(function (c) {
					var cv = obsValue(c), ct = ccText(c.code);
					return ct ? ct + (cv ? ': ' + cv : '') : cv;
				}).filter(Boolean);
				var range = refRange(r);
				if (val && range) val += ' (ref ' + range + ')';
				var interp = r.interpretation && r.interpretation[0] ? ccText(r.interpretation[0]) : null;
				var members = (r.hasMember || []).map(function (m) {
					var mr2 = resolveRef(m, index).resource;
					if (!mr2) return null;
					var mv = obsValue(mr2);
					return (getTitle(mr2, index) || 'Relatert observasjon') + (mv ? ': ' + mv : '');
				}).filter(Boolean);
				var oparts = [val ? 'Resultat: ' + val : null, interp].concat(comps, members).filter(Boolean);
				return oparts.length ? oparts.join(' · ') : null;
			}
			default:
				return null;
		}
	}

	function obsValue(r) {
		if (r.valueQuantity) return [r.valueQuantity.value, r.valueQuantity.unit || r.valueQuantity.code].filter(function (x) { return x != null; }).join(' ');
		if (r.valueCodeableConcept) return ccText(r.valueCodeableConcept);
		if (r.valueInteger != null) return String(r.valueInteger);
		if (r.valueDateTime) return r.valueDateTime;
		if (r.valueString) return r.valueString;
		if (r.valueBoolean != null) return r.valueBoolean ? 'Ja' : 'Nei';
		return null;
	}

	function refRange(r) {
		if (!r.referenceRange || !r.referenceRange.length) return null;
		var low = null, high = null, text = null;
		r.referenceRange.forEach(function (rr) {
			if (rr.low && rr.low.value != null && !low) low = rr.low;
			if (rr.high && rr.high.value != null && !high) high = rr.high;
			if (rr.text && !text) text = rr.text;
		});
		var unit = (high && (high.unit || high.code)) || (low && (low.unit || low.code)) || '';
		var s = low && high ? low.value + '–' + high.value : low ? '≥ ' + low.value : high ? '≤ ' + high.value : null;
		if (!s) return text;
		return s + (unit ? ' ' + unit : '');
	}

	function getCode(r, index) {
		var cc = primaryCC(r, index);
		return codingLabel(firstCoding(cc));
	}

	var DATE_KEYS = [
		'onsetDateTime', 'effectiveDateTime', 'performedDateTime', 'occurrenceDateTime',
		'recordedDate', 'date', 'dateTime', 'authoredOn'
	];
	var PERIOD_KEYS = ['onsetPeriod', 'effectivePeriod', 'performedPeriod', 'occurrencePeriod', 'timingPeriod', 'period'];
	function bestDate(r) {
		var start = null, end = null, i;
		for (i = 0; i < DATE_KEYS.length; i++) { if (r[DATE_KEYS[i]]) { start = r[DATE_KEYS[i]]; break; } }
		if (!start) {
			for (i = 0; i < PERIOD_KEYS.length; i++) {
				var p = r[PERIOD_KEYS[i]];
				if (p && (p.start || p.end)) { start = p.start || null; end = p.end || null; break; }
			}
		}
		if (r.abatementDateTime) end = r.abatementDateTime;
		else if (r.abatementPeriod && (r.abatementPeriod.end || r.abatementPeriod.start)) end = r.abatementPeriod.end || r.abatementPeriod.start;
		if (start && end) return start + ' – ' + end;
		if (end) return 'til ' + end;
		return start;
	}
	function bestStatus(r) {
		if (r.criticality) return { label: cap(r.criticality), tone: r.criticality === 'high' ? 'high' : 'moderate' };
		var src = r.clinicalStatus ? ccText(r.clinicalStatus) : (typeof r.status === 'string' ? r.status : null);
		if (!src) return null;
		var tone = /active|completed|confirmed|final|current/i.test(src) ? 'moderate'
			: /inactive|cancelled|entered-in-error|stopped|error/i.test(src) ? 'high' : 'neutral';
		return { label: cap(src), tone: tone };
	}

	function pill(status) {
		if (!status) return '<span class="fhir-unresolved">—</span>';
		return '<span class="fhir-pill ' + esc(status.tone) + '">' + esc(status.label) + '</span>';
	}

	function renderRows(resources, index) {
		return resources.map(function (item) {
			if (!item.resource) {
				return '<tr><td class="fhir-unresolved">' + esc(item.display || item.reference || 'Uløst referanse') + '</td>' +
					'<td class="fhir-unresolved">—</td><td class="fhir-unresolved">—</td><td class="fhir-unresolved">—</td></tr>';
			}
			var r = item.resource;
			var title = getTitle(r, index) || r.resourceType;
			var detail = getDetail(r, index);
			var code = getCode(r, index);
			var status = bestStatus(r);
			var date = bestDate(r);
			return '<tr>' +
				'<td>' + esc(title) + (detail ? '<div class="fhir-note-row">' + esc(detail) + '</div>' : '') + '</td>' +
				'<td>' + (code ? '<span class="fhir-code">' + esc(code) + '</span>' : '<span class="fhir-unresolved">—</span>') + '</td>' +
				'<td>' + pill(status) + '</td>' +
				'<td>' + (date ? esc(date) : '<span class="fhir-unresolved">—</span>') + '</td>' +
				'</tr>';
		}).join('');
	}

	function renderTable(entries, index, emptyText) {
		if (!entries.length) return '<p class="fhir-unresolved">Ingen oppføringer' + (emptyText ? ': ' + esc(emptyText) : '.') + '</p>';
		return '<div style="overflow-x:auto"><table>' +
			'<thead><tr><th>Beskrivelse</th><th>Kode</th><th>Status</th><th>Dato</th></tr></thead>' +
			'<tbody>' + renderRows(entries, index) + '</tbody>' +
			'</table></div>';
	}

	function tagHas(resource, system, code) {
		if (!resource || !resource.meta || !resource.meta.tag) return false;
		return resource.meta.tag.some(function (t) { return t.code === code && (!system || t.system === system); });
	}
	function bundleIsTest(bundle, resources) {
		if (tagHas(bundle, 'http://terminology.hl7.org/CodeSystem/v3-ActReason', 'HTEST')) return true;
		return resources.some(function (r) { return tagHas(r, 'http://terminology.hl7.org/CodeSystem/v3-ActReason', 'HTEST'); });
	}

	function field(label, value) {
		if (!value) return '';
		return '<div class="fhir-field"><div class="fhir-label">' + esc(label) + '</div><div class="fhir-value">' + esc(value) + '</div></div>';
	}

	function buildDocHtml(parsed) {
		var isBundle = parsed && parsed.resourceType === 'Bundle';
		var resources = [];
		var fullUrls = [];
		if (isBundle) {
			(parsed.entry || []).forEach(function (e) {
				if (e && e.resource) { resources.push(e.resource); fullUrls.push(e.fullUrl); }
			});
		} else if (parsed && parsed.resourceType) {
			resources.push(parsed);
			fullUrls.push(null);
		} else {
			throw new Error('Gjenkjenner ikke dette som en FHIR-ressurs (mangler "resourceType").');
		}
		if (!resources.length) {
			return '<div class="fhir-empty">Denne Bundle-en har ingen ressurser å vise.</div>';
		}

		var index = buildIndex(resources, fullUrls);
		var composition = resources.find(function (r) { return r.resourceType === 'Composition'; });
		var isTest = bundleIsTest(isBundle ? parsed : {}, resources);

		var html = '';
		html += '<div class="fhir-banner">⚠ <span>' +
			(isTest
				? '<b>Syntetiske testdata.</b> Merket <code>HTEST</code> (HL7 v3-ActReason).'
				: '<b>Generert fra mottatt JSON.</b> Dette er en visning for gjennomgang, ikke klinisk rådgivning.')
			+ '</span></div>';

		var patient = null, author = null, custodian = null, docTitle = null, docSub = null, docDate = null, sections = [];

		if (composition) {
			patient = composition.subject ? resolveRef(composition.subject, index).resource : null;
			author = composition.author && composition.author[0] ? resolveRef(composition.author[0], index) : null;
			custodian = composition.custodian ? resolveRef(composition.custodian, index) : null;
			var typeText = ccText(composition.type);
			var typeCoding = firstCoding(composition.type);
			docTitle = composition.title || typeText || 'Dokument';
			docSub = [typeText, typeCoding ? codingLabel(typeCoding) : null].filter(Boolean).join(' · ');
			docDate = composition.date || (isBundle ? parsed.timestamp : null);
			sections = (composition.section || []).map(function (s) {
				return {
					title: s.title || ccText(s.code) || 'Seksjon',
					empty: s.emptyReason ? ccText(s.emptyReason) : null,
					entries: (s.entry || []).map(function (ref) { return Object.assign(resolveRef(ref, index), { reference: ref.reference }); })
				};
			});

			// Ressurser i Bundle-en som ingenting peker på (ikke i noen seksjon, ikke referert av annen ressurs).
			var referenced = {};
			var collectRefs = function (o) {
				if (!o || typeof o !== 'object') return;
				if (Array.isArray(o)) { o.forEach(collectRefs); return; }
				if (typeof o.reference === 'string') referenced[o.reference] = true;
				for (var k in o) { if (k !== 'text' && Object.prototype.hasOwnProperty.call(o, k)) collectRefs(o[k]); }
			};
			resources.forEach(collectRefs);
			var refKeys = Object.keys(referenced);
			var orphans = [];
			resources.forEach(function (r, i) {
				if (r === composition) return;
				var local = r.resourceType + '/' + r.id;
				var hit = (fullUrls[i] && referenced[fullUrls[i]]) || (r.id && referenced[local]) ||
					(r.id && refKeys.some(function (ref) { return ref.slice(-(local.length + 1)) === '/' + local; }));
				if (!hit) orphans.push({ resource: r, display: getTitle(r, index), reference: fullUrls[i] });
			});
			if (orphans.length) {
				sections.push({
					title: 'Ikke oppført i noen seksjon',
					note: 'Disse ressursene finnes i Bundle-en, men ingen seksjon i Composition eller annen ressurs refererer til dem.',
					entries: orphans
				});
			}
		} else {
			if (!isBundle) {
				patient = parsed.resourceType === 'Patient' ? parsed : null;
			} else {
				patient = resources.find(function (r) { return r.resourceType === 'Patient'; }) || null;
			}
			docTitle = isBundle ? 'FHIR Bundle' : (resources[0].resourceType + '-ressurs');
			docSub = isBundle
				? resources.length + ' ressurs(er) · fant ingen Composition — gruppert etter type'
				: 'Enkeltressurs, vist alene';
			docDate = isBundle ? parsed.timestamp : null;

			var groups = {};
			resources.forEach(function (r, i) {
				if (r === patient) return;
				var key = r.resourceType || 'Ressurs';
				if (!groups[key]) groups[key] = [];
				groups[key].push({ resource: r, display: getTitle(r, index), reference: fullUrls[i] });
			});
			sections = Object.keys(groups).sort().map(function (k) {
				return { title: k + (groups[k].length === 1 ? '' : 's'), entries: groups[k] };
			});
		}

		html += '<h2>' + esc(docTitle) + '</h2>';
		if (docSub) html += '<div class="fhir-sub" style="margin-bottom:18px">' + esc(docSub) + '</div>';

		var idFields = '';
		if (patient) {
			var pname = humanName(patient.name && patient.name[0]);
			var pdemo = [pname, patient.gender, patient.birthDate ? 'f. ' + patient.birthDate : null].filter(Boolean).join(' · ');
			var pid = (patient.identifier || []).map(function (id) {
				var t = id.type ? (ccText(id.type) || (id.type.text)) : null;
				return [id.value, t ? '(' + t + ')' : null].filter(Boolean).join(' ');
			}).filter(Boolean).join('; ');
			var paddr = formatAddress(patient.address && patient.address[0]);
			idFields += field('Pasient', pdemo);
			idFields += field('Identifikator', pid);
			idFields += field('Adresse', paddr);
		}
		if (author) idFields += field('Forfatter', author.display);
		if (custodian) idFields += field('Ansvarlig', custodian.display);
		if (docDate) idFields += field('Dokumentdato', docDate);

		if (idFields) {
			html += '<div class="fhir-id-band">' + idFields + '</div>';
		}

		if (!sections.length) {
			html += '<div class="fhir-empty">Fant ingen seksjoner eller grupperbare ressurser.</div>';
		}
		sections.forEach(function (sec) {
			html += '<section class="fhir-section">' +
				'<div class="fhir-section-head"><h2>' + esc(sec.title) + '</h2><span class="fhir-count">' + sec.entries.length + ' oppføring' + (sec.entries.length === 1 ? '' : 'er') + '</span></div>' +
				(sec.note ? '<p class="fhir-sub" style="margin:0 0 8px">' + esc(sec.note) + '</p>' : '') +
				renderTable(sec.entries, index, sec.empty) +
				'</section>';
		});

		var profiles = isBundle && parsed.meta && parsed.meta.profile ? parsed.meta.profile.join(', ') : null;
		html += '<div class="fhir-meta-foot">' +
			'<span>' + esc(isBundle ? ('Bundle id: ' + (parsed.id || '—')) : ('Ressurs-id: ' + (parsed.id || '—'))) + ' · ' + resources.length + ' ressurs' + (resources.length === 1 ? '' : 'er') + '</span>' +
			(profiles ? '<span>' + esc(profiles) + '</span>' : '') +
			'</div>';

		return html;
	}

	window.tryRenderFhir = function (jsonText, containerId) {
		var container = document.getElementById(containerId);
		try {
			var parsed = JSON.parse(jsonText);
			container.innerHTML = buildDocHtml(parsed);
			container.style.display = 'block';
		} catch (e) {
			container.innerHTML = '<div class="fhir-empty">Kunne ikke vise dokumentet: ' + esc(e.message) + '</div>';
			container.style.display = 'block';
		}
	};
})();

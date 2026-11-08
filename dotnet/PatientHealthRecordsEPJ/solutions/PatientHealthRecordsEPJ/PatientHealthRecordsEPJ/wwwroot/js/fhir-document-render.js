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
	function primaryCC(r) {
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
			case 'Patient': case 'Practitioner': case 'RelatedPerson':
				return humanName(r.name && r.name[0]) || r.resourceType;
			case 'PractitionerRole':
				return (r.practitioner && resolveRef(r.practitioner, index).display) || r.resourceType;
			case 'Organization':
				return r.name || r.resourceType;
			case 'AllergyIntolerance':
				return ccText(r.code) || 'Allergi / intoleranse';
			case 'Condition':
				return ccText(r.code) || 'Tilstand';
			case 'MedicationStatement': case 'MedicationRequest':
				return ccText(r.medicationCodeableConcept) || (r.medicationReference && r.medicationReference.display) || 'Legemiddel';
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
				var reactions = (r.reaction || []).map(function (rx) {
					var m = (rx.manifestation || []).map(ccText).filter(Boolean).join(', ');
					return m + (rx.severity ? ' (' + rx.severity + ')' : '');
				}).filter(Boolean).join('; ');
				return reactions ? 'Reaksjon: ' + reactions : null;
			}
			case 'MedicationStatement': case 'MedicationRequest': {
				var dosage = r.dosage && r.dosage[0] && (r.dosage[0].text);
				var reason = null;
				if (r.reasonReference && r.reasonReference[0]) reason = resolveRef(r.reasonReference[0], index).display;
				else if (r.reasonCode && r.reasonCode[0]) reason = ccText(r.reasonCode[0]);
				var parts = [dosage, reason ? 'for ' + reason : null].filter(Boolean);
				return parts.length ? parts.join(' · ') : null;
			}
			case 'Procedure': {
				var performer = r.performer && r.performer[0] && r.performer[0].actor ? resolveRef(r.performer[0].actor, index).display : null;
				var outcome = r.outcome ? (r.outcome.text || ccText(r.outcome)) : null;
				var pparts = [performer ? 'utført av ' + performer : null, outcome].filter(Boolean);
				return pparts.length ? pparts.join(' · ') : null;
			}
			case 'DeviceUseStatement': {
				var dreason = r.reasonReference && r.reasonReference[0] ? resolveRef(r.reasonReference[0], index).display : null;
				return dreason ? 'for ' + dreason : null;
			}
			default:
				return null;
		}
	}

	function getCode(r) {
		var cc = primaryCC(r);
		return codingLabel(firstCoding(cc));
	}

	var DATE_KEYS = [
		'onsetDateTime', 'effectiveDateTime', 'performedDateTime', 'occurrenceDateTime',
		'recordedDate', 'date', 'authoredOn'
	];
	var PERIOD_KEYS = ['onsetPeriod', 'effectivePeriod', 'performedPeriod', 'occurrencePeriod', 'timingPeriod', 'period'];
	function bestDate(r) {
		for (var i = 0; i < DATE_KEYS.length; i++) { if (r[DATE_KEYS[i]]) return r[DATE_KEYS[i]]; }
		for (var j = 0; j < PERIOD_KEYS.length; j++) {
			var p = r[PERIOD_KEYS[j]];
			if (p && p.start) return p.start;
		}
		return null;
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
			var code = getCode(r);
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

	function renderTable(entries, index) {
		if (!entries.length) return '<p class="fhir-unresolved">Ingen oppføringer.</p>';
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
					entries: (s.entry || []).map(function (ref) { return Object.assign(resolveRef(ref, index), { reference: ref.reference }); })
				};
			});
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
				renderTable(sec.entries, index) +
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

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ============================================================================
// DATEPICKER – Flatpickr wrapper s hr/en lokalizacijom
// ============================================================================

function initDatePicker(config) {
    var displayInput = document.getElementById(config.fieldName + '-fp');
    var isoInput     = document.getElementById(config.fieldName);

    if (!displayInput || !isoInput) return;

    // Kultura s poslužitelja (postavljeno via UseRequestLocalization) ima prioritet.
    // navigator.language je fallback ako kultura nije proslijeđena.
    var lang       = (config.culture || navigator.language || navigator.userLanguage || 'en').toLowerCase();
    var isCroatian = lang.startsWith('hr');

    // Format prikazan korisniku (ovisno o lokalizaciji)
    var dispFormat = config.withTime
        ? (isCroatian ? 'd.m.Y H:i' : 'm/d/Y h:i K')
        : (isCroatian ? 'd.m.Y'     : 'm/d/Y');

    // ISO format koji se šalje serveru (uvijek 24h bez AM/PM)
    var isoFormat = config.withTime ? 'Y-m-dTH:i' : 'Y-m-d';

    var fpConfig = {
        enableTime:   config.withTime,
        time_24hr:    isCroatian,   // HR: 24h prikaz, EN: 12h AM/PM
        dateFormat:   dispFormat,
        locale:       isCroatian ? 'hr' : 'default',
        allowInput:   false,
        onChange: function (selectedDates, dateStr, instance) {
            if (selectedDates.length > 0) {
                isoInput.value = instance.formatDate(selectedDates[0], isoFormat);
                _dpClearError(config.fieldName, displayInput);
            } else {
                isoInput.value = '';
            }
        },
        onClose: function (selectedDates) {
            // Blur-like validacija: prikaži grešku ako je polje prazno
            if (selectedDates.length === 0 && isoInput.value === '') {
                _dpSetError(config.fieldName, displayInput);
            }
        }
    };

    if (config.minDate) fpConfig.minDate = config.minDate;
    if (config.maxDate) fpConfig.maxDate = config.maxDate;

    // Postavi početnu vrijednost prije inicijalizacije (Flatpickr čita defaultDate)
    if (config.initialValue) fpConfig.defaultDate = config.initialValue;

    // Postavi lokaliziran placeholder na vidljivom inputu
    displayInput.placeholder = config.withTime
        ? (isCroatian ? 'dd.mm.gggg hh:mm' : 'mm/dd/yyyy hh:mm AM')
        : (isCroatian ? 'dd.mm.gggg'        : 'mm/dd/yyyy');

    flatpickr(displayInput, fpConfig);

    // Postavi ISO input na početnu vrijednost (Flatpickr je ne mijenja)
    if (config.initialValue) isoInput.value = config.initialValue;

    // Ako je server vratio validation grešku, označi i vidljivi input
    if (isoInput.classList.contains('input-validation-error')) {
        displayInput.classList.add('input-validation-error');
    }

    // Validacija pri submit ako je ISO prazan
    var form = displayInput.closest('form');
    if (form) {
        form.addEventListener('submit', function () {
            if (!isoInput.value) _dpSetError(config.fieldName, displayInput);
        });
    }
}

function _dpSetError(fieldName, displayInput) {
    if (displayInput) displayInput.classList.add('input-validation-error');
    var span = document.querySelector('[data-valmsg-for="' + fieldName + '"]');
    if (span && !span.textContent.trim()) {
        span.textContent = 'Datum je obavezan.';
        span.className   = 'validation-text field-validation-error';
    }
}

function _dpClearError(fieldName, displayInput) {
    if (displayInput) displayInput.classList.remove('input-validation-error');
    var span = document.querySelector('[data-valmsg-for="' + fieldName + '"]');
    if (span) {
        span.textContent = '';
        span.className   = 'validation-text field-validation-valid';
    }
}

// ============================================================================
// REUSABLE AUTOCOMPLETE FUNKCIJA
// ============================================================================

function initAutocomplete(searchInputId, resultsId, hiddenInputName, endpoint) {
    var searchInput = document.getElementById(searchInputId);
    var resultsBox  = document.getElementById(resultsId);
    var hiddenInput = document.querySelector('input[name="' + hiddenInputName + '"]');

    if (!searchInput || !resultsBox || !hiddenInput) return;

    // ARIA setup
    searchInput.setAttribute('role', 'combobox');
    searchInput.setAttribute('aria-expanded', 'false');
    searchInput.setAttribute('aria-controls', resultsId);
    searchInput.setAttribute('aria-autocomplete', 'list');
    searchInput.setAttribute('aria-haspopup', 'listbox');
    resultsBox.setAttribute('role', 'listbox');

    var debounceTimer = null;
    var activeIndex   = -1;

    function getItems() {
        return resultsBox.querySelectorAll('.autocomplete-item');
    }

    function setActive(index) {
        var items = getItems();
        items.forEach(function (item, i) {
            var on = (i === index);
            item.classList.toggle('autocomplete-item-active', on);
            item.setAttribute('aria-selected', on ? 'true' : 'false');
        });
        activeIndex = index;
        if (index >= 0 && items[index]) {
            searchInput.setAttribute('aria-activedescendant', items[index].id);
        } else {
            searchInput.removeAttribute('aria-activedescendant');
        }
    }

    function selectItem(item) {
        hiddenInput.value  = item.dataset.id;
        searchInput.value  = item.textContent.trim();
        resultsBox.innerHTML = '';
        activeIndex = -1;
        searchInput.setAttribute('aria-expanded', 'false');
        searchInput.removeAttribute('aria-activedescendant');
    }

    function closeDropdown() {
        resultsBox.innerHTML = '';
        activeIndex = -1;
        searchInput.setAttribute('aria-expanded', 'false');
        searchInput.removeAttribute('aria-activedescendant');
    }

    // Input s debounce-om (300 ms)
    searchInput.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        activeIndex = -1;
        var q = this.value.trim();

        if (q.length < 2) {
            closeDropdown();
            hiddenInput.value = '';   // Briše ID kad korisnik briše tekst
            return;
        }

        debounceTimer = setTimeout(async function () {
            try {
                var res  = await fetch(endpoint + '?q=' + encodeURIComponent(q));
                var data = await res.json();

                if (Array.isArray(data) && data.length > 0) {
                    resultsBox.innerHTML = data.map(function (item, i) {
                        return '<div class="autocomplete-item" role="option"' +
                               ' id="' + resultsId + '-item-' + i + '"' +
                               ' data-id="' + item.id + '"' +
                               ' aria-selected="false">' +
                               item.naziv + '</div>';
                    }).join('');
                } else {
                    resultsBox.innerHTML = '<div class="autocomplete-empty">Nema rezultata</div>';
                }
                searchInput.setAttribute('aria-expanded', 'true');
            } catch (error) {
                console.error('Autocomplete error:', error);
                resultsBox.innerHTML = '<div class="autocomplete-empty">Greška pri pretrazi</div>';
                searchInput.setAttribute('aria-expanded', 'true');
            }
        }, 300);
    });

    // Tipkovnička navigacija
    searchInput.addEventListener('keydown', function (e) {
        var items = getItems();
        if (!items.length) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setActive(Math.min(activeIndex + 1, items.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setActive(Math.max(activeIndex - 1, 0));
        } else if (e.key === 'Enter') {
            e.preventDefault();
            var target = activeIndex >= 0 ? items[activeIndex] : items[0];
            if (target) selectItem(target);
        } else if (e.key === 'Escape') {
            closeDropdown();
        }
    });

    // Klik na rezultat
    resultsBox.addEventListener('click', function (e) {
        var item = e.target.closest('.autocomplete-item');
        if (item) selectItem(item);
    });

    // Zatvori dropdown na blur
    searchInput.addEventListener('blur', function () {
        setTimeout(closeDropdown, 200);
    });
}

// ============================================================================
// GENERIC INDEX SEARCH FUNKCIJA (za filtriranje listi)
// ============================================================================

function initIndexSearch(searchInputId, resultsId, endpoint) {
    const searchInput = document.getElementById(searchInputId);
    const resultsContainer = document.getElementById(resultsId);

    if (!searchInput || !resultsContainer) {
        return; // Search elementi nisu dostupni na ovoj stranici
    }

    searchInput.addEventListener('input', async function () {
        const q = this.value.trim();

        if (q.length < 2) {
            resultsContainer.innerHTML = '';
            resultsContainer.style.display = 'none';
            return;
        }

        try {
            const res = await fetch(`${endpoint}?q=${encodeURIComponent(q)}`);
            const data = await res.json();

            if (Array.isArray(data) && data.length > 0) {
                resultsContainer.innerHTML = data.map(item =>
                    `<div class="search-item" data-id="${item.id}">${item.naziv}</div>`
                ).join('');
                resultsContainer.style.display = 'block';

                // Click handler
                resultsContainer.querySelectorAll('.search-item').forEach(item => {
                    item.addEventListener('click', function () {
                        window.location.href = `/${endpoint.split('/')[1]}/${this.dataset.id}`;
                    });
                });
            } else {
                resultsContainer.innerHTML = '<div class="search-empty">Nema rezultata</div>';
                resultsContainer.style.display = 'block';
            }
        } catch (error) {
            console.error('Search error:', error);
            resultsContainer.innerHTML = '<div class="search-empty">Greška pri pretrazi</div>';
            resultsContainer.style.display = 'block';
        }
    });

    // Clear results on blur
    searchInput.addEventListener('blur', function () {
        setTimeout(() => {
            resultsContainer.innerHTML = '';
            resultsContainer.style.display = 'none';
        }, 200);
    });
}

// ============================================================================
// DOM SWAP HELPERS (za AJAX filter)
// ============================================================================

function _swapEl(sel, doc) {
    var nw  = doc.querySelector(sel);
    var old = document.querySelector(sel);
    if (nw && old) old.outerHTML = nw.outerHTML;
}

function _swapElGroup(selectors, doc) {
    var newEl = null, oldEl = null;
    for (var i = 0; i < selectors.length; i++) {
        if (!newEl) newEl = doc.querySelector(selectors[i]);
        if (!oldEl) oldEl = document.querySelector(selectors[i]);
        if (newEl && oldEl) break;
    }
    if (newEl && oldEl) oldEl.outerHTML = newEl.outerHTML;
}

function _animateNewCards(sel) {
    document.querySelectorAll(sel + ':not(.visible)').forEach(function (card, i) {
        setTimeout(function () { card.classList.add('visible'); }, i * 60 + 40);
    });
}

function _animirajBrojeve() {
    document.querySelectorAll('.animirani-broj').forEach(function (el) {
        if (el.textContent.trim() !== '0') return;
        var cilj = parseFloat(el.dataset.cilj);
        if (isNaN(cilj)) return;
        var isDecimal = (el.dataset.cilj.indexOf('.') !== -1);
        var dur = 800, t0 = performance.now();
        (function frame(now) {
            var p = Math.min((now - t0) / dur, 1);
            var e = 1 - Math.pow(1 - p, 3);
            el.textContent = isDecimal ? (e * cilj).toFixed(1) : Math.round(e * cilj);
            if (p < 1) requestAnimationFrame(frame);
        })(performance.now());
    });
}

// ============================================================================
// TRENING FILTER FUNKCIJA — AJAX (fetch + DOM update bez reload-a)
// ============================================================================

async function applyTrainingFilters() {
    var userId = (document.getElementById('userId') || {}).value || '';
    var vrsta  = (document.getElementById('vrsta')  || {}).value || '';
    var period = (document.getElementById('period') || {}).value || '';
    var sort   = (document.getElementById('sort')   || {}).value || '';

    var params = new URLSearchParams();
    if (userId) params.append('userId', userId);
    if (vrsta)  params.append('vrsta', vrsta);
    if (period && period !== '0') params.append('period', period);
    if (sort && sort !== 'date_desc') params.append('sort', sort);

    var url = '/treninzi' + (params.toString() ? '?' + params.toString() : '');

    try {
        var res  = await fetch(url);
        var html = await res.text();
        var doc  = (new DOMParser()).parseFromString(html, 'text/html');

        _swapEl('.training-summary-grid', doc);
        _swapEl('.training-count', doc);
        _swapElGroup(['.training-grid', '.empty-state'], doc);

        history.pushState({}, '', url);
        _animateNewCards('.trening-kartica');
        setTimeout(_animirajBrojeve, 80);
    } catch (e) {
        window.location.href = url;
    }
}

// ============================================================================
// GLOBAL SEARCH — Ctrl+K overlay pretraga stranica i podataka
// ============================================================================

(function () {
    'use strict';

    // Statični popis stranica (pretraživanje na klijentu)
    var PAGES = [
        { naziv: 'Početna',  sub: 'Nadzorna ploča',    url: '/',          ikona: 'fa-home' },
        { naziv: 'Treninzi', sub: 'Lista treninga',     url: '/treninzi',  ikona: 'fa-dumbbell' },
        { naziv: 'Vježbe',   sub: 'Katalog vježbi',     url: '/vjezbe',    ikona: 'fa-running' },
        { naziv: 'Planovi',  sub: 'Trenažni planovi',   url: '/planovi',   ikona: 'fa-clipboard-list' },
        { naziv: 'Ciljevi',  sub: 'Fitness ciljevi',    url: '/ciljevi',   ikona: 'fa-bullseye' },
        { naziv: 'Mjerenja', sub: 'Tjelesna mjerenja',  url: '/mjerenja',  ikona: 'fa-balance-scale' },
        { naziv: 'Korisnici',sub: 'Lista korisnika',    url: '/korisnici', ikona: 'fa-users' },
    ];

    var overlay   = document.getElementById('gs-overlay');
    var input     = document.getElementById('gs-input');
    var results   = document.getElementById('gs-results');
    var trigger   = document.getElementById('gs-trigger');
    var mobileBtn = document.getElementById('gs-mobile-btn');

    if (!overlay || !input || !results) return;

    var debounceTimer = null;
    var activeIdx     = -1;
    var currentItems  = [];

    // ── OTVARANJE / ZATVARANJE ────────────────────────────────────────────────

    function open() {
        overlay.hidden = false;
        input.value = '';
        activeIdx = -1;
        renderHint();
        setTimeout(function () { input.focus(); }, 30);
    }

    function close() {
        overlay.hidden = true;
        input.blur();
    }

    // ── PRIKAZ SADRŽAJA REZULTATA ─────────────────────────────────────────────

    function renderHint() {
        results.innerHTML =
            '<div class="gs-empty-hint">' +
            '<i class="fas fa-keyboard"></i>' +
            '<span>Počnite tipkati za pretragu&hellip;</span>' +
            '</div>';
        currentItems = [];
    }

    function renderNoResults() {
        results.innerHTML =
            '<div class="gs-no-results">' +
            '<i class="fas fa-search-minus"></i>' +
            '<span>Nema rezultata za traženi pojam.</span>' +
            '</div>';
        currentItems = [];
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function highlight(text, q) {
        if (!q) return escapeHtml(text);
        var re = new RegExp('(' + q.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'gi');
        return escapeHtml(text).replace(re, '<mark>$1</mark>');
    }

    function buildItem(url, ikona, naziv, sub, q) {
        return '<a class="gs-item" href="' + escapeHtml(url) + '" role="option" tabindex="-1">' +
            '<span class="gs-item-icon"><i class="fas ' + escapeHtml(ikona) + '"></i></span>' +
            '<span class="gs-item-text">' +
            '<span class="gs-item-name">' + highlight(naziv, q) + '</span>' +
            (sub ? '<span class="gs-item-sub">' + highlight(sub, q) + '</span>' : '') +
            '</span>' +
            '<i class="fas fa-arrow-right gs-item-arrow"></i>' +
            '</a>';
    }

    function buildGroup(label, ikona, items, q, ikona_item, subKey) {
        if (!items || items.length === 0) return '';
        var html = '<div class="gs-group">' +
            '<div class="gs-group-label"><i class="fas ' + ikona + '"></i>' + label + '</div>';
        items.forEach(function (item) {
            html += buildItem(item.url, ikona_item, item.naziv, item[subKey] || '', q);
        });
        html += '</div>';
        return html;
    }

    function render(q, data) {
        // Filtriraj stranice na klijentu
        var lq = q.toLowerCase();
        var pages = PAGES.filter(function (p) {
            return p.naziv.toLowerCase().includes(lq) || p.sub.toLowerCase().includes(lq);
        });

        var html = '';
        if (pages.length > 0) {
            html += '<div class="gs-group">' +
                '<div class="gs-group-label"><i class="fas fa-compass"></i>Stranice</div>';
            pages.forEach(function (p) {
                html += buildItem(p.url, p.ikona, p.naziv, p.sub, q);
            });
            html += '</div>';
        }

        html += buildGroup('Vježbe',   'fa-dumbbell',     data.vjezbe,    q, 'fa-running',       'opis');
        html += buildGroup('Planovi',  'fa-clipboard-list', data.planovi, q, 'fa-clipboard-list', 'opis');
        html += buildGroup('Korisnici','fa-users',         data.korisnici, q, 'fa-user-circle',   'opis');

        if (!html) {
            renderNoResults();
            return;
        }

        results.innerHTML = html;

        // Skupi sve <a> elemente za navigaciju tipkovnicom
        currentItems = Array.from(results.querySelectorAll('.gs-item'));
        activeIdx = -1;
    }

    // ── TIPKOVNIČKA NAVIGACIJA ─────────────────────────────────────────────────

    function setActive(idx) {
        currentItems.forEach(function (el, i) {
            el.classList.toggle('gs-active', i === idx);
            if (i === idx) el.scrollIntoView({ block: 'nearest' });
        });
        activeIdx = idx;
    }

    // ── FETCH S DEBOUNCE-OM ───────────────────────────────────────────────────

    function doSearch(q) {
        clearTimeout(debounceTimer);
        if (!q || q.length < 1) { renderHint(); return; }

        debounceTimer = setTimeout(function () {
            fetch('/api/pretraga?q=' + encodeURIComponent(q))
                .then(function (r) { return r.json(); })
                .then(function (data) { render(q, data); })
                .catch(function () { renderNoResults(); });
        }, 220);
    }

    // ── EVENT LISTENERI ───────────────────────────────────────────────────────

    // Input
    input.addEventListener('input', function () { doSearch(this.value.trim()); });

    // Tipkovnica unutar inputa
    input.addEventListener('keydown', function (e) {
        if (!currentItems.length) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setActive(Math.min(activeIdx + 1, currentItems.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setActive(Math.max(activeIdx - 1, 0));
        } else if (e.key === 'Enter') {
            e.preventDefault();
            var target = activeIdx >= 0 ? currentItems[activeIdx] : currentItems[0];
            if (target) window.location.href = target.getAttribute('href');
        }
    });

    // Klik na stavku
    results.addEventListener('click', function (e) {
        var item = e.target.closest('.gs-item');
        if (item) window.location.href = item.getAttribute('href');
    });

    // Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !overlay.hidden) close();
    });

    // Ctrl+K (ili Cmd+K na Macu)
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            if (overlay.hidden) open(); else close();
        }
    });

    // Klik na backdrop (izvan modala)
    overlay.addEventListener('click', function (e) {
        if (e.target === overlay) close();
    });

    // Escape tipka na <kbd class="gs-esc-hint">
    var escHint = overlay.querySelector('.gs-esc-hint');
    if (escHint) escHint.addEventListener('click', close);

    // Sidebar trigger
    if (trigger) trigger.addEventListener('click', open);

    // Mobile button
    if (mobileBtn) mobileBtn.addEventListener('click', open);

})();

// ============================================================================
// MJERENJE FILTER FUNKCIJA — AJAX (fetch + DOM update bez reload-a)
// ============================================================================

async function applyMjerenjeFilter() {
    var userId = (document.getElementById('userId-mjerenje') || {}).value || '';
    var url = '/mjerenja' + (userId ? '?userId=' + userId : '');

    var kartice = document.querySelectorAll('.mjerenje-kartica');
    kartice.forEach(function (k, i) {
        setTimeout(function () {
            k.style.transition = 'opacity 0.2s ease, transform 0.2s ease';
            k.style.opacity = '0';
            k.style.transform = 'translateY(-12px) scale(0.98)';
        }, i * 20);
    });

    await new Promise(function (resolve) { setTimeout(resolve, kartice.length * 20 + 200); });

    try {
        var res  = await fetch(url);
        var html = await res.text();
        var doc  = (new DOMParser()).parseFromString(html, 'text/html');

        _swapEl('.measurements-count', doc);
        _swapElGroup(['.measurements-grid', '.empty-state'], doc);

        history.pushState({}, '', url);
        _animateNewCards('.mjerenje-kartica');
        setTimeout(_animirajBrojeve, 80);
    } catch (e) {
        window.location.href = url;
    }
}

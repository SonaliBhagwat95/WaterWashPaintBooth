/* ============================================================
   Bullows PaintBooth Pro — bo.js
   JS for BO Calculations Create / Edit page
   Additional BO table = 7 columns:
   0 Item | 1 Description | 2 Item Code | 3 Qty | 4 Rate | 5 Amount | 6 Action
   ============================================================ */

/* ════════════════════════════════════════════════════════
   1. AUTOCOMPLETE
════════════════════════════════════════════════════════ */
function initAutocomplete(searchUrl) {
    $('#txtEnquiryCode').on('input', function () {
        var term = $.trim($(this).val());
        if (term.length < 1) {
            $('#boDropdown').hide().empty();
            return;
        }
        $.post(searchUrl, { enquiryCode: term }, function (data) {
            var $dd = $('#boDropdown').empty();
            if (data.success && data.results.length > 0) {
                $.each(data.results, function (i, code) {
                    $('<div class="bo-dropdown-item">').text(code).appendTo($dd);
                });
                $dd.show();
            } else {
                $dd.hide();
            }
        });
    });

    $(document).on('click', '.bo-dropdown-item', function () {
        $('#txtEnquiryCode').val($(this).text());
        $('#boDropdown').hide().empty();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtEnquiryCode, #boDropdown').length)
            $('#boDropdown').hide();
    });
}


/* ════════════════════════════════════════════════════════
   2. SEARCH — GET redirect with enquiryCode in query string
   Matches [HttpGet] BOCalculations_Create(string enquiryCode)
════════════════════════════════════════════════════════ */
function initSearch(boUrl) {
    $('#btnSearch').on('click', function () {
        var code = $.trim($('#txtEnquiryCode').val());
        if (!code) {
            PB.toast('Please enter an enquiry code.', 'warning');
            return;
        }

        var $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Searching...');

        window.location.href = boUrl + '?enquiryCode=' + encodeURIComponent(code);

        setTimeout(function () {
            $btn.prop('disabled', false)
                .html('<i class="bi bi-search"></i> Search');
        }, 3000);
    });

    // Enter key triggers search
    $('#txtEnquiryCode').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnSearch').trigger('click');
        }
    });
}


/* ════════════════════════════════════════════════════════
   3. COLLAPSIBLE SECTIONS
════════════════════════════════════════════════════════ */
function initSections() {
    $(document).on('click', '.bo-section-header', function () {
        var $header = $(this);
        var $body = $header.next('.bo-section-body');
        $header.toggleClass('active');
        $body.toggleClass('open');
    });

    // Only open first section on CREATE mode — edit mode opens all via inline script
    if (!$('#hiddenEnquiryCode').val()) {
        $('.bo-section-header').first().trigger('click');
    }
}


/* ════════════════════════════════════════════════════════
   4. COST CALCULATIONS
   Rate × Quantity = Cost   (Light + Exhaust)
   Rate × Area     = Cost   (Paint Arrested Filter)
   Item-code → rate fetch is handled by the inline script in the view
   (one handler for all grids + additional rows + modal).
════════════════════════════════════════════════════════ */
function initCostCalculations() {

    /* Light + Exhaust System: rate × quantity */
    $(document).on('input', '#lightGrid .rate-input, #lightGrid .quantity-input,' +
        '#exhaustGrid .rate-input, #exhaustGrid .quantity-input',
        function () {
            recalcRow($(this).closest('tr'), 'quantity');
            updateTotalBO();
        });

    /* Paint Arrested Filter: rate × area */
    $(document).on('input', '#filterGrid .rate-input', function () {
        recalcRow($(this).closest('tr'), 'area');
        updateTotalBO();
    });
}

function recalcRow($row, mode) {
    var rate = parseFloat($row.find('.rate-input').val()) || 0;
    var qty = parseFloat($row.find('.quantity-input').val()) || 0;
    var area = parseFloat($row.find('.area-input').val()) || 0;
    var cost = (mode === 'area') ? rate * area : rate * qty;

    $row.find('.cost-input').val(Math.round(cost));
}


/* ════════════════════════════════════════════════════════
   5. TOTAL BOUGHT OUT
   Light + Exhaust + Filter costs + Additional BO amount
════════════════════════════════════════════════════════ */
function updateTotalBO() {
    var total = 0;

    $('.cost-input').each(function () {
        total += parseFloat($(this).val()) || 0;
    });

    total += parseFloat($('#hAddBOCost').val() || '0') || 0;

    $('#txtTotalBoughtOut').val(Math.round(total));
}


/* ════════════════════════════════════════════════════════
   6. APPROVE CHECKBOX — enables Save button
════════════════════════════════════════════════════════ */
function initApproveCheckbox() {
    var $chk = $('#approvePriceBid');
    var $save = $('#btnSave');

    $save.prop('disabled', true);

    $chk.on('change', function () {
        if (this.checked) {
            var confirmed = confirm('Do you want to approve this Price Bid?');
            if (confirmed) {
                $save.prop('disabled', false);
            } else {
                this.checked = false;
                $save.prop('disabled', true);
            }
        } else {
            $save.prop('disabled', true);
        }
    });
}


/* ════════════════════════════════════════════════════════
   7. SAVE — Additional BO rows → hidden inputs (7-column layout)
════════════════════════════════════════════════════════ */
function initSave() {
    $('#boForm').on('submit', function () {
        var $form = $(this);
        $form.find('.bo-rm-row-input').remove();

        function cellVal($td) {
            var $inp = $td.find('input,select');
            return $.trim($inp.length ? $inp.val() : $td.text());
        }

        function addHidden(name, val) {
            $('<input>').attr({ type: 'hidden', name: name, class: 'bo-rm-row-input' }).val(val).appendTo($form);
        }

        $('#boComponentTableBody tr[data-row]').each(function (i) {
            var $tds = $('td', this);
            var prefix = 'AdditionalBOList[' + i + ']';

            var qty = parseFloat(cellVal($tds.eq(3))) || 0;
            var rate = parseFloat(cellVal($tds.eq(4))) || 0;

            addHidden(prefix + '.Description', cellVal($tds.eq(1)));
            addHidden(prefix + '.ItemCode', cellVal($tds.eq(2)));
            addHidden(prefix + '.Qty', qty);
            addHidden(prefix + '.Rate', rate);
            addHidden(prefix + '.Amount', Math.round(qty * rate));
            addHidden(prefix + '.UnitWeight', 0);       // BO items have no weight
            addHidden(prefix + '.TotalWeight', 0);
        });

        collectItemRates();
        $('#btnSave').addClass('loading').prop('disabled', true);
    });
}


/* ════════════════════════════════════════════════════════
   8. AUTO-DISMISS ALERTS
════════════════════════════════════════════════════════ */
function initAlerts() {
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () {
            $el.fadeOut(400, function () { $el.remove(); });
        }, 4000);
    });
}


/* ════════════════════════════════════════════════════════
   9. COLLECT ITEM RATES — Light / Exhaust / Filter grids → JSON
════════════════════════════════════════════════════════ */
function collectItemRates() {
    var rates = [];

    // ── Light ──
    $('#lightGrid tbody tr').each(function (i) {
        var idx = i + 1;
        var $row = $(this);
        if ($row.find('.rate-input').length === 0) return;

        rates.push({
            itemType: 'Light',
            rowIndex: idx,
            itemCode: $row.find('.additional-item-code').val() || '',
            rate: parseFloat($row.find('.additional-rate').val()) || 0,
            quantity: parseFloat($('#txtLightQty_' + idx).val()) || 0,
            cost: parseFloat($('#txtLightCost_' + idx).val()) || 0
        });
    });

    // ── Exhaust (Blower / Motor) — rowIndex = Sr. number ──
    $('#exhaustGrid tbody tr').each(function () {
        var $row = $(this);
        if ($row.find('.rate-input').length === 0) return;

        var srNo = parseInt($row.find('td:first').text().trim()) || 0;
        var itemText = $row.find('td:nth-child(2)').text().trim();
        var itemType = itemText.indexOf('Motor') > -1 ? 'Motor' : 'Blower';

        rates.push({
            itemType: itemType,
            rowIndex: srNo,
            itemCode: $row.find('.additional-item-code').val() || '',
            rate: parseFloat($row.find('.rate-input').val()) || 0,
            quantity: parseFloat($row.find('.quantity-input').val()) || 0,
            cost: parseFloat($row.find('.cost-input').val()) || 0
        });
    });

    // ── Filter ──
    $('#filterGrid tbody tr').each(function (i) {
        var idx = i + 1;
        var $row = $(this);
        if ($row.find('.rate-input').length === 0) return;

        rates.push({
            itemType: 'Filter',
            rowIndex: idx,
            itemCode: $row.find('.additional-item-code').val() || '',
            rate: parseFloat($row.find('.additional-rate').val()) || 0,
            quantity: 0,
            cost: parseFloat($('#txtFilterCost_' + idx).val()) || 0
        });
    });

    $('#hiddenItemRates').val(JSON.stringify(rates));
}


/* ════════════════════════════════════════════════════════
   10. RESTORE SAVED RATES — edit page load (reads window._savedBORates)
════════════════════════════════════════════════════════ */
function restoreSavedRates() {
    var savedRates = window['_savedBORates'] || [];
    if (!savedRates.length) return;

    savedRates.forEach(function (r) {
        var idx = r.rowIndex;
        var $row;

        if (r.itemType === 'Light') {
            $row = $('#lightGrid tbody tr').eq(idx - 1);
        } 
        else if (r.itemType === 'Blower' || r.itemType === 'Motor') {
            $row = $('#exhaustGrid tbody tr').filter(function () {
                return parseInt($(this).find('td:first').text().trim()) === idx;
            });
        } else if (r.itemType === 'Filter') {
            $row = $('#filterGrid tbody tr').eq(idx - 1);
        }

        if ($row && $row.length) {
            $row.find('.additional-item-code').val(r.itemCode || '');
            $row.find('.additional-rate')
                .val(r.rate)
                .data('last-rate', r.rate)       // fallback cache for the rate API
                .trigger('input');
        }
    });

    updateTotalBO();
}


/* ════════════════════════════════════════════════════════
   11. ADDITIONAL BO ITEMS — seeded rows, modal add, delete
════════════════════════════════════════════════════════ */
function initBOComponents() {
    var rowIndex = $('#boComponentTableBody tr[data-row]').length;

    // ── Default rows (from FI sheet) ──
    var defaultBORows = [
        'Bellow',
        'FLP Light Fixture',
        'Celling Filter',
        'Suction Filter',
        'Manual Damper',
        'Digital Manometer',
        'Hinge Door',
        'View Glass',
        'Paint',
        'Thinner',
        'Hardware'
    ];

    function buildInlineRow(desc) {
        rowIndex++;
        var n = $('#boComponentTableBody tr[data-row]').length + 1;
        return $('<tr>').attr('data-row', rowIndex).html(
            '<td>' + n + '</td>' +
            '<td>' + $('<span>').text(desc).html() + '</td>' +
            '<td><input type="text" class="pb-input rm-inline additional-item-code" data-f="itemcode" placeholder="Item Code" autocomplete="off"></td>' +
            '<td class="num"><input type="number" class="pb-input rm-inline" data-f="qty" placeholder="0" min="0" step="0.01"></td>' +
            '<td class="num"><input type="number" class="pb-input rm-inline additional-rate" data-f="rate" placeholder="0" min="0" step="0.01"><div class="rate-status" style="display:none;"></div></td>' +
            '<td class="num rm-amt">0</td>' +
            '<td><button type="button" class="btn-bo-comp-del" data-row="' + rowIndex + '" title="Remove"><i class="bi bi-trash"></i></button></td>'
        );
    }

    function seedDefaultRows() {
        $('#boCompEmptyRow').hide();

        var existing = [];
        $('#boComponentTableBody tr[data-row]').each(function () {
            existing.push($.trim($('td', this).eq(1).text()));
        });

        defaultBORows.forEach(function (d) {
            if (existing.indexOf(d) === -1) {
                $('#boComponentTableBody').append(buildInlineRow(d));
            }
        });

        $('#boCompFooter').show();
        recalcBOTotals();
        updateBOCompCount();
    }

    // Suction filter qty = Σ(capacity per blower × no. of blowers) ÷ 3500, rounded up
    function applySuctionFilterQty() {
        var totalCap = 0;
        $('#exhaustGrid tbody tr').each(function () {
            var name = $(this).find('td:nth-child(2)').text().trim();
            if (name.indexOf('Blower') > -1) {
                var cap = parseFloat($(this).find('td:nth-child(4) input').val()) || 0;
                var nos = parseFloat($(this).find('.quantity-input').val()) || 0;
                totalCap += cap * nos;
            }
        });

        var qty = Math.ceil(totalCap / 3500);

        $('#boComponentTableBody tr[data-row]').each(function () {
            if ($.trim($('td', this).eq(1).text()) === 'Suction Filter') {
                $(this).find('[data-f="qty"]')
                    .val(qty)
                    .prop('readonly', true)
                    .trigger('input');
            }
        });
    }

    // Rows that have an item code but no rate (e.g. saved before the column fix) → fetch rate again
    function refetchMissingRates() {
        $('#boComponentTableBody tr[data-row]').each(function () {
            var $code = $(this).find('.additional-item-code');
            var rate = parseFloat($(this).find('[data-f="rate"]').val()) || 0;
            if ($code.length && $.trim($code.val()) && rate <= 0) {
                $code.trigger('blur');          // inline handler in the view fetches the rate
            }
        });
    }

    // ── Live amount = qty × rate for inline rows ──
    $('#boComponentTableBody').on('input', '.rm-inline', function () {
        var $tr = $(this).closest('tr');
        var qty = parseFloat($tr.find('[data-f="qty"]').val()) || 0;
        var rate = parseFloat($tr.find('[data-f="rate"]').val()) || 0;

        $tr.find('.rm-amt').text(Math.round(qty * rate).toLocaleString('en-IN'));
        recalcBOTotals();
    });

    // ── Modal ──
    function openModal() {
        $('#boCompDesc').val('');
        $('#boCompItemCode').val('');
        $('#boCompQty').val('');
        $('#boCompRate').val('');
        $('#boCompCalcPreview').hide();
        $('#boErrDesc, #boErrQty, #boErrRate, #boErrItemCode').hide();
        $('#addBOComponentModal .rate-status').text('').hide();
        $('#addBOComponentModal').addClass('open');
        setTimeout(function () { $('#boCompDesc').focus(); }, 50);
    }

    function closeModal() {
        $('#addBOComponentModal').removeClass('open');
    }

    $('#btnAddBOComponent').on('click', openModal);
    $('#btnCloseBOModal, #btnCancelBOModal').on('click', closeModal);

    $('#addBOComponentModal').on('click', function (e) {
        if ($(e.target).is('#addBOComponentModal')) closeModal();
    });

    // preview: amount = qty × rate
    $('#boCompQty, #boCompRate').on('input', function () {
        var qty = parseFloat($('#boCompQty').val()) || 0;
        var rt = parseFloat($('#boCompRate').val()) || 0;

        if (qty > 0 || rt > 0) {
            $('#boPrevAmount').text('₹' + Math.round(qty * rt).toLocaleString('en-IN'));
            $('#boCompCalcPreview').show();
        } else {
            $('#boCompCalcPreview').hide();
        }
    });

    // add as an editable row (same layout as seeded rows)
    $('#btnConfirmBOAdd').on('click', function () {
        var desc = $.trim($('#boCompDesc').val());
        var itemCode = $.trim($('#boCompItemCode').val());
        var qty = parseFloat($('#boCompQty').val());
        var rate = parseFloat($('#boCompRate').val());

        $('#boErrDesc').toggle(!desc);
        $('#boErrQty').toggle(!qty || qty <= 0);
        $('#boErrRate').toggle(!rate || rate <= 0);
        if (!desc || !qty || qty <= 0 || !rate || rate <= 0) return;

        var $tr = buildInlineRow(desc);
        $tr.find('[data-f="itemcode"]').val(itemCode);
        $tr.find('[data-f="qty"]').val(qty);
        $tr.find('[data-f="rate"]').val(rate).data('last-rate', rate);
        $tr.find('.rm-amt').text(Math.round(qty * rate).toLocaleString('en-IN'));

        $('#boCompEmptyRow').hide();
        $('#boComponentTableBody').append($tr);
        $('#boCompFooter').show();

        recalcBOTotals();
        updateBOCompCount();
        closeModal();
    });

    // ── Delete ──
    $(document).on('click', '.btn-bo-comp-del', function () {
        $(this).closest('tr').remove();
        renumberBORows();
        recalcBOTotals();
        updateBOCompCount();

        if ($('#boComponentTableBody tr[data-row]').length === 0) {
            $('#boCompEmptyRow').show();
            $('#boCompFooter').hide();
            $('#hAddBOWeight').val('0');
            $('#hAddBOCost').val('0');
            updateTotalBO();
        }
    });

    // ── Totals: amount is column 5 ──
    function recalcBOTotals() {
        var ta = 0;
        $('#boComponentTableBody tr[data-row]').each(function () {
            ta += parseFloat($('td', this).eq(5).text().replace(/,/g, '')) || 0;
        });

        $('#boCompTotalAmount').text(Math.round(ta).toLocaleString('en-IN'));
        $('#hAddBOWeight').val('0');
        $('#hAddBOCost').val(Math.round(ta));
        updateTotalBO();
    }

    function renumberBORows() {
        $('#boComponentTableBody tr[data-row]').each(function (i) {
            $('td:first', this).text(i + 1);
        });
    }

    function updateBOCompCount() {
        $('#boCompCount').text($('#boComponentTableBody tr[data-row]').length);
    }

    // ── Page load ──
    if ($('#boComponentTableBody tr[data-row]').length > 0) {
        $('#boCompEmptyRow').hide();
        $('#boCompFooter').show();
    }
    seedDefaultRows();
    applySuctionFilterQty();
    refetchMissingRates();
}

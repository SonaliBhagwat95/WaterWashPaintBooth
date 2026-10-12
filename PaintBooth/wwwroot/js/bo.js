/* ============================================================
   Bullows PaintBooth Pro — bo.js
   JS for BO Calculations Create page
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

    $('#txtEnquiryCode').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnSearch').trigger('click');
        }
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

        // GET redirect — controller handles blank vs filled form
        window.location.href = boUrl + '?enquiryCode=' + encodeURIComponent(code);

        // Button re-enables if user navigates back
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

    // Only open first section on CREATE mode
    // Edit mode opens all sections via inline script
    if (!$('#hiddenEnquiryCode').val()) {
        $('.bo-section-header').first().trigger('click');
    }
}

/* ════════════════════════════════════════════════════════
   4. COST CALCULATIONS
   Rate × Quantity = Cost   (for Light + Exhaust sections)
   Rate × Area     = Cost   (for Paint Arrested Filter)
════════════════════════════════════════════════════════ */
//function initCostCalculations() {

//    /* Light + Exhaust System: rate × quantity */
//    $(document).on('input', '#lightGrid .rate-input, #lightGrid .quantity-input,' +
//                            '#exhaustGrid .rate-input, #exhaustGrid .quantity-input',
//    function () {
//        recalcRow($(this).closest('tr'), 'quantity');
//        updateTotalBO();
//    });

//    /* Paint Arrested Filter: rate × area */
//    $(document).on('input', '#filterGrid .rate-input', function () {
//        recalcRow($(this).closest('tr'), 'area');
//        updateTotalBO();
//    });
//}
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

    /* Item code → fetch rate, or clear rate when emptied */
    $(document).on('blur', '#exhaustGrid .additional-item-code, ' +
        '#lightGrid .additional-item-code, ' +
        '#filterGrid .additional-item-code',
        function () {
            var $row = $(this).closest('tr');
            var $rate = $row.find('.additional-rate');
            var itemCode = $.trim($(this).val());

            if (!itemCode) {
                $rate.val('');
                $row.find('.cost-input').val('0');
                updateTotalBO();
                return;
            }

            $.getJSON('/Costing/GetItemRate', { itemCode: itemCode }, function (res) {
                $rate.val(res.success ? res.rate.toFixed(2) : '0.00');
                var mode = $row.closest('table').attr('id') === 'filterGrid' ? 'area' : 'quantity';
                recalcRow($row, mode);
                updateTotalBO();
            }).fail(function () {
                $rate.val('0.00');
                updateTotalBO();
            });
        });
}
function recalcRow($row, mode) {
    var rate = parseFloat($row.find('.rate-input').val()) || 0;
    var qty = parseFloat($row.find('.quantity-input').val()) || 0;
    var area = parseFloat($row.find('.area-input').val()) || 0;
    var cost = (mode === 'area') ? rate * area : rate * qty;

    //$row.find('.cost-input').val(cost.toFixed(2));
    $row.find('.cost-input').val(Math.round(cost));
}


/* ════════════════════════════════════════════════════════
   5. TOTAL BOUGHT OUT CALCULATION
   Sums all .cost-input values across all sections
════════════════════════════════════════════════════════ */
//function updateTotalBO() {
//    var total = 0;
//    $('.cost-input').each(function () {
//        total += parseFloat($(this).val()) || 0;
//    });
//    //$('#txtTotalBoughtOut').val(total.toFixed(2));
//    $('#txtTotalBoughtOut').val(Math.round(total));
//}
function updateTotalBO() {
    var total = 0;

    // existing section costs (light + exhaust + filter)
    $('.cost-input').each(function () {
        total += parseFloat($(this).val()) || 0;
    });

    // add additional BO items amount
    total += parseFloat($('#hAddBOCost').val() || '0') || 0;

    $('#txtTotalBoughtOut').val(Math.round(total));
}

/* ════════════════════════════════════════════════════════
   6. APPROVE CHECKBOX — enables Save button
════════════════════════════════════════════════════════ */
function initApproveCheckbox() {
    var $chk = $('#approvePriceBid');
    var $save = $('#btnSave');

    // Start disabled
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
   7. SAVE — spinner on form submit
════════════════════════════════════════════════════════ */

function initSave() {
    $('#boForm').on('submit', function () {
        $(this).find('.bo-rm-row-input').remove();

        $('#boComponentTableBody tr[data-row]').each(function (i) {
            var $tds = $('td', this);
            var prefix = 'AdditionalBOList[' + i + ']';
            var $form = $('#boForm');

            function cellVal($td) {
                var $inp = $td.find('input,select');
                return $inp.length ? $inp.val() : $td.text().trim();
            }

            $('<input>').attr({ type: 'hidden', name: prefix + '.Description', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(1))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.ItemCode', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(2))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Qty', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(3))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.UnitWeight', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(4))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.TotalWeight', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(5))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Rate', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(6))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Amount', class: 'bo-rm-row-input' }).val(cellVal($tds.eq(7)).replace(/,/g, '')).appendTo($form);
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
   9. COLLECT ITEM RATES — called before form submit
   Gathers all rate/qty/cost values into JSON hidden field
════════════════════════════════════════════════════════ */
//function collectItemRates() {
//    var rates = [];

//    // ── Light rates ──────────────────────────────────────────
//    $('#lightGrid tbody tr').each(function (i) {
//        var idx = i + 1;
//        if ($(this).find('.rate-input').length === 0) return;

//        rates.push({
//            itemType: 'Light',
//            rowIndex: idx,
//            rate: parseFloat($('#txtLightRate_' + idx).val()) || 0,
//            quantity: parseFloat($('#txtLightQty_' + idx).val()) || 0,
//            cost: parseFloat($('#txtLightCost_' + idx).val()) || 0
//           // cost: parseInt($('#txtLightCost_' + idx).val()) || 0
//        });
//    });

//    // ── Exhaust rows — use the sr. number from first td ──────
//    // View generates Blower as odd rows (1,3,5) and Motor as even rows (2,4,6)
//    $('#exhaustGrid tbody tr').each(function () {
//        if ($(this).find('.rate-input').length === 0) return;

//        var srNo = parseInt($(this).find('td:first').text().trim()) || 0;
//        var itemText = $(this).find('td:nth-child(2)').text().trim();
//        var itemType = itemText.indexOf('Motor') > -1 ? 'Motor' : 'Blower';

//        rates.push({
//            itemType: itemType,
//            rowIndex: srNo,   // ← use the actual Sr. number from the table
//            rate: parseFloat($(this).find('.rate-input').val()) || 0,
//            quantity: parseFloat($(this).find('.quantity-input').val()) || 0,
//            cost: parseFloat($(this).find('.cost-input').val()) || 0
//        });
//    });

//    // ── Filter rates ─────────────────────────────────────────
//    $('#filterGrid tbody tr').each(function (i) {
//        var idx = i + 1;
//        if ($(this).find('.rate-input').length === 0) return;

//        rates.push({
//            itemType: 'Filter',
//            rowIndex: idx,
//            rate: parseFloat($('#txtFilterRate_' + idx).val()) || 0,
//            quantity: 0,
//            cost: parseFloat($('#txtFilterCost_' + idx).val()) || 0
//        });
//    });

//    console.log('Collecting rates:', rates); // ← remove after testing
//    $('#hiddenItemRates').val(JSON.stringify(rates));
//}

function collectItemRates() {
            var rates = [];

            // ── Light rates ──────────────────────────────────────────
            $('#lightGrid tbody tr').each(function (i) {
                var idx = i + 1;
                var $row = $(this);
                if ($row.find('.rate-input').length === 0) return;

                rates.push({
                    itemType: 'Light',
                    rowIndex: idx,
                    itemCode: $row.find('.additional-item-code').val() || '',   // ← NEW
                    rate: parseFloat($row.find('.additional-rate').val()) || 0,
                    quantity: parseFloat($('#txtLightQty_' + idx).val()) || 0,
                    cost: parseFloat($('#txtLightCost_' + idx).val()) || 0
                });
            });

            // ── Exhaust rows ──────────────────────────────────────────
            $('#exhaustGrid tbody tr').each(function () {
                var $row = $(this);
                if ($row.find('.rate-input').length === 0) return;

                var srNo = parseInt($row.find('td:first').text().trim()) || 0;
                var itemText = $row.find('td:nth-child(2)').text().trim();
                var itemType = itemText.indexOf('Motor') > -1 ? 'Motor' : 'Blower';

                rates.push({
                    itemType: itemType,
                    rowIndex: srNo,
                    itemCode: $row.find('.additional-item-code').val() || '',   // ← NEW
                    rate: parseFloat($row.find('.rate-input').val()) || 0,
                    quantity: parseFloat($row.find('.quantity-input').val()) || 0,
                    cost: parseFloat($row.find('.cost-input').val()) || 0
                });
            });

            // ── Filter rates ─────────────────────────────────────────
            $('#filterGrid tbody tr').each(function (i) {
                var idx = i + 1;
                var $row = $(this);
                if ($row.find('.rate-input').length === 0) return;

                rates.push({
                    itemType: 'Filter',
                    rowIndex: idx,
                    itemCode: $row.find('.additional-item-code').val() || '',   // ← NEW
                    rate: parseFloat($row.find('.additional-rate').val()) || 0,
                    quantity: 0,
                    cost: parseFloat($('#txtFilterCost_' + idx).val()) || 0
                });
            });

            console.log('Collecting rates:', rates);
            $('#hiddenItemRates').val(JSON.stringify(rates));
        }
/* ════════════════════════════════════════════════════════
   10. RESTORE SAVED RATES — called on edit page load
   Reads from #hiddenSavedRates and fills rate inputs,
   then triggers input event to recalculate costs
════════════════════════════════════════════════════════ */
//function restoreSavedRates() {
//    var savedRates = window._savedBORates || [];

//    if (!savedRates.length) return;

//    savedRates.forEach(function (r) {
//        var idx = r.rowIndex;
//        var itemType = r.itemType;
//        var rate = r.rate;
//        var qty = r.quantity;

//        if (itemType === 'Light') {
//            var $rateInput = $('#txtLightRate_' + idx);
//            var $qtyInput = $('#txtLightQty_' + idx);
//            if ($rateInput.length) {
//                $rateInput.val(rate);
//                $qtyInput.val(qty);
//                $rateInput.trigger('input');
//            }

//        } else if (itemType === 'Blower') {
//            var $rateInput = $('#txtBlowerRate_' + idx);
//            if ($rateInput.length) {
//                $rateInput.val(rate).trigger('input');
//            }

//        } else if (itemType === 'Motor') {
//            var $rateInput = $('#txtMotorRate_' + idx);
//            if ($rateInput.length) {
//                $rateInput.val(rate).trigger('input');
//            }

//        } else if (itemType === 'Filter') {
//            var $rateInput = $('#txtFilterRate_' + idx);
//            if ($rateInput.length) {
//                $rateInput.val(rate).trigger('input');
//            }
//        }
//    });

//    updateTotalBO();
//}
function restoreSavedRates() {
            var savedRates = window._savedBORates || [];
            if (!savedRates.length) return;

            savedRates.forEach(function (r) {
                var idx = r.rowIndex;
                var itemType = r.itemType;
                var rate = r.rate;
                var qty = r.quantity;
                var itemCode = r.itemCode || '';   // ← NEW

                var $row;
                if (itemType === 'Light') {
                    $row = $('#lightGrid tbody tr').eq(idx - 1);
                    $row.find('.additional-rate').val(rate);
                    $('#txtLightQty_' + idx).val(qty);
                } else if (itemType === 'Blower' || itemType === 'Motor') {
                    $row = $('#exhaustGrid tbody tr').filter(function () {
                        return parseInt($(this).find('td:first').text().trim()) === idx;
                    });
                    $row.find('.additional-rate, .rate-input').val(rate);
                } else if (itemType === 'Filter') {
                    $row = $('#filterGrid tbody tr').eq(idx - 1);
                    $row.find('.additional-rate').val(rate);
                }

                if ($row && $row.length) {
                    $row.find('.additional-item-code').val(itemCode);   // ← NEW
                    $row.find('.additional-rate').data('last-rate', rate);  // keep fallback cache in sync
                    $row.find('.additional-rate').trigger('input');
                }
            });

            updateTotalBO();
        }
/* ════════════════════════════════════════════════════════
11. ADDITIONAL BO ITEMS — modal add / delete
════════════════════════════════════════════════════════ */


function initBOComponents() {
            var rowIndex = $('#boComponentTableBody tr[data-row]').length;

            // ── Default Extraction-System rows (from FI sheet) ──────
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
                    '<td class="num"><input type="number" class="pb-input rm-inline" data-f="qty" placeholder="0" min="0" step="1"></td>' +
                    //'<td class="num"><input type="number" class="pb-input rm-inline" data-f="uw" placeholder="0.00" min="0" step="0.01"></td>' +
                    //'<td class="num rm-tw"><input type="number" class="pb-input rm-inline" data-f="tw" placeholder="0" min="0" step="1"></td>' +
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
                    if (existing.indexOf(d) === -1) {          // add only if missing
                        $('#boComponentTableBody').append(buildInlineRow(d));
                    }
                });

                $('#boCompFooter').show();
                recalcBOTotals();
                updateBOCompCount();
            }

            // Live compute for inline-editable rows
            //$('#boComponentTableBody').on('input', '.rm-inline', function () {
            //    var $tr = $(this).closest('tr');
            //    var fld = $(this).data('f');
            //    var qty = parseFloat($tr.find('[data-f="qty"]').val()) || 0;
            //    var uw = parseFloat($tr.find('[data-f="uw"]').val()) || 0;
            //    var rate = parseFloat($tr.find('[data-f="rate"]').val()) || 0;

            //    if (fld === 'qty' || fld === 'uw') {
            //        $tr.find('[data-f="tw"]').val((qty * uw).toFixed(0));
            //    }

            //    var tw = parseFloat($tr.find('[data-f="tw"]').val()) || 0;
            //    var amt = tw * rate;

            //    $tr.find('.rm-amt').text(Math.round(amt).toLocaleString('en-IN'));
            //    recalcBOTotals();
    //});
    function applySuctionFilterQty() {
        // Σ (capacity per blower × no. of blowers) ÷ 3500
        var totalCap = 0;
        $('#exhaustGrid tbody tr').each(function () {
            var name = $(this).find('td:nth-child(2)').text().trim();
            if (name.indexOf('Blower') > -1) {                    // blower rows only, skip motor rows
                var cap = parseFloat($(this).find('td:nth-child(4) input').val()) || 0;
                var nos = parseFloat($(this).find('.quantity-input').val()) || 0;
                totalCap += cap * nos;
            }
        });

        var qty = Math.ceil(totalCap / 3500);   // round up — you can't buy a partial filter

        // find the seeded "Suction Filter" row and fill its Qty
        $('#boComponentTableBody tr[data-row]').each(function () {
            if ($.trim($('td', this).eq(1).text()) === 'Suction Filter') {
                $(this).find('[data-f="qty"]')
                    .val(qty)
                    .prop('readonly', true)      // calculated, so lock it
                    .trigger('input');           // fires amount recompute (qty × rate)
            }
        });
    }
    $('#boComponentTableBody').on('input', '.rm-inline', function () {
        var $tr = $(this).closest('tr');
        var qty = parseFloat($tr.find('[data-f="qty"]').val()) || 0;
        var rate = parseFloat($tr.find('[data-f="rate"]').val()) || 0;
        var amt = qty * rate;

        $tr.find('.rm-amt').text(Math.round(amt).toLocaleString('en-IN'));
        recalcBOTotals();
    });
            function openModal() {
                $('#boCompDesc').val('');
                $('#boCompItemCode').val('');
                $('#boCompQty').val('');
                $('#boCompUnitWeight').val('');
                $('#boCompRate').val('');
                $('#boCompCalcPreview').hide();
                $('#boErrDesc, #boErrQty, #boErrUW, #boErrRate').hide();
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

            $('#boCompQty, #boCompUnitWeight, #boCompRate').on('input', function () {
                var qty = parseFloat($('#boCompQty').val()) || 0;
                var uw = parseFloat($('#boCompUnitWeight').val()) || 0;
                var rt = parseFloat($('#boCompRate').val()) || 0;
                var tw = qty * uw;
                var amt = tw * rt;

                if (qty > 0 || uw > 0 || rt > 0) {
                    $('#boPrevWeight').text(tw.toFixed(2) + ' kg');
                    $('#boPrevAmount').text('₹' + Math.round(amt).toLocaleString('en-IN'));
                    $('#boCompCalcPreview').show();
                } else {
                    $('#boCompCalcPreview').hide();
                }
            });

            $('#btnConfirmBOAdd').on('click', function () {
                var desc = $.trim($('#boCompDesc').val());
                var itemCode = $.trim($('#boCompItemCode').val());
                var qty = parseFloat($('#boCompQty').val());
                var uw = parseFloat($('#boCompUnitWeight').val());
                var rate = parseFloat($('#boCompRate').val());

                $('#boErrDesc').toggle(!desc);
                $('#boErrQty').toggle(!qty || qty <= 0);
                $('#boErrUW').toggle(!uw || uw <= 0);
                $('#boErrRate').toggle(!rate || rate <= 0);

                if (!desc || !qty || qty <= 0 || !uw || uw <= 0 || !rate || rate <= 0) return;

                var tw = qty * uw;
                var amt = tw * rate;

                rowIndex++;
                var n = $('#boComponentTableBody tr[data-row]').length + 1;

                var $tr = $('<tr>').attr('data-row', rowIndex).html(
                    '<td>' + n + '</td>' +
                    '<td>' + $('<span>').text(desc).html() + '</td>' +
                    '<td>' + $('<span>').text(itemCode).html() + '</td>' +
                    '<td class="num">' + qty + '</td>' +
                    '<td class="num">' + uw.toFixed(2) + '</td>' +
                    '<td class="num">' + tw.toFixed(0) + '</td>' +
                    '<td class="num">' + rate.toFixed(0) + '</td>' +
                    '<td class="num">' + Math.round(amt).toLocaleString('en-IN') + '</td>' +
                    '<td><button type="button" class="btn-bo-comp-del" data-row="' + rowIndex + '" title="Remove">' +
                    '<i class="bi bi-trash"></i></button></td>'
                );

                $('#boCompEmptyRow').hide();
                $('#boComponentTableBody').append($tr);
                $('#boCompFooter').show();

                recalcBOTotals();
                updateBOCompCount();
                closeModal();
            });

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

            // ── read input-or-text so seeded editable rows count ────
            function recalcBOTotals() {
                var tw = 0, ta = 0;
                $('#boComponentTableBody tr[data-row]').each(function () {
                    var $tds = $('td', this);
                    var $twInp = $tds.eq(5).find('input');
                    tw += ($twInp.length ? parseFloat($twInp.val()) : parseFloat($tds.eq(5).text().replace(/,/g, ''))) || 0;
                    ta += parseFloat($tds.eq(7).text().replace(/,/g, '')) || 0;
                });

                $('#boCompTotalWeight').text(tw.toFixed(0));
                $('#boCompTotalAmount').text(Math.round(ta).toLocaleString('en-IN'));
                $('#hAddBOWeight').val(tw.toFixed(0));
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

            // Page load: init from Razor rows, then ensure defaults
            if ($('#boComponentTableBody tr[data-row]').length > 0) {
                $('#boCompEmptyRow').hide();
                $('#boCompFooter').show();
                recalcBOTotals();
                updateBOCompCount();
            }
    seedDefaultRows();
    applySuctionFilterQty();     // ← adds missing Extraction rows, dedup by description
        }
/* ============================================================
   Bullows PaintBooth Pro — costing.js
   JS for Raw Material Costing Create / Edit page
   ============================================================ */

/* ════════════════════════════════════════════════════════
   1. AUTOCOMPLETE — Enquiry Code
════════════════════════════════════════════════════════ */
function initAutocomplete(searchUrl) {
    $('#txtEnquiryCode').on('input', function () {
        var term = $.trim($(this).val());
        if (term.length < 1) {
            $('#enquiryDropdown').hide().empty();
            return;
        }

        $.post(searchUrl, { enquiryCode: term }, function (data) {
            var $dd = $('#enquiryDropdown').empty();

            if (data.success && data.results.length > 0) {
                $.each(data.results, function (i, code) {
                    $('<div class="cost-dropdown-item">')
                        .text(code)
                        .appendTo($dd);
                });
                $dd.show();
            } else {
                $dd.hide();
            }
        });
    });

    /* Select from dropdown */
    $(document).on('click', '.cost-dropdown-item', function () {
        $('#txtEnquiryCode').val($(this).text());
        $('#enquiryDropdown').hide().empty();
    });

    /* Close dropdown on outside click */
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtEnquiryCode, #enquiryDropdown').length) {
            $('#enquiryDropdown').hide();
        }
    });

    /* Enter key triggers search */
    $('#txtEnquiryCode').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnSearch').trigger('click');
        }
    });
}


/* ════════════════════════════════════════════════════════
   2. SEARCH — loads panel / frame tables + summary
════════════════════════════════════════════════════════ */
function initSearch(searchUrl) {
    $('#btnSearch').on('click', function () {
        var code = $.trim($('#txtEnquiryCode').val());

        if (!code) {
            PB.toast('Please enter an enquiry code.', 'warning');
            return;
        }

        var $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Searching...');

        $.post(searchUrl, { EnquiryCode: code }, function (html) {
            var $res = $(html);

            /* Replace table bodies */
            var newPanelBody = $res.find('#panelTableBody').html();
            var newFrameBody = $res.find('#frameTableBody').html();
            var newPanelFoot = $res.find('#panelTable tfoot').html();
            var newFrameFoot = $res.find('#frameTable tfoot').html();

            if (newPanelBody) {
                $('#panelTableBody').html(newPanelBody);
                replaceFoot('#panelTable', newPanelFoot);
            }
            if (newFrameBody) {
                $('#frameTableBody').html(newFrameBody);
                replaceFoot('#frameTable', newFrameFoot);
            }

            // ── Replace additional RM table ──────────────────────
            var newCompBody = $res.find('#componentTableBody').html();
            var newCompFoot = $res.find('#compFooter').html();

            if (newCompBody) {
                $('#componentTableBody').html(newCompBody);
            }

            // restore footer visibility
            var compRowCount = $('#componentTableBody tr[data-row]').length;
            if (compRowCount > 0) {
                $('#compEmptyRow').hide();
                $('#compFooter').html(newCompFoot).show();
            } else {
                $('#compEmptyRow').show();
                $('#compFooter').hide();
            }

            // re-seed rowIndex and recalc
            // rowIndex is inside initComponents scope so trigger via custom event
            $(document).trigger('comp:reload');
            // ────────────────────────────────────────────────────

            /* Row counts */
            var pc = $res.find('#panelCount').text();
            var fc = $res.find('#frameCount').text();
            if (pc) $('#panelCount').text(pc);
            if (fc) $('#frameCount').text(fc);

            /* Summary cards */
            updateText($res, '#dispRMWeightPanels');
            updateText($res, '#dispTotalCostPanels');
            updateText($res, '#dispRMWeightFrame');
            updateText($res, '#dispTotalCostFrame');
            updateText($res, '#dispAddRMWeight');
            updateText($res, '#dispAddRMCost');
            updateText($res, '#dispTotalRM');

            /* Hidden form inputs */
            updateVal($res, '#hRMWeightOfPanels');
            updateVal($res, '#hTotalPriceofPanels');
            updateVal($res, '#hTotalWeightOfFrame');
            updateVal($res, '#hTotalPriceofFrame');
            updateVal($res, '#hTotalCostOfRawMaterials');
            updateVal($res, '#hAddRMWeight');
            updateVal($res, '#hAddRMCost');

            /* Show data sections */
            $('#divCostingData').show();
            PB.toast('Data loaded for ' + code, 'success');
        })



        .fail(function () {
            PB.toast('Error loading data. Please try again.', 'error');
        })
        .always(function () {
            $btn.prop('disabled', false)
                .html('<i class="bi bi-search"></i> Search');
        });
    });
}


/* ════════════════════════════════════════════════════════
   3. SAVE — spinner on submit, let form POST naturally
════════════════════════════════════════════════════════ */
//function initSave() {
//    $('#costingForm').on('submit', function () {
//        $('#btnSave')
//            .addClass('loading')
//            .prop('disabled', true);
//        // Do NOT return false — form POSTs naturally
//    });
//}
function initSave() {
    $('#costingForm').on('submit', function () {
        $(this).find('.rm-row-input').remove();

        //$('#componentTableBody tr[data-row]').each(function (i) {
        //    var $tds = $('td', this);
        //    var prefix = 'AdditionalRMList[' + i + ']';
        //    var $form = $('#costingForm');

        //    $('<input>').attr({ type: 'hidden', name: prefix + '.Description', class: 'rm-row-input' }).val($tds.eq(1).text()).appendTo($form);
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.ItemCode', class: 'rm-row-input' }).val($tds.eq(2).text()).appendTo($form);   // ← NEW
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.Qty', class: 'rm-row-input' }).val($tds.eq(3).text()).appendTo($form);         // was eq(2)
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.UnitWeight', class: 'rm-row-input' }).val($tds.eq(4).text()).appendTo($form);  // was eq(3)
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.TotalWeight', class: 'rm-row-input' }).val($tds.eq(5).text()).appendTo($form); // was eq(4)
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.Rate', class: 'rm-row-input' }).val($tds.eq(6).text()).appendTo($form);        // was eq(5)
        //    $('<input>').attr({ type: 'hidden', name: prefix + '.Amount', class: 'rm-row-input' }).val($tds.eq(7).text().replace(/,/g, '')).appendTo($form); // was eq(6)
        //});
        $('#componentTableBody tr[data-row]').each(function (i) {
            var $tds = $('td', this);
            var prefix = 'AdditionalRMList[' + i + ']';
            var $form = $('#costingForm');

            // reads an input's value if the cell has one, else the cell text
            function cellVal($td) {
                var $inp = $td.find('input,select');
                return $inp.length ? $inp.val() : $td.text();
            }

            $('<input>').attr({ type: 'hidden', name: prefix + '.Description', class: 'rm-row-input' }).val(cellVal($tds.eq(1))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.ItemCode', class: 'rm-row-input' }).val(cellVal($tds.eq(2))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Qty', class: 'rm-row-input' }).val(cellVal($tds.eq(3))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.UnitWeight', class: 'rm-row-input' }).val(cellVal($tds.eq(4))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.TotalWeight', class: 'rm-row-input' }).val(cellVal($tds.eq(5))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Rate', class: 'rm-row-input' }).val(cellVal($tds.eq(6))).appendTo($form);
            $('<input>').attr({ type: 'hidden', name: prefix + '.Amount', class: 'rm-row-input' }).val(cellVal($tds.eq(7)).replace(/,/g, '')).appendTo($form);
        });
        $('#btnSave').addClass('loading').prop('disabled', true);
    });
}


/* ════════════════════════════════════════════════════════
   4. EXPORT FI SHEET
════════════════════════════════════════════════════════ */
function initExport(exportUrl) {
    $('#btnExport').on('click', function () {
        var $btn = $(this);

        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Exporting...');

        window.location.href = exportUrl;

        // Re-enable after 3 s
        setTimeout(function () {
            $btn.prop('disabled', false)
                .html('<i class="bi bi-file-earmark-excel"></i> Export FI Sheet');
        }, 3000);
    });
}


/* ════════════════════════════════════════════════════════
   5. AUTO-DISMISS ALERTS
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
   6. ADDITIONAL RAW MATERIALS — modal add / delete
════════════════════════════════════════════════════════ */



function initComponents() {
    // ── Seed rowIndex from Razor-rendered rows ──────────────
    var rowIndex = $('#componentTableBody tr[data-row]').length;
    // ── Default inline-editable rows ──────────────────────────
    var defaultRMRows = [
        'Blower Resting Structure',
        'Paint Arrestor Filter frame',
        'Ceiling Filter Frame',
        'Sampling Inspection Platform',
        'Vertical Straight Structure',
        'Horizontal Structure',
        'Cross bracing',
        'Ladder',
        'Operator Platform',
        'Safety railing'
    ];

    function buildInlineRow(desc) {
        rowIndex++;
        var n = $('#componentTableBody tr[data-row]').length + 1;
        return $('<tr>').attr('data-row', rowIndex).html(
            '<td>' + n + '</td>' +
            '<td>' + $('<span>').text(desc).html() + '</td>' +
            '<td><input type="text" class="pb-input rm-inline additional-item-code" data-f="itemcode" placeholder="Item Code" autocomplete="off"></td>' +
            '<td class="num"><input type="number" class="pb-input rm-inline" data-f="qty" placeholder="0" min="0" step="1"></td>' +
            '<td class="num"><input type="number" class="pb-input rm-inline" data-f="uw" placeholder="0.00" min="0" step="0.01"></td>' +
            '<td class="num rm-tw"><input type="number" class="pb-input rm-inline" data-f="tw" placeholder="0" min="0" step="1"></td>' +
            '<td class="num"><input type="number" class="pb-input rm-inline additional-rate" data-f="rate" placeholder="0.00" min="0" step="0.01"><div class="rate-status" style="display:none;"></div></td>' +
            '<td class="num rm-amt">0</td>' +
            '<td><button type="button" class="btn-comp-del" data-row="' + rowIndex + '" title="Remove"><i class="bi bi-trash"></i></button></td>'
        );
    }
    
    function seedDefaultRows() {
        $('#compEmptyRow').hide();

        // collect descriptions already in the grid
        var existing = [];
        $('#componentTableBody tr[data-row]').each(function () {
            existing.push($.trim($('td', this).eq(1).text()));
        });

        defaultRMRows.forEach(function (d) {
            if (existing.indexOf(d) === -1) {          // add only if missing
                $('#componentTableBody').append(buildInlineRow(d));
            }
        });

        $('#compFooter').show();
        recalcTotals();
        updateCompCount();
    }
    // Live compute for inline-editable rows
    $('#componentTableBody').on('input', '.rm-inline', function () {
        var $tr = $(this).closest('tr');
        var fld = $(this).data('f');
        var qty = parseFloat($tr.find('[data-f="qty"]').val()) || 0;
        var uw = parseFloat($tr.find('[data-f="uw"]').val()) || 0;
        var rate = parseFloat($tr.find('[data-f="rate"]').val()) || 0;

        // qty or unit-weight changed → recompute total weight
        if (fld === 'qty' || fld === 'uw') {
            $tr.find('[data-f="tw"]').val((qty * uw).toFixed(0));
        }

        // read total weight from its input (may have been edited manually)
        var tw = parseFloat($tr.find('[data-f="tw"]').val()) || 0;
        var amt = tw * rate;

        $tr.find('.rm-amt').text(Math.round(amt).toLocaleString('en-IN'));
        recalcTotals();
    });
    /* ── Open / close modal ── */
    function openModal() {
        $('#compDesc').val('');
        $('#boCompItemCode').val('');   // ← NEW

        $('#compQty').val('');
        $('#compUnitWeight').val('');
        $('#compRate').val('');
        $('#compCalcPreview').hide();
        $('#errDesc, #errQty, #errUW, #errRate').hide();
        $('#addComponentModal').addClass('open');
        setTimeout(function () { $('#compDesc').focus(); }, 50);
    }

    function closeModal() {
        $('#addComponentModal').removeClass('open');
    }

    $('#btnAddComponent').on('click', openModal);
    $('#btnCloseModal, #btnCancelModal').on('click', closeModal);

    $('#addComponentModal').on('click', function (e) {
        if ($(e.target).is('#addComponentModal')) closeModal();
    });

    /* ── Live calc preview ── */
    $('#compQty, #compUnitWeight, #compRate').on('input', function () {
        var qty = parseFloat($('#compQty').val()) || 0;
        var uw = parseFloat($('#compUnitWeight').val()) || 0;
        var rt = parseFloat($('#compRate').val()) || 0;
        var tw = qty * uw;
        var amt = tw * rt;

        if (qty > 0 || uw > 0 || rt > 0) {
            $('#prevWeight').text(tw.toFixed(2) + ' kg');
            $('#prevAmount').text('₹' + Math.round(amt).toLocaleString('en-IN'));
            $('#compCalcPreview').show();
        } else {
            $('#compCalcPreview').hide();
        }
    });

    /* ── Confirm add ── */
    $('#btnConfirmAdd').on('click', function () {
        var desc = $.trim($('#compDesc').val());
        var itemCode = $.trim($('#boCompItemCode').val());   // ← NEW

        var qty = parseFloat($('#compQty').val());
        var uw = parseFloat($('#compUnitWeight').val());
        var rate = parseFloat($('#compRate').val());

        $('#errDesc').toggle(!desc);
        $('#boErrItemCode').toggle(!itemCode);   // ← NEW

        $('#errQty').toggle(!qty || qty <= 0);
        $('#errUW').toggle(!uw || uw <= 0);
        $('#errRate').toggle(!rate || rate <= 0);

        if (!desc || !qty || qty <= 0 || !uw || uw <= 0 || !rate || rate <= 0) return;

        var tw = qty * uw;
        var amt = tw * rate;

        rowIndex++;
        var n = $('#componentTableBody tr[data-row]').length + 1;

        var $tr = $('<tr>').attr('data-row', rowIndex).html(
            '<td>' + n + '</td>' +
            '<td>' + $('<span>').text(desc).html() + '</td>' +
            '<td>' + $('<span>').text(itemCode).html() + '</td>' +          // ← NEW

            '<td class="num">' + qty + '</td>' +
            '<td class="num">' + uw.toFixed(2) + '</td>' +
            '<td class="num">' + tw.toFixed(0) + '</td>' +
            '<td class="num">' + rate.toFixed(0) + '</td>' +
            '<td class="num">' + Math.round(amt).toLocaleString('en-IN') + '</td>' +
            '<td><button type="button" class="btn-comp-del" data-row="' + rowIndex + '" title="Remove">' +
            '<i class="bi bi-trash"></i>' +
            '</button></td>'
        );

        $('#compEmptyRow').hide();
        $('#componentTableBody').append($tr);
        $('#compFooter').show();

        recalcTotals();
        updateCompCount();
        closeModal();
    });

    /* ── Delete row ── */
    $(document).on('click', '.btn-comp-del', function () {
        $(this).closest('tr').remove();
        renumberRows();
        recalcTotals();
        updateCompCount();

        if ($('#componentTableBody tr[data-row]').length === 0) {
            $('#compEmptyRow').show();
            $('#compFooter').hide();

            // reset summary cards when all rows gone
            $('#dispAddRMWeight').text('0');
            $('#dispAddRMCost').text('0');
            $('#hAddRMWeight').val('0');
            $('#hAddRMCost').val('0');
            updateGrandTotal();
        }
    });

    /* ── Helpers ── */
    function recalcTotals() {
        var tw = 0, ta = 0;
        $('#componentTableBody tr[data-row]').each(function () {
            var $tds = $('td', this);
            var $twInp = $tds.eq(5).find('input');
            tw += ($twInp.length ? parseFloat($twInp.val()) : parseFloat($tds.eq(5).text().replace(/,/g, ''))) || 0;
            ta += parseFloat($tds.eq(7).text().replace(/,/g, '')) || 0;
        });

        $('#compTotalWeight').text(tw.toFixed(0));
        $('#compTotalAmount').text(Math.round(ta).toLocaleString('en-IN'));

        $('#dispAddRMWeight').text(tw.toFixed(0));
        $('#dispAddRMCost').text(Math.round(ta).toLocaleString('en-IN'));

        $('#hAddRMWeight').val(tw.toFixed(0));
        $('#hAddRMCost').val(Math.round(ta));

        updateGrandTotal();
    }
    //function updateGrandTotal() {
    //    var panelCost = parseFloat(($('#hTotalPriceofPanels').val() || '0').replace(/,/g, '')) || 0;
    //    var frameCost = parseFloat(($('#hTotalPriceofFrame').val() || '0').replace(/,/g, '')) || 0;
    //    var addRMCost = parseFloat(($('#hAddRMCost').val() || '0').replace(/,/g, '')) || 0;

    //    var grandTotal = panelCost + frameCost + addRMCost;

    //    $('#dispTotalRM').text(Math.round(grandTotal).toLocaleString('en-IN'));
    //    $('#hTotalCostOfRawMaterials').val(Math.round(grandTotal));
    //}
    function updateGrandTotal() {
        // strip all commas before parsing — handles Indian number format 64,89,674
        function cleanNum(id) {
            return parseFloat(($('#' + id).val() || '0').replace(/,/g, '')) || 0;
        }

        var panelCost = cleanNum('hTotalPriceofPanels');
        var frameCost = cleanNum('hTotalPriceofFrame');
        var addRMCost = cleanNum('hAddRMCost');

        var grandTotal = panelCost + frameCost + addRMCost;

        $('#dispTotalRM').text(Math.round(grandTotal).toLocaleString('en-IN'));
        $('#hTotalCostOfRawMaterials').val(Math.round(grandTotal));
        console.log($('#hTotalPriceofPanels').val());
        console.log($('#hTotalPriceofFrame').val());
    }
    function renumberRows() {
        $('#componentTableBody tr[data-row]').each(function (i) {
            $('td:first', this).text(i + 1);
        });
    }

    function updateCompCount() {
        $('#compCount').text($('#componentTableBody tr[data-row]').length);
    }

    // ── Page load: init from Razor-rendered rows ────────────
    if ($('#componentTableBody tr[data-row]').length > 0) {
        $('#compEmptyRow').hide();
        $('#compFooter').show();
        recalcTotals();
        updateCompCount();
    }
    // Always ensure the 2 default inline rows, plus any existing rows
    seedDefaultRows();
    //if ($('#componentTableBody tr[data-row]').length > 0) {
    //    $('#compEmptyRow').hide();
    //    $('#compFooter').show();
    //    recalcTotals();
    //    updateCompCount();
    //} else {
    //    seedDefaultRows();
    //}


    //-------------..//
    // ── Re-init when search loads new enquiry ───────────
    //$(document).on('comp:reload', function () {
    //    rowIndex = $('#componentTableBody tr[data-row]').length;

    //    if (rowIndex > 0) {
    //        $('#compEmptyRow').hide();
    //        $('#compFooter').show();
    //        recalcTotals();
    //        updateCompCount();
    //    } else {
    //        $('#compEmptyRow').show();
    //        $('#compFooter').hide();
    //        $('#dispAddRMWeight').text('0');
    //        $('#dispAddRMCost').text('0');
    //        $('#hAddRMWeight').val('0');
    //        $('#hAddRMCost').val('0');
    //        updateCompCount();
    //        seedDefaultRows();     
    //    }
    //});

    $(document).on('comp:reload', function () {
        rowIndex = $('#componentTableBody tr[data-row]').length;

        if (rowIndex > 0) {
            $('#compEmptyRow').hide();
            $('#compFooter').show();
            recalcTotals();
            updateCompCount();
        } else {
            $('#compEmptyRow').show();
            $('#compFooter').hide();
            $('#dispAddRMWeight').text('0');
            $('#dispAddRMCost').text('0');
            $('#hAddRMWeight').val('0');
            $('#hAddRMCost').val('0');
            updateCompCount();
        }
        seedDefaultRows();          // ← ensure defaults after any reload
    });
}
/* ════════════════════════════════════════════════════════
   PRIVATE HELPERS
════════════════════════════════════════════════════════ */
function replaceFoot(tableSelector, newFootHtml) {
    $(tableSelector).find('tfoot').remove();
    if (newFootHtml) {
        $(tableSelector).append('<tfoot>' + newFootHtml + '</tfoot>');
    }
}

function updateText($src, selector) {
    var val = $src.find(selector).text().trim();
    if (val) $(selector).text(val);
}

function updateVal($src, selector) {
    var val = $src.find(selector).val();
    if (val !== undefined) $(selector).val(val);
}

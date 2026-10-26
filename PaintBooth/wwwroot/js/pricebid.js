/* ============================================================
   Bullows PaintBooth Pro — pricebid.js
   Price BID Create page — all calculations + UI
   ============================================================ */

/* ════════════════════════════════════════════════════════
   1. AUTOCOMPLETE
════════════════════════════════════════════════════════ */
function initAutocomplete(searchUrl) {
    $('#txtEnquiryCode').on('input', function () {
        var term = $.trim($(this).val());
        if (term.length < 1) { $('#bidDropdown').hide().empty(); return; }

        $.post(searchUrl, { enquiryCode: term }, function (data) {
            var $dd = $('#bidDropdown').empty();
            if (data.success && data.results.length > 0) {
                $.each(data.results, function (i, code) {
                    $('<div class="bid-dropdown-item">').text(code).appendTo($dd);
                });
                $dd.show();
            } else { $dd.hide(); }
        });
    });

    $(document).on('click', '.bid-dropdown-item', function () {
        $('#txtEnquiryCode').val($(this).text());
        $('#bidDropdown').hide().empty();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtEnquiryCode, #bidDropdown').length)
            $('#bidDropdown').hide();
    });

    $('#txtEnquiryCode').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); $('#btnSearch').trigger('click'); }
    });
}


/* ════════════════════════════════════════════════════════
   2. SEARCH — AJAX POST → FetchPriceBIDData
════════════════════════════════════════════════════════ */
function initSearch(fetchUrl) {
    $('#btnSearch').on('click', function () {
        var code = $.trim($('#txtEnquiryCode').val());
        if (!code) { PB.toast('Please enter an enquiry code.', 'warning'); return; }

        var $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Searching...');

        $.post(fetchUrl, { enquiryCode: code }, function (res) {
            if (res.success) {
                /* Populate RM and BO from server */
                var item = res.data[0];
                $('#txtPriceOfRM').val(Math.round(item.priceOfRM));
                $('#txtPriceOfBO').val(Math.round(item.priceOfBO));

                /* Run the full calculation chain */
                calcLabour();
                recalcFromBase();

                /* Sync hidden SalesNO field so form POST has it */
                $('#hiddenSalesNO').val(code);

                /* Show form + approve section */
                $('#divBidForm').show();
                $('#divApprove').show();

                PB.toast('Data loaded for ' + code, 'success');
            } else {
                PB.toast(res.message || 'No data found.', 'warning');
            }
        })
            .fail(function () { PB.toast('Error fetching data. Try again.', 'error'); })
            .always(function () {
                $btn.prop('disabled', false)
                    .html('<i class="bi bi-search"></i> Search');
            });
    });
}


/* ════════════════════════════════════════════════════════
   3. HELPERS
════════════════════════════════════════════════════════ */
function v(id) { return parseFloat($('#' + id).val()) || 0; }
function s(id, val) { $('#' + id).val(Math.round(val)); }
function sf(id, val, dp) { $('#' + id).val(val.toFixed(dp !== undefined ? dp : 2)); }


/* ════════════════════════════════════════════════════════
   4. CALCULATION FUNCTIONS
════════════════════════════════════════════════════════ */

/* Labour = RM × 35 / 80 */
function calcLabour() {
    s('labourCost', (v('txtPriceOfRM') * 35) / 80);
}

/* Derived weight fields = RM + BO + Labour */
function calcTotalForDerivedFields() {
    var rounded = Math.round(v('txtPriceOfRM') + v('txtPriceOfBO') + v('labourCost'));
    $('#txtEnC').val(rounded);
    $('#txtPnF').val(rounded);
    $('#txtDesignCharges').val(rounded);
    $('#txtFraightValue').val(rounded);
    $('#txtContingency').val(rounded);
}

/* P&F */
function calcPnF() {
    s('txtPnFCost', v('txtPnF') * (v('txtPnFRate') / 100));
}

/* Freight */
function calcFreight() {
    s('txtFreight', v('txtFraightValue') * (v('txtFraightRate') / 100));
}

/* Design Charges */
function calcDesignCharges() {
    s('txtDesignChargesCost', v('txtDesignCharges') * (v('txtDesignChargesRate') / 100));
}

/* Basic Cost = RM + BO + Labour + E&C + P&F + Freight + Design Charges */
function calcTotalBasicCost() {
    var total = v('txtPriceOfRM') + v('txtPriceOfBO') + v('labourCost')
        + v('txtEnCCost') + v('txtPnFCost') + v('txtFreight')
        + v('txtDesignChargesCost');
    $('#txtBasicCost').val(total);

    /* Basic Cost feeds the factor weight fields
       (Contingency weight stays RM + BO + Labour) */
    $('#txtTPC').val(total);
    $('#txtInsurance').val(total);
    $('#txtCommercialFactor').val(total);
    $('#txtIncentiveFactor').val(total);
}

/* Insurance */
function calcInsurance() {
    s('txtInsuranceCost', v('txtInsurance') * (v('txtInsuranceRate') / 100));
}

/* Commercial Factor */
function calcCommercialFactor() {
    s('txtCommercialFactorCost', v('txtCommercialFactor') * (v('txtCommercialFactorRate') / 100));
}

/* Incentive Factor */
function calcIncentiveFactor() {
    s('txtIncentiveFactorCost', v('txtIncentiveFactor') * (v('txtIncentiveFactorRate') / 100));
}

/* TPC */
function calcTPC() {
    s('txtTPCCost', v('txtBasicCost') * (v('txtTPCRate') / 100));
}

/* Contingency */
function calcContingency() {
    s('txtContingencyCost', v('txtContingency') * (v('txtContingencyRate') / 100));
}

/* TVC = Basic Cost + Insurance + Commercial + Incentive + TPC + Contingency */
function calcTVC() {
    var tvc = Math.round(
        v('txtBasicCost') +
        v('txtInsuranceCost') +
        v('txtCommercialFactorCost') +
        v('txtIncentiveFactorCost') +
        v('txtTPCCost') +
        v('txtContingencyCost')
    );
    $('#txtTVCCost').val(tvc);
    $('#txtPOValue').val(tvc);
    calcThroughput();
}

/* PO VALUE = TVC × PO Rate% (min rate 100%) */
function calcPOValue() {
    var rate = v('txtPORate');
    if (rate < 100) {
        $('#errorPORate').text('Minimum value is 100%');
        $('#txtPOValueCost').val('');
        return;
    }
    $('#errorPORate').text('');
    var cost = Math.round(v('txtPOValue') * (rate / 100));
    $('#txtPOValueCost').val(cost);
    $('#NegotiationValue').val(cost);
    calcThroughput();
    calcNegotiation();          // also calls calcBestPrice
    calcPaymentCosts();
}

/* Throughput Value = PO Value Cost − TVC */
function calcThroughput() {
    var poCost = v('txtPOValueCost');
    if (poCost === 0) {
        $('#txtThroughputValue').val('');
        $('#txtThroughputValueRate').val('');
        return;
    }
    var tp = Math.round(poCost - v('txtTVCCost'));
    $('#txtThroughputValue').val(tp);
    sf('txtThroughputValueRate', (tp / poCost) * 100);
}

/* Negotiation Margin */
function calcNegotiation() {
    s('txtNegotiationCost', v('NegotiationValue') * (v('txtNegotiationRate') / 100));
    calcBestPrice();
}

/* Offer Price = PO Value Cost − Negotiation */
function calcBestPrice() {
    $('#txtBestPrice').val(Math.round(v('txtPOValueCost') - v('txtNegotiationCost')));
}


/* ════════════════════════════════════════════════════════
   5. PAYMENT TERMS (all based on PO Value Cost)
════════════════════════════════════════════════════════ */

/* Sum of all payment-term percentages */
function getPaymentTotal() {
    return v('txtAdvance') + v('txtProforma') + v('txtDAP')
        + v('txtSUPPLYBEFOREDISPATCH') + v('txtSUPPLYAFTERDISPATCH')
        + v('txtInstalation') + v('txtCOMMISSIONING');
}

function calcPaymentCosts() {
    var po = v('txtPOValueCost');
    s('txtAdvanceValueCost', po * (v('txtAdvance') / 100));
    s('txtProformaCost', po * (v('txtProforma') / 100));
    s('txtDAPCost', po * (v('txtDAP') / 100));
    s('txtSUPPLYBEFOREDISPATCHCost', po * (v('txtSUPPLYBEFOREDISPATCH') / 100));
    s('txtSUPPLYAFTERDISPATCHCost', po * (v('txtSUPPLYAFTERDISPATCH') / 100));
    s('txtInstalationCost', po * (v('txtInstalation') / 100));
    s('txtCOMMISSIONINGCost', po * (v('txtCOMMISSIONING') / 100));
}

/* Payment Remaining badge */
function updatePaymentRemaining() {
    var total = getPaymentTotal();
    var remaining = Math.round((100 - total) * 10) / 10;
    var $badge = $('#paymentBadge');

    $badge.text(remaining.toFixed(1) + '% remaining');
    $badge.removeClass('ok warning over');

    if (remaining === 0) $badge.addClass('ok');
    else if (remaining > 0) $badge.addClass('warning');
    else $badge.addClass('over');
}


/* ════════════════════════════════════════════════════════
   6. RECALC CHAINS — each step runs everything after it
════════════════════════════════════════════════════════ */

/* From TVC onward: TVC → PO Value → Throughput → Negotiation → Offer Price → Payments */
function recalcFromTVC() {
    calcTVC();
    calcPOValue();
    updatePaymentRemaining();
}

/* From Basic Cost onward: Basic → factors → Contingency → TVC … */
function recalcFromBasic() {
    calcTotalBasicCost();
    calcInsurance();
    calcCommercialFactor();
    calcIncentiveFactor();
    calcTPC();
    calcContingency();
    recalcFromTVC();
}

/* From RM / BO / Labour onward: derived weights → P&F, Freight, Design → Basic … */
function recalcFromBase() {
    calcTotalForDerivedFields();
    calcPnF();
    calcFreight();
    calcDesignCharges();
    recalcFromBasic();
}


/* ════════════════════════════════════════════════════════
   7. EVENT BINDINGS
════════════════════════════════════════════════════════ */
function initCalculations() {

    /* RM → recalculates Labour too */
    $(document).on('input', '#txtPriceOfRM', function () { calcLabour(); recalcFromBase(); });

    /* BO / Labour (manual) */
    $(document).on('input', '#txtPriceOfBO, #labourCost', recalcFromBase);

    /* E&C cost (typed, or applied from the calculator modal) */
    $(document).on('input', '#txtEnCCost', recalcFromBasic);

    /* P&F / Freight / Design Charges */
    $(document).on('input', '#txtPnFRate', function () { calcPnF(); recalcFromBasic(); });
    $(document).on('input', '#txtFraightRate', function () { calcFreight(); recalcFromBasic(); });
    $(document).on('input', '#txtDesignChargesRate', function () { calcDesignCharges(); recalcFromBasic(); });

    /* Factor costs */
    $(document).on('input', '#txtInsuranceRate', function () { calcInsurance(); recalcFromTVC(); });
    $(document).on('input', '#txtCommercialFactorRate', function () { calcCommercialFactor(); recalcFromTVC(); });
    $(document).on('input', '#txtIncentiveFactorRate', function () { calcIncentiveFactor(); recalcFromTVC(); });
    $(document).on('input', '#txtTPCRate', function () { calcTPC(); recalcFromTVC(); });
    $(document).on('input', '#txtContingencyRate', function () { calcContingency(); recalcFromTVC(); });

    /* PO Rate (validate min 100%) */
    $(document).on('input blur', '#txtPORate', function () {
        calcPOValue();
        updatePaymentRemaining();
    });

    /* PO Value cost typed directly */
    $(document).on('input', '#txtPOValueCost', function () {
        $('#NegotiationValue').val(v('txtPOValueCost'));
        calcThroughput();
        calcNegotiation();
        calcPaymentCosts();
    });

    /* Negotiation */
    $(document).on('input', '#txtNegotiationRate', calcNegotiation);

    /* Payment terms */
    $(document).on('input',
        '#txtAdvance, #txtProforma, #txtDAP, #txtSUPPLYBEFOREDISPATCH, ' +
        '#txtSUPPLYAFTERDISPATCH, #txtInstalation, #txtCOMMISSIONING',
        function () {
            calcPaymentCosts();
            updatePaymentRemaining();
        }
    );
    function toggleStageRows() {
        var useProforma = v('txtProforma') > 0;
        $('#txtDAP, #txtSUPPLYBEFOREDISPATCH, #txtSUPPLYAFTERDISPATCH, #txtInstalation, #txtCOMMISSIONING')
            .closest('tr').toggle(!useProforma);
    }
    $(document).on('input', '#txtProforma', toggleStageRows);
    toggleStageRows();
}


/* ════════════════════════════════════════════════════════
   8. APPROVE CHECKBOX
════════════════════════════════════════════════════════ */
function initApprove() {
    var $chk = $('#approvePriceBid');
    var $save = $('#btnSave');
    $save.prop('disabled', true);

    $chk.on('change', function () {
        if (this.checked) {
            if (confirm('Do you want to approve this Price Bid?')) {
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
   9. FORM SUBMIT — sync SalesNO + validate payment terms
════════════════════════════════════════════════════════ */
function initSave() {
    $('#bidForm').on('submit', function (e) {

        // Always sync SalesNO from the visible search input
        var code = $.trim($('#txtEnquiryCode').val());
        $('#hiddenSalesNO').val(code);
        $('#hiddenSalesNOBackup').val(code);

        if (!code) {
            e.preventDefault();
            PB.toast('Please search an enquiry code first.', 'warning');
            return;
        }

        // Payment terms must total exactly 100%
        if (Math.abs(getPaymentTotal() - 100) > 0.001) {
            e.preventDefault();
            PB.toast('Payment Terms must total exactly 100% before saving.', 'warning');
            return;
        }

        $('#btnSave').addClass('loading').prop('disabled', true);
    });
}


/* ════════════════════════════════════════════════════════
   10. AUTO-DISMISS ALERTS
════════════════════════════════════════════════════════ */
function initAlerts() {
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () { $el.fadeOut(400, function () { $el.remove(); }); }, 4000);
    });
}


/* ════════════════════════════════════════════════════════
   11. EDIT MODE — recalculate all fields from saved values
════════════════════════════════════════════════════════ */
function initEditMode() {
    $('#txtEnquiryCode').prop('readonly', true);
    $('#btnSearch').prop('disabled', true);

    // Rates are already bound from the model — run the full chain
    calcLabour();
    recalcFromBase();

    if ($('#approvePriceBid').prop('checked')) {
        $('#btnSave').prop('disabled', false);
    }
}


/* ════════════════════════════════════════════════════════
   12. E&C CALCULATOR MODAL
════════════════════════════════════════════════════════ */
function initEncCalculator(saveUrl, loadUrl) {

    /* Open modal — load saved data if exists */
    $('#btnCalcEnC').on('click', function () {
        var salesNO = $.trim($('#hiddenSalesNO').val() || $('#txtEnquiryCode').val());

        if (salesNO && loadUrl) {
            $.get(loadUrl, { salesNO: salesNO }, function (res) {
                if (res.success && res.rowsJson) {
                    restoreEncRows(res.rowsJson);
                    $('#encMarginAmt').val(res.margin);
                    recalcEncTotals();
                }
            });
        }

        $('#encModal').addClass('open');
    });

    $('#btnCloseEncModal, #btnCancelEncModal').on('click', function () {
        $('#encModal').removeClass('open');
    });

    $('#encModal').on('click', function (e) {
        if ($(e.target).is('#encModal')) $('#encModal').removeClass('open');
    });

    $(document).on('input', '#encTableBody .enc-persons, #encTableBody .enc-rate, #encTableBody .enc-days',
        function () {
            recalcEncRow($(this).closest('tr'));
            recalcEncTotals();
        });

    $('#encMarginAmt').on('input', recalcEncTotals);

    /* Apply + Save */
    $('#btnApplyEnC').on('click', function () {
        var finalVal = parseFloat(($('#encFinalValue').val() || '0').replace(/,/g, '')) || 0;

        $('#txtEnCCost').val(Math.round(finalVal)).trigger('input');

        var salesNO = $.trim($('#hiddenSalesNO').val() || $('#txtEnquiryCode').val());
        var rowsJson = collectEncRows();
        var margin = parseFloat($('#encMarginAmt').val()) || 0;
        var subTotal = parseFloat(($('#encSubTotal').text() || '0').replace(/,/g, '')) || 0;

        if (salesNO && saveUrl) {
            $.post(saveUrl, {
                salesNO: salesNO,
                rowsJson: rowsJson,
                margin: margin,
                subTotal: subTotal,
                finalValue: finalVal
            }, function (res) {
                if (res.success) PB.toast('E&C calculation saved.', 'success');
            });
        }

        $('#encModal').removeClass('open');
    });

    /* Helpers */
    function recalcEncRow($tr) {
        var persons = parseFloat($tr.find('.enc-persons').val()) || 0;
        var rate = parseFloat($tr.find('.enc-rate').val()) || 0;
        var days = parseFloat($tr.find('.enc-days').val()) || 0;
        var cost = persons * rate * days;
        $tr.find('.enc-cost').text(cost > 0 ? Math.round(cost).toLocaleString('en-IN') : '0');
    }

    function recalcEncTotals() {
        var subTotal = 0;
        $('#encTableBody tr').each(function () {
            subTotal += parseFloat($(this).find('.enc-cost').text().replace(/,/g, '')) || 0;
        });

        var finalVal = subTotal + (parseFloat($('#encMarginAmt').val()) || 0);

        $('#encSubTotal').text(Math.round(subTotal).toLocaleString('en-IN'));
        $('#encSubTotalDisplay').val(Math.round(subTotal).toLocaleString('en-IN'));
        $('#encFinalValue').val(Math.round(finalVal).toLocaleString('en-IN'));
    }

    function collectEncRows() {
        var rows = [];
        $('#encTableBody tr').each(function (i) {
            rows.push({
                rowIndex: i + 1,
                description: $(this).find('.enc-desc').val(),
                persons: parseFloat($(this).find('.enc-persons').val()) || 0,
                perDayRate: parseFloat($(this).find('.enc-rate').val()) || 0,
                totalDays: parseFloat($(this).find('.enc-days').val()) || 0,
                totalCost: parseFloat($(this).find('.enc-cost').text().replace(/,/g, '')) || 0
            });
        });
        return JSON.stringify(rows);
    }

    function restoreEncRows(rowsJson) {
        try {
            var rows = JSON.parse(rowsJson);
            $('#encTableBody tr').each(function (i) {
                if (i < rows.length) {
                    var r = rows[i];
                    $(this).find('.enc-desc').val(r.description);
                    $(this).find('.enc-persons').val(r.persons);
                    $(this).find('.enc-rate').val(r.perDayRate);
                    $(this).find('.enc-days').val(r.totalDays);
                    recalcEncRow($(this));
                }
            });
        } catch (e) {
            console.error('restoreEncRows failed:', e);
        }
    }
}

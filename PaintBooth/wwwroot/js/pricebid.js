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

                /* Run all calculations in order */
                calcLabour();
                calcTotalForDerivedFields();
               /* calcEnC();*/
                calcPnF();
                calcFreight();
                calcDesignCharges();
                calcTotalBasicCost();
                calcInsurance();
                calcCommercialFactor();
                calcIncentiveFactor();
                calcTPC();
                calcContingency();
                calcTVC();
                calcPOValue();
                calcNegotiation();
                calcBestPrice();
                updatePaymentRemaining();

                /* ── Sync hidden SalesNO field so form POST has it ── */
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
   3. ALL CALCULATION FUNCTIONS
   (exact port of old JS — same logic, cleaner structure)
════════════════════════════════════════════════════════ */

function v(id) { return parseFloat($('#' + id).val()) || 0; }
function s(id, val) { $('#' + id).val(Math.round(val)); }
function sf(id, val, dp) { $('#' + id).val(val.toFixed(dp !== undefined ? dp : 2)); }

/* Labour = RM × 35 / 80 */
function calcLabour() {
    s('labourCost', (v('txtPriceOfRM') * 35) / 80);
}

/* Set derived Weight fields = RM + BO + Labour */
function calcTotalForDerivedFields() {
    var total = v('txtPriceOfRM') + v('txtPriceOfBO') + v('labourCost');
    var rounded = Math.round(total);
    $('#txtEnC').val(rounded);
    $('#txtPnF').val(rounded);
    $('#txtDesignCharges').val(rounded);
    $('#txtFraightValue').val(rounded);
    $('#txtContingency').val(rounded);
}

/* E&C = weight × rate% */
//function calcEnC() {
//    s('txtEnCCost', v('txtEnC') * (v('txtEnCRate') / 100));
//}

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

/* Basic Cost = RM + BO + Labour + E&C + P&F + Freight + DesignCharges */
function calcTotalBasicCost() {
    var total = v('txtPriceOfRM') + v('txtPriceOfBO') + v('labourCost')
              + v('txtEnCCost')  + v('txtPnFCost')   + v('txtFreight')
              + v('txtDesignChargesCost');
    $('#txtBasicCost').val(total);
    /* Feed BasicCost into factor weight fields */
    $('#txtTPC').val(total);
    $('#txtInsurance').val(total);
    $('#txtCommercialFactor').val(total);
    $('#txtIncentiveFactor').val(total);
    /* Contingency weight is RM+BO+Labour — keep it separate */
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
    calcTVC();
}

/* TVC = BasicCost + Insurance + CommercialFactor + IncentiveFactor + TPC + Contingency */
function calcTVC() {
    var tvc = Math.round(
        v('txtBasicCost')           +
        v('txtInsuranceCost')       +
        v('txtCommercialFactorCost')+
        v('txtIncentiveFactorCost') +
        v('txtTPCCost')             +
        v('txtContingencyCost')
    );
    $('#txtTVCCost').val(tvc);
    $('#txtPOValue').val(tvc);
    calcThroughput();
}

/* PO VALUE = POValue × PORate% (min rate 100%) */
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
    calcNegotiation();
    calcBestPrice();
    calcPaymentCosts();
}

/* Throughput Value = POValueCost - TVCCost */
function calcThroughput() {
    var poCost = v('txtPOValueCost');
    if (poCost === 0) { $('#txtThroughputValue').val(''); return; }
    var tp = Math.round(poCost - v('txtTVCCost'));
    $('#txtThroughputValue').val(tp);
    /* Throughput rate % */
    if (poCost > 0)
        sf('txtThroughputValueRate', (tp / poCost) * 100);
}

/* Negotiation Margin */
function calcNegotiation() {
    s('txtNegotiationCost', v('NegotiationValue') * (v('txtNegotiationRate') / 100));
    calcBestPrice();
}

/* Offer Price (Best Price) = POValueCost - NegotiationCost */
function calcBestPrice() {
    var best = v('txtPOValueCost') - v('txtNegotiationCost');
    $('#txtBestPrice').val(Math.round(best));
}

/* Payment term costs (all based on POValueCost) */
//function calcPaymentCosts() {
//    var po = v('txtPOValueCost');
//    sf('txtAdvanceValueCost',          po * (v('txtAdvance')                / 100));
//    sf('txtDAPCost',                   po * (v('txtDAP')                    / 100));
//    sf('txtSUPPLYBEFOREDISPATCHCost',  po * (v('txtSUPPLYBEFOREDISPATCH') / 100));
//    sf('txtSUPPLYAFTERDISPATCHCost',   po * (v('txtSUPPLYAFTERDISPATCH')  / 100));
//    sf('txtInstalationCost',            po * (v('txtInstalation')           / 100));
//    sf('txtCOMMISSIONINGCost',          po * (v('txtCOMMISSIONING')        / 100));
//}
function calcPaymentCosts() {
    var po = v('txtPOValueCost');
    s('txtAdvanceValueCost', po * (v('txtAdvance') / 100));
    s('txtDAPCost', po * (v('txtDAP') / 100));
    s('txtSUPPLYBEFOREDISPATCHCost', po * (v('txtSUPPLYBEFOREDISPATCH') / 100));
    s('txtSUPPLYAFTERDISPATCHCost', po * (v('txtSUPPLYAFTERDISPATCH') / 100));
    s('txtInstalationCost', po * (v('txtInstalation') / 100));
    s('txtCOMMISSIONINGCost', po * (v('txtCOMMISSIONING') / 100));
}
/* Payment Remaining badge */
function updatePaymentRemaining() {
    var total = v('txtAdvance') + v('txtDAP') + v('txtSUPPLYBEFOREDISPATCH')
              + v('txtSUPPLYAFTERDISPATCH') + v('txtInstalation') + v('txtCOMMISSIONING');
    var remaining = 100 - total;
    var $badge = $('#paymentBadge');

    $badge.text(remaining.toFixed(1) + '% remaining');
    $badge.removeClass('ok warning over');

    if (total === 100)       $badge.addClass('ok');
    else if (total < 100)    $badge.addClass('warning');
    else                     $badge.addClass('over');
}


/* ════════════════════════════════════════════════════════
   4. EVENT BINDINGS
════════════════════════════════════════════════════════ */
function initCalculations() {

    /* RM / BO change → recalc chain */
    $(document).on('input', '#txtPriceOfRM', function () {
        calcLabour(); calcTotalForDerivedFields();
        /*calcEnC();*/ calcPnF(); calcFreight(); calcDesignCharges();
        calcTotalBasicCost(); calcTVC();
    });
    $(document).on('input', '#txtPriceOfBO, #labourCost', function () {
        calcTotalForDerivedFields();
        calcEnC(); calcPnF(); calcFreight(); calcDesignCharges();
        calcTotalBasicCost(); calcTVC();
    });

    /* E&C */
    /*$(document).on('input', '#txtEnCRate', function () { calcEnC(); calcTotalBasicCost(); calcTVC(); });*/

    /* E&C Cost direct change (from calculator modal) ← ADD HERE */
    $(document).on('input', '#txtEnCCost', function () {
        calcTotalBasicCost();
        calcInsurance();
        calcCommercialFactor();
        calcIncentiveFactor();
        calcTPC();
        calcContingency();
        calcTVC();
        calcPOValue();
        calcNegotiation();
        calcBestPrice();
        calcPaymentCosts();
    });

    /* P&F */
    $(document).on('input', '#txtPnFRate', function () { calcPnF(); calcTotalBasicCost(); calcTVC(); });

    /* Freight */
    $(document).on('input', '#txtFraightRate', function () { calcFreight(); calcTotalBasicCost(); calcTVC(); });

    /* Design Charges */
    $(document).on('input', '#txtDesignChargesRate', function () {
        calcDesignCharges(); calcTotalBasicCost(); calcTVC();
    });

    /* Insurance */
    $(document).on('input', '#txtInsuranceRate', function () { calcInsurance(); calcTVC(); });

    /* Commercial Factor */
    $(document).on('input', '#txtCommercialFactorRate', function () { calcCommercialFactor(); calcTVC(); });

    /* Incentive Factor */
    $(document).on('input', '#txtIncentiveFactorRate', function () { calcIncentiveFactor(); calcTVC(); });

    /* TPC */
    $(document).on('input', '#txtTPCRate', function () { calcTPC(); calcTVC(); });

    /* Contingency */
    $(document).on('input', '#txtContingencyRate', function () { calcContingency(); });

    /* PO Rate (validate min 100%) */
    $(document).on('input blur', '#txtPORate', function () { calcPOValue(); });

    /* Negotiation */
    $(document).on('input', '#txtNegotiationRate', function () { calcNegotiation(); });

    /* Payment terms */
    $(document).on('input',
        '#txtAdvance, #txtDAP, #txtSUPPLYBEFOREDISPATCH, ' +
        '#txtSUPPLYAFTERDISPATCH, #txtInstalation, #txtCOMMISSIONING',
        function () {
            calcPaymentCosts();
            updatePaymentRemaining();
        }
    );

    /* PO Value changes → recalc throughput + payment */
    $(document).on('input', '#txtPOValueCost', function () {
        calcThroughput(); calcNegotiation(); calcBestPrice(); calcPaymentCosts();
    });
}


/* ════════════════════════════════════════════════════════
   5. APPROVE CHECKBOX
════════════════════════════════════════════════════════ */
function initApprove() {
    var $chk  = $('#approvePriceBid');
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
   6. FORM SUBMIT — sync SalesNO + validate payment terms
════════════════════════════════════════════════════════ */
function initSave() {
    $('#bidForm').on('submit', function (e) {

        // ── Always sync SalesNO from the visible search input ──
        // This is the safety net — even if JS search didn't fire,
        // the value the user typed is sent with the form.
        var code = $.trim($('#txtEnquiryCode').val());
        $('#hiddenSalesNO').val(code);
        $('#hiddenSalesNOBackup').val(code);

        // ── Validate SalesNO is present ───────────────────────
        if (!code) {
            e.preventDefault();
            PB.toast('Please search an enquiry code first.', 'warning');
            return;
        }

        // ── Validate payment terms total = 100% ──────────────
        var total = v('txtAdvance') + v('txtDAP')
                  + v('txtSUPPLYBEFOREDISPATCH') + v('txtSUPPLYAFTERDISPATCH')
                  + v('txtInstalation') + v('txtCOMMISSIONING');

        if (total !== 100) {
            e.preventDefault();
            PB.toast('Payment Terms must total exactly 100% before saving.', 'warning');
            return;
        }

        $('#btnSave').addClass('loading').prop('disabled', true);
    });
}


/* ════════════════════════════════════════════════════════
   7. AUTO-DISMISS ALERTS
════════════════════════════════════════════════════════ */
function initAlerts() {
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () { $el.fadeOut(400, function () { $el.remove(); }); }, 4000);
    });
}
/* ════════════════════════════════════════════════════════
   8. EDIT MODE — recalculate all fields from saved values
════════════════════════════════════════════════════════ */
function initEditMode() {
    // Lock enquiry field
    $('#txtEnquiryCode').prop('readonly', true);
    $('#btnSearch').prop('disabled', true);

    // Run full calculation chain using saved rate values
    // already bound from model via asp-for / @Html.TextBoxFor
    calcLabour();
    calcTotalForDerivedFields();
    
    calcPnF();
    calcFreight();
    calcDesignCharges();
    calcTotalBasicCost();
    calcInsurance();
    calcCommercialFactor();
    calcIncentiveFactor();
    calcTPC();
    calcContingency();
    calcTVC();
    calcPOValue();
    calcNegotiation();
    calcBestPrice();
    calcPaymentCosts();
    updatePaymentRemaining();

    // Enable save if already approved
    if ($('#approvePriceBid').prop('checked')) {
        $('#btnSave').prop('disabled', false);
    }
}
/* ════════════════════════════════════════════════════════
   E&C CALCULATOR MODAL
════════════════════════════════════════════════════════ */
function initEncCalculator(saveUrl, loadUrl) {

    /* ── Open modal — load saved data if exists ── */
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

    /* ── Apply + Save ── */
    $('#btnApplyEnC').on('click', function () {
        var finalVal = parseFloat(
            ($('#encFinalValue').val() || '0').replace(/,/g, '')
        ) || 0;

        $('#txtEnCCost').val(Math.round(finalVal));
        $('#txtEnCCost').trigger('input');

        // save to DB
        var salesNO = $.trim($('#hiddenSalesNO').val() || $('#txtEnquiryCode').val());
        var rowsJson = collectEncRows();
        var margin = parseFloat($('#encMarginAmt').val()) || 0;
        var subTotal = parseFloat(
            ($('#encSubTotal').text() || '0').replace(/,/g, '')
        ) || 0;

        if (salesNO && saveUrl) {
            $.post(saveUrl, {
                salesNO: salesNO,
                rowsJson: rowsJson,
                margin: margin,
                subTotal: subTotal,
                finalValue: finalVal
            }, function (res) {
                if (res.success)
                    PB.toast('E&C calculation saved.', 'success');
            });
        }

        $('#encModal').removeClass('open');
    });

    /* ── Helpers ── */
    function recalcEncRow($tr) {
        var persons = parseFloat($tr.find('.enc-persons').val()) || 0;
        var rate = parseFloat($tr.find('.enc-rate').val()) || 0;
        var days = parseFloat($tr.find('.enc-days').val()) || 0;
        var cost = persons * rate * days;
        $tr.find('.enc-cost').text(
            cost > 0 ? Math.round(cost).toLocaleString('en-IN') : '0'
        );
    }

    function recalcEncTotals() {
        var subTotal = 0;
        $('#encTableBody tr').each(function () {
            subTotal += parseFloat(
                $(this).find('.enc-cost').text().replace(/,/g, '')
            ) || 0;
        });

        var marginAmt = parseFloat($('#encMarginAmt').val()) || 0;
        var finalVal = subTotal + marginAmt;

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
                totalCost: parseFloat(
                    $(this).find('.enc-cost').text().replace(/,/g, '')
                ) || 0
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
/* ============================================================
   Bullows PaintBooth Pro — setting.js
   jQuery for PanelSetting Create / Edit page
   ============================================================ */

/* ─────────────────────────────────────────────────────────────
   IN-MEMORY SETTINGS LIST
───────────────────────────────────────────────────────────── */
var settingsList = [];
var currentCategory = '';

function isWaterWash() {
    return $.trim(currentCategory).toLowerCase() === 'water wash paint booth';
}

/* Water Curtain — only for Washing Chamber (Extraction + water wash) */
function toggleWaterCurtain() {
    var show = $('#SettingType').val() === 'Extraction' && isWaterWash();
    $('#divWaterCurtain').toggle(show);
    if (!show) $('#drpWaterCurtain').val('').removeClass('pb-input-error');
}
/* ─────────────────────────────────────────────────────────────
   SETTING TYPE LABEL — display text for a stored type value
   (Extraction → "Washing Chamber" for water wash booths)
───────────────────────────────────────────────────────────── */
function getSettingTypeLabel(value) {
    if (!value) return '';
    var $opt = $('#SettingType option[value="' + value + '"]');
    return $opt.length ? $opt.text() : value;
}
/* ─────────────────────────────────────────────────────────────
   BUILD GRID — renders settingsList into the table
───────────────────────────────────────────────────────────── */
function buildGrid() {
    var $tbody = $('#settingsTableBody');
    $tbody.empty();

    if (settingsList.length === 0) {
        $tbody.html(
            '<tr id="emptySettingRow"><td colspan="7">' +
            '<div class="setting-empty">' +
            '<i class="bi bi-inbox"></i>No settings added yet' +
            '<div class="setting-empty-sub">Fill the form above and click "Add to List"</div>' +
            '</div></td></tr>'
        );
        $('#addedCount').text('0 added');
        $('#saveButton').prop('disabled', true);
        return;
    }

    settingsList.forEach(function (item, index) {
        var row =
            '<tr class="setting-data-row" data-index="' + index + '">' +
            /*'<td><span class="badge-status badge-inprogress">' + (item.SettingType || '') + '</span></td>' +*/
            '<td><span class="badge-status badge-inprogress">' + getSettingTypeLabel(item.SettingType) + '</span></td>' +
                '<td>' + (item.PanelWidth  || '—') + '</td>' +
                '<td>' + (item.PanelHeight || '—') + '</td>' +
                '<td>' + (item.SheetThickness || '—') + '</td>' +
                '<td>' + (item.Section || '—') + '</td>' +
            '<td>' + (item.Materials || '—') + '</td>' +
            '<td>' + (item.ItemCode || '—') + '</td>' +
            '<td>' + (item.Rate || '—') + '</td>' +
                '<td style="text-align:right;">' +
                    '<button type="button" class="setting-tbl-edit me-1" onclick="editSetting(' + index + ')">' +
                        '<i class="bi bi-pencil"></i> Edit' +
                    '</button>' +
                    '<button type="button" class="setting-tbl-remove" onclick="removeSetting(' + index + ')">' +
                        '<i class="bi bi-trash3"></i>' +
                    '</button>' +
                '</td>' +
            '</tr>';
        $tbody.append(row);
    });

    $('#addedCount').text(settingsList.length + ' added');
    $('#saveButton').prop('disabled', false);
}

/* ─────────────────────────────────────────────────────────────
   BUILD HIDDEN INPUTS — for form POST (includes SId for update)
───────────────────────────────────────────────────────────── */
function buildHiddenInputs() {
    var $container = $('#hiddenInputsContainer');
    $container.empty();

    settingsList.forEach(function (item, index) {
        for (var key in item) {
            $container.append(
                '<input type="hidden" name="Settings[' + index + '].' + key + '" value="' + (item[key] || '') + '" />'
            );
        }
    });
}

/* ─────────────────────────────────────────────────────────────
   REMOVE SETTING
───────────────────────────────────────────────────────────── */
function removeSetting(index) {
    settingsList.splice(index, 1);
    buildGrid();

    buildHiddenInputs();
}
/* ─────────────────────────────────────────────────────────────
   APPLY PAINTBOOTH TYPE RESTRICTIONS
───────────────────────────────────────────────────────────── */
function applyPaintboothTypeRestrictions(type) {
    var restrictedTypes = ['4', '5'];
    var $extractionOption = $('#SettingType option[value="Extraction"]');

    if (restrictedTypes.indexOf(type) !== -1) {
        $extractionOption.hide().prop('disabled', true);

        // If Extraction is currently selected, reset it
        if ($('#SettingType').val() === 'Extraction') {
            $('#SettingType').val('');
        }
    } else {
        $extractionOption.show().prop('disabled', false);
    }
}
/* ─────────────────────────────────────────────────────────────
   EDIT SETTING — loads row back into form for editing
───────────────────────────────────────────────────────────── */
function editSetting(index) {
    var item = settingsList[index];

    $('#SettingType').val(item.SettingType);
    toggleWaterCurtain();
    $('#drpWaterCurtain').val(item.WaterCurtain || '');
    $('#txtPanelWidth').val(item.PanelWidth);
    $('#txtPanelHeight').val(item.PanelHeight);
    $('#SheetThickness').val(item.SheetThickness);
    $('#Materials').val(item.Materials);
    $('#txtStandardBend1').val(item.StandardBend1);
    $('#txtStandardBend2').val(item.StandardBend2);
    $('#txtPitchDistance').val(item.PitchDistance);
    $('#SlotDimensions').val(item.SlotDimensions);
    $('#drpLightType').val(item.LightTypes);
    $('#drpLightSubType').val(item.LightSubTypes);
    $('#txtLuxLevel').val(item.LuxLevel);
    $('#txtLumens').val(item.Lumens);
    $('#txtItemCode').val(item.ItemCode);   // ← NEW
    $('#txtRate').val(item.Rate);           // ← NEW
    // Section + Bend — trigger section change first then set bend
    $('#drpSection').val(item.Section).trigger('change');

    setTimeout(function () {
        var $bend = $('#drpBendStructural');
        if ($bend.find('option[value="' + item.BendStructural + '"]').length > 0) {
            $bend.val(item.BendStructural);
            $('#txtCustomBendSection').hide().val('');
            $bend.prop('disabled', false);
        } else if (item.BendStructural) {
            // Custom value
            $bend.val('custom').prop('disabled', true);
            $('#txtCustomBendSection').val(item.BendStructural).show();
        }
           // ← keep box in sync on edit
    }, 200);

    // Remove from list — user will re-add after editing
    settingsList.splice(index, 1);
    buildGrid();
    toggleWaterCurtain();
    buildHiddenInputs();

    $('#txtLuxLevel').trigger('change');
    // ──────────────────────────────────────────────────

    // Update button text
    $('#btnAddSetting').html('<i class="bi bi-check-circle"></i> Update');

    // Scroll to form
    $('html, body').animate({ scrollTop: $('#settingForm').offset().top - 80 }, 300);
}

/* ─────────────────────────────────────────────────────────────
   CLEAR FORM
───────────────────────────────────────────────────────────── */
function clearForm() {
    $('#SettingType').val('');
    $('#txtPanelWidth').val('');
    $('#txtPanelHeight').val('');
    $('#SheetThickness').val('');
    $('#drpSection').val('');
    $('#drpBendStructural').empty().append('<option value="">-- Select --</option>').prop('disabled', false);
    $('#txtCustomBendSection').hide().val('');
    $('#txtStandardBend1').val('');
    $('#txtStandardBend2').val('');
    $('#txtPitchDistance').val('');
    $('#SlotDimensions').val('');
    $('#Materials').val('');
    $('#drpLightType').val('');
    $('#drpLightSubType').val('');
    $('#txtLuxLevel').val('1000');
    $('#txtLumens').val('');
    $('#txtItemCode').val('');   // ← NEW
    $('#txtRate').val('');       // ← NEW
    $('#luxLevelMsg').hide().text('');
    $('#btnAddSetting').html('<i class="bi bi-plus-circle"></i> Add to List');
}

/* ─────────────────────────────────────────────────────────────
   LOAD SETTINGS FOR ENQUIRY — shared helper used by both
   autocomplete selection AND edit-mode page load
───────────────────────────────────────────────────────────── */
function loadSettingsForEnquiry(enquiryCode, onComplete) {
    $.ajax({
        url: '/PanelSetting/GetSettingsByEnquiry',
        type: 'POST',
        data: { enquiryCode: enquiryCode },
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (response) {
            settingsList = [];

            if (response.exists && response.settings.length > 0) {
                response.settings.forEach(function (item) {
                    settingsList.push({
                        SId           : item.sId,           // ← carries DB id for UPDATE
                        SettingType   : item.settingType,
                        PanelWidth    : item.panelWidth,
                        PanelHeight   : item.panelHeight,
                        SheetThickness: item.sheetThickness,
                        Section       : item.section,
                        BendStructural: item.bendStructural,
                        Materials     : item.materials,
                        StandardBend1 : item.standardBend1,
                        StandardBend2 : item.standardBend2,
                        PitchDistance : item.pitchDistance,
                        SlotDimensions: item.slotDimensions,
                        LightTypes    : item.lightType,
                        LightSubTypes : item.lightSubType,
                        LuxLevel      : item.luxLevel,
                        Lumens        : item.lumens,
                        ItemCode      : item.itemCode,    // ← NEW
                        Rate          : item.rate,        // ← NEW
                        ItemCodeForStructuralM : item.itemCodeForStructuralM,
                        RateForStructuralM: item.rateForStructuralM,
                        WaterCurtain: item.waterCurtain
                        
                    });
                });

                $('#saveButton').find('.btn-text').html(
                    '<i class="bi bi-check-lg"></i> Update Settings'
                );
            } else {
                $('#saveButton').find('.btn-text').html(
                    '<i class="bi bi-check-lg"></i> Save Settings'
                );
            }

            buildGrid();
            buildHiddenInputs();

            if (typeof onComplete === 'function') onComplete(response);
        },
        error: function () {
            PB.toast('Failed to load existing settings.', 'danger');
        }
    });
}
$('#txtLuxLevel').on('input change', function () {
    var lux = parseFloat($(this).val()) || 0;
    var type = $('#SettingType').val();

    if (type === 'Paintbooth' && lux <= 0) {
        $('#luxLevelMsg')
            .text('Booth Lux Level is 0 — tube light quantity will not be calculated.')
            .show();
    } else {
        $('#luxLevelMsg').hide().text('');
    }
});

/* ─────────────────────────────────────────────────────────────
   DOCUMENT READY
───────────────────────────────────────────────────────────── */
$(function () {

    var bendOptions = [
        { Text: '70*40*3',  Value: '70*40*3'  },
        { Text: '75*40*2',  Value: '75*40*2'  },
        { Text: '100*50*2', Value: '100*50*2' },
        { Text: '100*50*3', Value: '100*50*3' }
    ];

    var structuralOptions = [
        { Text: 'ISMC 75*40',  Value: 'ISMC 75*40'  },
        { Text: 'ISMC 100*50', Value: 'ISMC 100*50' }
    ];
    $('#txtLuxLevel').on('input', function () {
        if ((parseFloat($(this).val()) || 0) > 0) {
            $('#luxLevelMsg').hide().text('');
        }
    });

    /* ─────────────────────────────────────────────────────────
       1. ENQUIRY CODE AUTOCOMPLETE
    ───────────────────────────────────────────────────────── */
    //$('#txtEnquiryCode').on('input', function () {
    //    var term = $.trim($(this).val());
    //    $('#searchbtn').prop('disabled', term.length === 0);

    //    if (term.length < 1) {
    //        $('#resultsContainer').hide().empty();
    //        return;
    //    }
    $('#txtEnquiryCode').on('input', function () {
        var term = $.trim($(this).val());
        $('#searchbtn').prop('disabled', term.length === 0);

        // ← NEW: reset label + restrictions until a new enquiry is selected
        $('#SettingType option[value="Extraction"]')
            .text('Extraction Chamber')
            .show()
            .prop('disabled', false);

        if (term.length < 1) {
            $('#resultsContainer').hide().empty();
            return;
        }

        $.ajax({
            url: '/PanelSetting/GetEnquiryCodes',
            type: 'POST',
            data: { enquiryCode: term },
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (res) {
                var $dd = $('#resultsContainer').empty();
                if (res.success && res.results.length > 0) {
                    $.each(res.results, function (i, code) {
                        $('<div class="pb-dropdown-item">').text(code).appendTo($dd);
                    });
                    $dd.show();
                } else {
                    $dd.append('<div class="pb-dropdown-item" style="color:var(--text-muted);">No results</div>').show();
                }
            }
        });
    });

    // Select from autocomplete dropdown
    $(document).on('click', '.pb-dropdown-item', function () {
        var code = $(this).text();
        if (code === 'No results') return;
        console.log('Selected enquiry code:', code);
        $('#txtEnquiryCode').val(code);
        $('#resultsContainer').hide().empty();
        $('#searchbtn').prop('disabled', false);
        fetchEnquiryCategory(code); 
        // Load any existing settings for selected enquiry
        loadSettingsForEnquiry(code, function (response) {
            if (response.exists) {
                PB.toast('Existing settings loaded for ' + code, 'info');
            }
        });
    });

    // Close autocomplete when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtEnquiryCode, #resultsContainer').length) {
            $('#resultsContainer').hide();
        }
    });
    $('#SettingType').on('change', function () {
        $('#txtLuxLevel').trigger('change');
        toggleWaterCurtain(); 
    });
    function fetchEnquiryCategory(salesNo) {
        $.ajax({
            url: '/PanelSetting/GetEnquiryCategory',
            type: 'GET',
            data: { salesNo: salesNo },
            success: function (res) {
                console.log('Category response:', res);   // remove after testing
                if (res.success) {
                    currentCategory = res.category || '';  // ← this was missing
                    updateSettingTypeLabel(res.category);
                    applyPaintboothTypeRestrictions(res.paintboothType);
                    buildGrid();
                    toggleWaterCurtain();
                }
            }
        });
    }

    function updateSettingTypeLabel(category) {
        var $opt = $('#SettingType option[value="Extraction"]');
        $opt.text(
            category === 'Water wash paint booth'
                ? 'Washing Chamber'
                : 'Extraction Chamber'
        );
    }
    /* ─────────────────────────────────────────────────────────
       2. SECTION → BEND/STRUCTURAL DROPDOWN
    ───────────────────────────────────────────────────────── */
    function populateBendDropdown(options) {
        var $bend = $('#drpBendStructural');
        $bend.empty().append('<option value="">-- Select --</option>');

        $.each(options, function (i, opt) {
            $bend.append('<option value="' + opt.Value + '">' + opt.Text + '</option>');
        });

        $bend.append('<option value="custom">Custom</option>');
        $bend.prop('disabled', false);
        $('#txtCustomBendSection').hide().val('');
    }

    $('#drpSection').on('change', function () {
        var val = $(this).val();

        // Show the whole structural-member block only for "Structural member"
        if (val === 'Structural member') {
            $('#divstructuralmember').show();
            populateBendDropdown(structuralOptions);
            $('#txtStructuralM').val($('#drpBendStructural').val() || '');
        } else {
            $('#divstructuralmember').hide();
            // clear the fields inside so stale values aren't saved
            $('#txtStructuralM, #txtItemCodeForStructuralM, #txtRateForStructuralM').val('');
            $('#divstructuralmember .rate-status').hide();

            if (val === 'Bend Section') {
                populateBendDropdown(bendOptions);
            } else {
                $('#drpBendStructural').empty().append('<option value="">-- Select --</option>');
                $('#txtCustomBendSection').hide().val('');
            }
        }
    });

    /* ─────────────────────────────────────────────────────────
       3. BEND DROPDOWN → show custom input if "Custom" selected
    ───────────────────────────────────────────────────────── */
    $('#drpBendStructural').on('change', function () {
        if ($(this).val() === 'custom') {
            $(this).prop('disabled', true);
            $('#txtCustomBendSection').show().focus();
        } else {
            $('#txtCustomBendSection').hide().val('');
            $('#txtStructuralM').val($(this).val());
        }
    });

    // Allow switching back from custom
    $('#txtCustomBendSection').on('dblclick', function () {
        $(this).hide().val('');
        $('#drpBendStructural').prop('disabled', false).val('');
    });
    $('#txtCustomBendSection').on('input', function () {
        $('#txtStructuralM').val($(this).val());
    });

    /* ─────────────────────────────────────────────────────────
       4. ADD TO LIST BUTTON
    ───────────────────────────────────────────────────────── */
    $('#btnAddSetting').on('click', function () {

        var type = $('#SettingType').val();
        if (!type) {
            PB.toast('Please select a Setting Type first.', 'warning');
            return;
        }

        // Prevent duplicate types — editSetting() splices the row
        // out first so re-adding after edit will never hit this
        var exists = settingsList.some(function (s) {
            return s.SettingType === type;
        });
        var typeLabel = getSettingTypeLabel(type);

        if (exists) {
            PB.toast(typeLabel + ' setting already exists. Use the row Edit button to modify it.', 'warning');
            return;
        }
        //if (exists) {
        //    PB.toast(type + ' setting already exists. Use the row Edit button to modify it.', 'warning');
        //    return;
        //}
        // ── Required field validation ─────────────────────────────
        var errors = [];

        function reqField(selector, label) {
            var $el = $(selector);
            var val = ($el.val() || '').trim();
            if (!val) {
                $el.addClass('pb-input-error');
                errors.push(label);
            } else {
                $el.removeClass('pb-input-error');
            }
        }

        function reqSelect(selector, label) {
            var $el = $(selector);
            if (!$el.val()) {
                $el.addClass('pb-input-error');
                errors.push(label);
            } else {
                $el.removeClass('pb-input-error');
            }
        }

        reqField('#txtPanelWidth', 'Panel Width');
        reqField('#txtPanelHeight', 'Panel Height');
        reqSelect('#SheetThickness', 'Sheet Thickness');
        reqSelect('#drpSection', 'Section');
        reqSelect('#Materials', 'Materials');
        reqSelect('#drpLightType', 'Light Type');
        reqSelect('#drpLightSubType', 'Light Sub Type');
        reqField('#txtLuxLevel', 'Lux Level');
        reqField('#txtLumens', 'Lumens');

        if (errors.length > 0) {
            var msg = errors.length <= 3
                ? 'Required: ' + errors.join(', ') + '.'
                : 'Required: ' + errors.slice(0, 3).join(', ') + ' and ' + (errors.length - 3) + ' more.';
            PB.toast(msg, 'danger');
            return;
        }

        // ── Lux Level = 0 warning for Paintbooth (tube light qty won't calculate) ──
        var luxLevel = parseFloat($('#txtLuxLevel').val()) || 0;
        if (type === 'Paintbooth' && luxLevel <= 0) {
            $('#luxLevelMsg')
                .text('Booth Lux Level is 0 — tube light quantity will not be calculated.')
                .show();
        } else {
            $('#luxLevelMsg').hide().text('');
        }
        // ────────────────────────────────────────────────────────────
        // Get bend value — either from dropdown or custom input
        var bendValue = $('#drpBendStructural').prop('disabled')
            ? $('#txtCustomBendSection').val()
            : $('#drpBendStructural').val();

        var setting = {
            SId           : 0,   // 0 = new insert; DB rows carry their real SId
            SettingType   : type,
            PanelWidth    : $('#txtPanelWidth').val(),
            PanelHeight   : $('#txtPanelHeight').val(),
            SheetThickness: $('#SheetThickness').val(),
            Section       : $('#drpSection').val(),
            BendStructural: bendValue,
            Materials     : $('#Materials').val(),
            StandardBend1 : $('#txtStandardBend1').val(),
            StandardBend2 : $('#txtStandardBend2').val(),
            PitchDistance : $('#txtPitchDistance').val(),
            SlotDimensions: $('#SlotDimensions').val(),
            LightTypes    : $('#drpLightType').val(),
            LightSubTypes : $('#drpLightSubType').val(),
            LuxLevel      : $('#txtLuxLevel').val(),
            Lumens        : $('#txtLumens').val(),
            ItemCode      : $('#txtItemCode').val(),   // ← NEW
            Rate: $('#txtRate').val() ,       // ← NEW
            ItemCodeForStructuralM: $('#txtItemCodeForStructuralM').val(),   // ← NEW
            RateForStructuralM: $('#txtRateForStructuralM').val(),        // ← NEW
            WaterCurtain: $('#drpWaterCurtain').val(),
        };

        settingsList.push(setting);
        buildGrid();
        buildHiddenInputs();
        clearForm();

        PB.toast(type + ' setting added successfully!', 'success');
    });


    /* ─────────────────────────────────────────────────────────
       5. CLEAR FORM BUTTON
    ───────────────────────────────────────────────────────── */
    $('#btnClearForm').on('click', function () {
        clearForm();
    });
    // Clear error highlight as user corrects fields
    $(document).on('input change', '.pb-input-error', function () {
        if ($(this).val()) $(this).removeClass('pb-input-error');
    });

    /* ─────────────────────────────────────────────────────────
       6. BEND DIAGRAM TOGGLE
    ───────────────────────────────────────────────────────── */
    $('#btnShowDiagram').on('click', function () {
        var $col = $('#bendDiagramCol');
        var isHidden = $col.is(':hidden');

        if (isHidden) {
            $col.show();
            $(this).addClass('active').html('<i class="bi bi-image"></i> Bend Diagram');
        } else {
            $col.hide();
            $(this).removeClass('active').html('<i class="bi bi-image-fill"></i> Show Diagram');
        }
    });


    /* ─────────────────────────────────────────────────────────
       7. FORM SUBMIT — loading state
    ───────────────────────────────────────────────────────── */
    $('#settingForm').on('submit', function () {
        if (!$('#txtEnquiryCode').val()) {
            PB.toast('Please enter an Enquiry Code.', 'warning');
            return false;
        }

        if (settingsList.length === 0) {
            PB.toast('Please add at least one setting before saving.', 'warning');
            return false;
        }

        var $btn = $('#saveButton');
        $btn.addClass('loading').prop('disabled', true);

        setTimeout(function () {
            $btn.removeClass('loading').prop('disabled', false);
        }, 8000);
    });


    /* ─────────────────────────────────────────────────────────
       8. INITIAL STATE
    ───────────────────────────────────────────────────────── */
    buildGrid();
    // Default Lux Level for new entries
    if (!$('#txtLuxLevel').val()) $('#txtLuxLevel').val('1000');

    /* ─────────────────────────────────────────────────────────
       9. EDIT MODE — auto-load when page opens with a
          pre-filled enquiry code (navigating from Index → Edit)
    ───────────────────────────────────────────────────────── */
    var prefilledCode = $.trim($('#txtEnquiryCode').val());

    if (prefilledCode) {
        // Lock field so enquiry code can't be changed mid-edit
        $('#txtEnquiryCode').prop('readonly', true);
        var initialType = $.trim($('#hdnPaintboothType').val());
        if (initialType) {
            applyPaintboothTypeRestrictions(initialType);
        }
        fetchEnquiryCategory(prefilledCode);   // ← NEW: rename Extraction → Washing Chamber in edit mode
        loadSettingsForEnquiry(prefilledCode, function (response) {
            if (response.exists && response.settings.length > 0) {
                PB.toast('Existing settings loaded for ' + prefilledCode, 'info');
            }
        });
    }

});
/* ─────────────────────────────────────────────────────────────
   ITEM CODE → AUTO-FETCH RATE
───────────────────────────────────────────────────────────── */
$(document).on('blur', '#txtItemCode', function () {
    var itemCode = $.trim($(this).val());
    var $rateInput = $('#txtRate');
    var $status = $('.rate-status');

    if (!itemCode) return;

    $.getJSON('/Costing/GetItemRate', { itemCode: itemCode }, function (res) {
        if (res.success) {
            $rateInput.val(res.rate.toFixed(2));
            $status.text('').hide();
        } else {
            $rateInput.val('0.00');
            $status.text('⚠ ' + (res.message || 'Item not found')).show();
        }
    }).fail(function () {
        $rateInput.val('0.00');
        $status.text('⚠ API unreachable').show();
    });
});

$(document).on('blur', '#txtItemCodeForStructuralM', function () {
    var itemCode = $.trim($(this).val());
    var $rateInput = $('#txtRateForStructuralM');
    var $status = $('.rate-status');

    if (!itemCode) return;

    $.getJSON('/Costing/GetItemRate', { itemCode: itemCode }, function (res) {
        if (res.success) {
            $rateInput.val(res.rate.toFixed(2));
            $status.text('').hide();
        } else {
            $rateInput.val('0.00');
            $status.text('⚠ ' + (res.message || 'Item not found')).show();
        }
    }).fail(function () {
        $rateInput.val('0.00');
        $status.text('⚠ API unreachable').show();
    });
});
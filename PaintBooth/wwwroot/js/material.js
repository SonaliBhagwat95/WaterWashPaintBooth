/* ============================================================
   Bullows PaintBooth Pro — material.js
   JS for Material Master page
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       1. LIVE SEARCH — filter table rows by MOC name
    ───────────────────────────────────────────────────────── */
    $('#matSearch').on('input', function () {
        var keyword = $(this).val().toLowerCase().trim();
        var visible = 0;

        $('#matTable tbody tr.mat-row').each(function () {
            var text = $(this).text().toLowerCase();
            if (text.indexOf(keyword) > -1) {
                $(this).removeClass('hidden');
                visible++;
            } else {
                $(this).addClass('hidden');
            }
        });
    });


    /* ─────────────────────────────────────────────────────────
       2. DELETE CONFIRMATION — two-click pattern
       First click: button turns red + shows "Confirm?"
       Second click: submits the parent form
    ───────────────────────────────────────────────────────── */
    $(document).on('click', '.mat-delete-btn', function () {
        var $btn  = $(this);
        var name  = $btn.data('name') || 'this material';

        if ($btn.hasClass('confirming')) {
            // Second click — submit delete form
            $btn.closest('form').submit();
        } else {
            // First click — ask for confirmation
            $btn.addClass('confirming')
                .html('<i class="bi bi-exclamation-triangle"></i> Sure?')
                .attr('title', 'Click again to confirm delete');

            // Reset after 3 seconds if user doesn't confirm
            setTimeout(function () {
                if ($btn.hasClass('confirming')) {
                    $btn.removeClass('confirming')
                        .html('<i class="bi bi-trash3"></i>')
                        .attr('title', 'Delete');
                }
            }, 3000);
        }
    });


    /* ─────────────────────────────────────────────────────────
       3. FORM VALIDATION — client side before submit
    ───────────────────────────────────────────────────────── */
    $('#materialForm').on('submit', function (e) {
        var moc     = $.trim($('#txtMOC').val());
        var density = $.trim($('#txtDensity').val());
        var rate    = $.trim($('#txtRate').val());

        if (!moc) {
            e.preventDefault();
            PB.toast('Please enter MOC name.', 'warning');
            $('#txtMOC').focus();
            return;
        }

        if (!density || isNaN(parseFloat(density))) {
            e.preventDefault();
            PB.toast('Please enter a valid Density value.', 'warning');
            $('#txtDensity').focus();
            return;
        }

        if (!rate || isNaN(parseFloat(rate))) {
            e.preventDefault();
            PB.toast('Please enter a valid Rate value.', 'warning');
            $('#txtRate').focus();
            return;
        }

        // Show loading state on button
        $('#btnSave').prop('disabled', true)
                    .html('<div class="btn-spinner"></div><span class="btn-text">Saving...</span>');
    });


    /* ─────────────────────────────────────────────────────────
       4. CLEAR BUTTON
    ───────────────────────────────────────────────────────── */
    $('#btnClear').on('click', function () {
        $('#txtMOC').val('').focus();
        $('#txtDensity').val('');
        $('#txtRate').val('');
    });


    /* ─────────────────────────────────────────────────────────
       5. AUTO-DISMISS ALERTS
    ───────────────────────────────────────────────────────── */
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () {
            $el.fadeOut(400, function () { $el.remove(); });
        }, 4000);
    });


    /* ─────────────────────────────────────────────────────────
       6. SCROLL TO ACTIVE ROW (when editing)
    ───────────────────────────────────────────────────────── */
    var $activeRow = $('.mat-row-active');
    if ($activeRow.length) {
        setTimeout(function () {
            $activeRow[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
        }, 300);
    }

});

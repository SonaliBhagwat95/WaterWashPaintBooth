/* ============================================================
   Bullows PaintBooth Pro — proposal-index.js
   Proposal Approval Index page
   ============================================================ */
$(function () {

    /* ── Live search ─────────────────────────────────────────── */
    $('#propSearch').on('input', function () {
        var term    = $(this).val().toLowerCase().trim();
        var visible = 0;
        $('#propTable tbody tr.prop-row').each(function () {
            var show = !term || $(this).text().toLowerCase().includes(term);
            $(this).toggleClass('hidden', !show);
            if (show) visible++;
        });
        $('#visibleCount').text(visible);
    });

    /* ── Auto-dismiss alerts ─────────────────────────────────── */
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () { $el.fadeOut(400, function () { $el.remove(); }); }, 4000);
    });

});

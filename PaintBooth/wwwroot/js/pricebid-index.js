/* ============================================================
   Bullows PaintBooth Pro — pricebid-index.js
   JS for Price BID Index page
   ============================================================ */
$(function () {

    /* ── Live search ─────────────────────────────────────────── */
    $('#bidSearch').on('input', function () {
        var term    = $(this).val().toLowerCase().trim();
        var visible = 0;

        $('#bidTable tbody tr.pb-row').each(function () {
            var show = !term || $(this).text().toLowerCase().includes(term);
            $(this).toggleClass('hidden', !show);
            if (show) visible++;
        });

        $('#visibleCount').text(visible);
    });

    /* ── Auto-dismiss alerts after 4 s ─────────────────────── */
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () {
            $el.fadeOut(400, function () { $el.remove(); });
        }, 4000);
    });

});

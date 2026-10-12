/* ============================================================
   Bullows — enquiry-index.js
   jQuery for Enquiry Index page only
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       LIVE SEARCH — filters table rows as user types
    ───────────────────────────────────────────────────────── */
    $('#enquirySearch').on('input', function () {
        var keyword = $(this).val().toLowerCase().trim();
        var visible = 0;

        $('#enquiryTable tbody tr.enq-row').each(function () {
            if ($(this).text().toLowerCase().indexOf(keyword) > -1) {
                $(this).removeClass('hidden');
                visible++;
            } else {
                $(this).addClass('hidden');
            }
        });

        $('#visibleCount').text(visible);
    });

});

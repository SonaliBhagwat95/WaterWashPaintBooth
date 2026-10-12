/* ============================================================
   Bullows — setting-index.js
   jQuery for PanelSetting Index page
   ============================================================ */

$(function () {

    /* ── Live search ─────────────────────────────────────────── */
    $('#settingSearch').on('input', function () {
        var keyword = $(this).val().toLowerCase().trim();
        var visible = 0;

        $('#settingTable tbody tr.setting-row').each(function () {
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

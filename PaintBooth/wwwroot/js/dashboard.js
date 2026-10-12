/* ============================================================
   Bullows — dashboard.js
   jQuery for Dashboard page interactions
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       1. CLICKABLE TABLE ROWS
       Any <tr data-href="..."> becomes clickable
    ───────────────────────────────────────────────────────── */
    $(document).on('click', 'tr[data-href]', function () {
        window.location.href = $(this).data('href');
    });


    /* ─────────────────────────────────────────────────────────
       2. PIPELINE STAGE — hover tooltip showing label
    ───────────────────────────────────────────────────────── */
    $('.pipeline-stage').on('mouseenter', function () {
        $(this).css('cursor', 'default');
    });


    /* ─────────────────────────────────────────────────────────
       3. STAT CARDS — animate count up on page load
    ───────────────────────────────────────────────────────── */
    $('.stat-value').each(function () {
        var $el      = $(this);
        var target   = parseInt($el.text(), 10);

        // Only animate if it's a number and greater than 0
        if (isNaN(target) || target === 0) return;

        var current  = 0;
        var duration = 800;
        var steps    = 30;
        var increment = target / steps;
        var interval = duration / steps;

        $el.text('0');

        var timer = setInterval(function () {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            $el.text(Math.floor(current));
        }, interval);
    });


    /* ─────────────────────────────────────────────────────────
       4. AUTO REFRESH every 5 minutes
       Reloads dashboard data without full page refresh
    ───────────────────────────────────────────────────────── */
    var refreshInterval = 5 * 60 * 1000; // 5 minutes

    setTimeout(function () {
        location.reload();
    }, refreshInterval);


    /* ─────────────────────────────────────────────────────────
       5. PIPELINE STAGE CLICK — go to filtered enquiry list
    ───────────────────────────────────────────────────────── */
    $('.pipeline-stage').on('click', function () {
        var label = $(this).find('.pipeline-label').text().trim();
        if (label) {
            window.location.href = '/Enquiry/Index?draftType=' + encodeURIComponent(label);
        }
    }).css('cursor', 'pointer');

});

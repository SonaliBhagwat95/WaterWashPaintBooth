/* ============================================================
   Bullows PaintBooth Pro — site.js
   jQuery — theme toggle, sidebar, toast, confirm delete
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       1. THEME TOGGLE
       Saved in localStorage as 'bullows-theme'
    ───────────────────────────────────────────────────────── */
    var THEME_KEY = 'bullows-theme';

    function applyTheme(theme) {
        // Set on <html> tag so all CSS variables switch
        $('html').attr('data-theme', theme);

        // Swap icon sun ↔ moon
        if (theme === 'dark') {
            $('#themeIcon').removeClass('bi-moon-fill').addClass('bi-sun-fill');
        } else {
            $('#themeIcon').removeClass('bi-sun-fill').addClass('bi-moon-fill');
        }

        // Save preference
        localStorage.setItem(THEME_KEY, theme);
    }

    function getSavedTheme() {
        return localStorage.getItem(THEME_KEY) || 'dark';
    }

    // Apply saved theme when page loads
    applyTheme(getSavedTheme());

    // Toggle on button click
    $('#themeToggle').on('click', function () {
        var current = $('html').attr('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    });


    /* ─────────────────────────────────────────────────────────
       2. MOBILE SIDEBAR
    ───────────────────────────────────────────────────────── */
    function openSidebar() {
        $('#sidebar').addClass('open');
        $('#sidebarOverlay').addClass('open');
        $('body').css('overflow', 'hidden');
    }

    function closeSidebar() {
        $('#sidebar').removeClass('open');
        $('#sidebarOverlay').removeClass('open');
        $('body').css('overflow', '');
    }

    $('#hamburgerBtn').on('click', function () {
        if ($('#sidebar').hasClass('open')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    $('#sidebarOverlay').on('click', function () {
        closeSidebar();
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') closeSidebar();
    });


    /* ─────────────────────────────────────────────────────────
       3. TOPBAR SEARCH
    ───────────────────────────────────────────────────────── */
    $('#topbarSearch').on('keydown', function (e) {
        if (e.key === 'Enter') {
            var query = $.trim($(this).val());
            if (query.length > 1) {
                window.location.href = '/Search?q=' + encodeURIComponent(query);
            }
        }
    });


    /* ─────────────────────────────────────────────────────────
       4. CLICKABLE TABLE ROWS
       Add data-href="/Controller/Action/id" on any <tr>
    ───────────────────────────────────────────────────────── */
    $(document).on('click', 'tr[data-href]', function () {
        window.location.href = $(this).data('href');
    });

    $('tr[data-href]').css('cursor', 'pointer');


    /* ─────────────────────────────────────────────────────────
       5. TOAST NOTIFICATION
       Usage: PB.toast('Saved!', 'success');
       Types: success | danger | warning | info
    ───────────────────────────────────────────────────────── */
    window.PB = window.PB || {};

    window.PB.toast = function (message, type) {
        type = type || 'info';

        var icons = {
            success : 'bi-check-circle-fill',
            danger  : 'bi-exclamation-circle-fill',
            warning : 'bi-exclamation-triangle-fill',
            info    : 'bi-info-circle-fill'
        };

        if ($('#pb-toast-container').length === 0) {
            $('body').append(
                '<div id="pb-toast-container" ' +
                'style="position:fixed;top:76px;right:22px;' +
                'z-index:9999;display:flex;flex-direction:column;' +
                'gap:10px;pointer-events:none;"></div>'
            );
        }

        var $toast = $(
            '<div class="pb-alert pb-alert-' + type + '" ' +
            'style="pointer-events:all;min-width:280px;max-width:360px;' +
            'opacity:0;transform:translateX(20px);' +
            'transition:opacity 0.3s ease,transform 0.3s ease;">' +
                '<i class="bi ' + icons[type] + '"></i>' +
                '<span>' + message + '</span>' +
                '<button type="button" style="margin-left:auto;' +
                'background:none;border:none;cursor:pointer;' +
                'color:inherit;font-size:16px;padding:0;line-height:1;">' +
                '&times;</button>' +
            '</div>'
        );

        $('#pb-toast-container').append($toast);

        // Animate in
        setTimeout(function () {
            $toast.css({ opacity: 1, transform: 'translateX(0)' });
        }, 10);

        // Auto dismiss after 4 seconds
        var timer = setTimeout(function () {
            dismissToast($toast);
        }, 4000);

        $toast.find('button').on('click', function () {
            clearTimeout(timer);
            dismissToast($toast);
        });
    };

    function dismissToast($t) {
        $t.css({ opacity: 0, transform: 'translateX(20px)' });
        setTimeout(function () { $t.remove(); }, 320);
    }


    /* ─────────────────────────────────────────────────────────
       6. CONFIRM DELETE DIALOG
       Usage: add data-confirm="Are you sure?" on delete button
    ───────────────────────────────────────────────────────── */
    $(document).on('click', '[data-confirm]', function (e) {
        e.preventDefault();

        var $trigger  = $(this);
        var message   = $trigger.data('confirm') || 'Are you sure you want to delete this?';
        var actionUrl = $trigger.attr('href') || $trigger.data('url');

        $('#pb-confirm-modal').remove();

        var $modal = $(
            '<div id="pb-confirm-modal" ' +
            'style="position:fixed;inset:0;z-index:10000;' +
            'display:flex;align-items:center;justify-content:center;' +
            'background:rgba(0,0,0,0.6);">' +
                '<div style="background:var(--bg-card);border:1px solid var(--border);' +
                'border-radius:16px;padding:28px;max-width:380px;width:90%;">' +
                    '<div style="display:flex;align-items:center;gap:12px;margin-bottom:16px;">' +
                        '<div style="width:40px;height:40px;border-radius:10px;flex-shrink:0;' +
                        'background:rgba(240,72,72,0.1);display:flex;align-items:center;' +
                        'justify-content:center;color:var(--danger);font-size:18px;">' +
                            '<i class="bi bi-trash3"></i>' +
                        '</div>' +
                        '<div>' +
                            '<div style="font-family:var(--font-display);font-weight:600;' +
                            'font-size:15px;color:var(--text-primary);">Confirm Delete</div>' +
                            '<div style="font-size:13px;color:var(--text-muted);margin-top:2px;">' +
                                message +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                    '<div style="display:flex;gap:10px;justify-content:flex-end;margin-top:20px;">' +
                        '<button id="pb-confirm-cancel" class="btn-pb-secondary">Cancel</button>' +
                        '<button id="pb-confirm-ok" class="btn-pb-danger">Delete</button>' +
                    '</div>' +
                '</div>' +
            '</div>'
        );

        $('body').append($modal);

        $('#pb-confirm-cancel').on('click', function () { $modal.remove(); });

        $modal.on('click', function (ev) {
            if ($(ev.target).is($modal)) $modal.remove();
        });

        $('#pb-confirm-ok').on('click', function () {
            $modal.remove();
            var $form = $(
                '<form method="POST" action="' + actionUrl + '">' +
                '<input name="__RequestVerificationToken" value="' +
                $('input[name="__RequestVerificationToken"]').first().val() +
                '" /></form>'
            );
            $('body').append($form);
            $form.submit();
        });
    });


    /* ─────────────────────────────────────────────────────────
       7. AUTO DISMISS ALERTS
       Add data-auto-dismiss on any .pb-alert to auto-hide
    ───────────────────────────────────────────────────────── */
    setTimeout(function () {
        $('.pb-alert[data-auto-dismiss]').fadeOut(400, function () {
            $(this).remove();
        });
    }, 5000);


    /* ─────────────────────────────────────────────────────────
       8. FORM LOADING STATE ON SUBMIT
    ───────────────────────────────────────────────────────── */
    $(document).on('submit', 'form', function () {
        var $btn = $(this).find('[type="submit"]');
        $btn.prop('disabled', true).addClass('loading');
        setTimeout(function () {
            $btn.prop('disabled', false).removeClass('loading');
        }, 8000);
    });

});

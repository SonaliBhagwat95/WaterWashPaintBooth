/* ============================================================
   PaintBooth PaintBooth — login.js
   jQuery — handles login form submit, validation, theme toggle
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       1. THEME TOGGLE (dark / light)
    ───────────────────────────────────────────────────────── */
    function applyTheme(theme) {
        $('html').attr('data-theme', theme);
        var icon = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
        $('#themeIcon').attr('class', icon);
        localStorage.setItem('pb-theme', theme);
    }

    // Apply saved theme when page loads
    applyTheme(localStorage.getItem('pb-theme') || 'dark');

    // Toggle on button click
    $('#themeBtnLogin').on('click', function () {
        var current = $('html').attr('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
    });


    /* ─────────────────────────────────────────────────────────
       2. SHOW / HIDE PASSWORD
    ───────────────────────────────────────────────────────── */
    $('#togglePassBtn').on('click', function () {
        var $input = $('#Password');
        if ($input.attr('type') === 'password') {
            $input.attr('type', 'text');
            $('#eyeIcon').attr('class', 'bi bi-eye-slash');
        } else {
            $input.attr('type', 'password');
            $('#eyeIcon').attr('class', 'bi bi-eye');
        }
    });


    /* ─────────────────────────────────────────────────────────
   3. CAPS LOCK WARNING
───────────────────────────────────────────────────────── */
    //$('#Password').on('keyup', function (e) {
    //    var nativeEvent = e.originalEvent || e;
    //    var capsOn = false;

    //    if (typeof nativeEvent.getModifierState === 'function') {
    //        capsOn = nativeEvent.getModifierState('CapsLock');
    //    }

    //    if (capsOn && $('#capsWarning').length === 0) {
    //        $(this).after(
    //            '<div id="capsWarning" class="field-error" style="margin-top:6px;">' +
    //            '<i class="bi bi-exclamation-circle"></i> Caps Lock is ON</div>'
    //        );
    //    } else if (!capsOn) {
    //        $('#capsWarning').remove();
    //    }
    //});


    /* ─────────────────────────────────────────────────────────
       4. ENTER KEY — Username → Password → Submit
    ───────────────────────────────────────────────────────── */
    $('#Username').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#Password').focus();
        }
    });

    $('#Password').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#loginForm').submit();
        }
    });


    /* ─────────────────────────────────────────────────────────
       5. CLEAR ERROR when user starts typing
    ───────────────────────────────────────────────────────── */
    $('#Username').on('input', function () { hideFieldError('usernameError'); });
    $('#Password').on('input', function () { hideFieldError('passwordError'); });


    /* ─────────────────────────────────────────────────────────
       6. FORM SUBMIT — sends AJAX POST to AccountController
    ───────────────────────────────────────────────────────── */
    $('#loginForm').on('submit', function (e) {
        e.preventDefault();

        // Clear old errors first
        hideAlert();
        hideFieldError('usernameError');
        hideFieldError('passwordError');

        // Basic client-side check
        var username = $.trim($('#Username').val());
        var password = $('#Password').val();
        var isValid  = true;

        if (username === '') {
            showFieldError('usernameError', 'Username is required.');
            isValid = false;
        }

        if (password === '') {
            showFieldError('passwordError', 'Password is required.');
            isValid = false;
        }

        if (!isValid) return;

        // Show loading state on button
        setLoading(true);

        // Build data to send
        var formData = {
            Username:   username,
            Password:   password,
            RememberMe: $('#RememberMe').is(':checked'),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        };

        // POST to /Account/Login
        $.ajax({
            url:  '/Account/Login',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    // Redirect to dashboard
                    window.location.href = response.redirectUrl;
                } else {
                    showAlert(response.message);
                    setLoading(false);
                }
            },
            error: function () {
                showAlert('Something went wrong. Please try again.');
                setLoading(false);
            }
        });

    });


    /* ─────────────────────────────────────────────────────────
       HELPER FUNCTIONS
    ───────────────────────────────────────────────────────── */

    // Show red error below a field
    function showFieldError(fieldId, message) {
        $('#' + fieldId).find('span').text(message);
        $('#' + fieldId).show();
    }

    // Hide field error
    function hideFieldError(fieldId) {
        $('#' + fieldId).hide();
    }

    // Show top alert box
    function showAlert(message) {
        $('#loginAlertMsg').text(message);
        $('#loginAlert').show();
    }

    // Hide top alert box
    function hideAlert() {
        $('#loginAlert').hide();
    }

    // Button loading state
    function setLoading(isLoading) {
        var $btn = $('#loginBtn');
        $btn.prop('disabled', isLoading);
        if (isLoading) {
            $btn.addClass('loading');
        } else {
            $btn.removeClass('loading');
        }
    }

});

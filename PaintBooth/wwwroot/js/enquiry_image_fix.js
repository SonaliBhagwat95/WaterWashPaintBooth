/* ============================================================
   REPLACE functions 4 and 5 in your enquiry.js
   All 4 images available:
   ~/img/Rectangular.PNG          — Horizontal Rectangular
   ~/img/VerticalRectangular.PNG  — Vertical Rectangular
   ~/img/Circular.PNG             — Vertical Circular
   ~/img/HorizontalCircular.PNG   — Horizontal Circular
   ============================================================ */


    /* ─────────────────────────────────────────────────────────
       4. SHAPE CHANGE
    ───────────────────────────────────────────────────────── */
    $('#drpShape').on('change', function () {
        triggerShapeChange();
    });

    function triggerShapeChange() {
        var shape = $('#drpShape').val();

        if (shape === 'Rectangular') {
            $('#rectangularSection').show();
            $('#circularSection').hide();

            // Set default orientation if empty
            if (!$('#drpOrientation').val()) {
                $('#drpOrientation').val('Horizontal');
            }

        } else if (shape === 'Circular') {
            $('#circularSection').show();
            $('#rectangularSection').hide();

            // Set default orientation if empty
            if (!$('#drpOrientation').val()) {
                $('#drpOrientation').val('Vertical');
            }

        } else {
            $('#rectangularSection').hide();
            $('#circularSection').hide();
        }

        updateShapeImage();
        calculateSurfaceArea();
    }


    /* ─────────────────────────────────────────────────────────
       5. ORIENTATION CHANGE — swap image based on shape + orientation
    ───────────────────────────────────────────────────────── */
    $('#drpOrientation').on('change', function () {
        updateShapeImage();
    });

    function updateShapeImage() {
        var shape       = $('#drpShape').val();
        var orientation = $('#drpOrientation').val();

        if (!shape || !orientation) return;

        var labelTxt = orientation + ' — ' + shape;

        // ── RECTANGULAR ───────────────────────────────────────
        if (shape === 'Rectangular') {

            if (orientation === 'Vertical') {
                swapImage('#rectImg', '/img/VerticalRectangular.PNG');
            } else {
                // Horizontal (default)
                swapImage('#rectImg', '/img/Rectangular.PNG');
            }

            $('#rectOrientationLabel').text(labelTxt);
        }

        // ── CIRCULAR ──────────────────────────────────────────
        else if (shape === 'Circular') {

            if (orientation === 'Horizontal') {
                swapImage('#circularImg', '/img/HorizontalCircular.PNG');
            } else {
                // Vertical (default)
                swapImage('#circularImg', '/img/Circular.PNG');
            }

            $('#circOrientationLabel').text(labelTxt);
        }
    }


    /* ─────────────────────────────────────────────────────────
       Smooth fade animation on image swap
    ───────────────────────────────────────────────────────── */
    function swapImage(selector, newSrc) {
        var $img = $(selector);

        // Skip if already showing same image
        if ($img.attr('src') === newSrc) return;

        // Fade out
        $img.addClass('switching');

        setTimeout(function () {
            $img.attr('src', newSrc);
            // Fade back in
            $img.removeClass('switching');
        }, 180);
    }

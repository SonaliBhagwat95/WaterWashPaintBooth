/* ============================================================
   Bullows PaintBooth Pro — customer.js
   jQuery — Customer Create, Edit, Index pages
   ============================================================ */

$(function () {

    /* ─────────────────────────────────────────────────────────
       1. STATE → CITY AJAX DROPDOWN
    ───────────────────────────────────────────────────────── */
    //$('#stateSelect').on('change', function () {
    //    var stateId     = $(this).val();
    //    var $citySelect = $('#citySelect');

    //    $citySelect.html('<option value="">-- Select City --</option>');

    //    if (!stateId) return;

    //    $.ajax({
    //        url     : '/CustomerDetails/GetCitiesByState',
    //        type    : 'GET',
    //        data    : { stateId: stateId },
    //        success : function (cities) {
    //            $.each(cities, function (i, city) {
    //                $citySelect.append(
    //                    $('<option>').val(city.value).text(city.text)
    //                );
    //            });
    //        },
    //        error: function () {
    //            PB.toast('Could not load cities. Please try again.', 'danger');
    //        }
    //    });
    //});
    function loadCities(stateId, selectedCityId) {
        var $citySelect = $('#citySelect');
        $citySelect.html('<option value="">-- Select City --</option>');

        if (!stateId) return;

        $.ajax({
            url: '/CustomerDetails/GetCitiesByState',
            type: 'GET',
            data: { stateId: stateId },
            success: function (cities) {
                $.each(cities, function (i, city) {
                    $citySelect.append($('<option>').val(city.value).text(city.text));
                });
                if (selectedCityId) {
                    $citySelect.val(selectedCityId);
                }
            },
            error: function () {
                PB.toast('Could not load cities. Please try again.', 'danger');
            }
        });
    }

    $('#stateSelect').on('change', function () {
        loadCities($(this).val(), null);
    });

    // Restore state->city binding on postback / validation-failed reload
    var initialStateId = $('#stateSelect').val();
    var initialCityId = $('#citySelect').data('selected');
    if (initialStateId) {
        loadCities(initialStateId, initialCityId);
    }

    /* ─────────────────────────────────────────────────────────
       2. ADD CONTACT ROW
       Clones a new empty row and appends to container
    ───────────────────────────────────────────────────────── */
    $('#btnAddContact').on('click', function () {
        addContactRow();
    });

    function addContactRow() {
        var $container = $('#contactsContainer');
        var index      = $container.find('.contact-row').length;

        var $newRow = $(
            '<div class="contact-row" data-index="' + index + '">' +

                '<div class="ct-col ct-name">' +
                    '<input type="text" ' +
                    'name="ContactPersons[' + index + '].ContactPerson" ' +
                    'class="pb-input ct-input" placeholder="Full name" />' +
                '</div>' +

                '<div class="ct-col ct-desig">' +
                    '<input type="text" ' +
                    'name="ContactPersons[' + index + '].Designation" ' +
                    'class="pb-input ct-input" placeholder="Designation" />' +
                '</div>' +

                '<div class="ct-col ct-mobile">' +
                    '<input type="text" ' +
                    'name="ContactPersons[' + index + '].Mobile" ' +
                    'class="pb-input ct-input ct-mobile-input" ' +
                    'placeholder="10-digit number" maxlength="10" />' +
                '</div>' +

                '<div class="ct-col ct-email">' +
                    '<input type="email" ' +
                    'name="ContactPersons[' + index + '].EmailId" ' +
                    'class="pb-input ct-input" placeholder="email@company.com" />' +
                '</div>' +

                '<div class="ct-col ct-pan">' +
                    '<input type="text" ' +
                    'name="ContactPersons[' + index + '].PAN" ' +
                    'class="pb-input ct-input ct-pan-input" ' +
                    'placeholder="ABCDE1234F" maxlength="10" />' +
                '</div>' +

                '<div class="ct-col ct-action">' +
                    '<button type="button" class="btn-remove-contact" ' +
                    'title="Remove this contact">' +
                        '<i class="bi bi-trash3"></i>' +
                    '</button>' +
                '</div>' +

            '</div>'
        );

        $container.append($newRow);
        updateContactCount();

        // Focus first input of new row
        $newRow.find('input:first').focus();
    }

    /* ─────────────────────────────────────────────────────────
       3. REMOVE CONTACT ROW
    ───────────────────────────────────────────────────────── */
    $(document).on('click', '.btn-remove-contact', function () {
        var $row = $(this).closest('.contact-row');

        // Animate out then remove
        $row.addClass('removing');
        setTimeout(function () {
            $row.remove();
            reindexContactRows();
            updateContactCount();
        }, 200);
    });

    /* ─────────────────────────────────────────────────────────
       4. RE-INDEX contact rows after a row is removed
       Keeps name="ContactPersons[0]...", [1]..., [2]... correct
    ───────────────────────────────────────────────────────── */
    function reindexContactRows() {
        $('#contactsContainer .contact-row').each(function (newIndex) {
            $(this).attr('data-index', newIndex);

            // Update all input names in this row
            $(this).find('input[name]').each(function () {
                var oldName = $(this).attr('name');
                // Replace the index number: ContactPersons[OLD].Field
                var newName = oldName.replace(/ContactPersons\[\d+\]/, 'ContactPersons[' + newIndex + ']');
                $(this).attr('name', newName);
            });
        });
    }

    /* ─────────────────────────────────────────────────────────
       5. UPDATE CONTACT COUNT BADGE
    ───────────────────────────────────────────────────────── */
    function updateContactCount() {
        var count = $('#contactsContainer .contact-row').length;
        $('#contactCountBadge').text(count);
    }


    /* ─────────────────────────────────────────────────────────
       6. PAN — auto uppercase
    ───────────────────────────────────────────────────────── */
    $(document).on('input', '.ct-pan-input', function () {
        var pos = this.selectionStart;
        $(this).val($(this).val().toUpperCase());
        this.setSelectionRange(pos, pos);
    });


    /* ─────────────────────────────────────────────────────────
       7. MOBILE — numbers only, max 10 digits
    ───────────────────────────────────────────────────────── */
    $(document).on('input', '.ct-mobile-input', function () {
        $(this).val($(this).val().replace(/\D/g, '').substring(0, 10));
    });


    /* ─────────────────────────────────────────────────────────
       8. PIN — numbers only, max 6 digits
    ───────────────────────────────────────────────────────── */
    $('input[name="Pin"]').on('input', function () {
        $(this).val($(this).val().replace(/\D/g, '').substring(0, 6));
    });


    /* ─────────────────────────────────────────────────────────
       9. FORM SUBMIT — loading state
    ───────────────────────────────────────────────────────── */
    $('#customerForm').on('submit', function () {
        var $btn = $('#submitBtn');
        $btn.addClass('loading').prop('disabled', true);
        setTimeout(function () {
            $btn.removeClass('loading').prop('disabled', false);
        }, 8000);
    });


    /* ─────────────────────────────────────────────────────────
       10. INDEX LIVE SEARCH
    ───────────────────────────────────────────────────────── */
    $('#customerSearch').on('input', function () {
        var keyword = $(this).val().toLowerCase().trim();
        var visible = 0;

        $('#customerTable tbody tr.cust-row').each(function () {
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

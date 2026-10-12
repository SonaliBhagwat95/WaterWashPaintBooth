/* ============================================================
   Bullows PaintBooth Pro — proposal.js
   Proposal Create page — autocomplete + PDF + price calc
   ============================================================ */

/* ════════════════════════════════════════════════════════
   1. AUTOCOMPLETE
════════════════════════════════════════════════════════ */
function initAutocomplete(searchUrl) {
    $('#txtSalesNO').on('input', function () {
        var term = $.trim($(this).val());
        if (term.length < 1) { $('#propDropdown').hide().empty(); return; }

        $.post(searchUrl, { enquiryCode: term }, function (data) {
            var $dd = $('#propDropdown').empty();
            if (data.success && data.results.length > 0) {
                $.each(data.results, function (i, code) {
                    $('<div class="prop-dropdown-item">').text(code).appendTo($dd);
                });
                $dd.show();
            } else { $dd.hide(); }
        });
    });

    $(document).on('click', '.prop-dropdown-item', function () {
        $('#txtSalesNO').val($(this).text());
        $('#propDropdown').hide().empty();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtSalesNO, #propDropdown').length)
            $('#propDropdown').hide();
    });

    $('#txtSalesNO').on('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); $('#btnSearch').trigger('click'); }
    });
}


/* ════════════════════════════════════════════════════════
   2. SEARCH — POST to ShowProposalForm, redirect with SalesNO
════════════════════════════════════════════════════════ */
function initSearch(showUrl) {
    $('#btnSearch').on('click', function () {
        var code = $.trim($('#txtSalesNO').val());
        if (!code) { PB.toast('Please enter an enquiry code.', 'warning'); return; }

        var $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Loading...');

        // POST form submit to ShowProposalForm
        var $form = $('<form method="post" style="display:none">')
            .attr('action', showUrl)
            .append($('<input type="hidden" name="SalesNO">').val(code))
            .append($('<input type="hidden" name="__RequestVerificationToken">')
                .val($('input[name="__RequestVerificationToken"]').val()));

        $('body').append($form);
        $form.submit();
    });
}


/* ════════════════════════════════════════════════════════
   3. PRICE CALCULATIONS  (Total, Discount, GST, Gross)
════════════════════════════════════════════════════════ */
function initPriceCalc() {

    function recalc() {
        var total = 0;
        $('.price-input').each(function () {
            total += parseFloat($(this).val()) || 0;
        });
        $('#totalPrice').val(total.toFixed(0));

        var discPct = parseFloat($('#discountPercentage').val()) || 0;
        var discAmt = (total * discPct) / 100;
        $('#discountAmount').val(discAmt.toFixed(0));

        var finalPrice = total - discAmt;
        $('#finalPrice').val(finalPrice.toFixed(0));

        var gstPct = parseFloat($('#gstPercentage').val()) || 0;
        var gstAmt = (finalPrice * gstPct) / 100;
        $('#gstAmount').val(gstAmt.toFixed(0));

        $('#grossAmount').val((finalPrice + gstAmt).toFixed(0));
    }

    $(document).on('input', '.price-input, #discountPercentage, #gstPercentage', recalc);
    recalc(); // run on page load for edit mode
}



function initPDF(salesNo, revisionNo, currentDate) {
    $('#btnDownloadPDF').on('click', function () {
        var $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Generating PDF...');

        var element = document.getElementById('proposalContent');
        var fileNo = salesNo || $.trim($('#txtSalesNO').val()) || 'Proposal';
        var revLabel = 'R' + (revisionNo || '0');

       

        // ── Pre-load logo before generating ──────────────────

        var options = {
            filename: 'Proposal_' + fileNo + '.pdf',
            margin: [30, 10, 18, 10],          // top 30 clears your injected header band
            //html2canvas: {
            //    scale: 2,
            //    useCORS: true,
            //    logging: false,
            //    letterRendering: true,
            //    windowWidth: 794,
            //    scrollX: 0,
            //    scrollY: -window.scrollY
            //},
            html2canvas: {
                scale: 2,
                useCORS: true,
                logging: false,
                letterRendering: true,
                windowWidth: 794,
                scrollX: 0,
                scrollY: -window.scrollY,
                backgroundColor: '#ffffff',          // ← canvas base always white
                onclone: function (clonedDoc) {
                    // Force light theme on the cloned document only (screen stays dark)
                    var root = clonedDoc.documentElement;
                    root.setAttribute('data-theme', 'light');
                    root.setAttribute('data-bs-theme', 'light');
                    root.classList.remove('dark', 'dark-mode', 'theme-dark');
                    clonedDoc.body.classList.remove('dark', 'dark-mode', 'theme-dark');

                    // html2pdf duplicates #proposalContent, so tag every copy
                    clonedDoc.querySelectorAll('#proposalContent').forEach(function (el) {
                        el.classList.add('pdf-export');
                    });
                }
            },
            jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait', compress: true },
            pagebreak: {
                mode: ['css', 'legacy'],
                // page breaks come from your CSS. Only ROWS/small blocks are protected.
                avoid: ['tr', '.pb-keep', '.prop-no-break','.prop-signature', '.prop-cc-block',
                    '.prop-payment-table', '.prop-price-total-row', '.prop-doc-footer']
            }
        };
        //var logoImg = new Image();
        //logoImg.src = '/img/logo.png';
        //logoImg.crossOrigin = 'anonymous';
        var logoImg = new Image();
        logoImg.crossOrigin = 'anonymous';
        logoImg.onload = function () { generate(logoImg); };
        logoImg.onerror = function () { generate(null); };
        logoImg.src = '/img/logo.png';
        logoImg.onload = function () {
            html2pdf()
                .set(options)
                .from(element)
                .toPdf()
                .get('pdf')
                .then(function (pdf) {

                    var totalPages = pdf.internal.getNumberOfPages();
                    var pageW = pdf.internal.pageSize.getWidth();
                    var pageH = pdf.internal.pageSize.getHeight();

                    for (var i = 1; i <= totalPages; i++) {
                        pdf.setPage(i);

                        // ── Skip header on page 1 (already has company header in content) ──
                        if (i > 1) {

                            // ── White header background ──────────────────
                            pdf.setFillColor(255, 255, 255);
                            pdf.rect(0, 0, pageW, 28, 'F');

                            // ── Logo ─────────────────────────────────────
                            try {
                                pdf.addImage(logoImg, 'PNG', 10, 3, 18, 10);
                            } catch (e) { /* logo failed — skip silently */ }

                            // ── Company name (red) ───────────────────────
                            pdf.setFont('helvetica', 'bold');
                            pdf.setFontSize(10);
                            pdf.setTextColor(231, 60, 37);
                            pdf.text('Bullows Paint Equipment Private Limited', 31, 9);

                            // ── Ref + revision ───────────────────────────
                            pdf.setFont('helvetica', 'normal');
                            pdf.setFontSize(7.5);
                            pdf.setTextColor(80, 80, 80);
                            //pdf.text('BP/AVG/' + fileNo + '  ' + revLabel, 10, 17);
                            pdf.text('BP/AVG/' + fileNo + '/' + revLabel, 10, 17);
                            // ── Date ─────────────────────────────────────
                           // pdf.text(currentDate || '', 10, 22);
                            // ── Date (right-aligned) ─────────────────────
                            pdf.text(currentDate || '', pageW - 10, 22, { align: 'right' });
                            // ── Separator line ───────────────────────────
                            pdf.setDrawColor(220, 220, 220);
                            pdf.setLineWidth(0.3);
                            pdf.line(10, 26, pageW - 10, 26);
                        }

                        // ── Footer: page number (centre) — ALL pages ─────
                        pdf.setFontSize(8);
                        pdf.setTextColor(160, 160, 160);
                        pdf.text(
                            'Page ' + i + ' of ' + totalPages,
                            pageW / 2,
                            pageH - 6,
                            { align: 'center' }
                        );

                        // ── Footer: company name (left) — ALL pages ──────
                        pdf.setFontSize(7);
                        pdf.text('Bullows Paint Equipment Pvt. Ltd.', 10, pageH - 6);
                    }
                })
                .save()
                .then(function () {
                    $btn.prop('disabled', false)
                        .html('<i class="bi bi-file-pdf"></i> Download PDF');
                })
                .catch(function (err) {
                    console.error('PDF error:', err);
                    $btn.prop('disabled', false)
                        .html('<i class="bi bi-file-pdf"></i> Download PDF');
                });
        };

        // ── Fallback if logo fails to load ────────────────────
        logoImg.onerror = function () {
            logoImg = null;
            logoImg = { failed: true };
            logoImg.onload && logoImg.onload();
        };
    });
}

/* ════════════════════════════════════════════════════════
   5. WORD DOCUMENT DOWNLOAD
   Uses docx.js 7.x to build a proper .docx file
════════════════════════════════════════════════════════ */
function initWord(d) {
    // ── Wait until docx is confirmed available ────────────────
    // unpkg loads async so check after a short delay
    function waitForDocx(callback, attempts) {
        attempts = attempts || 0;
        if (typeof docx !== 'undefined' && typeof docx.Document !== 'undefined') {
            callback();
        } else if (attempts < 20) {
            setTimeout(function () { waitForDocx(callback, attempts + 1); }, 300);
        } else {
            $('#btnDownloadWord')
                .prop('disabled', true)
                .attr('title', 'Word library failed to load. Check your internet connection.')
                .html('<i class="bi bi-exclamation-triangle"></i> Word unavailable');
        }
    }

    // Enable button only once docx is ready
    $('#btnDownloadWord').prop('disabled', true)
        .html('<i class="bi bi-hourglass-split"></i> Loading...');

    waitForDocx(function () {
        $('#btnDownloadWord').prop('disabled', false)
            .html('<i class="bi bi-file-word"></i> Download Word');
    });

    // Actual click handler

    $('#btnDownloadWord').on('click', function () {
        var $btn = $(this);

        // ── Safety check: docx library must be loaded ─────────
        if (typeof docx === 'undefined' || typeof docx.Document === 'undefined') {
            alert('Word library is still loading. Please wait a moment and try again.');
            return;
        }

        $btn.prop('disabled', true)
            .html('<i class="bi bi-hourglass-split"></i> Generating Word...');

        try {
            var doc = buildWordDoc(d);

            docx.Packer.toBlob(doc).then(function (blob) {
                saveAs(blob, 'Proposal_' + d.salesNo + '.docx');
                $btn.prop('disabled', false)
                    .html('<i class="bi bi-file-word"></i> Download Word');
            }).catch(function (err) {
                console.error('Packer error:', err);
                $btn.prop('disabled', false)
                    .html('<i class="bi bi-file-word"></i> Download Word');
            });
        } catch (err) {
            console.error('Word generation error:', err);
            $btn.prop('disabled', false)
                .html('<i class="bi bi-file-word"></i> Download Word');
        }
    });
}

function buildWordDoc(d) {

    // ── Shorthand helpers ─────────────────────────────────────
    var P   = docx.Paragraph;
    var T   = docx.TextRun;
    var H   = docx.HeadingLevel;
    var AlignmentType = docx.AlignmentType;
    var WidthType      = docx.WidthType;
    var BorderStyle    = docx.BorderStyle;

    // Accent colour (Bullows orange)
    var ORANGE = 'E85D10';
    var GRAY   = '5A5F78';
    var LGRAY  = 'F0F2F8';

    // ── Style helpers ─────────────────────────────────────────
    function heading(text) {
        return new P({
            children: [new T({
                text    : text,
                bold    : true,
                color   : ORANGE,
                size    : 26,
                font    : 'Calibri'
            })],
            spacing : { before: 240, after: 80 },
            border  : {
                bottom: { style: BorderStyle.SINGLE, size: 6,
                          color: ORANGE, space: 4 }
            }
        });
    }

    function para(text, opts) {
        opts = opts || {};
        return new P({
            children: [new T({
                text  : text || '',
                size  : opts.size  || 20,
                bold  : opts.bold  || false,
                color : opts.color || '12151F',
                font  : 'Calibri',
                italics: opts.italics || false
            })],
            spacing: { before: opts.before || 40, after: opts.after || 40 },
            alignment: opts.align || AlignmentType.LEFT
        });
    }

    function blankLine() {
        return new P({ children: [new T({ text: '' })], spacing: { after: 80 } });
    }

    function sectionTitle(text) {
        return new P({
            children: [new T({
                text : text.toUpperCase(),
                bold : true,
                size : 22,
                font : 'Calibri',
                color: ORANGE
            })],
            spacing: { before: 280, after: 80 },
            border : {
                bottom: { style: BorderStyle.SINGLE, size: 4,
                          color: ORANGE, space: 4 }
            }
        });
    }

    // ── Table builder ─────────────────────────────────────────
    function makeTable(headers, rows, colWidths) {
        var Table    = docx.Table;
        var TableRow = docx.TableRow;
        var TableCell= docx.TableCell;
        var ShadingType = docx.ShadingType;

        var headerRow = new TableRow({
            children: headers.map(function (h, i) {
                return new TableCell({
                    children: [new P({
                        children: [new T({
                            text : h, bold: true, size: 18,
                            font: 'Calibri', color: GRAY
                        })]
                    })],
                    width   : { size: colWidths ? colWidths[i] : 2500,
                                type: WidthType.DXA },
                    shading : { type: ShadingType.CLEAR, fill: LGRAY },
                    margins : { top: 60, bottom: 60, left: 80, right: 80 }
                });
            })
        });

        var dataRows = rows.map(function (row) {
            return new TableRow({
                children: row.map(function (cell, i) {
                    var isHeader = (i === 0 && headers[0] !== 'Sr.');
                    return new TableCell({
                        children: [new P({
                            children: [new T({
                                text : String(cell || '—'),
                                size : 18,
                                bold : isHeader,
                                font : 'Calibri',
                                color: isHeader ? GRAY : '12151F'
                            })]
                        })],
                        width  : { size: colWidths ? colWidths[i] : 2500,
                                   type: WidthType.DXA },
                        margins: { top: 60, bottom: 60, left: 80, right: 80 }
                    });
                })
            });
        });

        return new Table({
            rows  : [headerRow].concat(dataRows),
            width : { size: 100, type: WidthType.PERCENTAGE },
            margins: { top: 0, bottom: 0 }
        });
    }

    // ── Collect current price inputs from the page ────────────
    var prices = [];
    $('.price-input').each(function () { prices.push($(this).val() || ''); });

    // ── Build document sections ───────────────────────────────
    var children = [];

    // ── 1. Company Header ─────────────────────────────────────
    children.push(new P({
        children: [new T({
            text : 'Bullows Paint Equipment Private Limited',
            bold : true, size: 36, color: ORANGE, font: 'Calibri'
        })],
        spacing: { after: 60 }
    }));
    children.push(para(
        'Unit No. 404, Lodha Supremus 2, Plot no. F4 & F4/1 MIDC, Wagle Industrial Area, Thane 400 604',
        { size: 16, color: GRAY }
    ));
    children.push(para(
        'Mob: +91 84339 30782 | Email: bullows@bullows.com | www.bullows.com',
        { size: 16, color: GRAY }
    ));
    children.push(para('CIN: U29221MH1963PTC012756', { size: 16, color: GRAY }));
    children.push(blankLine());
    children.push(para('BP/AVG/' + d.salesNo + '    ' + d.date, { bold: true }));
    children.push(blankLine());

    // ── 2. Client info table ──────────────────────────────────
    children.push(makeTable(
        ['Field', 'Details'],
        [
            ['Client Name',   d.companyName],
            ['Attention',     d.contactPerson],
            ['Project Title', 'Requirement of Paint Booth & Paint Drying Oven'],
            ['Date',          d.date]
        ],
        [2500, 6500]
    ));

    // ── 3. Introduction ───────────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Introduction'));
    children.push(para('Dear Sir,'));
    children.push(para(
        'We thankfully acknowledge receipt of your inquiry regarding your requirement ' +
        'for the mentioned subject. We are pleased to submit herewith our proposal.'
    ));
    children.push(blankLine());
    children.push(para('The contents of the proposal are as below:', { bold: true }));

    var contents = [
        '1. Basis of Design', '2. Scope of Supply',
        '3. Technical Specifications for all equipment',
        '4. Utility Requirements for the Plant',
        '5. Make of Components', '6. Exclusion List',
        '7. Price Bid', '8. Terms & Conditions'
    ];
    contents.forEach(function (item) {
        children.push(new P({
            children : [new T({ text: '☑  ' + item, size: 20, font: 'Calibri' })],
            spacing  : { before: 30, after: 30 }
        }));
    });
    children.push(blankLine());
    children.push(para('Yours faithfully,', { align: AlignmentType.RIGHT }));
    children.push(para('For Bullows Paint Equipment Private Limited',
                        { bold: true, align: AlignmentType.RIGHT }));
    children.push(blankLine());
    children.push(blankLine());
    children.push(para('Regional Manager — East & North',
                        { align: AlignmentType.RIGHT, color: GRAY }));

    // ── 4. Basis of Design ────────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Basis of Design'));
    children.push(makeTable(
        ['Parameter', 'Value'],
        [
            ['Component Dimension', 'L: ' + d.length + ' mm | W: ' + d.widthSize + ' mm | H: ' + d.heightSize + ' mm'],
            ['Production',          d.production + ' Nos. per shift'],
            ['Component Weight',    d.compWeight],
            ['No. of Shifts / Day', d.shifts + ' Nos.'],
            ['Effective Working Hrs', d.effectiveHrs + ' Hours'],
            ['Working Days / Annum', d.workingDays],
            ['Paint Type',          d.paintType],
            ['Viscosity',           d.viscosity],
            ['Primer & Topcoat DFT',d.dft],
            ['No. of Colors',       d.noOfColors],
            ['Specific Heat',       d.specificHeat],
            ['Component Handling',  d.compHandling],
            ['Cycle Time',          d.cycleTime + ' min'],
            ['Loading & Unloading', d.loadUnload]
        ],
        [3000, 6000]
    ));

    // ── 5. Scope of Supply ────────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Scope of Supply'));
    children.push(makeTable(
        ['Sr.', 'Description', 'Qty.'],
        [
            ['1', 'Dry Type PaintBooth', '1 Set'],
            ['2', 'Exhaust Ducting with Angle Supports', '1 Set'],
            ['3', 'Electrical Control Panel and Cable Trays for Paint Booth', '1 Set']
        ],
        [600, 7400, 1000]
    ));

    // ── 6. Technical Specification ────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Technical Specification for Paint Booth'));
    children.push(makeTable(
        ['Sr.', 'Description', 'Specification'],
        [
            ['1',  'Type', d.boothType],
            ['2',  'Working Size (mm)', 'L: ' + d.boothW + ' | W: ' + d.boothD + ' | H: ' + d.boothH],
            ['3',  'Extraction Chamber', 'L: ' + d.boothW + ' | W: ' + d.boothD3 + ' | H: ' + d.ecHeight],
            ['7',  'Material of Construction (Booth)', d.pbMaterial],
            ['',   'Material of Construction (EC)', d.ecMaterial],
            ['10', 'Blower Type', 'Centrifugal Blower'],
            ['',   'Exhaust Capacity', d.exhaustCap + ' m³/hr'],
            ['',   'Motor Capacity', d.motorCap + ' HP'],
            ['',   'Blower Mounting', d.blowerMounting],
            ['',   'Illumination Level', d.luxLevel + ' Lux'],
            ['',   'Lumens', d.lumens],
            ['',   'Light Quantity', d.lightQty + ' Nos.']
        ],
        [500, 4000, 4500]
    ));

    // ── 7. Exclusions ─────────────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Exclusions'));
    var excList = [
        'All civil engineering work for Pit, Trench, RCC design.',
        'All service utilities points piping up to off take points.',
        'Material unloading and shifting to weatherproof shed at site.',
        'Earthing and earthing pits — to be provided by the Customer.',
        'Power correction equipment, if necessary, to be provided by Customer.',
        'Compressor, Air dryer, Air receiver, generator set if required.',
        'Shop and maintenance lighting other than equipment lighting specified.',
        'Puncturing of roof for duct routing and sealing during erection.',
        'Platform and ladder for inspection and sampling port, if required.',
        'All rotating equipment placed outside should be protected by Customer.',
        'Customer will provide forklift / stacker support during installation.',
        'Site office, drinking water and toilet facility for site engineers.',
        'Fire extinguishing system, water circulation pumps.',
        'Chemical, paint, grit and all utilities required for trial/commissioning.',
        'Pollution Control Certificate.',
        'Unused & excess material will be taken back to Bullows.'
    ];
    excList.forEach(function (item, i) {
        children.push(makeTable(
            [],
            [[String(i + 1), item]],
            [500, 8500]
        ));
    });

    // ── 8. Price Details ──────────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Price Details'));

    var totalPrice = 0;
    $('.price-input').each(function () {
        totalPrice += parseFloat($(this).val()) || 0;
    });
    var discPct   = parseFloat($('#discountPercentage').val()) || 0;
    var discAmt   = (totalPrice * discPct) / 100;
    var finalP    = totalPrice - discAmt;
    var gstPct    = parseFloat($('#gstPercentage').val()) || 18;
    var gstAmt    = (finalP * gstPct) / 100;
    var grossAmt  = finalP + gstAmt;

    children.push(makeTable(
        ['Sr.', 'Description', 'Qty.', 'Price (INR)'],
        [
            ['1', 'Dry Type Paint Booth', '1 Set', prices[0] || ''],
            ['2', 'Exhaust Ducting with Angle Supports', '1 Set', prices[1] || ''],
            ['3', 'Electrical Control Panel and Cable Trays', '1 Set', prices[2] || ''],
            ['4', 'Erection & Commissioning', '1', prices[3] || ''],
            ['5', 'Packing & Forwarding', '1', prices[4] || ''],
            ['6', 'Freight, Insurance', "Buyer's Scope", prices[5] || ''],
            ['', 'Total Price', '', totalPrice.toFixed(2)],
            ['', 'Discount (' + discPct + '%)', '', discAmt.toFixed(2)],
            ['', 'Final Best Price', '', finalP.toFixed(2)],
            ['', 'GST (' + gstPct + '%)', '', gstAmt.toFixed(2)],
            ['', 'Gross Amount', '', grossAmt.toFixed(2)]
        ],
        [500, 5000, 1200, 2300]
    ));
    children.push(para(
        'HSN/SAC Code for Plant Equipment: 8424 2000 | Erection & Commissioning: 998731',
        { size: 16, color: GRAY, italics: true }
    ));

    // ── 9. Terms & Conditions ─────────────────────────────────
    children.push(blankLine());
    children.push(sectionTitle('Terms & Conditions'));
    children.push(makeTable(
        ['Item', 'Details'],
        [
            ['1',              '18% GST applicable at extra. GST No: 27AAACB5489M1Z5'],
            ['2',              'Any statutory variations in above taxes to be borne by the buyer.'],
            ['Advance',        d.advance + ' %'],
            ['DAP',            d.dap + ' %'],
            ['Supply Before Dispatch', d.supplyBefore + ' %'],
            ['Supply After Dispatch',  d.supplyAfter + ' %'],
            ['Installation',   d.installation + ' %'],
            ['Commissioning',  d.commissioning + ' %'],
            ['Delivery',       (d.delivery || '12-14 weeks') + ' from DAP & receipt of Advance'],
            ['Erection',       (d.erection  || '4-6 weeks')  + ' subject to site readiness'],
            ['Validity',       '30 days, subject to confirmation thereafter']
        ],
        [2500, 6500]
    ));
    children.push(blankLine());
    children.push(para(
        'Warranty: 12 months from commissioning. Electrical items: 6 months back-to-back from dispatch.',
        { bold: true, size: 18 }
    ));
    children.push(blankLine());
    children.push(para('Yours faithfully,', { align: AlignmentType.RIGHT }));
    children.push(para('For Bullows Paint Equipment Private Limited',
                        { bold: true, align: AlignmentType.RIGHT }));
    children.push(blankLine());
    children.push(blankLine());
    children.push(para('Regional Manager — East & North',
                        { align: AlignmentType.RIGHT, color: GRAY }));
    children.push(blankLine());
    children.push(para(
        'CC: Mr. Abhay V. Gurav – Sales and Application, Sr. Manager',
        { size: 16, color: GRAY }
    ));
    children.push(para(
        'Mr. Prashant R Waghulade — General Manager & Business Head',
        { size: 16, color: GRAY }
    ));

    // ── Assemble document ─────────────────────────────────────
    return new docx.Document({
        creator : 'Bullows PaintBooth Pro',
        title   : 'Proposal - ' + d.salesNo,
        styles  : {
            default: {
                document: {
                    run: { font: 'Calibri', size: 20, color: '12151F' }
                }
            }
        },
        sections: [{
            properties: {
                page: {
                    margin: {
                        top   : 720,   // 0.5 inch
                        right : 720,
                        bottom: 720,
                        left  : 720
                    }
                }
            },
            children: children
        }]
    });
}


/* ════════════════════════════════════════════════════════
   6. AUTO-DISMISS ALERTS
════════════════════════════════════════════════════════ */
function initAlerts() {
    $('[data-auto-dismiss]').each(function () {
        var $el = $(this);
        setTimeout(function () { $el.fadeOut(400, function () { $el.remove(); }); }, 4000);
    });
}




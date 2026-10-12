/* ============================================================
   Bullows PaintBooth Pro — enquiry.js
   jQuery for Enquiry Create & Edit pages
   ============================================================ */

/* ─────────────────────────────────────────────────────────────
   GLOBAL FUNCTIONS — defined outside $(function(){})
   so they can be called from anywhere
───────────────────────────────────────────────────────────── */

// Surface area calculation — called from shape/orientation change
function calculateSurfaceArea() {
    var shape = $('#drpShape').val();

    if (shape === 'Rectangular') {
        var depth  = parseFloat($('#txtLengthSize').val()) || 0;
        var width  = parseFloat($('#txtWidthSize').val())  || 0;
        var height = parseFloat($('#txtHeightSize').val()) || 0;

        var surface = 2 * (depth * width + depth * height + width * height);
        /*$('#txtSurfaceArea').val((surface / 1000000).toFixed(2));*/
        $('#txtSurfaceArea').val(Math.round(surface / 1000000));
    } else if (shape === 'Circular') {
        var diameter = parseFloat($('#txtDiameter').val())      || 0;
        var radius   = diameter / 2;
        var height   = parseFloat($('#txtCircularHeight').val()) || 0;

        var topBase  = 2 * Math.PI * (radius / 1000) * (radius / 1000);
        var sideArea = 2 * Math.PI * (radius / 1000) * (height / 1000);
        $('#txtSurfaceArea').val((topBase + sideArea).toFixed(2));

    } else {
        $('#txtSurfaceArea').val('0.00');
    }
}

// Image swap with smooth fade animation
function swapImage(selector, newSrc) {
    var $img = $(selector);

    // Skip if already showing the same image
    if ($img.attr('src') === newSrc) return;

    $img.addClass('switching');

    setTimeout(function () {
        $img.attr('src', newSrc);
        $img.removeClass('switching');
    }, 180);
}

// Update shape diagram image based on shape + orientation
function updateShapeImage() {
    var shape       = $('#drpShape').val();
    var orientation = $('#drpOrientation').val();

    if (!shape || !orientation) return;

    var labelTxt = orientation + ' — ' + shape;

    if (shape === 'Rectangular') {
        if (orientation === 'Vertical') {
            swapImage('#rectImg', '/img/VerticalRectangular.PNG');
        } else {
            swapImage('#rectImg', '/img/Rectangular.PNG');
        }
        $('#rectOrientationLabel').text(labelTxt);

    } else if (shape === 'Circular') {
        if (orientation === 'Horizontal') {
            swapImage('#circularImg', '/img/HorizontalCircular.PNG');
        } else {
            swapImage('#circularImg', '/img/Circular.PNG');
        }
        $('#circOrientationLabel').text(labelTxt);
    }
}

// Show/hide Rectangular or Circular section
function triggerShapeChange() {
    var shape = $('#drpShape').val();

    if (shape === 'Rectangular') {
        $('#rectangularSection').show();
        $('#circularSection').hide();

        if (!$('#drpOrientation').val()) {
            $('#drpOrientation').val('Horizontal');
        }

    } else if (shape === 'Circular') {
        $('#circularSection').show();
        $('#rectangularSection').hide();

        if (!$('#drpOrientation').val()) {
            $('#drpOrientation').val('Vertical');
        }

    } else {
        $('#rectangularSection').hide();
        $('#circularSection').hide();
    }

    updateShapeImage();
    calculateSurfaceArea();   // ✅ now accessible — defined above
}

var boothPreview = (function () {
    var scene, camera, renderer, world, component, motion,
        inited = false, started = false, container, MM = 1 / 1000;
    var orbit = { az: 0.72, el: 0.34, dist: 12, auto: true, target: null };

    function green() { return new THREE.MeshStandardMaterial({ color: 0x3aa564, transparent: true, opacity: 0.42, side: THREE.DoubleSide, roughness: 0.95 }); }
    function tan() { return new THREE.MeshStandardMaterial({ color: 0xc9c0af, transparent: true, opacity: 0.55, side: THREE.DoubleSide, roughness: 1 }); }
    function floorM() { return new THREE.MeshStandardMaterial({ color: 0x2a3038, side: THREE.DoubleSide, roughness: 1 }); }

    function plane(w, h, m, pos, rot) {
        var mesh = new THREE.Mesh(new THREE.PlaneGeometry(w, h), m);
        mesh.position.set(pos[0], pos[1], pos[2]);
        if (rot) mesh.rotation.set(rot[0], rot[1], rot[2]);
        return mesh;
    }
    function edges(geo, c) { return new THREE.LineSegments(new THREE.EdgesGeometry(geo), new THREE.LineBasicMaterial({ color: c })); }

    function dimLine(x1, y1, z1, x2, y2, z2, hex) {
        var g = new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(x1, y1, z1), new THREE.Vector3(x2, y2, z2)
        ]);
        return new THREE.Line(g, new THREE.LineBasicMaterial({ color: hex, linewidth: 3 }));
    }
    function labelSprite(text) {
        var c = document.createElement('canvas'); c.width = 512; c.height = 128;
        var g = c.getContext('2d');
        g.fillStyle = 'rgba(20,25,34,.82)'; g.fillRect(0, 0, 512, 128);
        g.strokeStyle = '#f4c430'; g.lineWidth = 4; g.strokeRect(2, 2, 508, 124);
        g.fillStyle = '#f4c430'; g.font = 'bold 44px Segoe UI, sans-serif';
        g.textAlign = 'center'; g.textBaseline = 'middle'; g.fillText(text, 256, 64);
        var s = new THREE.Sprite(new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(c), transparent: true }));
        s.scale.set(1.3, 0.32, 1); return s;
    }
    //function dimSprite(text) {
    //    var c = document.createElement('canvas'); c.width = 256; c.height = 64;
    //    var g = c.getContext('2d');
    //    g.fillStyle = '#e8eaed'; g.font = 'bold 34px Segoe UI, sans-serif';
    //    g.textAlign = 'center'; g.textBaseline = 'middle';
    //    g.fillText(text, 128, 32);
    //    var s = new THREE.Sprite(new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(c), transparent: true, depthTest: false }));
    //    s.scale.set(0.9, 0.22, 1);
    //    return s;
    //}
    function dimSprite(text, hex) {
        var c = document.createElement('canvas'); c.width = 256; c.height = 64;
        var g = c.getContext('2d');
        g.fillStyle = hex || '#e8eaed'; g.font = 'bold 34px Segoe UI, sans-serif';
        g.textAlign = 'center'; g.textBaseline = 'middle';
        g.fillText(text, 128, 32);
        var s = new THREE.Sprite(new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(c), transparent: true, depthTest: false }));
        s.scale.set(0.9, 0.22, 1);
        return s;
    }
    function updateCamera() {
        var t = orbit.target;
        camera.position.set(
            t.x + orbit.dist * Math.cos(orbit.el) * Math.sin(orbit.az),
            t.y + orbit.dist * Math.sin(orbit.el),
            t.z + orbit.dist * Math.cos(orbit.el) * Math.cos(orbit.az));
        camera.lookAt(t);
    }
    function ease(x) { return x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2; }

    function entryDir() {
        if ($('#drpSubType').val() === '1') return 'Front';   // Open = always front
        return $('[name="Component.ComponentEntry"]').val() || 'Front';
    }
    function readDims() {
        var num = function (id) { return Math.max(1, parseFloat($('#' + id).val()) || 0); };
        if ($('#drpShape').val() === 'Circular')
            return { shape: 'circle', dia: num('txtDiameter'), w: num('txtDiameter'), d: num('txtDiameter'), h: num('txtCircularHeight') };
        return { shape: 'rect', w: num('txtWidthSize'), d: num('txtLengthSize'), h: num('txtHeightSize') };
    }

    function init() {
        container = document.getElementById('entryViewer');
        if (!container || typeof THREE === 'undefined' || inited) return;
        var w = container.clientWidth || 600, h = container.clientHeight || 420;
        scene = new THREE.Scene(); scene.background = new THREE.Color(0x171b22);
        camera = new THREE.PerspectiveCamera(42, w / h, 0.01, 1000);
        renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        renderer.setSize(w, h);
        container.appendChild(renderer.domElement);
        scene.add(new THREE.AmbientLight(0xffffff, 0.85));
        var k = new THREE.DirectionalLight(0xffffff, 0.75); k.position.set(6, 12, 9); scene.add(k);
        var f = new THREE.DirectionalLight(0xffffff, 0.30); f.position.set(-7, 5, -5); scene.add(f);
        world = new THREE.Group(); scene.add(world);
        orbit.target = new THREE.Vector3();

        var drag = false, px = 0, py = 0;
        renderer.domElement.addEventListener('pointerdown', function (e) { drag = true; orbit.auto = false; px = e.clientX; py = e.clientY; });
        window.addEventListener('pointerup', function () { drag = false; });
        window.addEventListener('pointermove', function (e) {
            if (!drag) return;
            orbit.az -= (e.clientX - px) * 0.006;
            orbit.el = Math.max(0.08, Math.min(1.2, orbit.el + (e.clientY - py) * 0.006));
            px = e.clientX; py = e.clientY;
        });
        renderer.domElement.addEventListener('wheel', function (e) {
            e.preventDefault();
            orbit.dist = Math.max(3, Math.min(80, orbit.dist * (1 + Math.sign(e.deltaY) * 0.08)));
        }, { passive: false });

        inited = true;
        if (!started) { started = true; animate(); }
    }

    function rebuild() {
        if (!inited) return;
        while (world.children.length) world.remove(world.children[0]);
        if (component) { scene.remove(component); if (component.userData.outline) scene.remove(component.userData.outline); component = null; }

        var p = readDims(), side = (entryDir() === 'Side');
        var cW = p.w * MM, cH = p.h * MM, cD = p.d * MM;
        var mx = Math.max(0.4, cW * 0.18), mt = Math.max(0.4, cH * 0.28), mb = Math.max(0.7, cD * 0.55);
        var W = cW + 2 * mx, H = cH + mt, D = cD + mb;

        world.add(plane(W, H, green(), [0, H / 2, -D / 2]));                            // back
        world.add(plane(D, H, green(), [-W / 2, H / 2, 0], [0, Math.PI / 2, 0]));       // left
        if (!side) world.add(plane(D, H, green(), [W / 2, H / 2, 0], [0, Math.PI / 2, 0])); // right (open for Side)
        world.add(plane(W, D, floorM(), [0, 0, 0], [-Math.PI / 2, 0, 0]));              // floor
        world.add(plane(W, D, tan(), [0, H, 0], [-Math.PI / 2, 0, 0]));                 // ceiling
        if (side) world.add(plane(W, H, green(), [0, H / 2, D / 2]));                   // front (open for Front)

        var frame = edges(new THREE.BoxGeometry(W, H, D), 0xf4c430); frame.position.set(0, H / 2, 0); world.add(frame);
        
        var geo = p.shape === 'circle'
            ? new THREE.CylinderGeometry(p.dia * MM / 2, p.dia * MM / 2, cH, 40)
            : new THREE.BoxGeometry(cW, cH, cD);
        component = new THREE.Mesh(geo, new THREE.MeshStandardMaterial({ color: 0xe8804a, roughness: 0.5, metalness: 0.15 }));
        var outline = edges(geo, 0x8a3d16); component.userData.outline = outline;
        scene.add(component); scene.add(outline);

        // ▼▼▼ ADD DIMENSION LABELS HERE ▼▼▼
        if (p.shape === 'circle') {
            var lblD = dimSprite('Ø ' + Math.round(p.dia) + 'mm');
            var lblHc = dimSprite('H ' + Math.round(p.h) + 'mm');
            lblD.position.set(0, -cH / 2 - 0.15, 0);
            lblHc.position.set(p.dia * MM / 2 + 0.15, 0, 0);
            component.add(lblD); component.add(lblHc);
        } else {
            var lblW = dimSprite('W ' + Math.round(p.d) + 'mm');   // MAX Depth (W) = txtLengthSize
            var lblH = dimSprite('H ' + Math.round(p.h) + 'mm');   // MAX Height
            var lblL = dimSprite('L ' + Math.round(p.w) + 'mm');   // MAX Width (L) = txtWidthSize
            lblW.position.set(0, -cH / 2 - 0.15, cD / 2 + 0.05);   // bottom-front edge
            lblH.position.set(cW / 2 + 0.15, 0, cD / 2 + 0.05);    // right-front edge
            lblL.position.set(cW / 2 + 0.05, -cH / 2 - 0.15, 0);   // bottom-right edge
            component.add(lblW); component.add(lblH); component.add(lblL);
        }
        // ▲▲▲ END DIMENSION LABELS ▲▲▲
        var dir, lab;
        if (side) {
            var sx = W / 2 + cW + Math.max(0.6, cW * 0.3), lenS = W * 0.55;
            dir = new THREE.Vector3(-1, 0, 0);
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(sx, H * 0.72, 0), lenS, 0xf4c430, lenS * 0.22, lenS * 0.13));
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(sx, 0.02, -D * 0.28), lenS * 0.9, 0xffe066, lenS * 0.2, lenS * 0.12));
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(sx, 0.02, D * 0.28), lenS * 0.9, 0xffe066, lenS * 0.2, lenS * 0.12));
            lab = labelSprite('SIDE — ENTRY'); lab.position.set(W / 2 + 0.4, H + 1.0, 0); world.add(lab);
            motion = { axis: 'x', start: sx - cW / 2, rest: 0, p: 0, hold: 0 };
            component.position.set(motion.start, cH / 2, 0);
            orbit.az = 1.15;
        } else {
            var sz = D / 2 + cD + Math.max(0.6, cD * 0.3), lenF = D * 0.55;
            dir = new THREE.Vector3(0, 0, -1);
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(0, H * 0.72, sz), lenF, 0xf4c430, lenF * 0.22, lenF * 0.13));
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(-W * 0.28, 0.02, sz), lenF * 0.9, 0xffe066, lenF * 0.2, lenF * 0.12));
            world.add(new THREE.ArrowHelper(dir, new THREE.Vector3(W * 0.28, 0.02, sz), lenF * 0.9, 0xffe066, lenF * 0.2, lenF * 0.12));
            lab = labelSprite('FRONT — ENTRY'); lab.position.set(0, H + 1.0, D / 2 + 0.3); world.add(lab);
            motion = { axis: 'z', start: sz - cD / 2, rest: 0, p: 0, hold: 0 };
            component.position.set(0, cH / 2, motion.start);
            orbit.az = 0.72;
        }
        outline.position.copy(component.position);
        orbit.target.set(0, H / 2, 0);
        orbit.dist = Math.max(W, H, D) * 2.8 + 2;
        $('#entryDirBadge').html('Entry direction: <b style="color:#f4c430">' + (side ? 'SIDE →' : 'FRONT →') + '</b>');
    }

    function step() {
        if (!motion || !component) return;
        motion.p += 0.006;
        if (motion.p >= 1) { motion.hold++; if (motion.hold > 90) { motion.p = 0; motion.hold = 0; } else motion.p = 1; }
        component.position[motion.axis] = motion.start + (motion.rest - motion.start) * ease(Math.min(motion.p, 1));
        if (component.userData.outline) component.userData.outline.position.copy(component.position);
    }
    function animate() {
        requestAnimationFrame(animate);
        if (!inited) return;
        updateCamera(); step(); renderer.render(scene, camera);   // ← no orbit.auto increment
    }
    function resize() {
        if (!inited || !container) return;
        var w = container.clientWidth, h = container.clientHeight;
        if (!w || !h) return;
        renderer.setSize(w, h); camera.aspect = w / h; camera.updateProjectionMatrix();
    }
    return { init: init, rebuild: rebuild, resize: resize, isInited: function () { return inited; } };
})();

function toggleEntryPreview() {
    var shape = $('#drpShape').val();
    var subType = $('#drpSubType').val();

    // Open (1) → always Front, show right away. Others → wait for the dropdown.
    var entryChosen = (subType === '1') || !!$('[name="Component.ComponentEntry"]').val();

    if (!shape || !entryChosen) { $('#entryPreviewWrap').hide(); return; }

    var $col = shape === 'Circular' ? $('#circPreviewCol') : $('#rectPreviewCol');
    $col.append($('#entryPreviewWrap').show());   // move the single viewer into the active column

    if (!boothPreview.isInited()) boothPreview.init();
    boothPreview.resize();
    boothPreview.rebuild();
}/* ─────────────────────────────────────────────────────────────
   DOCUMENT READY — all event bindings go here
───────────────────────────────────────────────────────────── */
$(function () {

    /* ─────────────────────────────────────────────────────────
       1. PAGE INIT — set defaults on load
    ───────────────────────────────────────────────────────── */
    if (!$('#txtProposalDate').val()) {
        var today = new Date();
        var dd    = String(today.getDate()).padStart(2, '0');
        var mm    = String(today.getMonth() + 1).padStart(2, '0');
        $('#txtProposalDate').val(today.getFullYear() + '-' + mm + '-' + dd);
    }

    if (!$('#txtExtractionHeight').val()) $('#txtExtractionHeight').val('2100');
    if (!$('#txtPlenumHeight').val())     $('#txtPlenumHeight').val('800');

    // Run all toggles on load (important for Edit mode)
    toggleCoatingType();
    toggleCategoryDropdowns();
    toggleSubTypeVisibility();
    toggleComponentHandling();
    toggleConveyorType();
    toggleDoorType();
    toggleManualOperation();
    toggleMaintenanceDoor();
    triggerShapeChange();      // also calls calculateSurfaceArea + updateShapeImage
    toggleMOC();
    // with the other on-load toggles (after triggerShapeChange();)
    toggleEntryPreview();

    $('#drpShape, #drpSubType, #drpDraftType').on('change', toggleEntryPreview);
    $('[name="Component.ComponentEntry"]').on('change', toggleEntryPreview);
    $(document).on('input', '.enq-calc', function () { boothPreview.rebuild(); });
    $(window).on('resize', function () { boothPreview.resize(); });
    /* ─────────────────────────────────────────────────────────
       2. COMPANY AUTOCOMPLETE
    ───────────────────────────────────────────────────────── */
    $('#txtCompanyName').on('input', function () {
        var query = $.trim($(this).val());

        if (query.length < 1) {
            $('#companyDropdown').hide().empty();
            return;
        }

        $.ajax({
            url    : '/Enquiry/SearchCompanies',
            type   : 'GET',
            data   : { term: query },
            success: function (data) {
                var $dd = $('#companyDropdown').empty();

                if (data.length > 0) {
                    $.each(data, function (i, name) {
                        $('<div class="enq-dropdown-item">')
                            .text(name)
                            .appendTo($dd);
                    });
                    $dd.show();
                } else {
                    $dd.hide();
                }
            }
        });
    });

    $(document).on('click', '.enq-dropdown-item', function () {
        var companyName = $(this).text();
        $('#txtCompanyName').val(companyName);
        $('#companyDropdown').hide().empty();

        $.ajax({
            url    : '/Enquiry/SearchByCompanyName',
            type   : 'POST',
            data   : { companyName: companyName },
            headers: {
                'RequestVerificationToken':
                $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (data) {
                $('#hdnCustomerID').val(data.customerId      || '');
                $('#txtAddress').val(data.customerAddress    || '');
                $('#txtContactPerson').val(data.contactperson || '');
                $('#txtDesignation').val(data.designation    || '');
                $('#txtMobile').val(data.mobileNo            || '');
                $('#txtEmail').val(data.emailId              || '');
            }
        });
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtCompanyName, #companyDropdown').length) {
            $('#companyDropdown').hide();
        }
    });


    /* ─────────────────────────────────────────────────────────
       3. SALES NO — validate uniqueness on blur
    ───────────────────────────────────────────────────────── */
    $('#txtSalesNO').on('blur', function () {
        if ($(this).is('[readonly]')) return;
        var salesNo   = $.trim($(this).val());
        var enquiryId = $('#EnquiryID').val() || 0;

        if (!salesNo) return;

        $.ajax({
            url    : '/Enquiry/CheckSalesNo',
            type   : 'GET',
            data   : { salesNo: salesNo, enquiryId: enquiryId },
            success: function (res) {
                if (res.exists) {
                    $('#salesNoError').show();
                    $('#txtSalesNO').addClass('is-invalid');
                } else {
                    $('#salesNoError').hide();
                    $('#txtSalesNO').removeClass('is-invalid');
                }
            }
        });
    });

    $('#txtSalesNO').on('input', function () {
        $('#salesNoError').hide();
        $(this).removeClass('is-invalid');
    });


    /* ─────────────────────────────────────────────────────────
       4. SHAPE CHANGE
    ───────────────────────────────────────────────────────── */
    $('#drpShape').on('change', function () {
        triggerShapeChange();
    });


    /* ─────────────────────────────────────────────────────────
       5. ORIENTATION CHANGE
    ───────────────────────────────────────────────────────── */
    $('#drpOrientation').on('change', function () {
        updateShapeImage();
    });


    /* ─────────────────────────────────────────────────────────
       6. SURFACE AREA — recalculate when dimension inputs change
    ───────────────────────────────────────────────────────── */
    $(document).on('input', '.enq-calc', function () {
        calculateSurfaceArea();
    });


    /* ─────────────────────────────────────────────────────────
       7. COATING TYPE — Paint vs Powder
    ───────────────────────────────────────────────────────── */
    $('input[name="Component.ConsumptionPerDay"]').on('change', function () {
        toggleCoatingType();
    });
    $('#drpWashingChamberTank').on('change', function () {
        toggleMOC();
    });
    function toggleCoatingType() {
        var val = $('input[name="Component.ConsumptionPerDay"]:checked').val();

        if (val === 'Paint') {
            $('#paintTypeDiv').show();
            $('#paintConsDiv').show();
            $('#powderTypeDiv').hide();
            $('#powderConsDiv').hide();
        } else if (val === 'Powder') {
            $('#powderTypeDiv').show();
            $('#powderConsDiv').show();
            $('#paintTypeDiv').hide();
            $('#paintConsDiv').hide();
        } else {
            $('#paintTypeDiv, #paintConsDiv').hide();
            $('#powderTypeDiv, #powderConsDiv').hide();
        }
    }


    /* ─────────────────────────────────────────────────────────
       8. PAINTBOOTH CATEGORY
    ───────────────────────────────────────────────────────── */
    $('#drpCategory').on('change', function () {
        toggleCategoryDropdowns();
    });

    function toggleCategoryDropdowns() {
        var val = $('#drpCategory').val();
        if (val === '1' || val === 'Dry back paint booth') {
            $('#draftTypeDiv').show();
            $('#subTypeDiv').show();
        } else {
            $('#draftTypeDiv').hide();
            $('#subTypeDiv').hide();
        }
        if (val === 'Water wash paint booth') {
            $('#divWashingChamberTank').show();
            toggleMOC();     // ✅ let MOC visibility depend on tank selection
        } else {
            $('#divWashingChamberTank').hide();
            $('#divMOC').hide();  // ✅ always hide MOC for non-water-wash
        }

        $('#lblExtractionHeight').text(
            val === 'Water wash paint booth'
                ? 'Water Wash Height (mm)'
                : 'Extraction Chamber Height (mm)'
        );
    }
    function toggleMOC() {
        var val = $('#drpWashingChamberTank').val();
        if (val === 'Fabricated') {
            $('#divMOC').show();
        } else {
            $('#divMOC').hide();
        }
    }

    /* ─────────────────────────────────────────────────────────
       9. SUB TYPE
    ───────────────────────────────────────────────────────── */
    $('#drpSubType').on('change', function () {
        toggleSubTypeVisibility();
    });

   
    function toggleSubTypeVisibility() {
        var val = $('#drpSubType').val();

        if (val === '1' || val === '') {
            $('#componentEntrySection').hide();
            $('#componentEntryDoorDiv').hide();
        } else {
            $('#componentEntrySection').show();
            $('#componentEntryDoorDiv').show();
        }

        if (val === '3') {
            $('#plenumHeightDiv').show();
            $('#maintenanceDoorSection').show();
        } else {
            $('#plenumHeightDiv').hide();
            $('#maintenanceDoorSection').hide();
        }

        if (val != "1") {
            $("#divFilterTypes").show();
        } else {
            $("#divFilterTypes").hide();
        }

        // Component Entry options visibility
        if (val === '8' || val === '9') {
            $('#drpComponentEntry option[value="Front"]').hide();
            $('#drpComponentEntry option[value="Side"]').show();
            $('#drpComponentEntry').val('Side');
        } else if (val === '4' || val === '5') {
            $('#drpComponentEntry option[value="Front"]').hide();
            $('#drpComponentEntry option[value="Side"]').show();
            if ($('#drpComponentEntry').val() === 'Front') {
                $('#drpComponentEntry').val('');
            }
        } else {
            $('#drpComponentEntry option[value="Front"]').show();
            $('#drpComponentEntry option[value="Side"]').show();
        }
    }

    /* ─────────────────────────────────────────────────────────
       10. COMPONENT HANDLING
    ───────────────────────────────────────────────────────── */
    $('#drpComponentHandling').on('change', function () {
        toggleComponentHandling();
    });

    function toggleComponentHandling() {
        var val = $('#drpComponentHandling').val();

        if (val === 'Applicable' || val === '1') {
            $('#conveyorTypeDiv').show();
        } else {
            $('#conveyorTypeDiv').hide();
            $('#conveyorSubTypeDiv').hide();
            $('#conveyorNumberDiv').hide();
            $('#carryCapacityDiv').hide();
            $('#pitchDiv').hide();
            $('#speedDiv').hide();
        }
    }


    /* ─────────────────────────────────────────────────────────
       11. CONVEYOR TYPE
    ───────────────────────────────────────────────────────── */
    $('#drpConveyor').on('change', function () {
        toggleConveyorType();
    });

    function toggleConveyorType() {
        var val = $('#drpConveyor').val();

        $('#carryCapacityDiv').hide();
        $('#conveyorSubTypeDiv').hide();
        $('#conveyorNumberDiv').hide();
        $('#pitchDiv').hide();
        $('#speedDiv').hide();

        if (val === '3') {
            $('#conveyorSubTypeDiv').show();
            $('#conveyorNumberDiv').show();
            $('#pitchDiv').show();
            $('#speedDiv').show();
        } else if (['1','2','6','7','8'].indexOf(val) > -1) {
            $('#carryCapacityDiv').show();
        }
    }
    // Define all sub-options as a static map — no DOM cloning needed
    var subTypeOptions = {
        "1": [ // Side Draft
            { val: "1", text: "Front Open" },
            { val: "2", text: "Negative Pressurized Booth" },
            { val: "3", text: "Positive Pressurized Booth" }
        ],
        "2": [ // Down Draft
            { val: "4", text: "With FAS & Civil" },
            { val: "5", text: "With FAS & Without Civil" },
            
        ],
        "3": [ // Semi Down Draft
            { val: "8", text: "With FAS" },
            { val: "9", text: "With Filters on Top" }
        ]
    };

    function populateSubType(selectedDraft, selectedValue) {
        var $sub = $("#drpSubType");
        $sub.empty().append('<option value="">--Select--</option>');

        var options = subTypeOptions[selectedDraft] || [];
        $.each(options, function (i, opt) {
            $sub.append($('<option>').val(opt.val).text(opt.text));
        });

        if (selectedValue) {
            $sub.val(selectedValue);
        }
    }

    // Init on page load — handles both Create and Edit mode
    (function initSubType() {
        var draftVal = $("#drpDraftType").val();
        if (!draftVal) {
            $("#drpSubType").empty().append('<option value="">--Select--</option>');
        } else {
            var savedSubType = $("#drpSubType").val(); // Razor-bound value
            populateSubType(draftVal, savedSubType);
        }
    })();

    // Live change handler
    $("#drpDraftType").change(function () {
        populateSubType($(this).val(), null);
    });



    /* ─────────────────────────────────────────────────────────
       12. DOOR TYPE
    ───────────────────────────────────────────────────────── */
    $('#drpDoorType').on('change', function () {
        toggleDoorType();
    });

    function toggleDoorType() {
        var val = $('#drpDoorType').val();

        if (val === 'Manual') {
            $('#manualOperationDiv').show();
            $('#motorisedOperationDiv').hide();
        } else if (val === 'Motorised') {
            $('#motorisedOperationDiv').show();
            $('#manualOperationDiv').hide();
            $('#hingedDoorDiv').hide();
            $('#hingesTypeDiv').hide();
        } else {
            $('#manualOperationDiv').hide();
            $('#motorisedOperationDiv').hide();
        }
    }


    /* ─────────────────────────────────────────────────────────
       13. MANUAL OPERATION
    ───────────────────────────────────────────────────────── */
    $('#drpManualOperation').on('change', function () {
        toggleManualOperation();
    });

    function toggleManualOperation() {
        var val = $('#drpManualOperation').val();

        if (val === 'Hinged') {
            $('#hingedDoorDiv').show();
            $('#hingesTypeDiv').show();
        } else {
            $('#hingedDoorDiv').hide();
            $('#hingesTypeDiv').hide();
        }
    }


    /* ─────────────────────────────────────────────────────────
       14. MAINTENANCE DOOR CHECKBOX
    ───────────────────────────────────────────────────────── */
    $('#chkMaintenanceDoor').on('change', function () {
        toggleMaintenanceDoor();
    });

    function toggleMaintenanceDoor() {
        if ($('#chkMaintenanceDoor').is(':checked')) {
            $('#maintenanceDoorFields').show();
        } else {
            $('#maintenanceDoorFields').hide();
        }
    }


    /* ─────────────────────────────────────────────────────────
       15. DOOR HEIGHT VALIDATION
    ───────────────────────────────────────────────────────── */
    $('#txtDoorHeight, #txtPlenumHeight').on('input change', function () {
        var doorHeight   = parseFloat($('#txtDoorHeight').val())   || 0;
        var plenumHeight = parseFloat($('#txtPlenumHeight').val()) || 0;

        if (doorHeight > 0 && plenumHeight > 0 && doorHeight > plenumHeight) {
            $('#doorHeightError').show();
            $('#txtDoorHeight').val('');
        } else {
            $('#doorHeightError').hide();
        }
    });


    /* ─────────────────────────────────────────────────────────
       16. FILE UPLOAD
    ───────────────────────────────────────────────────────── */
    var uploadedFiles = [];

    $('#imageFileInput').on('change', function () {
        uploadedFiles = uploadedFiles.concat(Array.from(this.files));

        if (uploadedFiles.length > 0) {
            $('#fileNameDisplay').text(uploadedFiles.length + ' file(s) selected');
            $('#showFilesBtn').prop('disabled', false);
        }
    });

    
    $('#showFilesBtn').on('click', function () {
        var $list = $('#uploadedFilesList').empty();

        // ── Show newly selected files (new upload) ──
        if (uploadedFiles.length > 0) {
            $.each(uploadedFiles, function (i, file) {
                var url = URL.createObjectURL(file);
                $list.append(
                    '<li>' +
                    '<i class="bi bi-file-earmark"></i>' +
                    '<a href="' + url + '" download="' + file.name + '">' +
                    file.name +
                    '</a>' +
                    '</li>'
                );
            });

            // ── Show existing saved file (edit mode) ──
        } else {
            var existingPath = $('#showFilesBtn').data('existing-path');
            if (existingPath) {
                var fileName = existingPath.split('/').pop();
                $list.append(
                    '<li>' +
                    '<i class="bi bi-file-earmark"></i>' +
                    '<a href="' + existingPath + '" target="_blank">' +
                    fileName +
                    '</a>' +
                    '</li>'
                );
            }
        }

        $('#filesModalOverlay').show();
    });
    $('#closeFilesModal, #closeFilesModal2').on('click', function () {
        $('#filesModalOverlay').hide();
    });

    $('#filesModalOverlay').on('click', function (e) {
        if ($(e.target).is('#filesModalOverlay')) $(this).hide();
    });

    $('#downloadAllBtn').on('click', function () {

        // ── Newly selected files (new upload) ──
        if (uploadedFiles.length > 0) {
            $.each(uploadedFiles, function (i, file) {
                var url = URL.createObjectURL(file);
                var $a = $('<a>').attr({ href: url, download: file.name });
                $('body').append($a);
                $a[0].click();
                $a.remove();
            });

            // ── Existing saved file (edit mode) ──
        } else {
            var existingPath = $('#showFilesBtn').data('existing-path');
            if (existingPath) {
                var fileName = existingPath.split('/').pop();
                var $a = $('<a>').attr({ href: existingPath, download: fileName });
                $('body').append($a);
                $a[0].click();
                $a.remove();
            }
        }
    });

    
    /* ─────────────────────────────────────────────────────────
   17. FORM SUBMIT
───────────────────────────────────────────────────────── */
    $('#enquiryForm').on('submit', function (e) {
        if ($('#salesNoError').is(':visible')) {
            PB.toast('Please fix the Enquiry No error before saving.', 'danger');
            return false;
        }

        // ── Required field validation ─────────────────────────
        var errors = [];

        function reqField(selector, label) {
            var $el = $(selector);
            var val = ($el.val() || '').trim();
            if (!val) {
                $el.addClass('pb-input-error');
                errors.push(label);
            } else {
                $el.removeClass('pb-input-error');
            }
        }

        function reqSelect(selector, label) {
            var $el = $(selector);
            if (!$el.val()) {
                $el.addClass('pb-input-error');
                errors.push(label);
            } else {
                $el.removeClass('pb-input-error');
            }
        }

        // Section 1 — Enquiry Details
        reqField('[name="Enquiry.SalesNO"]', 'Enquiry No');
        reqSelect('#drpSalesEngineer', 'Sales Engineer');
        // Section 2 — Company Details
        reqField('#txtCompanyName', 'Company Name');

        // Section 3 — Basis of Design (non-orange labels only)
        reqField('[name="Component.Component"]', 'Component Name');
        reqField('[name="Component.MaterialofConstruction"]', 'Material of Construction');
        reqSelect('#drpShape', 'Shape');
        reqSelect('#drpOrientation', 'Orientation');

        // Rectangular dims — only when visible
        if ($('#rectangularSection').is(':visible')) {
            reqField('#txtLengthSize', 'MAX Depth (W)');
            reqField('#txtWidthSize', 'MAX Width (L)');
            reqField('#txtHeightSize', 'MAX Height');
        }

        // Circular dims — only when visible
        if ($('#circularSection').is(':visible')) {
            reqField('#txtDiameter', 'MAX Diameter');
            reqField('#txtCircularHeight', 'MAX Height');
        }

        reqSelect('#drpCategory', 'PaintBooth Type');
        reqSelect('#drpDraftType', 'Draft Type');
        reqSelect('#drpSubType', 'Sub Type');

        // Coating Type radio
        if (!$('input[name="Component.ConsumptionPerDay"]:checked').length) {
            errors.push('Coating Type');
            $('#radioPaint').closest('.enq-radio-group').addClass('pb-radio-error');
        } else {
            $('#radioPaint').closest('.enq-radio-group').removeClass('pb-radio-error');
        }

        // Coating sub-fields — only when visible
        if ($('#paintTypeDiv').is(':visible')) reqSelect('[name="Component.TypeOfPaint"]', 'Type of Paint');
        if ($('#powderTypeDiv').is(':visible')) reqSelect('[name="Component.TypeOfPowder"]', 'Type of Powder');

        // Section 4 — Component Entry & Door Types (only when visible)
        if ($('#componentEntrySection').is(':visible')) {
            reqSelect('[name="Component.ComponentEntry"]', 'Component Entry');
            reqSelect('#drpDoorType', 'Door Type');

            if ($('#manualOperationDiv').is(':visible')) reqSelect('#drpManualOperation', 'Type of Operation');
            if ($('#hingedDoorDiv').is(':visible')) reqSelect('[name="Component.TypeOfHingedDoor"]', 'Type of Hinged Door');
            if ($('#hingesTypeDiv').is(':visible')) reqSelect('[name="Component.TypeOfHinges"]', 'Type of Hinges');
            //if ($('#motorisedOperationDiv').is(':visible')) reqSelect('[name="Component.DoorSubType"]', 'Type of Operation (Motorised)');
            if ($('#motorisedOperationDiv').is(':visible')) {
                reqSelect('#motorisedOperationDiv [name="Component.DoorSubType"]', 'Type of Operation (Motorised)');
            }
        }

        // Section 5 — Motor & Chamber
        reqSelect('#drpMotorType', 'Motor Type');

        if ($('#conveyorTypeDiv').is(':visible')) reqSelect('#drpConveyor', 'Conveyor Type');

        // Maintenance door fields — only when visible
        if ($('#maintenanceDoorFields').is(':visible')) {
            reqSelect('[name="Enquiry.SideLocationDoorFAS"]', 'Side Location');
            reqField('#txtDoorWidth', 'Maintenance Door Width');
            reqField('#txtDoorHeight', 'Maintenance Door Height');
        }

        // ── Show errors or proceed ────────────────────────────
        if (errors.length > 0) {
            e.preventDefault();

            var msg = errors.length <= 3
                ? 'Required: ' + errors.join(', ') + '.'
                : 'Required: ' + errors.slice(0, 3).join(', ') + ' and ' + (errors.length - 3) + ' more field(s).';

            PB.toast(msg, 'danger');

            // Scroll to first error
            var $first = $('.pb-input-error').first();
            if ($first.length) {
                $('html, body').animate({ scrollTop: $first.offset().top - 120 }, 300);
                $first.focus();
            }
            return false;
        }

        // All valid — show loading spinner
        var $btn = $('#submitBtn');
        $btn.addClass('loading').prop('disabled', true);
        setTimeout(function () {
            $btn.removeClass('loading').prop('disabled', false);
        }, 8000);
    });

    // Clear error highlight as user corrects fields
    $(document).on('input change', '.pb-input-error', function () {
        if ($(this).val()) $(this).removeClass('pb-input-error');
    });
    $(document).on('change', 'input[name="Component.ConsumptionPerDay"]', function () {
        $(this).closest('.enq-radio-group').removeClass('pb-radio-error');
    });

    /* ─────────────────────────────────────────────────────────
       18. CLEAR BUTTON
    ───────────────────────────────────────────────────────── */
    $('#clearBtn').on('click', function () {
        $('#rectangularSection, #circularSection').hide();
        $('#paintTypeDiv, #paintConsDiv, #powderTypeDiv, #powderConsDiv').hide();
        $('#componentEntrySection, #plenumHeightDiv, #maintenanceDoorSection').hide();
        $('#conveyorTypeDiv, #conveyorSubTypeDiv, #conveyorNumberDiv').hide();
        $('#carryCapacityDiv, #pitchDiv, #speedDiv').hide();
        $('#manualOperationDiv, #motorisedOperationDiv').hide();
        $('#hingedDoorDiv, #hingesTypeDiv').hide();
        $('#maintenanceDoorFields').hide();
        $('#salesNoError, #doorHeightError').hide();
        $('#companyDropdown').hide();

        uploadedFiles = [];
        $('#fileNameDisplay').text('No file chosen');
        $('#showFilesBtn').prop('disabled', true);
        $('#txtSurfaceArea').val('0.00');

        var today = new Date();
        var dd    = String(today.getDate()).padStart(2, '0');
        var mm    = String(today.getMonth() + 1).padStart(2, '0');
        $('#txtProposalDate').val(today.getFullYear() + '-' + mm + '-' + dd);
    });

    $('#entryPreviewSection').hide();
    
    $('#txtExtractionHeight').on('input blur', function () {
        var $err = $('span[data-valmsg-for="Component.ExtractionChamberHeight"]');
        var val = parseInt($(this).val(), 10);

        if (isNaN(val) || val < 2000 || val > 3000) {
            $err.text('Extraction Chamber Height must be between 2000 and 3000 mm');
            $(this).addClass('input-error');
        } else {
            $err.text('');
            $(this).removeClass('input-error');
        }
    });

});
